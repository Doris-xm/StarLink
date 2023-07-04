import tensorflow as tf
import numpy as np
from . import Model
# import Model
import csv

n_lstm = 128
lstm_step = 6
batch_size = 1

class TrackModel:
    def __init__(self) -> None:
        self.Encoder = Model.Encoder(n_lstm, batch_size)
        self.DecoderAttention = Model.DecoderAttention(n_lstm, batch_size, 'general')

        checkpointEncode = tf.train.Checkpoint(EncoderAttention = self.Encoder)
        checkpointEncode.restore(tf.train.latest_checkpoint('./SaveEncoderAttention'))

        checkpointDecode = tf.train.Checkpoint(DecoderAttention = self.DecoderAttention)
        checkpointDecode.restore(tf.train.latest_checkpoint('./SaveDecoderAttention'))

    def predict(self, sequence, pred_len):
        """Test restored attention_seq2seq model.

        Args:
            source_seq (shape of [source_length, 7])
        Returns:
            pred [np.array(pred)]: The prediction of points. shape of [pred_length, 5].
        """
        sequence = np.array(sequence)
        source_seq, max_seq_data, min_seq_data = self.preprocess(sequence)
        print("predicting")
        pred = []
        decoder_length = pred_len
        encode_length = source_seq.shape[1]

        encode_seq = source_seq[:, :encode_length - 1, :]
        # Encode the source.
        encoder_outputs = self.Encoder(encode_seq)
        states = encoder_outputs[1:]
        history = encoder_outputs[0]
        # Decoder predicts the target_seq. decoder_in shape of [batch_size, 1, 5]
        decoder_in = tf.expand_dims(source_seq[:, -1], 1)
        print(decoder_in)
        for t in range(decoder_length):
            logit, lstm_out, de_state_h, de_state_c, _= self.DecoderAttention(decoder_in, states, history)
            # logit shape of [batch_size, 5]
            decoder_in = tf.expand_dims(logit, 1)
            history_new = tf.expand_dims(lstm_out, 1)
            history = tf.concat([history[:, 1:], history_new], 1)
            states = de_state_h, de_state_c
            pred.append(logit[0, :])

        pred = np.array(pred)

        pred_result = self.cal_position(pred, max_seq_data, min_seq_data, sequence[-1, 5], sequence[-1, 6])
        print(pred_result)
        return pred_result
    
    def preprocess(self, sequence):
        print(sequence, type(sequence))
        # normalization
        max_seq_data, min_seq_data = sequence.max(axis = 0), sequence.min(axis = 0)
        sequence = (sequence - min_seq_data) / (max_seq_data - min_seq_data)

        seq_reshape = []
        seq_reshape.append(sequence)
        seq_encode = np.array(seq_reshape)[:, :, :5]
        print(seq_encode.shape)

        return seq_encode, max_seq_data, min_seq_data
    
    def cal_position(self, pred, max, min, lng_start, lat_start):
        delta_time = pred[:, 0]
        delta_lng = pred[:, 1]
        delta_lat = pred[:, 2]
        delta_time = delta_time * (max[0] - min[0]) + min[0]
        delta_lng = delta_lng * (max[1] - min[1]) + min[1]
        delta_lat = delta_lat * (max[2] - min[2]) + min[2]
        delta_time = delta_time.tolist()
        delta_lng = delta_lng.tolist()
        delta_lat = delta_lat.tolist()
        print(pred)

        lng0 = lng_start
        lat0 = lat_start

        # [lng, lat]
        pred_list = []
        for i in range(len(delta_lng)):
            # time = delta_time[i]
            lng = lng0 + delta_lng[i]
            lat = lat0 + delta_lat[i]

            pred_list.append([lng, lat])

            lng0 = lng
            lat0 = lat
        
        return pred_list

def row2array(row):
    """Convert a row of cvs to a trajecotry point.
    
    Args:
        row (list): a row of csv file.

    Returns:
        np.array: point: delta_time(ms), delta_lng, delta_lat, sog, cog, time(ms), lng, lat
    """        
    array = np.array([row[6], row[7], row[8], row[2], row[5], row[3], row[4]], np.float32)
    return array

if __name__ == '__main__':
    # test prediction
    file_name = "./DataSet/test_fix.csv"
    points_list = []
    with open(file_name, 'r') as f:
        reader = csv.reader(f)
        for row in reader:
            point = row2array(row)
            points_list.append(point)
    
    trajectory = np.array(points_list)
    print("trajectory", trajectory.shape)

    seq_length = 121
    
    seq_temp = []
    is_valid = False
    while not is_valid:
        index = np.random.randint(0, len(trajectory) - seq_length + 1)
        seq_temp = trajectory[index: index + seq_length]
        is_valid = True
        for point in seq_temp:
            if point[0] == 0.0:
                is_valid = False
                break
    
    model = TrackModel()
    model.predict(seq_temp, 20)