import logging
from time import sleep

import grpc
import os
from protos import SatCom_pb2
from protos import SatCom_pb2_grpc
from satellite import SatelliteInfo
from utils.tile import getTile
from gRPC.Ports import ports

import threading

class SatelliteClient:
    def __init__(self, sat_id, port):
        self.sat_id = sat_id
        self.satellite = SatelliteInfo(sat_id, port)
        self.channel = None  # 保存通道对象
        self.stub = None  # 保存存根对象

    def connect(self):
        print("Trying to connect ...")
        channel_options = [
            ('grpc.keepalive_time_ms', 8000),
            ('grpc.keepalive_timeout_ms', 5000),
            ('grpc.http2.max_pings_without_data', 10),
            ('grpc.keepalive_permit_without_calls', 1),
            ('grpc.initial_reconnect_backoff_ms', 5000)
        ]
        # 创建通道和存根
        # self.channel = grpc.insecure_channel("localhost:50051", options=channel_options)
        self.channel = grpc.insecure_channel("43.142.83.201:50051", options=channel_options)
        # self.channel = grpc.insecure_channel("47.115.214.51:50051", options=channel_options)
        self.stub = SatCom_pb2_grpc.SatComStub(self.channel)

    def disconnect(self):
        # 关闭通道
        if self.channel:
            self.channel.close()
        self.channel = None
        self.stub = None

    def generate_request(self):
        info, stamp = self.satellite.get_satellite_info()
        #   获取当前物体位置
        curr_obj = self.satellite.get_object_info()
        # 当前没有目标
        if curr_obj is None:
            return SatCom_pb2.SatRequest(
                sat_info=SatCom_pb2.SatelliteInfo(
                    sat_name=info["name"],
                    sat_position=SatCom_pb2.LLAPosition(
                        timestamp=str(stamp),
                        alt=info["alt"],
                        lat=info["lat"],
                        lng=info["lng"]
                    )
                ),
                find_target=False,
            )

        print ("curr_obj", curr_obj)

        #   判断是否在监控范围内
        find_target = self.satellite.calculate_azimuth(curr_obj) < 60.0
        # 随便加的经纬度约束
        # if abs(info["lat"] - curr_obj[2]) > 10.0:
        #     find_target = False
        # if abs(info["lng"] - curr_obj[3]) > 10.0:
        #     find_target = False
        if find_target:
            print(info["name"] + " find target!")
            #   获取预测轨迹
            predict_results = self.satellite.predict_trajectory()
            print("predict_results", len(predict_results))

            stamp_tmp = stamp
            for predict_res in predict_results:
                stamp_tmp += 1
                predict_res.append(stamp_tmp)

        else:
            predict_results = []

        request = SatCom_pb2.SatRequest(
            sat_info=SatCom_pb2.SatelliteInfo(
                sat_name=info["name"],
                sat_position=SatCom_pb2.LLAPosition(
                    timestamp=str(stamp),
                    alt=info["alt"],
                    lat=info["lat"],
                    lng=info["lng"]
                )
            ),
            find_target=find_target,
            target_info=[
                SatCom_pb2.TargetInfo(
                    target_name="test obj",
                    target_position=SatCom_pb2.LLAPosition(
                        timestamp=str(stamp),
                        alt=0,
                        lat=curr_obj[6],
                        lng=curr_obj[5],
                    )
                )
            ] + [
                SatCom_pb2.TargetInfo(
                    target_name="test obj",
                    target_position=SatCom_pb2.LLAPosition(
                        timestamp=str(predict_res[2]),
                        alt=0,
                        lat=predict_res[0],
                        lng=predict_res[1],
                    )
                )
                for predict_res in predict_results
            ]
        )
        print(request)
        return request

    def generate_requests(self):
        requests = [
            self.generate_request()
        ]
        for request in requests:
            yield request

    def generate_photo_request(self, zone):
        x1 = zone.upper_left.lat
        y1 = zone.upper_left.lng
        x2 = zone.bottom_right.lat
        y2 = zone.bottom_right.lng
        tile = getTile(x1, y1, x2, y2)
        if zone.request_identify:
            with open('tile.jpg', 'wb') as f:
                f.write(tile)
            self.detector.detect()
            with open('./models/output/tile.jpg', 'rb') as f:
                tile = f.read()
            # delete both files
            os.remove('tile.jpg')
            os.remove('./models/output/tile.jpg')
        sat_photo_request = SatCom_pb2.SatPhotoRequest(
            timestamp = str(self.satellite.get_satellite_info()[1]), # 获取时间戳
            zone = zone,
            image_data = tile,
        )
        return sat_photo_request

    def run(self):
        logging.basicConfig()

        print("Will try to send location ...")

        while True:
            try:
                # 连接到服务器
                if self.channel==None:
                    self.connect()

                # 发送位置请求
                request = self.generate_requests()
                response_iterator = self.stub.CommuWizSat(request)

                print("Satellite client received: ")
                # for response in response_iterator:
                #     print(response)
                    # if response.take_photo:
                    #     zones = response.zone
                    #     for zone in zones:
                    #         request = self.generate_photo_request(zone)
                    #         self.stub.TakePhotos(request)

                # 添加间隔时间
                sleep(5)  # 在每次请求之后等待 5 秒

            except grpc.RpcError as e:
                print(e)
                sleep(1)
                # 断开与服务器的连接
                self.disconnect()
                continue


IDs = [
    25544,
    44713,
    44717,
    44752,
    44718,
    44725,
    44738,
    44757,
    44920,
    44926,
    45365,
    45379,
]

class SatelliteClientThread(threading.Thread):
    def __init__(self, sat_id, port):
        threading.Thread.__init__(self)
        self.client = SatelliteClient(sat_id, port)
    def run(self):
        self.client.run()


if __name__ == "__main__":
    # 创建并启动多个线程
    threads = []
    for i in range(len(IDs)):
        thread = SatelliteClientThread(IDs[i], ports[i])
        threads.append(thread)
        thread.start()

    # 等待所有线程完成
    for thread in threads:
        thread.join()

# # 单线程
# if __name__ == "__main__":
#     client = SatelliteClient(25544, ports[0])
#     print("Satellite client started ...")
#     client.run()