from collections.abc import Callable, Iterable, Mapping
from typing import Any
import numpy as np
import csv

import grpc
from protos import ObjGen_pb2
from protos import ObjGen_pb2_grpc
import threading
from time import sleep, time
from concurrent import futures
import logging
from gRPC.Ports import ports

LocalObjID = 0

class ObjDetect():
    def __init__(self):
        self.addr = "localhost"
        self.objID = LocalObjID

    def run(self):
        obj_loader = ObjectLoader()
        obj_loader.loadTrajectory('./DataSet/TrajectoryMillionClean.csv')
        seq = obj_loader.SelectPiece(2000)

        while True:
            seq = obj_loader.SelectPiece(2000)
            for i in range(len(seq)):
                ObjInfo = ObjGen_pb2.ObjPos()
                ObjInfo.ObjID = self.objID
                # print(seq[i])
                ObjInfo.delta_time = seq[i, 0]
                ObjInfo.delta_lng = seq[i, 1]
                ObjInfo.delta_lat = seq[i, 2]
                ObjInfo.sog = seq[i, 3]
                ObjInfo.cog = seq[i, 4]
                ObjInfo.lng = seq[i, 5]
                ObjInfo.lat = seq[i, 6]
                for port in ports:  # 依次向各个端口发送
                    print(port)
                    try:
                        with grpc.insecure_channel(self.addr + ":" + str(port)) as channel:
                            stub = ObjGen_pb2_grpc.ObjGenStub(channel)
                            stub.SendObjPos(ObjInfo)    # 不等待返回
                    except Exception as e:
                        print(e)
                        continue

                if i >= 120:
                    sleep(0.1)
                print(ObjInfo)
            self.objID = self.objID + 1


class ObjectLoader():
    def row2array(self, row):
        """Convert a row of cvs to a trajecotry point.
        
        Args:
            row (list): a row of csv file.

        Returns:
            np.array: point: delta_time(ms), delta_lng, delta_lat, sog, cog, time(ms), lng, lat
        """        
        array = np.array([row[6], row[7], row[8], row[2], row[5], row[3], row[4]], np.float32)
        return array
    
    def loadTrajectory(self, file_name):
        """Load trajectory data for testing. 

        trajectory.shape = [N, 7]
        The trajectories are converted to np.array([N, 7]), normalized and stored in self.trajectory. 
        (including lng, lat)

        Args:
            file_name (string): file name of the csv.
        """        
        self.file_name = file_name
        points_list = []
        with open(self.file_name, 'r') as f:
            reader = csv.reader(f)
            for row in reader:
                point = self.row2array(row)
                points_list.append(point)
        
        self.trajectory = np.array(points_list)
        print("trajectory", self.trajectory.shape)

    def SelectPiece(self, length):
        is_valid = False
        while not is_valid:
            index = np.random.randint(0, len(self.trajectory) - length + 1)
            self.sequence = self.trajectory[index: index + length]
            is_valid = True
            for point in self.sequence:
                if point[0] == 0.0:
                    is_valid = False
                    break
        return self.sequence
    
    def SendObjInfo(self):
        pass


# def broadcast():
#     port = "0717"
#     server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))
#     ObjGen_pb2_grpc.add_ObjGenServicer_to_server(ObjDetect(), server)
#     server.add_insecure_port("[::]:" + port)
#     server.start()
#     print("Server started, listening on " + port)
#     server.wait_for_termination()


if __name__ == "__main__":
    logging.basicConfig()
    # broadcast()
    obj_detect = ObjDetect()
    obj_detect.run()
