import tensorflow as tf
import numpy as np
import Model
import csv


class ObjectLoader():
    def __init__(self, addr, port) -> None:
        self.target_addr = addr
        self.target_port = port

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
    
    def SendObjInfo(self):
        pass