using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Akka.NUnit
{
	/// <summary>
	/// Kills child processes on exit of current process.
	/// </summary>
	internal sealed class ProcessList : IDisposable
	{
		// windows job to kill child slave processes
		// BTW it does not work when running master from VisualStudio
		private readonly JobObject _killer = new JobObject();
		private readonly List<Process> _list = new List<Process>();
		private bool _disposed;

		public void Add(Process process)
		{
			if (_disposed) throw new ObjectDisposedException("process-list");
			_killer.AddProcess(process);
			_list.Add(process);
		}

		public void Dispose()
		{
			if (_disposed) return;
			_disposed = true;

			_killer.Dispose();

			foreach (var process in _list.Where(p => !p.HasExited))
			{
				process.Kill();
			}

			_list.Clear();
		}
	}
}