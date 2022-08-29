import numpy as np
import sys
import tensorflow as tf
import os
from threading import Thread
from datetime import datetime
import cv2
#from object_detection.utils import label_map_util
from collections import defaultdict


detection_graph = tf.Graph()


#PATH_TO_LABELS = 'hand_label_map.pbtxt'

#NUM_CLASSES = 1
#label_map = label_map_util.load_labelmap(PATH_TO_LABELS)
#categories = label_map_util.convert_label_map_to_categories(label_map, max_num_classes=NUM_CLASSES, use_display_name=True)
#category_index = label_map_util.create_category_index(categories)


def load_inference_graph(path_to_ckpt):

	detection_graph = tf.Graph()
	with detection_graph.as_default():
		od_graph_def = tf.GraphDef()
		with tf.gfile.GFile(path_to_ckpt, 'rb') as fid:
			serialized_graph = fid.read()
			od_graph_def.ParseFromString(serialized_graph)
			tf.import_graph_def(od_graph_def, name='')
			sess = tf.Session(graph=detection_graph)
	return detection_graph, sess



def list_scaled_boxes(num_detect, score_thresh,id, scores, boxes,ids, im_width, im_height):

	sboxes = []
	for i in range(num_detect):
		if (id != 0):
			if (scores[i] >= score_thresh) and (ids[i] == id):
				(left, right, top, bottom) = (boxes[i][1] * im_width, boxes[i][3] * im_width,boxes[i][0] * im_height, boxes[i][2] * im_height)
				p = int(scores[i] * 100)
				x = int(left)
				y = int (top)
				w = int(right) - int(left)
				h = int(bottom) - int(top)
				sboxes.append([p,x,y,w,h])
			elif (scores[i] < score_thresh):
				break
		else:
			if (scores[i] >= score_thresh):
				(left, right, top, bottom) = (boxes[i][1] * im_width, boxes[i][3] * im_width,boxes[i][0] * im_height, boxes[i][2] * im_height)
				p = int(scores[i] * 100)
				oid = int(ids[i])
				x = int(left)
				y = int (top)
				w = int(right) - int(left)
				h = int(bottom) - int(top)
				sboxes.append([p,oid,x,y,w,h])
			elif (scores[i] < score_thresh):
				break

	return(sboxes)



def detect_objects(image_np, detection_graph, sess):

	image_tensor = detection_graph.get_tensor_by_name('image_tensor:0')
	detection_boxes = detection_graph.get_tensor_by_name('detection_boxes:0')
	detection_scores = detection_graph.get_tensor_by_name('detection_scores:0')
	detection_classes = detection_graph.get_tensor_by_name('detection_classes:0')
	num_detections = detection_graph.get_tensor_by_name('num_detections:0')
	image_np_expanded = np.expand_dims(image_np, axis=0)
	(boxes, scores, classes, num) = sess.run([detection_boxes, detection_scores,detection_classes, num_detections],feed_dict={image_tensor: image_np_expanded})
	return np.squeeze(boxes), np.squeeze(scores),np.squeeze(classes)


