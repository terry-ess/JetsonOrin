# Jetson AGX Orin Developer Kit
This project is a quick prototype to determine if the Jetson AGX Orin  actually provides some real deep learning inference power that is likely to be of use in the development of autonomous robotics. It uses some TensorFlow 1 object detection models from my [autonomous robotic arm project (ARA)](https://github.com/terry-ess/ARA) to quantify the impact of the Orin compared to other options. It consists of two Visual Studio 2019 projects, the inference server to run on the Orin Developer Kit and the Windows .NET based test interface.

Please note that this project is not ongoing. It has an [MIT license](LICENSE.TXT) and is provided here for those interested in AI engineering that would like to use it as a starting point for their own work.

### Orin Setup
 
I used a setup some what different then that shown in the Nvidia examples since I had no need for Docker containers but did want easy file transferring with Windows based computers. It is summarized below:

1. Initial setup per https://developer.nvidia.com/embedded/learn/get-started-jetson-agx-orin-devkit for headless configuration.

2. Installing SAMBA (used for file transfer from the test interface)

    $ sudo apt install samba

    Edit /etc/samba/smb.conf file, uncomment the [homes] entry in the  "Share Definitions" section

    Start the Samba and NetBios nameservice services:
    $ sudo systemctl start smbd
    $ sudo systemctl start nmbd

    From Windows platform connect using "Map network drive" to "\\\<orin hostname>"\<orin user name>

3. Installing NVIDA container of TensorFlow 1.15 that matches the Orin jetpack

    $ sudo apt-get update
    $ sudo apt-get install libhdf5-serial-dev hdf5-tools libhdf5-dev zlib1g-dev zip libjpeg8-dev liblapack-dev libblas-dev gfortran
    $ sudo apt-get install python3-pip
    $ sudo pip3 install -U pip testresources setuptools\==49.6.0
    $ sudo pip3 install -U --no-deps numpy\==1.19.4 future\==0.18.2 mock\==3.0.5 keras_preprocessing\==1.1.2 keras_applications\==1.0.8 gast\==0.4.0 protobuf\==3.02 pybind11 cython pkgconfig packaging
    $ sudo env H5PY_SETUP_REQUIRES=0 pip3 install -U h5py==3.1.0

    Download tensorflow-1.15.5+nv22.5-cp38-cp38-linux_aarch64 from https://developer.download.nvidia.com/compute/redist/jp/v50/tensorflow/.  Transfer to Orin.

    $ python -m pip install tensorflow-1.15.5+nv22.5-cp38-cp38-linux_aarch64.whl


### Orin Server

The server is a Python 3.8 application that implements a simple UDP/IP "server" that uses TensorFlow 1.15 for object detection inference. It is basically the same server as the ODServer in the [ARA project](https://github.com/terry-ess/ARA) with minor modification. The actual models are stored in a sub-directory on the Orin along with the test image used in the initial load. The picture files to be used in an inference are uploaded to another sub-directory on the Orin prior to each inference command.

Supported commands:

- Are you there? - hello
- Load a model - load,model name, frozen graph name, test image name
- Unload all the loaded models - unload
- Shutdown - exit
- Run an inference -  model name, image name, min. score, object ID

Models used in this experiment are from my [ARA project](https://github.com/terry-ess/ARA):

1. [TensorFlow 1 object detection model zoo pre-trained models](https://github.com/tensorflow/models/blob/master/research/object_detection/g3doc/tf1_detection_zoo.md)
2. Parts: [based on ssd_resnet50_v1_fpn](http://download.tensorflow.org/models/object_detection/ssd_resnet50_v1_fpn_shared_box_predictor_640x640_coco14_sync_2018_07_03.tar.gz)
3. Hand (quick scans): [based on ssd_mobilenet_v1](http://download.tensorflow.org/models/object_detection/ssd_mobilenet_v1_coco_2018_01_28.tar.gz)
4. Hand HD: [based on ssd_mobilenet_v1_fpn](http://download.tensorflow.org/models/object_detection/ssd_mobilenet_v1_fpn_shared_box_predictor_640x640_coco14_sync_2018_07_03.tar.gz)

### Local host Servers
The TensorFlow and OpenVINO servers used in local host connections are the ODServer and OVServer from the [ARA project](https://github.com/terry-ess/ARA).

### Orin Test Interface
The test interface is a simple Windows .NET 4.8 form application. It provides the means to load and unload models and to run multiple inferences.  It does the measurement of round trip detection time, checks the results against the labels and logs everything.

### Results Summary
The following chart shows the average round trip single inference time with different loaded ARA models using their evaluation dataset as input. Time is in milliseconds

| Inference HW/SW | Hand ssd_moblenet_v1(300 x 300) | Hand HD ssd_moblenet_v1_fpn(640 x 640) | Parts ssd_resnet50_v1_fpn(640 x 640) | Connection |
|--------------|------|---------|--------|---|
|AGX Orin / TensorFlow 1.15|82.9|92.5|88.4| 1000 Mbps Ethernet [^1] |
|RTX 2060  Super / TensorFlow 1.15|76.8|76.0|73.5| 433 Mbps WIFI [^1]|
|i7-6700 CPU / OpenVINO (FP32)|37.9|341.8|481.0| local host |
|i7-6700 CPU / TensorFlow 1.15|62.8|387.4|588.4|local host |
|i7-10700 CPU / OpenVINO (FP32)|31.9|158.9|219.8| local host |
|i7-10700 CPU/ TensorFlow 1.15|44.2|234.9|349.0|local host|

It should be noted that using TensofFow-TensorRT on the AGX Orin with FP32 or FP16 was slightly slower then just TensorFlow. 

The accuracy of the Orin versus the "base case" (the i7-6700 CPU used in the development of the ARA project) as determined by the test interface, was the same for both TensorFlow and OpenVINO.

In addition, the full set of object detection models used on the [ARA project](https://github.com/terry-ess/ARA) were loaded and inferences run to check "loaded" capability.  At no time during the inference runs did the Orin's fan come on, so as expected, it used a lot less then its rated 60 watts to run single inferences even if they occurred immediately one after and other.

### Conclusions
 For everything except the smallest object detection models, the Orin provides significantly improved inference performance. Both its form factor and power consumption will fit on most autonomous robotic platforms.  However, it is expensive with a price around $2000.

By offloading object detection from the main compute platform, some important large scale parallelisms are possible.  Interestingly, the results indicate that the optimal results when both simple and more complex models are used is to keep the smaller models on the main compute platform using OpenVINO and place the larger models on an AGX Orin.

What would be really interesting is to see how the Orin stacks up with the [GrAI Matter Labs VIP nueromorphic chip ](https://www.graimatterlabs.ai/product).

[^1]: File transfer time on the Ethernet took from 10 to 13 ms and on the WIFI 20 ms.

