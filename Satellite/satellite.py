import csv
import datetime
import multiprocessing
from enum import Enum

import numpy as np
import requests
import json
import math
from utils.j_time import TimeConverter
from model.TrackModel import TrackModel, row2array
from obj_detect_server import Detect_obj

tracking_length = 32
input_length = 120


class TARGET(Enum):
    NOTFOUND = 0
    FIND = 1
    NEAR = 2


class SatelliteInfo:
    def __init__(self, satellite_id, port):
        self.url = "https://satellitemap.space/json/sl.json?0.9801881003148946+v1007+43361"
        self.satellite = None
        self.satellite_id = satellite_id
        self.stamp = None
        self.satellite_latitude = None
        self.satellite_longitude = None
        self.satellite_altitude = None
        self.obj_latitude = 0.0
        self.obj_longitude = 0.0
        self.obj_altitude = 0.0
        #  正在跟踪的物体
        self.obj_source_seq = {}
        self.targets = {}
        #   预测模型
        self.track_model = TrackModel()
        self.port = port
        self.detect_obj = Detect_obj(self.port)
        # 创建队列用于进程间通信
        self.queue = multiprocessing.Queue()
        # 开启守护进程
        self.daemon = multiprocessing.Process(target=self.detect_obj.run, args=(self.queue,))
        self.daemon.daemon = True  # 设置守护进程
        self.daemon.start()

    def fetch_data(self):
        response = requests.get(self.url)
        data = response.json()
        self.stamp = data["stamp"]
        satellites = data["sats"]
        for satellite in satellites:
            if satellite["id"] == self.satellite_id:
                self.satellite = satellite
                self.satellite_latitude = satellite["lat"]
                self.satellite_longitude = satellite["lng"]
                self.satellite_altitude = satellite["alt"]

    def predict_trajectory(self, obj_name):

        seq_len = len(self.obj_source_seq[obj_name])

        if seq_len > input_length:
            self.obj_source_seq[obj_name] = self.obj_source_seq[obj_name][seq_len - input_length:]

        if seq_len < 5:
            return []
        predict_results = self.track_model.predict(self.obj_source_seq[obj_name], tracking_length)
        return predict_results


    def get_object_info(self):
        # 向检测模块请求当前目标的位置信息
        results = []
        try:
            while True:
                result = self.queue.get_nowait()
                objID = result[1]
                results.append(result[0])
        except Exception as e:  # nowait：队列为空时，会抛出异常Empty
            if results == []:
                return None, None
            #   获取当前位置
            curr_obj = results[len(results) - 1][0]
            # print("当前位置: ", curr_obj[0])
            #   计算是否在观测范围内
            find_target = self.calculate_azimuth([curr_obj[6], curr_obj[5]]) < 60.0
            # find_target = True
            self.targets[str(objID)] = TARGET.FIND
            if find_target:
                results = np.concatenate(results, axis=0)
                if str(objID) not in self.obj_source_seq:
                    self.obj_source_seq[str(objID)] = results
                else:
                    self.obj_source_seq[str(objID)] = np.concatenate((self.obj_source_seq[str(objID)], results), axis=0)
                return results[len(results) - 1], str(objID)
            else:
                return None, None

    def set_pos(self, pos):
        self.obj_latitude = pos.lat
        self.obj_longitude = pos.lng
        self.obj_altitude = pos.alt

    def get_satellite_info(self):
        self.fetch_data()
        return self.satellite, self.stamp

    # 增加新的观测目标
    def add_new_target(self, obj_name, history_pos):
        if obj_name not in self.targets or self.targets[obj_name] == TARGET.NOTFOUND:
            self.targets[obj_name] = TARGET.NEAR
            if history_pos is not None:
                if obj_name not in self.obj_source_seq:
                    self.obj_source_seq[obj_name] = history_pos
                else:
                    self.obj_source_seq[obj_name] = np.concatenate((self.obj_source_seq[obj_name], history_pos), axis=0)


    # 计算偏角, 返回角度
    def calculate_azimuth(self, obj_pos):
        self.fetch_data()  # 获取最新的卫星数据
        # self.detect_obj()  # 检测目标物位置
        self.obj_latitude = obj_pos[0]
        self.obj_longitude = obj_pos[1]
        self.obj_altitude = 0.0

        time_converter = TimeConverter(datetime.datetime.now())

        delta_pos = self.get_delta_pos()  # 获取卫星和目标物位置差
        sin_lat = math.sin(math.radians(self.satellite_latitude))
        cos_lat = math.cos(math.radians(self.satellite_latitude))
        theta_place = time_converter.to_lms_time(math.radians(self.obj_longitude))  # 获取LMS时间, 单位为弧度
        sin_theta = math.sin(theta_place)
        cos_theta = math.cos(theta_place)

        top_s = sin_lat * cos_theta * delta_pos[0] + sin_lat * sin_theta * delta_pos[1] - cos_lat * delta_pos[2]
        top_e = -sin_theta * delta_pos[0] + cos_theta * delta_pos[1]
        top_z = cos_lat * cos_theta * delta_pos[0] + cos_lat * sin_theta * delta_pos[1] + sin_lat * delta_pos[2]

        azimuth_rad = math.atan(-top_e / top_s)
        if top_s > 0.0:
            azimuth_rad += math.pi
        if azimuth_rad < 0.0:
            azimuth_rad += 2.0 * math.pi

        azimuth_deg = math.degrees(azimuth_rad)  # 转换为角度
        return azimuth_deg

    def get_delta_pos(self):
        longitude_rad = math.radians(self.satellite_longitude)
        obj_longitude_rad = math.radians(self.obj_longitude)

        delta_longitude = longitude_rad - obj_longitude_rad

        x = math.cos(math.radians(self.obj_latitude)) * math.sin(math.radians(self.satellite_latitude)) - math.sin(
            math.radians(self.obj_latitude)) * math.cos(math.radians(self.satellite_latitude)) * math.cos(
            delta_longitude)
        y = math.sin(delta_longitude) * math.cos(math.radians(self.satellite_latitude))
        z = math.sin(math.radians(self.obj_latitude)) * math.sin(math.radians(self.satellite_latitude)) + math.cos(
            math.radians(self.obj_latitude)) * math.cos(math.radians(self.satellite_latitude)) * math.cos(
            delta_longitude)

        return [x, y, z]
