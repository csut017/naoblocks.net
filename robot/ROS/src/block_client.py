#! /usr/bin/env python

import rospy
import logger
from communications import Communications

class BlockClientNode():
    comms = None
    
    def __init__(self):
        rospy.init_node('BlockClientNode', anonymous=False)

        reconnect_count = int(rospy.get_param('/block_client/reconnect_count', '10'))
        use_ssl = rospy.get_param('/block_client/use_ssl', True)
        verify_ssl = rospy.get_param('/block_client/verify_ssl', True)
        server_address = rospy.get_param('/block_client/server_address', None)
        server_pwd = rospy.get_param('/block_client/server_pwd', None)
        use_robot_mock = rospy.get_param('/block_client/robot_mock', False)

        logger.log("[Main] Starting BlockClientNode")
        rospy.on_shutdown(self.shutdown)
        logger.log("[Main] Use SSL:         %s", ("YES" if use_ssl else "NO"))
        logger.log("[Main] Verify SSL:      %s", ("YES" if verify_ssl else "NO"))
        logger.log("[Main] Reconnect Count: %d", reconnect_count)
        logger.log("[Main] Use Robot Mock:  %s", ("YES" if use_robot_mock else "NO"))

        BlockClientNode.comms = Communications(not use_robot_mock, reconnect_count)
        if not verify_ssl:
            logger.log('[Main] WARNING: ignoring SSL errors')

        connected = False
        if not server_address is None:
            logger.log('[Main] Connecting to %s', server_address)
            BlockClientNode.comms.start(server_address, server_pwd, verify_ssl, use_ssl)
            connected = True
        else:
            logger.log('[Main] Server address has not been set')

        if not connected:
            logger.log('[Main] Unable to connect')

    def shutdown(self):
        logger.log("[Main] Stopping BlockClientNode")
        if not BlockClientNode.comms is None:
            BlockClientNode.comms.close()
        rospy.sleep(1)

if __name__ == '__main__':
    try:
        BlockClientNode()
    except rospy.ROSInterruptException:
        rospy.logfatal("BlockClientNode node terminated.")