using System;
using System.Globalization;
using System.Xml.Linq;
using System.Xml.XPath;
using Akka.NUnit.Runtime.Messages;
using NUnit.Engine;

namespace Akka.NUnit.Runtime.Reporters
{
	/// <summary>
	/// Processes NUnit runner events.
	/// </summary>
	internal sealed class EventListener : ITestEventListener
	{
		private readonly string _workerName;
		private readonly Action<TestEvent> _handler;

		public EventListener(string workerName, Action<TestEvent> handler)
		{
			if (handler == null) throw new ArgumentNullException("handler");

			_workerName = workerName;
			_handler = handler;
		}

		public void OnTestEvent(string report)
		{
			var e = ParseEvent(XElement.Parse(report));
			_handler(e);
		}

		private TestEvent ParseEvent(XElement e)
		{
			double duration;
			double.TryParse(e.GetAttribute("duration", ""), NumberStyles.Float, CultureInfo.InvariantCulture, out duration);

			var output = e.Element("output").IfNotNull(x => x.Value);
			var reason = e.XPathSelectElement("reason/message").IfNotNull(x => x.Value);
			var error = e.XPathSelectElement("failure/message").IfNotNull(x => x.Value);
			var stackTrace = e.XPathSelectElement("failure/stack-trace").IfNotNull(x => x.Value);

			var name = e.GetAttribute("name", "");

			return new TestEvent
			{
				Worker = _workerName,
				Kind = e.Name.LocalName,
				Id = e.GetAttribute("id", 0),
				Name = name,
				FullName = e.GetAttribute("fullname", (string) null) ?? name,
				ClassName = e.GetAttribute("classname", ""),
				MethodName = e.GetAttribute("methodname", ""),
				Result = e.GetAttribute("result", "").ToUpperInvariant(),
				Asserts = e.GetAttribute("asserts", 0),
				Duration = duration,

				Output = output ?? string.Empty,
				Error = error ?? string.Empty,
				StackTrace = stackTrace ?? string.Empty,
				IgnoreReason = reason
			};
		}
	}
}