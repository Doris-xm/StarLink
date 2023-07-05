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
import logging
from concurrent import futures


import grpc
from protos import SatCom_pb2_grpc
from protos import SatCom_pb2
# from satellite import SatelliteInfo


class Test(SatCom_pb2_grpc.SatComServicer):
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
            response = SatCom_pb2.Base2Sat(
                base_position=SatCom_pb2.LLPosition(timestamp="123", lat=27.0, lng=15.0,),
                find_target=True,
                target_info = [ SatCom_pb2.TargetInfo(target_name="test1",
                                                          target_position=SatCom_pb2.LLAPosition(timestamp="123", lat=27.0, lng=15.0, alt=100.0),),
                                SatCom_pb2.TargetInfo(target_name="test2",
                                                            target_position=SatCom_pb2.LLAPosition(timestamp="123", lat=27.0, lng=15.0, alt=100.0),),
                                ],
                take_photo=True,
                zone = [
                    SatCom_pb2.ZoneInfo(request_identify=True,
                                        upper_left=SatCom_pb2.LLPosition(timestamp="123", lat=27.0, lng=15.0,),
                                        bottom_right=SatCom_pb2.LLPosition(timestamp="123", lat=27.0, lng=15.0,),),
                    SatCom_pb2.ZoneInfo(request_identify=True,
                                        upper_left=SatCom_pb2.LLPosition(timestamp="123", lat=27.0, lng=15.0,),
                                        bottom_right=SatCom_pb2.LLPosition(timestamp="123", lat=27.0, lng=15.0,),),
                ]
            )

            # Yield the response
            yield response


def serve():
    port = "50051"
    server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))
    SatCom_pb2_grpc.add_SatComServicer_to_server(Test(), server)
    server.add_insecure_port("[::]:" + port)
    server.start()
    print("Server started, listening on " + port)
    server.wait_for_termination()


if __name__ == "__main__":
    logging.basicConfig()
    serve()

