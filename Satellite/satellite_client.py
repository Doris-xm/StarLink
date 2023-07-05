import logging
from time import sleep

import grpc
from protos import SatCom_pb2
from protos import SatCom_pb2_grpc
from satellite import SatelliteInfo


class SatelliteClient:
    def __init__(self, sat_id):
        self.sat_id = sat_id
        self.satellite = SatelliteInfo(sat_id)
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
        self.channel = grpc.insecure_channel("43.142.83.201:50051", options=channel_options)
        # self.channel = grpc.insecure_channel("localhost:50051", options=channel_options)
        self.stub = SatCom_pb2_grpc.SatComStub(self.channel)

    def disconnect(self):
        # 关闭通道
        if self.channel:
            self.channel.close()
        self.channel = None
        self.stub = None

    def generate_request(self):
        info, stamp = self.satellite.get_satellite_info()
        curr_obj, predict_results = self.satellite.predict_trajectory()
        find_target = self.satellite.calculate_azimuth() < 120

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
            # find_target=find_target,
            find_target=True,
            target_info=[
                SatCom_pb2.TargetInfo(
                    target_name=str(curr_obj[0]),
                    target_position=SatCom_pb2.LLAPosition(
                        timestamp=curr_obj[1],
                        alt=0,
                        lat=curr_obj[2],
                        lng=curr_obj[3],
                    )
                )
            ] + [
                SatCom_pb2.TargetInfo(
                    target_name="test1",
                    target_position=SatCom_pb2.LLAPosition(
                        timestamp=curr_obj[1],
                        alt=0,
                        lat=predict_res[0],
                        lng=predict_res[1],
                    )
                )
                for predict_res in predict_results
            ]
        )

        return request

    def generate_requests(self):
        requests = [
            self.generate_request()
        ]
        for request in requests:
            yield request

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
                for response in response_iterator:
                    print(response)

                # 添加间隔时间
                sleep(5)  # 在每次请求之后等待 5 秒

            except grpc.RpcError as e:
                print(e)
                sleep(1)
                # 断开与服务器的连接
                self.disconnect()
                continue


if __name__ == "__main__":
    client = SatelliteClient(44752)
    client.run()
