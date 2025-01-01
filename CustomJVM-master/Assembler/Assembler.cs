using System;
using System.Collections.Generic;
using System.IO;

namespace _7._24.ComputerArchitecture.Assembler
{
    class Program
    {
        static Dictionary<string, byte> OpCodes = new Dictionary<string, byte>() 
        {
            ["SET"] = 0x40,
            ["ADD"] = 0x10,
            ["SUB"] = 0x11,
            ["MUL"] = 0x12,
            ["DIV"] = 0x13,
            ["MOD"] = 0x14,
            ["JMP"] = 0x30,
            ["JMPT"] = 0x32,
            ["NOT"] = 0x20, 
            ["AND"] = 0x21, 
            ["OR"] = 0x22, 
            ["XOR"] = 0x23, 
            ["EQ"] = 0x24, 
            ["GT"] = 0x28, 
            ["LT"] = 0x29, 
            ["WRT"] = 0x50,
            ["WRN"] = 0x51,
            ["RD"] = 0x60,
            ["COP"] = 0x70,
        };
        static Dictionary<string, int> labels = new Dictionary<string, int>()
        {   
        
        };
        static Dictionary<string, byte> Registers = new Dictionary<string, byte>()
        { 
            ["R0"] = 00,
            ["R1"] = 01,  
            ["R2"] = 02,
            ["R3"] = 03,
            ["R4"] = 04,
            ["R5"] = 05,
            ["R6"] = 06,
            ["R7"] = 07,
            ["R8"] = 08,
            ["R9"] = 09,
            ["R10"] = 10,
            ["R11"] = 11,
            ["R12"] = 12,
            ["R13"] = 13,
            ["R14"] = 14,
            ["R15"] = 15,
            ["R16"] = 16,
            ["R17"] = 17,
            ["R18"] = 18,
            ["R19"] = 19,
            ["R20"] = 20,
            ["R21"] = 21,
            ["R22"] = 22,
            ["R23"] = 23,
            ["R24"] = 24,
            ["R25"] = 25,
            ["R26"] = 26,
            ["R27"] = 27,
            ["R28"] = 28,
            ["R29"] = 29,
            ["R30"] = 30,
            ["R31"] = 31,
            
        };

        static void Main(string[] args)
        {
            //string[] lines = File.ReadAllLines(args[0]);
            
            var bigFile = File.ReadAllText(args[0]);
            string[] lines = bigFile.Split(new char[]{'\r','\n','\t'}, StringSplitOptions.RemoveEmptyEntries);
            List<byte> assembledBytes = new List<byte>();
            int lineCounter = 0;
         
            foreach (var line in lines)
            {
                lineCounter++;
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if(parts.Length == 0)
                {
                    lineCounter--;
                }
                else if (!OpCodes.ContainsKey(parts[0]) && !Registers.ContainsKey(parts[0])) 
                {
                    lineCounter--;
                    parts[0] = parts[0].TrimEnd(':'); 
                    labels.Add(parts[0], lineCounter); 
                }
            }
            foreach (var line in lines)
            {
                var bytes = GetAssembly(line);
                if (bytes.Length == 0)
                {
                    continue; 
                }
                for (int i = 0; i < bytes.Length; i++)
                {
                    if (labels.ContainsKey(bytes[i].ToString()))
                    {
                        bytes[i] = (byte)labels[bytes[i].ToString()] ; 
                    }
                }
                assembledBytes.AddRange(bytes); 
            }
            System.IO.File.WriteAllBytes(@"C:\Users\rohan\OneDrive\Desktop\BinaryCopy.txt", assembledBytes.ToArray()); 
            PrintMachine(assembledBytes.ToArray());
        }
        private static void PrintMachine(byte[] bytes)
        {
            ushort address = 0x00;
            ushort line = 0x10;
            for (int i = 0; i < bytes.Length; i++)
            {
                if (i % line == 0)
                {
                    Console.Write($"\n{ address:X4}");
                    address += line;
                }
                Console.Write($"{bytes[i]:X4}");
            }
        }

        static byte[] GetAssembly(string line)
        {
            byte OpCode = 0; 
            var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            parts[0] = parts[0].Trim(':');
            if (OpCodes.ContainsKey(parts[0]))
            {

                OpCode = OpCodes[parts[0]];

                switch (OpCode)
                {
                    case 0x40:
                        var value = ushort.Parse(parts[2], System.Globalization.NumberStyles.HexNumber);
                        byte valueLB = (byte)value;
                        byte valueHB = (byte)(value >> 8);
                        return new byte[] { OpCode, Registers[parts[1]], valueHB, valueLB };
                        break;

                    case 0x30:
                        byte val = (byte)labels[parts[1]];
                        byte AddrHB = (byte)(val >> 8);
                        byte AddrLB = (byte)val;
                        return new byte[] { OpCode, AddrHB, AddrLB, 0xFF };
                        break;
                    case 0x32:
                        byte val1 = (byte)labels[parts[2]];
                        byte AddrHB2 = (byte)(val1 >> 8);
                        byte AddrLB2 = (byte)val1;

                        return new byte[] { OpCode, Registers[parts[1]] ,AddrHB2, AddrLB2};
                    case 0x24:
                    case 0x28:
                    case 0x29: 
                    case 0x10:
                    case 0x11: 
                    case 0x12:
                    case 0x13:
                    case 0x14:
                        return new byte[] { OpCode, Registers[parts[1]], Registers[parts[2]], Registers[parts[3]] };
                 
                    case 0x70:
                        return new byte[] { OpCode, Registers[parts[1]], Registers[parts[2]], byte.MaxValue }; 
                    default:
                        throw new Exception("Invalid Command");
                        break;
                }
            }
            return new byte[0]; 
        }
            
        
    }
}
