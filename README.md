# Vindinium for C#

Original Starter Code by Mark Tanner

This code base consists of a vindinium server emulator and bots. The purpose is to find the most optimal AI to compete against other bots on the actual Vindinium servers.


## Run with

    client <key> <[training|arena]> <number-of-turns> [server-url] [number of games]

To keep sensitive information out of the code base, create a file with the parameters on each line as a text file in c:\vindinium.txt

## Examples:

    client mySecretKey arena 10
    client mySecretKey training 10 http://localhost:9000