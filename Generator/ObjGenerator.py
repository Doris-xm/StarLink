from collections.abc import Callable, Iterable, Mapping
from typing import Any
import tensorflow as tf
import numpy as np
import csv

import grpc
import ObjGen_pb2
import ObjGen_pb2_grpc
import threading
from time import sleep
from concurrent import futures

LocalObjID = 0

class ObjLoaderIns(threading.Thread):
    def __init__(self, objID, addr, port):
        threading.Thread.__init__(self)
        self.objID = objID
        self.addr = addr
        self.port = port
    def run(self):
        obj_loader = ObjectLoader()
        obj_loader.loadTrajectory('./DataSet/TrajectoryMillion.csv')
        seq = obj_loader.SelectPiece(200)
        print(seq.shape)
        channel_options = [('grpc.keepalive_time_ms', 80000),
                    ('grpc.keepalive_timeout_ms', 50000),
                    ('grpc.http2.max_pings_without_data', 10),
                    ('grpc.keepalive_permit_without_calls', 1),
                    ('grpc.initial_reconnect_backoff_ms', 50000)  # 设置初始重连等待时间为5秒
                    ]
        with grpc.insecure_channel(self.addr+ ":" + self.port, channel_options) as channel:
            stub = ObjGen_pb2_grpc.SatServerStub(channel)
            for i in range(len(seq)):
                ObjInfo = ObjGen_pb2.ObjPosition()
                ObjInfo.ObjID = self.objID
                print(seq[i])
                ObjInfo.delta_time = seq[i, 0]
                ObjInfo.delta_lng = seq[i, 1]
                ObjInfo.delta_lat = seq[i, 2]
                ObjInfo.sog = seq[i, 3]
                ObjInfo.cog = seq[i, 4]
                ObjInfo.lng = seq[i, 5]
                ObjInfo.lat = seq[i, 6]
                
                response = stub.GetObjPosition(ObjInfo)
                # print(response)

                sleep(0.1)
                if(i >= 120):
                    sleep(10)

# for Predict test
class PredIns(threading.Thread):
    def __init__(self, addr, port):
        threading.Thread.__init__(self)
        self.addr = addr
        self.port = port
    def run(self) -> None:
        while True:
            sleep(15)
            with grpc.insecure_channel(self.addr+ ":" + self.port) as channel:
                stub = ObjGen_pb2_grpc.SatServerStub(channel)
                objInfo = ObjGen_pb2.ObjInfo(ObjID=0)
                pred = stub.PredictObjTrack(objInfo)
                print(pred)


class Greeter(ObjGen_pb2_grpc.ObjGenServicer):
    def StartLoadObj(self, request, context):
        thread = ObjLoaderIns(LocalObjID, request.addr, request.port)
        thread.start()
        response = ObjGen_pb2.LoadResponse(successful = True, ObjID = LocalObjID)
        return response
    def testLoad(self):
        thread = ObjLoaderIns(LocalObjID, "localhost", '7777')
        thread.start()

        # for Predict test

        # thread2 = PredIns("localhost", '7777')
        # thread2.start()


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
    ObjGen_pb2_grpc.add_SatComServicer_to_server(Greeter(), server)
    server.add_insecure_port("[::]:" + port)
    server.start()
    print("Server started, listening on " + port)
    server.wait_for_termination()


if __name__ == "__main__":
    test = Greeter()
    test.testLoad()
