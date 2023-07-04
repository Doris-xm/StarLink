import csv
import datetime

import numpy as np
import requests
import json
import math
from utils.j_time import TimeConverter
from model.TrackModel import TrackModel, row2array


class SatelliteInfo:
    def __init__(self, satellite_id):
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
        self.obj_source_seq = []
        self.track_model = TrackModel()

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

    def detect_obj_server(self):
        # TODO: listen on port to get object position info
        tracking_length = 32
        input_length = 120
        location_info = self.get_object_info()  # ndarray： 121*7

        # self.obj_source_seq.append(location_info)  # list：1*121*7
        if len(self.obj_source_seq) == 0:
            self.obj_source_seq = location_info
        else:
            self.obj_source_seq = np.concatenate((self.obj_source_seq, location_info), axis=0)  # 第一维拼接 ndarray：121*7

        seq_len = len(self.obj_source_seq)

        if seq_len > input_length:
            self.obj_source_seq = self.obj_source_seq[seq_len - input_length:]

        # self.obj_source_seq = np.array(self.obj_source_seq)  # 将列表转换为ndarray
        predict_result = self.track_model.predict(self.obj_source_seq, tracking_length)
        return predict_result

    def get_object_info(self):
        # TODO: listen on port to get object position info
        file_name = "../model/DataSet/test_fix.csv"
        points_list = []
        with open(file_name, 'r') as f:
            reader = csv.reader(f)
            for row in reader:
                point = row2array(row)
                points_list.append(point)

        trajectory = np.array(points_list)
        seq_length = 121

        seq_temp = []
        is_valid = False
        while not is_valid:
            index = np.random.randint(0, len(trajectory) - seq_length + 1)
            seq_temp = trajectory[index: index + seq_length]
            is_valid = True
            for point in seq_temp:
                if point[0] == 0.0:
                    is_valid = False
                    break
        return seq_temp

    def set_pos(self, pos):
        self.obj_latitude = pos.lat
        self.obj_longitude = pos.lng
        self.obj_altitude = pos.alt

    def get_satellite_info(self):
        self.fetch_data()
        return self.satellite, self.stamp

    # 计算偏角, 返回角度
    def calculate_azimuth(self):
        self.fetch_data()  # 获取最新的卫星数据
        # self.detect_obj()  # 检测目标物位置

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
