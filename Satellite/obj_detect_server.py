from __future__ import print_function

import logging
import multiprocessing

import grpc
from concurrent import futures

import numpy as np

from protos import ObjGen_pb2_grpc


class Detect_obj(ObjGen_pb2_grpc.ObjGenServicer):

    def __init__(self, port):
        print("obj server init")
        self.port = port
        self.traj = []
        self.curr_traj = None
        self.queue = multiprocessing.Queue()  # 创建队列

    def run(self, queue):
        self.queue = queue
        server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))
        ObjGen_pb2_grpc.add_ObjGenServicer_to_server(self, server)
        server.add_insecure_port("[::]:" + self.port)
        server.start()
        print("Server started, listening on " + self.port)
        server.wait_for_termination()

    def SendObjPos(self, request, context):
        print("obj server send obj pos", request)
        request_ndarray = np.array([[request.delta_time, request.delta_lng, request.delta_lat, request.sog,
                                     request.cog, request.lng, request.lat]])
        self.queue.put(request_ndarray)
        print("queue size", self.queue.empty())
        # if self.traj == []:
        #     self.traj = request_ndarray
        # else:
        #     self.traj = np.concatenate((self.traj, request_ndarray), axis=0)
        #
        # self.curr_traj = request_ndarray
        # print("curr_traj", self.curr_traj)


# if __name__ == "__main__":
#     logging.basicConfig()
#     detect_obj(0)
