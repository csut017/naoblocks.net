import rospy

def log(message, *args):
    rospy.loginfo(message % args)
