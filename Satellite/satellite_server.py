import datetime
import requests
import json
import math
import grpc

import sys
sys.path.append(".")
sys.path.append("./protos")

for i in sys.path:
     print(i)

from concurrent import futures

from protos import SatCom_pb2_grpc
from protos import SatCom_pb2

from satellite import SatelliteInfo

import threading
from time import sleep


class PredDaemon(threading.Thread):
    def __init__(self, satellite):
        threading.Thread.__init__(self)
        self.satellite = satellite
    
    def run(self):
        while True:
            sleep(1)
            self.satellite.detect_obj()


class SatelliteServer(SatCom_pb2_grpc.SatComServicer):
    def __init__(self, satellite_id):
        self.satellite_id = satellite_id
        self.satelliteInfo = SatelliteInfo(satellite_id)
        # pred = PredDaemon(self.satelliteInfo)
        # pred.start()

    def GetObjPosition(self, request, context):
        len = self.satelliteInfo.set_object_info(request.ObjID,
                                           request.delta_time,
                                           request.delta_lng,
                                           request.delta_lat,
                                           request.sog,
                                           request.cog,
                                           request.lng,
                                           request.lat)
        response = SatCom_pb2.ObjPosResponse(seq_len=len)
        print(len)
        return response
    
    def PredictObjTrack(self, request, context):
        predict_result = self.satelliteInfo.detect_obj()
        # predict_result =  self.satelliteInfo.predict_result
        response = SatCom_pb2.PredictTrack()
        for i in range(len(predict_result)):
            pred_point = SatCom_pb2.TrackPoint()
            pred_point.lng = predict_result[i][0]
            pred_point.lat = predict_result[i][1]
            response.pred_point.append(pred_point)
            # point_list.append(pred_point)
        # response.pred_point.extend(point_list)
        return response

def serve():
    port = "7777"
    server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))
    SatCom_pb2_grpc.add_SatComServicer_to_server(SatelliteServer(2012), server)
    server.add_insecure_port("[::]:" + port)
    server.start()
    print("Server started, listening on " + port)
    server.wait_for_termination()


if __name__ == "__main__":
    serve()