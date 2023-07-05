from collections.abc import Callable, Iterable, Mapping
from typing import Any
import tensorflow as tf
import numpy as np
import csv

import grpc
from protos import ObjGen_pb2
from protos import ObjGen_pb2_grpc
import threading
from time import sleep, time
from concurrent import futures
import logging

LocalObjID = 0

class ObjLoaderIns(threading.Thread):
    def __init__(self, objID):
        threading.Thread.__init__(self)
        self.seq = None
        self.objID = objID
        self.curr = 0
    def run(self):
        obj_loader = ObjectLoader()
        obj_loader.loadTrajectory('./DataSet/TrajectoryMillion.csv')
        self.seq = obj_loader.SelectPiece(200)

    def getPos(self):
        if self.curr >= len(self.seq):
            self.curr = 0
            obj_loader = ObjectLoader()
            obj_loader.loadTrajectory('./DataSet/TrajectoryMillion.csv')
            self.seq = obj_loader.SelectPiece(200)
        pos = self.seq[self.curr]
        self.curr += 1
        return pos


class ObjDetect(ObjGen_pb2_grpc.ObjGenServicer):
    def __init__(self):
        self.thread = ObjLoaderIns(LocalObjID)
        self.thread.start()
        # response = ObjGen_pb2.LoadResponse(successful = True, ObjID = LocalObjID)
        # return response

    def GetObjPos(self, request, context):
        # 返回当前位置
        print("request", request)
        pos = self.thread.getPos()
        response = ObjGen_pb2.ObjPos(
            ObjID=request.ObjID,
            timestamp=str(int(time() + 30)),
            delta_time=pos[0],
            delta_lng=pos[1],
            delta_lat=pos[2],
            sog=pos[3],
            cog=pos[4],
            lng=pos[5],
            lat=pos[6]
        )

        return response


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


def serve():
    port = "0717"
    server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))
    ObjGen_pb2_grpc.add_ObjGenServicer_to_server(ObjDetect(), server)
    server.add_insecure_port("[::]:" + port)
    server.start()
    print("Server started, listening on " + port)
    server.wait_for_termination()


if __name__ == "__main__":
    logging.basicConfig()
    serve()
