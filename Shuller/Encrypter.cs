﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Shuller
{
    public abstract class Encrypter
    {
        internal class Argument
        {
            public char Name { get; set; }
            public string Information { get; set; }
        }

        internal class Command
        {
            public string Name { get; set; }
            public string Information { get; set; }
            public string MoreInformation { get; set; }
        }

        private static readonly Dictionary<String, Command> ValidCommands = new Dictionary<string, Command>()
        {
            {
                "help", new Command()
                {
                    Name = "help",
                    Information = "shows avaiable commands or details about specified command.",
                    MoreInformation =
                        "syntax: help {commands}\n",
                }
            },

            {
                "encrypt", new Command()
                {
                    Name = "encrypt",
                    Information = "encrypts your text with a string of your choice.",
                    MoreInformation =
                        "syntax: encrypt {file path} {new file path}\n" +
                        "Note: the file path should include the name of the file you want to encrypt.\n" +
                        "Note: if no new file name is given the encrypted text will only be viewed as output.\n",
                }
            },

            {
                "decrypt", new Command()
                {
                    Name = "decrypt",
                    Information = "decrypts your text with a string of your choice.",
                    MoreInformation =
                        "syntax: decrypt {file path} {new file path}\n" +
                        "Note: the file path should include the name of the file you want to decrypt.\n" +
                        "Note: if no new file name is given the decrypted text will only be viewed as output.\n",
                }
            },

            {
                "cls", new Command()
                {
                    Name = "cls",
                    Information = "clears screen",
                    MoreInformation =
                        "syntax: cls\n",
                }
            }
        };

        private static void ShowLogo()
        {
            Console.WriteLine(
                "   _____ _           _ _\n" +
                "  / ____| |         | | |\n" +
                " | (___ | |__  _   _| | | ___ _ __\n" +
                "  \\___ \\| \'_ \\| | | | | |/ _ \\ \'__|\n" +
                "  ____) | | | | |_| | | |  __/ |\n" +
                " |_____/|_| |_|\\__,_|_|_|\\___|_|\n");
        }

        private static string EncryptionReadLine()
        {
            Console.Write("> ");
            return Console.ReadLine();
        }

        private static bool CheckCommand(string command)
        {
            if (!ValidCommands.ContainsKey(command))
            {
                Console.WriteLine($"The command '{command}' doesn't exist. Type 'help' for avaiable commands\n");
                return false;
            }
            return true;
        }

        private static void Help(string[] arguments)
        {
            if (arguments.Length > 0)
            {
                foreach (var argument in arguments)
                {
                    if (!CheckCommand(argument))
                        return;
                }

                foreach (var argument in arguments)
                {
                    string commandName = ValidCommands[argument].Name;
                    string commandInfo = ValidCommands[argument].Information;
                    string commandMoreInfo = ValidCommands[argument].MoreInformation;
                    Console.WriteLine($"{commandName}: {commandInfo}\n{commandMoreInfo}");
                }
            }
            else
            {
                foreach (var command in ValidCommands.Keys)
                {
                    string commandName = ValidCommands[command].Name;
                    string commandInfo = ValidCommands[command].Information;
                    Console.WriteLine($"{commandName}: {commandInfo}");
                }
            }
        }

        private static void Encrypt(string[] arguments)
        {

            /*
			 * Create a new instance of the RijndaelManaged 
			 * class.  This generates a new key and initialization  
			 * vector (IV). 
			 */
            using (RijndaelManaged myRijndael = new RijndaelManaged())
            {
                Console.WriteLine("Input key path. (Press 'Enter' to autogenerate)");
                string keyPath = EncryptionReadLine();
                string IVPath = string.Empty;

                if (keyPath == String.Empty)
                {
                    myRijndael.GenerateKey();
                    myRijndael.GenerateIV();
                }
                else
                {
                    myRijndael.Key = File.ReadAllBytes(keyPath);

                    Console.WriteLine("Input IV path. (Press 'Enter' to autogenerate)");
                    IVPath = EncryptionReadLine();

                    if (IVPath == String.Empty)
                        myRijndael.GenerateIV();
                    else
                        myRijndael.IV = File.ReadAllBytes(IVPath);
                }

                // Example: encrypt {filePath} {encryptedFilePath} 
                // Reads from filePath and writes encrypted output to encryptedFilePath.
                if (arguments.Length > 1)
                {
                    string filePath = arguments[0];
                    string encrypredFilePath = arguments[1];
                    string text = File.ReadAllText(filePath);

                    // Encrypt the string to an array of bytes. 
                    byte[] encrypted = Crypher.EncryptStringToBytes(text, myRijndael.Key, myRijndael.IV);

                    // Create file with the encrypted output.
                    File.WriteAllBytes(encrypredFilePath, encrypted);
                }
                // Example: encrypt
                // Simply prints encrypted input to console.
                else
                {
                    Console.WriteLine("Text to encrypt: ");
                    string text = EncryptionReadLine();

                    byte[] encrypted = Crypher.EncryptStringToBytes(text, myRijndael.Key, myRijndael.IV);

                    Console.WriteLine(Convert.ToBase64String(encrypted));
                }

                Console.WriteLine("\nEncryption finished.");

                // Send key to separate text file.
                if (keyPath == String.Empty)
                {
                    File.WriteAllBytes("key.txt", myRijndael.Key);
                    File.WriteAllBytes("IV.txt", myRijndael.IV);
                    Console.WriteLine("Your key is in 'key.txt'.\nYour IV is in 'IV.txt'.");
                }
                else if (IVPath == String.Empty)
                {
                    File.WriteAllBytes("IV.txt", myRijndael.IV);
                    Console.WriteLine("Your IV is in 'IV.txt'.");
                }
            }
        }

        private static void Decrypt(string[] arguments)
        {
            using (RijndaelManaged myRijndael = new RijndaelManaged())
            {
                Console.WriteLine("Key path:");
                byte[] key = File.ReadAllBytes(EncryptionReadLine());
                Console.WriteLine("IV path ('Enter' for none):");
                byte[] IV = File.ReadAllBytes(EncryptionReadLine());

                Console.WriteLine("\nDecryption finished.");

                // Use key from key path
                myRijndael.Key = key;
                if (IV.Length > 0)
                    myRijndael.IV = IV;
                else
                    myRijndael.GenerateIV();

                string filePath = arguments[0];

                // Get the decrypted byte array from filePath
                byte[] encrypted = File.ReadAllBytes(filePath);

                // Decrypt the bytes to a string. 
                string decrypted = Crypher.DecryptStringFromBytes(encrypted, myRijndael.Key, myRijndael.IV);

                if (arguments.Length > 1)
                {
                    string decryptedFilePath = arguments[1];
                    Console.WriteLine($"Decrypted text is in: '{decryptedFilePath}'");
                    File.WriteAllText(decryptedFilePath, decrypted);
                }
                else
                {
                    Console.WriteLine("Decrypted text is:\n\n" + decrypted);
                }
            }
        }

        private static void GetCommand(string input)
        {
            string[] inputTokens = input.Split().Select(c => c.Trim()).ToArray();
            string command = inputTokens[0];
            string[] arguments = inputTokens.Skip(1).ToArray();

            Console.WriteLine();

            if (!CheckCommand(inputTokens[0]))
                return;

            switch (command)
            {
                case "help":
                    Help(arguments);
                    break;
                case "encrypt":
                    Encrypt(arguments);
                    break;
                case "decrypt":
                    Decrypt(arguments);
                    break;
                case "cls":
                    Console.Clear();
                    ShowLogo();
                    break;
            }

            Console.WriteLine();
        }

        public static void StartEncrypter()
        {
            ShowLogo();
            while (true)
            {
                string input = EncryptionReadLine();
                if (input == "exit")
                    return;

                GetCommand(input);
            }
        }
    }
}
