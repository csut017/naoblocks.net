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

If the robot is not connected, you will need to copy the dependencies to the robot first.

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

## Command-line Arguments
The client has the following arguments:

* `--server <hostname>` the hostname of the NaoBlocks.Net server. This can be an IP address (or IP address and port combination) or a name that is resolved via DNS. Do not include the transport scheme (e.g. http:// or https://). If the server is not set, the client will use connect.txt to try to find a server.
* `--port <port>` a specific port to use when connecting to the server. Not really needed, but it does allow the seperation of hostname and port if desired.
* `--name <name>` the name of the robot. If this argument is omitted, the client will use the default machine name.
* `--password <password>` the password to use when connecting to the server.
* `--pip <IP address>` the Nao robot's IP address. This needs to be set if the robot is being remote controlled from a PC. If running on the robot, it will default to the local loopback address (127.0.0.1).
* `--pport <port>` the port to use for the Nao robot. Again, only needed if remote controlling a robot.
* `--test` changes to test mode. This mode will bypass sending commands to the robot: instead it will log them in the console. Mainly used for testing the client code on a PC.
* `--ignoreSSL` will cause the client to ignore any SSL errors. This mode is typically used when we don't have a valid, trusted SSL certificate. This mode is a **HUGE** security hole, so only use it you know what you are doing.
* `--reconnect <number>` is the number of reconnect attempts to make if the connection to the server is lost. If the number of connection attempts exceeds this number, the client will give up and exit. The default is 25.

## Automatically starting and updating the Nao client

The Nao client contains an automatic uploader. Rather than using *main.py*, you can start the client using *auto.py*:

```
python auto.py
```

This command will start the Nao client, attempt to automatically download the application files, then start the client using connect.txt (see below).

**Note:** The automatic download uses the same command-line arguments as *main.py*. If you know the server name and port, you can use the server and password parameters. It has the following extra arguments:
* `--updateOnly` will only attempt to update the client code. When the code has been updated it will exit without running the client.

To automatically start the client when the robot starts you need to update autoload.ini. 

The file must be saved to `/home/nao/naoqi/preferences`. The file will typically contain something like the following:

```python
[user]
# Load a user library - needs to use full path

[python]
# Load a python script - needs to use full path
/home/nao/naoblocks/auto.py

[program]
# Start a program - needs to use full path
```

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