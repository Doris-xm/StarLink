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

def generate_request():
    satellite = SatelliteInfo(25544)
    info, stamp = satellite.get_satellite_info()
    # Create a Sat2BaseInfo request object
    request = helloworld_pb2.Sat2BaseInfo()
    request.sat_name = info["name"]

    # Set sat_position
    sat_position = helloworld_pb2.PositionInfo()
    sat_position.timestamp = str(stamp)
    sat_position.alt = 100.0
    sat_position.lat = 27.0
    sat_position.lng = 15.0
    sat_position.target_name = "test"
    request.sat_position.CopyFrom(sat_position)

    # Set target_position (repeated field)
    target_positions = [
        helloworld_pb2.PositionInfo(timestamp=str(int(time.time()+30)), alt=100.0, lat=27.0, lng=15.0, target_name="test"),
        helloworld_pb2.PositionInfo(timestamp=str(int(time.time()+30)), alt=200.0, lat=28.0, lng=16.0, target_name="test"),
        helloworld_pb2.PositionInfo(timestamp=str(int(time.time()+30)), alt=200.0, lat=28.0, lng=16.0, target_name="test"),
        helloworld_pb2.PositionInfo(timestamp=str(int(time.time()+30)), alt=200.0, lat=28.0, lng=16.0, target_name="test"),
    ]
    request.target_position.extend(target_positions)

    # Set find_target
    request.find_target = True

    # print(request)
    return request


def generate_requests():
    requests = [
        generate_request(),
        # generate_request()
    ]
    for request in requests:
        yield request

def run():
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
                #发送位置请求
                request = generate_requests()
                # response = stub.CommuWizSat(request)
                # print(response.code())
                response_iterator = stub.CommuWizSat(request)

                print("Satellite client received: ")
                for response in response_iterator:
                    print(response)


                #处理响应
                # print(response)
                satellite = SatelliteInfo(25544)
                satellite.detect_obj(generate_request().target_position[0])
                print("calculate_azimuth: ")
                print(satellite.calculate_azimuth())

                # 休眠一段时间
                # sleep(1)
            except grpc.RpcError as e:
                print(e)
                sleep(1)
                continue


if __name__ == "__main__":
    logging.basicConfig()
    run()
