using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

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
			public List<char> Arguments { get; set; }
		}

		private static Dictionary<String, Command> validCommands = new Dictionary<string, Command>()
		{
			{
				"encrypt", new Command()
				{
					Name = "encrypt",
					Information = "encrypts your text with a string of your choice.",
					MoreInformation =
						"\tencrypt {file path} {arguments} {new file path}\n" +
						"\tNote: the file path should include the name of the file you want to encrypt.\n" +
						"\tNote: if no new file name is given the encrypted text will only be viewed as output.\n",
					Arguments = new List<char>()
					{
					}
				}
			},

			{
				"decrypt", new Command()
				{
					Name = "decrypt",
					Information = "decrypts your text with a string of your choice.",
					MoreInformation =
						"\tdecrypt {file path} {arguments} {new file path}\n" +
						"\tNote: the file path should include the name of the file you want to decrypt.\n" +
						"\tNote: if no new file name is given the decrypted text will only be viewed as output.\n",
					Arguments = new List<char>()
					{
					}
				}
			}
		};

		private static Dictionary<char, Argument> valArguments = new Dictionary<char, Argument>()
		{
			{
				'r', new Argument()
				{
					Name = 'r',
					Information = "only output as text."
				}
			},
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

		private static void Help(string[] arguments)
		{
			if (arguments.Length > 0)
			{
				foreach (var argument in arguments)
				{
					if (!validCommands.ContainsKey(argument))
					{
						Console.WriteLine($"The command '{argument}' doesn't exist. Type 'help' for avaiable commands");
						return;
					}
				}

				foreach (var argument in arguments)
				{
					string commandName = validCommands[argument].Name;
					string commandInfo = validCommands[argument].Information;
					string commandMoreInfo = validCommands[argument].MoreInformation;
					Console.WriteLine($"{commandName}: {commandInfo}\n{commandMoreInfo}");
				}
			}
			else
			{
				foreach (var command in validCommands.Keys)
				{
					string commandName = validCommands[command].Name;
					string commandInfo = validCommands[command].Information;
					Console.WriteLine($"{commandName}: {commandInfo}");
				}
			}
		}

		private static void Encrypt(string[] arguments)
		{
			if (arguments.Length > 0)
			{
				string filePath = arguments[0];
				string encrypredFilePath = arguments[1];

				string text = File.ReadAllText(filePath);

				// Create a new instance of the RijndaelManaged 
				// class.  This generates a new key and initialization  
				// vector (IV). 
				using (RijndaelManaged myRijndael = new RijndaelManaged())
				{
					myRijndael.GenerateKey();
					myRijndael.GenerateIV();

					byte[] key = myRijndael.Key;

					// Encrypt the string to an array of bytes. 
					byte[] encrypted = Crypher.EncryptStringToBytes(text, myRijndael.Key, myRijndael.IV);

					File.WriteAllBytes("key.txt", key);
					File.WriteAllBytes(encrypredFilePath, encrypted);
					Console.WriteLine("Encryption finished. Your key is in 'key.txt'.");
				}

			}
			else
			{
				Console.WriteLine("String:");
				string str = Console.ReadLine();
				using (RijndaelManaged myRijndael = new RijndaelManaged())
				{
					myRijndael.GenerateKey();
					myRijndael.GenerateIV();
					byte[] encrypted = Crypher.EncryptStringToBytes(str, myRijndael.Key, myRijndael.IV);
					byte[] key = myRijndael.Key;

					File.WriteAllBytes("key.txt", key);
					Console.WriteLine(string.Join(" ", encrypted.Select(b => b.ToString()).ToArray()) +
						"Encryption finished. Your key is in 'key.txt'");
				}
			}
		}

		private static void Decrypt(string[] arguments)
		{
			if (arguments.Length > 1)
			{
				Console.WriteLine("Key path:");
				byte[] key = File.ReadAllBytes(Console.ReadLine());

				string filePath = arguments[0];
				string decryptedFilePath = arguments[1];

				using (RijndaelManaged myRijndael = new RijndaelManaged())
				{
					myRijndael.Key = key;
					myRijndael.GenerateIV();

					byte[] encrypted = File.ReadAllBytes(filePath);

					// Decrypt the bytes to a string. 
					string decrypted = Crypher.DecryptStringFromBytes(encrypted, myRijndael.Key, myRijndael.IV);

					if (!File.Exists(decryptedFilePath))
						File.Create(decryptedFilePath);

					File.WriteAllText(decryptedFilePath, string.Join("", decrypted.Select(b => b.ToString()).ToArray()));
				}
			}
			else if (arguments.Length > 0)
			{
				Console.WriteLine("Key path:");
				byte[] key = File.ReadAllBytes(Console.ReadLine());

				string filePath = arguments[0];

				using (RijndaelManaged myRijndael = new RijndaelManaged())
				{
					myRijndael.Key = key;
					myRijndael.GenerateIV();

					byte[] encrypted = File.ReadAllBytes(filePath);

					// Decrypt the bytes to a string. 
					string decrypted = Crypher.DecryptStringFromBytes(encrypted, myRijndael.Key, myRijndael.IV);

					Console.WriteLine(string.Join("", decrypted.Select(b => b.ToString()).ToArray()));
				}
			}
			else
			{
				Console.WriteLine("String: ");
				string str = Console.ReadLine();

				Console.WriteLine("Key path:");
				byte[] key = File.ReadAllBytes(Console.ReadLine());

				string filePath = arguments[0];

				using (RijndaelManaged myRijndael = new RijndaelManaged())
				{
					myRijndael.Key = key;
					myRijndael.GenerateIV();

					byte[] encrypted = File.ReadAllBytes(filePath);

					// Decrypt the bytes to a string. 
					string decrypted = Crypher.DecryptStringFromBytes(encrypted, myRijndael.Key, myRijndael.IV);

					Console.WriteLine(string.Join("", decrypted.Select(b => b.ToString()).ToArray()));
				}
			}
		}

		private static void GetCommand(string input)
		{
			string[] inputTokens = input.Split().Select(c => c.Trim()).ToArray();
			string command = inputTokens[0];
			string[] arguments = inputTokens.Skip(1).ToArray();

			Console.WriteLine();

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
				Console.Write("> ");
				string input = Console.ReadLine();
				if (input == "exit")
					return;

				GetCommand(input);
			}
		}
	}
}
