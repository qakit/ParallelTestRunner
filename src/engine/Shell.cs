using System;
using System.Collections.Generic;
using System.Linq;

namespace Akka.NUnit.Runtime
{
	/// <summary>
	/// Command line "read, eval, print, loop".
	/// </summary>
	public static class Shell
	{
		public sealed class Command
		{
			public readonly string Name;
			public readonly IDictionary<string, string> Options;
			public readonly string[] Input;

			public Command(string[] args)
			{
				Name = (args[0] ?? string.Empty).ToLowerInvariant();
				Options = args.Skip(1).ParseOptions();
				Input = args.Skip(1).Where(a => !(a.StartsWith("--") || a.StartsWith("/"))).ToArray();
			}
		}

		public static void Run(Func<Command, bool> eval)
		{
			while (true)
			{
				Console.Write(">>> ");

				var line = (Console.ReadLine() ?? string.Empty).Trim();
				if (line.Length == 0) continue;

				var args = Scan(line).Where(s => !string.IsNullOrEmpty(s)).ToArray();
				if (args.Length == 0) continue;

				try
				{
					if (!eval(new Command(args))) break;
				}
				catch (Exception e)
				{
					Console.WriteLine("error: {0}", e.Message);
				}
			}
		}

		private static string[] Scan(string line)
		{
			return ScanImpl(line).Where(token => !string.IsNullOrEmpty(token)).ToArray();
		}

		// TODO reduce ScanImpl

		private static IEnumerable<string> ScanImpl(string line)
		{
			if (string.IsNullOrWhiteSpace(line))
				yield break;

			string s = "";
			for (int i = 0; i < line.Length; i++)
			{
				var c = line[i];
				if (c == '\"')
				{
					s += c;
					for (i++; i < line.Length; i++)
					{
						if (line[i] == '\"')
						{
							s += line[i];
							break;
						}
						s += line[i];
					}
				}
				else if (c == ' ')
				{
					if (s.Length > 0)
					{
						yield return s;
						s = "";
					}
				}
				else
				{
					s += c;
				}
			}

			if (s.Length > 0)
			{
				yield return s;
			}
		}
	}
}
