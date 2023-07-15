#!/bin/bash  

for i in $(seq 0 11)  
do
    python satellite_client.py $i &
done