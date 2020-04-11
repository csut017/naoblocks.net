# ROS Client

The ROS client is currently under development. It is currently a plain port of the Nao client. As such it does not use the standard ROS functionality.

## Installing

The client needs to be installed onto the robot to run.

*Setup the environment*
On the robot:
1. mkdir -p ~/naoblocks/src
1. cd ~/naoblocks
1. catkin_make
1. cd src
1. catkin_create_pkg naoblocks_client std_msgs rospy
1. mkdir -p ~/naoblocks/src/naoblocks_client/launch

*Copy over the files*
On the source machine:
1. cd robot\ROS
1. scp CMakeLists.txt robot@192.168.0.14://home/robot/naoblocks/src/naoblocks_client/
1. scp launch/*.launch robot@192.168.0.14://home/robot/naoblocks/src/naoblocks_client/launch/
1. scp src/*.py robot@192.168.0.14://home/robot/naoblocks/src/naoblocks_client/src/

*Configure and everything*
On the robot:
1. cd ~/naoblocks
1. chmod +x ~/naoblocks/src/naoblocks_client/src/block_client.py
1. dos2unix ~/naoblocks/src/naoblocks_client/src/*.py
1. dos2unix ~/naoblocks/src/naoblocks_client/launch/*.launch
1. catkin_make

*Optional: Build the install package*
1. cd ~/naoblocks
1. catkin_make

## Dependencies

The client depends on the following packages:

* requests ([site](https://pypi.org/project/requests/))
* websocket-client ([site](https://pypi.org/project/websocket_client/))
