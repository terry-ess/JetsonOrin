#!/bin/sh
cd /home/terry/server
ip -4 -o a | cut -d ' ' -f 2,7 | cut -d '/' -f 1 > ip.txt
python ODServer.py
