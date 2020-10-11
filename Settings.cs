//******************************************************************************************************
//  Settings.cs - Gbtc
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
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using Gemstone.Configuration.AppSettings;
using Microsoft.Extensions.Configuration;
using static udp2serial.ExitCode;

namespace udp2serial
{
    public enum ExitCode
    {
        Success = 0,
        InvalidCommandLineArgs = 1,
        InvalidUDPPort = 2,
        InvalidCOMPort = 3,
        NoCOMPortsFound = 4,
        HelpDisplay = 255
    }

    public class Settings
    {
        public const string UDPSection = "UDP";
        public const string SerialSection = "Serial";

        public const string DefaultInterfaceIP = "0.0.0.0";
        public const int DefaultBaudRate = 115200;
        public const int DefaultDataBits = 8;
        public const Parity DefaultParity = Parity.None;
        public const StopBits DefaultStopBits = StopBits.One;
        public const bool DefaultDtrEnable = false;
        public const bool DefaultRtsEnable = false;

        // Fixed postion settings from command line
        public ushort UDPPort { get; set; }
        public string COMPort { get; set; } = default!;

        // Optional settings (defaults from config file)
        public string InterfaceIP { get; set; }
        public int BaudRate { get; set; }
        public int DataBits { get; set; }
        public Parity Parity { get; set; }
        public StopBits StopBits { get; set; }
        public bool DtrEnable { get; set; }
        public bool RtsEnable { get; set; }

        public Settings(IConfiguration configuration)
        {
            IConfigurationSection udpSettings = configuration.GetSection(UDPSection);
            InterfaceIP = udpSettings[nameof(InterfaceIP)];

            IConfigurationSection serialSettings = configuration.GetSection(SerialSection);
            BaudRate = int.Parse(serialSettings[nameof(BaudRate)]);
            DataBits = int.Parse(serialSettings[nameof(DataBits)]);
            Parity = Enum.Parse<Parity>(serialSettings[nameof(Parity)]);
            StopBits = Enum.Parse<StopBits>(serialSettings[nameof(StopBits)]);
            DtrEnable = bool.Parse(serialSettings[nameof(DtrEnable)]);
            RtsEnable = bool.Parse(serialSettings[nameof(RtsEnable)]);
        }

        public static void ConfigureAppSettings(IAppSettingsBuilder builder)
        {
            // UDP configuration settings
            builder.Add($"{UDPSection}:{nameof(InterfaceIP)}", DefaultInterfaceIP, "Defines the IP of the network interface to use for the UDP socket. Use 0.0.0.0 to listen on all interfaces with IPv4 or ::0 for all interfaces with IPv6.");
            
            // Serial configuration settings
            builder.Add($"{SerialSection}:{nameof(BaudRate)}", DefaultBaudRate.ToString(), "Defines the serial baud rate. Standard values: 110, 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 38400, 57600, 115200, 128000, or 256000.");
            builder.Add($"{SerialSection}:{nameof(DataBits)}", DefaultDataBits.ToString(), "Defines the standard length of data bits per byte. Standard values: 5, 6, 7 or 8.");
            builder.Add($"{SerialSection}:{nameof(Parity)}", DefaultParity.ToString(), "Defines the parity-checking protocol. Value is one of: Even, Mark, None, Odd or Space.");
            builder.Add($"{SerialSection}:{nameof(StopBits)}", DefaultStopBits.ToString(), "Defines the standard number of stopbits per byte. Value is one of: None, One, OnePointFive or Two.");
            builder.Add($"{SerialSection}:{nameof(DtrEnable)}", DefaultDtrEnable.ToString(), "Defines the value that enables the Data Terminal Ready (DTR) signal during serial communication.");
            builder.Add($"{SerialSection}:{nameof(RtsEnable)}", DefaultRtsEnable.ToString(), "Defines the value indicating whether the Request to Send (RTS) signal is enabled during serial communication.");
        }

        public static Dictionary<string, string> SwitchMappings => new Dictionary<string, string>
        {
            [$"--{nameof(InterfaceIP)}"] = $"{UDPSection}:{nameof(InterfaceIP)}",
            [$"--{nameof(BaudRate)}"] = $"{SerialSection}:{nameof(BaudRate)}",
            [$"--{nameof(DataBits)}"] = $"{SerialSection}:{nameof(DataBits)}",
            [$"--{nameof(Parity)}"] = $"{SerialSection}:{nameof(Parity)}",
            [$"--{nameof(StopBits)}"] = $"{SerialSection}:{nameof(StopBits)}",
            [$"--{nameof(DtrEnable)}"] = $"{SerialSection}:{nameof(DtrEnable)}",
            [$"--{nameof(RtsEnable)}"] = $"{SerialSection}:{nameof(RtsEnable)}",
            ["-i"] = $"{UDPSection}:{nameof(InterfaceIP)}",
            ["-b"] = $"{SerialSection}:{nameof(BaudRate)}",
            ["-d"] = $"{SerialSection}:{nameof(DataBits)}",
            ["-p"] = $"{SerialSection}:{nameof(Parity)}",
            ["-s"] = $"{SerialSection}:{nameof(StopBits)}",
            ["-d"] = $"{SerialSection}:{nameof(DtrEnable)}",
            ["-r"] = $"{SerialSection}:{nameof(RtsEnable)}"
        };

        public ExitCode Parse(string[] args)
        {
            HashSet<string> optionArgs = args.Where(arg => arg.StartsWith("--") || arg.StartsWith('/')).ToHashSet(StringComparer.OrdinalIgnoreCase);
            args = args.Where(arg => !optionArgs.Contains(arg)).ToArray();

            if (args.Length < 1 || args.Length > 2)
            {
                HandleError($"Expected 1 or 2 arguments, received {args.Length:N0}.");
                return InvalidCommandLineArgs;
            }

            if (optionArgs.Contains("--help") || optionArgs.Contains("-?") || optionArgs.Contains("/?"))
            {
                ShowHelp();
                return HelpDisplay;
            }

            if (!ushort.TryParse(args[0], out ushort udpPort) || udpPort == 0)
            {
                HandleError($"Bad UDP port \"{args[0]}\".");
                return InvalidUDPPort;
            }

            UDPPort = udpPort;

            string[] portNames = SerialPort.GetPortNames();

            if (args.Length > 1)
            {
                if (!portNames.Contains(args[1]))
                {
                    StringBuilder localPorts = new StringBuilder();

                    localPorts.Append($"{Environment.NewLine}{Environment.NewLine}");

                    if (portNames.Length == 0)
                    {
                        localPorts.Append("No local COM ports found.");
                    }
                    else
                    {
                        localPorts.AppendLine("Available COM ports:");
                        localPorts.Append(string.Join(Environment.NewLine, portNames.Select(portName => $"    {portName}")));
                    }

                    HandleError($"Serial port \"{args[1]}\" not found.{localPorts}");
                    return InvalidCOMPort;
                }

                COMPort = args[1];
            }
            else
            {
                if (portNames.Length == 0)
                {
                    HandleError("No local COM ports found.");
                    return NoCOMPortsFound;
                }

                COMPort = portNames.FirstOrDefault() ?? "";
            }

            return Success;
        }

        private static void HandleError(string errorMessage)
        {
            Console.Error.WriteLine($"ERROR: {errorMessage}{Environment.NewLine}");
            ShowHelp();
        }

        private static void ShowHelp()
        {
            Console.WriteLine("USAGE:");
            Console.WriteLine($"    {nameof(udp2serial)} [options] UDPPort [COMPortID]");
            Console.WriteLine();
            Console.WriteLine("OPTIONS:");
            Console.WriteLine($"  -i, --{nameof(InterfaceIP)}  Defines the IP of the network interface to use for UDP socket, e.g.: 0.0.0.0 or ::0");
            Console.WriteLine($"  -b, --{nameof(BaudRate)}     Defines the serial baud rate, e.g.: 4800, 9600, 14400, 19200, 38400, 57600 or 115200");
            Console.WriteLine($"  -d, --{nameof(DataBits)}     Defines the standard length of data bits per byte, e.g.:  5, 6, 7 or 8");
            Console.WriteLine($"  -p, --{nameof(Parity)}       Defines the parity-checking protocol, one of: Even, Mark, None, Odd or Space");
            Console.WriteLine($"  -s, --{nameof(StopBits)}     Defines the standard number of stopbits per byte, one of: None, One, OnePointFive or Two");
            Console.WriteLine($"  -d, --{nameof(DtrEnable)}    Defines boolean value that enables Data Terminal Ready signal, either: true or false");
            Console.WriteLine($"  -r, --{nameof(RtsEnable)}    Defines boolean value that enables Request to Send signal, either: true or false");
            Console.WriteLine("  -?, --help         Shows usage");
            Console.WriteLine();
            Console.WriteLine("EXAMPLES:");
            Console.WriteLine("  > Forward UDP on 5505 to Windows serial port COM2 at 9600 baud:");
            Console.WriteLine($"       {nameof(udp2serial)} -b=9600 5505 COM2{Environment.NewLine}");
            Console.WriteLine("  > Forward UDP on 8505 to Linux serial port /dev/ttyS2:");
            Console.WriteLine($"       {nameof(udp2serial)} 8505 /dev/ttyS2{Environment.NewLine}");
            Console.WriteLine("  > Forward UDP on port 6704 using IPv6 to first defined serial port:");
            Console.WriteLine($"       {nameof(udp2serial)} --InterfaceIP=::0 6704");
        }
    }
}