#! /usr/bin/env python

import rospy
from geometry_msgs.msg import Twist

class WheelControllerNode():
    move_cmd = Twist()
    
    def __init__(self):
        rospy.init_node('WheelControllerNode', anonymous=False)
       
        rospy.Subscriber("/naoblocks/wheel", Twist, self.RobotMovedCallback)

        rospy.loginfo("[Wheel] Starting WheelControllerNode")
        rospy.on_shutdown(self.shutdown)
        self.cmd_vel=rospy.Publisher('/cmd_vel_mux/input/teleop',Twist,queue_size=10)
        r = rospy.Rate(10);
        while not rospy.is_shutdown():
            self.cmd_vel.publish(WheelControllerNode.move_cmd)
            r.sleep()

    def RobotMovedCallback(self, data):
        rospy.loginfo("[Wheel] Changing speed to %f forward, %f turning", data.linear.x, data.angular.z)
        WheelControllerNode.move_cmd.linear.x = data.linear.x
        WheelControllerNode.move_cmd.angular.z = data.angular.z

    def shutdown(self):
        rospy.loginfo("[Wheel] Stopping WheelControllerNode")
        self.cmd_vel.publish(Twist())
        rospy.sleep(1)

if __name__ == '__main__':
    try:
        WheelControllerNode()
    except rospy.ROSInterruptException:
        rospy.logfatal("[Wheel] WheelControllerNode node terminated.")