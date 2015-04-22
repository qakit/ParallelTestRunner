using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Akka.NUnit.Runtime.Messages;

namespace Akka.NUnit.Runtime.Reporters
{
	internal interface IReporter
	{
		void Report(TestEvent e);
	}

	internal sealed class NullReporter : IReporter
	{
		public static readonly IReporter Instance = new NullReporter();
		private NullReporter (){}
		public void Report(TestEvent e){}
	}

	internal sealed class TextReporter : MarshalByRefObject, IReporter
	{
		private readonly TextWriter _writer;

		public TextReporter(TextWriter writer = null)
		{
			_writer = writer ?? Console.Out;
		}

		public void Report(TestEvent e)
		{
			switch (e.Kind)
			{
				case EventKind.RunStarted:
					_writer.WriteLine("test run '{0}' with {1} tests started ", e.FullName, e.TestCount);
					break;
				case EventKind.RunFinished:
					_writer.WriteLine("test run '{0}' finished in {1} seconds", e.FullName, e.Duration);
					break;
				case EventKind.TestStarted:
					_writer.WriteLine("test '{0}' started ", e.FullName);
					break;
				case EventKind.TestFinishied:
					_writer.WriteLine("test '{0}' finished in {1} seconds", e.FullName, e.Duration);
					break;
				case EventKind.SuiteStarted:
					_writer.WriteLine("test suite '{0}' started ", e.FullName);
					break;
				case EventKind.SuiteFinished:
					_writer.WriteLine("test suite '{0}' finished in {1} seconds", e.FullName, e.Duration);
					break;
			}
		}
	}

	internal sealed class CompositeReporter : IReporter
	{
		private readonly List<IReporter> _list;

		public CompositeReporter(IEnumerable<IReporter> reporters)
		{
			_list = reporters.ToList();
		}

		public void Report(TestEvent e)
		{
			_list.ForEach(r => r.Report(e));
		}
	}
}
