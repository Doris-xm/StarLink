import logging
from time import sleep

import grpc
from protos import SatCom_pb2
from protos import SatCom_pb2_grpc
from satellite import SatelliteInfo
from utils.tile import getTile


class SatelliteClient:
    def __init__(self, sat_id):
        self.sat_id = sat_id
        self.satellite = SatelliteInfo(sat_id)

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
            find_target=find_target,
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
    
    def generate_photo_request(self, zone):
        x1 = zone.upper_left.lat
        y1 = zone.upper_left.lng
        x2 = zone.bottom_right.lat
        y2 = zone.bottom_right.lng
        tile = getTile(x1, y1, x2, y2)
        sat_photo_request = SatCom_pb2.SatPhotoRequest(
            timestamp = str(self.satellite.get_satellite_info()[1]), # 获取时间戳
            zone = zone,
            image_data = tile,
        )
        return sat_photo_request

    def run(self):
        logging.basicConfig()

        print("Will try to send location ...")
        channel_options = [
            ('grpc.keepalive_time_ms', 8000),
            ('grpc.keepalive_timeout_ms', 5000),
            ('grpc.http2.max_pings_without_data', 10),
            ('grpc.keepalive_permit_without_calls', 1),
            ('grpc.initial_reconnect_backoff_ms', 5000)
        ]

        with grpc.insecure_channel("localhost:50051", options=channel_options) as channel:
            stub = SatCom_pb2_grpc.SatComStub(channel)

            while True:
                try:
                    # 发送位置请求
                    request = self.generate_requests()
                    response_iterator = stub.CommuWizSat(request)

                    print("Satellite client received: ")
                    for response in response_iterator:
                        if response.take_photo:
                            zones = response.zone
                            for zone in zones:
                                request = self.generate_photo_request(zone)
                                stub.TakePhotos(request)

                    # 添加间隔时间
                    sleep(5)  # 在每次请求之后等待 5 秒

                except grpc.RpcError as e:
                    print(e)
                    sleep(1)
                    continue


if __name__ == "__main__":
    client = SatelliteClient(44752)
    client.run()
