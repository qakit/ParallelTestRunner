using System;
using System.Xml.Linq;
using Akka.Actor;
using NUnit.Engine;

namespace Akka.NUnit.Runtime
{
	/// <summary>
	/// Posts NUnit runner events to manager.
	/// </summary>
	internal sealed class RemoteReporter : ITestEventListener
	{
		private readonly IActorRef _worker;
		private readonly IActorRef _manager;
		private readonly string _agentName;

		public RemoteReporter(IActorRef worker, IActorRef manager, string agentName)
		{
			if (manager == null) throw new ArgumentNullException("manager");

			_worker = worker;
			_manager = manager;
			_agentName = agentName;
		}

		public void OnTestEvent(string report)
		{
			var doc = XDocument.Parse(report);
			var result = doc.Root;
			switch (result.Name.LocalName)
			{
				case "start-test":
					break;

				case "test-case":
					TestFinished(result);
					break;

				case "start-suite":
					break;

				case "test-suite":
					TestSuiteFinished(result);
					break;

				case "start-run":
					break;
			}

		}

		private void TestFinished(XElement result)
		{
			var testName = result.GetAttribute("fullname");
			var outputNode = result.Element("output");
			var output = outputNode != null ? outputNode.Value : string.Empty;
			_manager.Tell(new TestReport(_agentName, testName, false, output), _worker);
		}

		private void TestSuiteFinished(XElement result)
		{
			var testName = result.GetAttribute("fullname");
			_manager.Tell(new SuiteReport(_agentName, testName, 0, 0), _worker);
		}
	}
}