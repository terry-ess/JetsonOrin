import time
start = time.perf_counter()
print("Loading imports")

import socket
import Log
import os
import detector_utils as du
import csv
import numpy as np
import sys
import cv2
import traceback

PORT = 60000
HOST_NAME = "O1"
MODEL_DIR = "/home/terry/server/models/"
PIC_DIR = "/home/terry/server/pics/"
NETWORK = "192.168.0"



lf = Log.Log("tensorflow object detection inference server")
md = dict()


def Load(line):

	start = time.perf_counter();
	data = []
	row = line.split(',')
	name = row[0]
	row.pop(0)
	print("loading " + name + " model")
	detect_graph,sess = du.load_inference_graph(MODEL_DIR + row[1])
	data.append(detect_graph)
	data.append(sess)
	md[name] = data
	image = cv2.imread(MODEL_DIR + row[2])
	cimage = cv2.cvtColor(image,cv2.COLOR_BGR2RGB)
	du.detect_objects(cimage,data[0],data[1])
	stop = time.perf_counter()
	lf.WriteLine("load in {} ms".format(int((stop - start) * 1000)))



def Unload():

	for name in md:
		data = md[name]
		data[1].close()
	du.tf.compat.v1.reset_default_graph()
	md.clear()
	print("unloaded models")



def Server():

	print("Running tensorflow object detection inference server")
	lf.Open()
	print("Opening UDP socket")
	f = open("ip.txt",'r')		# when try to use gethostname etc. on Linux it only provides localhost, so use script to determine IP address
	ethaddr = ""
	wladdr = ""
	for line in f:	
		if (line.startswith("eth0")):
			ethaddr = line.split(' ')[1].rstrip()
			if (ethaddr.startswith(NETWORK)):
				print(ethaddr)
			else:
				ethaddr = ""
		elif (line.startswith('wlan0')):
			wladdr = line.split(' ')[1].rstrip()
			if (wladdr.startswith(NETWORK)):
				print(wladdr)
			else:
				wladdr = ""
	if (len(ethaddr) > 0):
		ME = ethaddr
	elif (len(wladdr) > 0):
		ME = wladdr
	else:
		print("Could not determine IP address.")
		lf.WriteLine("Could not determine IP address.")
		return(1)

	try:
		sock = socket.socket(socket.AF_INET,socket.SOCK_DGRAM)
		sock.bind((ME,PORT))
		lf.WriteLine("socket: {0},{1}".format(ME,PORT))
		print("socket: {0},{1}".format(ME,PORT))

	except:
		traceback.print_exc(1)
		print("Could not open UDP socket")
		lf.WriteLine("Could not open UDP socket")
		return(1)

	print("Starting server loop")
	stop = time.perf_counter()
	lf.WriteLine("Load {0} sec".format(stop - start))

	try:
		while True:

			try:
				data,conn = sock.recvfrom(1024)

			except KeyboardInterrupt:
				print("User close")
				return(1)

			except:
				lf.WriteLine("Socket recvfrom exception {0}".format(err.errno))
				break

			if (len(data) > 0):
				s = bytes.decode(data)
				lf.WriteLine(s)
				if s == "exit":
					break
				elif (s== "hello"):
					sock.sendto(bytes("OK","ascii"),conn)
					lf.WriteLine("OK")
				elif (s.startswith("load,")):
					try:
						Load(s.replace("load,",""))
						sock.sendto(bytes("OK","ascii"),conn)
						lf.WriteLine("OK")
					except:
						sock.sendto(bytes("FAIL","ascii"),conn)
						lf.WriteLine("FAIL, " + traceback.format_exc())
				elif (s == "unload"):
					Unload()
					sock.sendto(bytes("OK","ascii"),conn)
					lf.WriteLine("OK")
				else:
					sa = str.split(s,",")
					if len(sa) == 4:
						if (os.path.exists(PIC_DIR + sa[1])):
							image = cv2.imread(PIC_DIR + sa[1])
							if (image.size > 0):
								cimage = cv2.cvtColor(image,cv2.COLOR_BGR2RGB)
								score_limit = float(sa[2])
								id = int(sa[3])
								try:
									data = md[sa[0]]
								except:
									lf.WriteLine("Dictionary key exception")
									data = None
								if data != None:
									boxes,scores,ids = du.detect_objects(cimage,data[0],data[1])
									if (scores[0] > score_limit):
										dboxes = du.list_scaled_boxes(5,score_limit,id, scores, boxes,ids,image.shape[1],image.shape[0])
										if (len(dboxes) > 0):
											rsp = "OK "
											for b in dboxes:
												rsp += str(b)
											sock.sendto(bytes(rsp,"ascii"),conn)
											lf.WriteLine(rsp)
										else:
											sock.sendto(bytes("FAIL no detection","ascii"),conn)
											lf.WriteLine("no detection")
									else:
										sock.sendto(bytes("FAIL bad detection object","ascii"),conn)
										lf.WriteLine("bad detection object")
								else:
									sock.sendto(bytes("FAIL,unknown object","ascii"),conn)
									lf.WriteLine("unknown object")
							else:
								sock.sendto(bytes("FAIL,image size 0","ascii"),conn)
								lf.WriteLine("image size 0")
							os.remove(PIC_DIR + sa[1],dir_fd=None)
						else:
							sock.sendto(bytes("FAIL,file does not exist","ascii"),conn)
							lf.WriteLine("file does not exist")
					else:
						sock.sendto(bytes("FAIL,incorrect format","ascii"),conn)
						lf.WriteLine("file incorrect format")
			else:
				lf.WriteLine("no data reception")
				break

	except KeyboardInterrupt:
		print("User close")

	lf.WriteLine("Tensorflow object detection inference server closed.")
	lf.Close()



if __name__ == "__main__":
	Server()
	

