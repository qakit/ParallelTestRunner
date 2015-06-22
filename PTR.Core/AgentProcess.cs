using System.Diagnostics;
using Akka.Actor;

namespace PTR.Core
{
	public class AgentProcess
	{
		public Process Process { get; set; }
		public int Port { get; set; }
		public Address Address { get; set; }
	}
}
