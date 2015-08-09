# UBNT MPower Reader
read the power data from your Ubiquiti Networks MPower device using C#

##What is it

Ubiquiti Networks make a device, called an [MPower][3] which allows you to remotely turn on and off power switches. It also allows you to see what power is being used. Since it runs an embedded copy of Linux, with the help of both [SSH.NET][2] and [this guide from LinITX][1] I build this tool.

##Whats it do?
Simply put, it connects to 1 or more MPower devices via SSH and reads the info from the /proc/power directory. At the moment, it just writes the text to the console, but i am planning on getting more creative.


[1]:https://blog.linitx.com/ubiquiti-mfi-mpower/
[2]:https://sshnet.codeplex.com/
[3]:https://www.ubnt.com/mfi/mpower/
