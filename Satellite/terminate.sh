#!/bin/bash  


# ps -def |grep 'satellite_client' |cut -c 9-15 | xargs kill -9
ps -ef | grep 'python satellite_client.py' | grep -v grep | awk '{print $2}' | xargs kill -9
# ps -aux | grep fisco | awk '{print $2}' | xargs kill -9

# ps -ef | grep 'kettle' | grep -v grep