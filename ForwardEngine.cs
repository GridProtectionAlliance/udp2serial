//******************************************************************************************************
//  ForwardEngine.cs - Gbtc
//
//  Copyright © 2020, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the MIT License (MIT), the "License"; you may not use this
//  file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://opensource.org/licenses/MIT
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  10/10/2020 - J. Ritchie Carroll
//       Generated original version of source code.
//
//******************************************************************************************************

using System;
using System.IO.Ports;
using Gemstone;
using Gemstone.Communication;

namespace udp2serial
{
    public static class ForwardEngine
    {
        private static readonly long s_displayInterval = TimeSpan.FromSeconds(10.0D).Ticks;
        private static Settings s_settings = default!;
        private static SerialPort s_serialPort = default!;
        private static long s_bytesReceived;
        private static long s_lastDisplayTime;

        public static void Start(Settings settings)
        {
            try
            {
                s_settings = settings ?? throw new ArgumentNullException(nameof(settings));

                Console.WriteLine($"Establishing forward to serial port \"{s_settings.COMPort}\" from data received on UDP port {s_settings.UDPPort}");
                Console.WriteLine($"     UDP Interface IP: {s_settings.InterfaceIP}");
                Console.WriteLine($"     Serial Baud Rate: {s_settings.BaudRate}");
                Console.WriteLine($"     Serial Data Bits: {s_settings.DataBits}");
                Console.WriteLine($"        Serial Parity: {s_settings.Parity}");
                Console.WriteLine($"     Serial Stop Bits: {s_settings.StopBits}");
                Console.WriteLine($"    Serial DTR Enable: {s_settings.DtrEnable}");
                Console.WriteLine($"    Serial RTS Enable: {s_settings.RtsEnable}");
                Console.WriteLine($"Press any key to stop...{Environment.NewLine}");

                s_serialPort = ConnectSerialPort();
                using UdpClient udpClient = ConnectUDPClient();

                Console.ReadKey();
                
                DisconnectUDPClient(udpClient);
                DisconnectSerialPort();
            }
            catch (Exception ex)
            {
                if (!(ex is PlatformNotSupportedException))
                    Console.Error.WriteLine($"ERROR: {ex.Message}");
            }
        }

        private static SerialPort ConnectSerialPort()
        {
            SerialPort serialPort = new SerialPort(
                s_settings.COMPort,
                s_settings.BaudRate,
                s_settings.Parity,
                s_settings.DataBits,
                s_settings.StopBits
            )
            {
                DtrEnable = s_settings.DtrEnable,
                RtsEnable = s_settings.RtsEnable
            };

            serialPort.Open();

            return serialPort;
        }

        private static void DisconnectSerialPort()
        {
            s_serialPort.Close();
            s_serialPort.Dispose();
        }

        private static UdpClient ConnectUDPClient()
        {
            UdpClient udpClient = new UdpClient
            { 
                ConnectionString = $"port={s_settings.UDPPort}; interface={s_settings.InterfaceIP}"
            };

            udpClient.ConnectionAttempt += UdpClient_ConnectionAttempt;
            udpClient.ConnectionEstablished += UdpClient_ConnectionEstablished;
            udpClient.ConnectionException += UdpClient_ConnectionException;
            udpClient.ConnectionTerminated += UdpClient_ConnectionTerminated;
            udpClient.ReceiveDataException += UdpClient_ReceiveDataException;
            udpClient.ReceiveDataComplete += UdpClient_ReceiveDataComplete;

            udpClient.ConnectAsync();

            return udpClient;
        }

        private static void DisconnectUDPClient(UdpClient udpClient)
        {
            udpClient.Disconnect();

            udpClient.ReceiveDataComplete -= UdpClient_ReceiveDataComplete;
            udpClient.ReceiveDataException -= UdpClient_ReceiveDataException;
            udpClient.ConnectionTerminated -= UdpClient_ConnectionTerminated;
            udpClient.ConnectionException -= UdpClient_ConnectionException;
            udpClient.ConnectionEstablished -= UdpClient_ConnectionEstablished;
            udpClient.ConnectionAttempt -= UdpClient_ConnectionAttempt;
        }

        private static void UdpClient_ReceiveDataComplete(object? sender, EventArgs<byte[], int> e)
        {
            bool showMessage = s_bytesReceived > 0L && DateTime.UtcNow.Ticks - s_lastDisplayTime > s_displayInterval;
            byte[] bytes = e.Argument1;
            int length = e.Argument2;

            s_serialPort.Write(bytes, 0, length);
            s_bytesReceived += length;

            if (!showMessage)
                return;

            s_lastDisplayTime = DateTime.UtcNow.Ticks;
            Console.WriteLine($"Forwarded {s_bytesReceived:N0} bytes received from UDP port {s_settings.UDPPort} so far...");
        }

        private static void UdpClient_ReceiveDataException(object? sender, EventArgs<Exception> e)
        {
            Console.Error.WriteLine($"UDP receive data exception: {e.Argument.Message}");
        }

        private static void UdpClient_ConnectionTerminated(object? sender, EventArgs e)
        {
            Console.WriteLine($"UDP connection on port {s_settings.UDPPort} terminated.");
        }

        private static void UdpClient_ConnectionException(object? sender, EventArgs<Exception> e)
        {
            Console.Error.WriteLine($"UDP connection on port {s_settings.UDPPort} failed: {e.Argument.Message}");
        }

        private static void UdpClient_ConnectionEstablished(object? sender, EventArgs e)
        {
            Console.Error.WriteLine("UDP connection established.");
        }

        private static void UdpClient_ConnectionAttempt(object? sender, EventArgs e)
        {
            Console.Error.WriteLine($"Attempting UDP connection on port {s_settings.UDPPort}...");
        }
    }
}
