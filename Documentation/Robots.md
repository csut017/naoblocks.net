# Robots

NaoBlocks.Net treats robots as seperate clients. The only requirement is the client can communicate with the server via HTTP and Web Sockets.

Currently, the only client that has been developed is for a Nao robot.

# Nao Robot Client

This client runs directly on the robot itself. The client is written in Python (v2.7).

## Installing on a PC

This process assumes you have Python 2.7 installed on your machine (yes, it's End-Of-Life but it is the version installed on the robot!)

Install the required dependencies:
```
py -2 -m pip install requests
py -2 -m pip install websocket-client
```

You can then test the client by running:
```
py -2 main.py --test --server localhost:5001 --ignoreSSL
```

This assumes you are running the server from Visual Studio. You can replace the server argument (*--server*) with the hostname and port of any server running NaoBlocks.Net. 

If you are using HTTPS, remove the *--ignoreSSL* argument.

Assuming the server is running, this command will start the client and attempt to connect to the server. Unless you have registered the robot previously, the command will fail, but you should see the following line in the output:

```
[Comms] -> robot registered
```

This indicates the robot has been registered. Now you will need to go to the server and set a **type** and **password** for the robot.

Once the password has been set, you can run the client in test mode using the following command:
```
py -2 main.py --test --server localhost:5001 --ignoreSSL --password password
```

You will need to change the password (*--password*) to the password that you set.

You can then use it to test that your system is correctly working (it will just log any robot functions rather than execute them.)

## Installing on a robot

Installing on a robot is more complicated due to the closed nature of the Nao operating system.

1. On the PC, copy websocket.tar.gz to the robot (you can use scp to do this, or any FTP client)
1. Connect to the robot and execute the following commands:
    ```
    mkdir websocket
    tar -zxvf websocket.tar.gz -C ./websocket
    su
    cd /usr/lib/python2.7/site-packages/
    mkdir websocket
    cp -R -v /home/nao/install/websocket/usr/lib/python2.7/site-packages/websocket ./
    ```
1. Copy all the .py files to a new folder on the robot

_**TODO**: add information about the autoload.ini_

_**TODO**: add information about connect.txt_

## Dependencies

The client depends on the following packages:

* requests ([site](https://pypi.org/project/requests/)) - installed by default on Nao robots
* websocket-client ([site](https://pypi.org/project/websocket_client/))