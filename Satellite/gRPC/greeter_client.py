# Copyright 2015 gRPC authors.
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
"""The Python implementation of the GRPC helloworld.Greeter client."""

from __future__ import print_function

import logging
from time import sleep

import grpc
import helloworld_pb2
import helloworld_pb2_grpc
from satellite import SatelliteInfo
import time


def generate_request(id):
    satellite = SatelliteInfo(id)
    info, stamp = satellite.get_satellite_info()
    predict_results = satellite.detect_obj_server()
    # Create a Sat2BaseInfo request object
    # print("predict_results: ", predict_results)
    find_target = False
    print("azimuth: ", satellite.calculate_azimuth())
    if satellite.calculate_azimuth() < 120:
        find_target = True

    request = helloworld_pb2.SatRequest(
        sat_info=helloworld_pb2.SatelliteInfo(
            sat_name=info["name"],
            sat_position=helloworld_pb2.LLAPosition(
                timestamp=str(stamp),
                alt=info["alt"],
                lat=info["lat"],
                lng=info["lng"]
            )
        ),
        find_target=find_target,
        target_info=[
                        helloworld_pb2.TargetInfo(
                            target_name="test1",
                            target_position=helloworld_pb2.LLAPosition(
                                timestamp=str(int(time.time() + 30)),
                                alt=100.0,
                                lat=27.0,
                                lng=15.0
                            )
                        )
                    ] + [
                        helloworld_pb2.TargetInfo(
                            target_name="test1",
                            target_position=helloworld_pb2.LLAPosition(
                                timestamp=str(int(time.time() + 30)),
                                alt=0,
                                lat=predict_res[0],
                                lng=predict_res[1],
                            )
                        )
                        for predict_res in predict_results
                    ]
    )
    # print("Satellite client send: ")
    # print(request)
    return request


def generate_requests(id):
    requests = [
        generate_request(id),
        # generate_request()
    ]
    for request in requests:
        yield request


def run(sat_id):
    # NOTE(gRPC Python Team): .close() is possible on a channel and should be
    # used in circumstances in which the with statement does not fit the needs
    # of the code.
    print("Will try to send location ...")
    channel_options = [('grpc.keepalive_time_ms', 8000),
                       ('grpc.keepalive_timeout_ms', 5000),
                       ('grpc.http2.max_pings_without_data', 10),
                       ('grpc.keepalive_permit_without_calls', 1),
                       ('grpc.initial_reconnect_backoff_ms', 5000)  # 设置初始重连等待时间为5秒
                       ]
    with grpc.insecure_channel("43.142.83.201:50051", options=channel_options) as channel:
    # with grpc.insecure_channel("localhost:50051", options=channel_options) as channel:
        stub = helloworld_pb2_grpc.SatComStub(channel)
        while True:
            try:
                # 发送位置请求
                request = generate_requests(sat_id)
                response_iterator = stub.CommuWizSat(request)

                print("Satellite client received: ")
                for response in response_iterator:
                    print(response)

            except grpc.RpcError as e:
                print(e)
                sleep(1)
                continue


if __name__ == "__main__":
    logging.basicConfig()
    run(44752)
