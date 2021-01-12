[![udp2serial](https://gridprotectionalliance.org/images/products/productTitles75/UDP2Serial.png)](https://gridprotectionalliance.github.io/udp2serial/)

This is a simple console application that takes data received on a UDP/IP socket and forwards it to a serial port.

## Usage
```shell
udp2serial [options] UDPPort [COMPortID]
```

### Options

| Short | Long | Description |
|:-----:| ---- | ----------- |
| &#x2011;i | &#x2011;&#x2011;InterfaceIP | Defines the IP of the network interface to use for the UDP socket. Use 0.0.0.0 to listen on all interfaces with IPv4 or ::0 for all interfaces with IPv6. |
| &#x2011;b | &#x2011;&#x2011;BaudRate | Defines the serial baud rate. Standard values: 110, 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 38400, 57600, 115200, 128000, or 256000. |
| &#x2011;d | &#x2011;&#x2011;DataBits | Defines the standard length of data bits per byte. Standard values: 5, 6, 7 or 8. |
| &#x2011;p | &#x2011;&#x2011;Parity | Defines the parity-checking protocol. Value is one of: Even, Mark, None, Odd or Space. |
| &#x2011;s | &#x2011;&#x2011;StopBits | Defines the standard number of stopbits per byte. Value is one of: None, One, OnePointFive or Two. |
| &#x2011;t | &#x2011;&#x2011;DtrEnable | Defines boolean value, true or false, that enables the Data Terminal Ready (DTR) signal during serial communication. |
| &#x2011;r | &#x2011;&#x2011;RtsEnable | Defines boolean value, true or false, indicating whether the Request to Send (RTS) signal is enabled during serial communication. |
| &#x2011;? | &#x2011;&#x2011;help | Shows usage. |

## Examples
* Forward UDP on 5505 to Windows serial port COM2 at 9600 baud:
```shell
udp2serial -b=9600 5505 COM2
```
* Forward UDP on 8505 to Linux serial port /dev/ttyS2:
```shell
udp2serial 8505 /dev/ttyS2
```
* Forward UDP on port 6704 using IPv6 to first defined serial port:
```shell
udp2serial --InterfaceIP=::0 6704
```

## Default Settings File
Default settings for optional parameters, e.g., serial baud rate and UDP network interface, are configured in a `settings.ini` file. This file will be automatically created when the application is first run.

Common locations for this configuration file are as follows:

|      OS     | Settings File Path                       |
|:-----------:|------------------------------------------|
|   Windows   | `C:\ProgramData\udp2serial\settings.ini` |
| Linux / OSX | `/usr/share/udp2serial/settings.ini`     |

The original default values are initially commented out. Following is an example of the default settings file with a few overrides:

```ini
[Serial]
; Defines the serial baud rate. Standard values: 110, 300, 600, 1200, 2400,
; 4800, 9600, 14400, 19200, 38400, 57600, 115200, 128000, or 256000.
;BaudRate=115200
BaudRate=9600

; Defines the standard length of data bits per byte. Standard values: 5, 6,
; 7 or 8.
;DataBits=8

; Defines the parity-checking protocol. Value is one of: Even, Mark, None, Odd
; or Space.
;Parity=None

; Defines the standard number of stopbits per byte. Value is one of: None, One,
; OnePointFive or Two.
;StopBits=One

; Defines the value that enables the Data Terminal Ready (DTR) signal during
; serial communication.
;DtrEnable=False

; Defines the value indicating whether the Request to Send (RTS) signal is
; enabled during serial communication.
;RtsEnable=False

[UDP]
; Defines the IP of the network interface to use for the UDP socket. Use 0.0.0.0
; to listen on all interfaces with IPv4 or ::0 for all interfaces with IPv6.
;InterfaceIP=0.0.0.0
InterfaceIP=::0
```
![Logo](udp2serial.png)
