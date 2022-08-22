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

### Installing the dependencies on a Nao 5 robot

On the Nao 5 we need to manually install the packages as pip is not available.

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

### Installing the dependencies on a Nao 6 robot

On a Nao 6 robot we can use pip to install the dependencies.

If the robot is connected to the Internet, ssh to the robot and type in the following:

```
pip install websocket-client --user
```

If the robot is not connected, you will need to copy the dependencies to the robot first/

On the robot:

```
mkdir dependencies
```

On a PC (assuming you are in the Robots/nao folder):
```
scp dependencies/*.whl nao@ip_address://home/nao/dependencies
```

And back on the robot:
```
pip install websocket-client --no-index --find-link dependencies --user
```

## Automatically starting on the Nao robot
_**TODO**: add information about the autoload.ini_

## Connect.txt

connect.txt contains a list of potential NaoBlocks servers. The file uses the following format:

```
server,password,secure
```

Where
* *server* is the name and port of the server to connect to (the port is only needed if connecting to non-standard ports for http and https.)
* *password* is the password to use for authenticating the client (this must match the password for the robot on the server.)
* *secure* is **yes** to use https or **no** for http.

The following is an example connect.txt for connecting to a NaoBlocks server running using the standard Visual Studio settings:

```
localhost:5001,one,no
```

When the client loads, it will perform the following steps to determine which server to connect to:
1. Is there a server in the command-line arguments:
    * Attempt to connect using the server in the command-line
1. Is there a connect.txt
    * Check each server in connect.txt
    * Start with the first line and check each in order
    * Stop at the first successful connection *(**Note:** the connection may still fail if the password is invalid.)*

## Dependencies

The client depends on the following packages:

* requests ([site](https://pypi.org/project/requests/)) - installed by default on Nao robots
* websocket-client ([site](https://pypi.org/project/websocket_client/))