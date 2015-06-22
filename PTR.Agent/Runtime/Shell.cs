using System.Collections.Generic;
using System.Linq;
using PTR.Core.Extensions;

namespace PTR.Agent.Runtime
{
	public static class Shell
	{
		public sealed class Command
		{
			public readonly IDictionary<string, string> Options;
			public readonly string[] Input;

			public Command(string[] args)
			{
				Options = args.ParseOptions();
				Input = args.Skip(1).Where(a => !(a.StartsWith("--") || a.StartsWith("/"))).ToArray();
			}
		}
	}
}
