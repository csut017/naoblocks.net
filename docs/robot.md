# Robots

NaoBlocks.Net treats robots as seperate clients. The only requirement is the client can communicate with the server via HTTP and Web Sockets.

Currently, the only client that has been developed is for a Nao robot. 

## Nao Robot Client

This client runs directly on the robot itself. The client is written in Python (v2.7).

### Client Setup and Installation

 1. [PC ] Copy websocket.tar.gz to robot
 2. [Nao] > mkdir websocket
 4. [Nao] > tar -zxvf websocket.tar.gz -C ./websocket
 5. [Nao] > su
 6. [Nao] > cd /usr/lib/python2.7/site-packages/
 7. [Nao] > mkdir websocket
 8. [Nao] > cp -R -v /home/nao/install/websocket/usr/lib/python2.7/site-packages/websocket ./
 9. [PC ] Delete install folder and all files
10. [PC ] Make naoblocks folder
11. [PC ] Copy all source files to folder

### Dependencies

The client depends on the following packages:

* requests ([site](https://pypi.org/project/requests/)) - installed by default on Nao robots
* websocket-client ([site](https://pypi.org/project/websocket_client/))
