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
"""The Python implementation of the GRPC helloworld.Greeter server."""

from concurrent import futures
import logging

import grpc
import helloworld_pb2
import helloworld_pb2_grpc
from satellite import SatelliteInfo


class Greeter(helloworld_pb2_grpc.SatComServicer):
    # def SayHello(self, request, context):
    #     print(request)
    #     satellite_ = SatelliteInfo(25544)
    #     print(satellite_.get_satellite_info())
    #     angle = satellite_.detect_obj(request)
    #     return helloworld_pb2._builder.BuildTopDescriptorsAndMessages(message="info:%s\n angle:%f" % (satellite_.get_satellite_info(), angle))

    def CommuWizSat(self, request_iterator, context):
        for request in request_iterator:
            print("receive request:\n")
            print(request)
            # Handle the request and generate the response
            # You can access the request fields and perform necessary computations
            # Generate the response message

            # For example, you can return a dummy response
            response = helloworld_pb2.Base2SatInfo(
                base_position=helloworld_pb2.PositionInfo(timestamp="123", alt=100.0, lat=27.0, lng=15.0, target_name="test"),
                find_target=True,
                target_position=[
                    helloworld_pb2.PositionInfo(timestamp="123", alt=100.0, lat=27.0, lng=15.0, target_name="test"),
                    helloworld_pb2.PositionInfo(timestamp="456", alt=200.0, lat=28.0, lng=16.0, target_name="test"),
                ]
            )

            # Yield the response
            yield response


def serve():
    port = "50051"
    server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))
    helloworld_pb2_grpc.add_SatComServicer_to_server(Greeter(), server)
    server.add_insecure_port("[::]:" + port)
    server.start()
    print("Server started, listening on " + port)
    server.wait_for_termination()


if __name__ == "__main__":
    logging.basicConfig()
    serve()

