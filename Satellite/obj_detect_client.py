from __future__ import print_function

import logging
from time import sleep

import grpc
from protos import ObjGen_pb2
from protos import ObjGen_pb2_grpc
import time

def detect_obj(obj_id):
    # NOTE(gRPC Python Team): .close() is possible on a channel and should be
    # used in circumstances in which the with statement does not fit the needs
    # of the code.
    print("Will try to detect object ...")
    channel_options = [('grpc.keepalive_time_ms', 8000),
                       ('grpc.keepalive_timeout_ms', 5000),
                       ('grpc.http2.max_pings_without_data', 10),
                       ('grpc.keepalive_permit_without_calls', 1),
                       ('grpc.initial_reconnect_backoff_ms', 5000)  # 设置初始重连等待时间为5秒
                       ]
    with grpc.insecure_channel("localhost:0717", options=channel_options) as channel:
        stub = ObjGen_pb2_grpc.ObjGenStub(channel)

        try:
            request = ObjGen_pb2.Obj(
                ObjID=obj_id,
            )
            response = stub.GetObjPos(request)

            print(response)
            return response

        except grpc.RpcError as e:
            print(e)
            sleep(1)


# if __name__ == "__main__":
#     logging.basicConfig()
#     detect_obj(0)
