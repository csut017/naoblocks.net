cmake_minimum_required(VERSION 2.8.3)
project(naoblocks_client)

find_package(catkin REQUIRED COMPONENTS
  rospy
  std_msgs
)

catkin_package(
)

include_directories(
  ${catkin_INCLUDE_DIRS}
)

catkin_install_python(PROGRAMS 
  src/block_client.py
  src/communications.py
  src/engine.py
  src/logger.py
  src/noRobot.py
  src/robot.py
  src/six.py
  DESTINATION ${CATKIN_PACKAGE_BIN_DESTINATION})

install(DIRECTORY launch/
  DESTINATION ${CATKIN_PACKAGE_SHARE_DESTINATION}/launch
  PATTERN ".svn" EXCLUDE)