﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Akka.Actor;
using Akka.NUnit.Runtime.Messages;
using NUnit.Engine;
using TestRun = Akka.NUnit.Runtime.Messages.TestRun;

namespace Akka.NUnit.Runtime
{
	public sealed class Manager : ReceiveActor
	{
		private readonly HashSet<IActorRef> _workers = new HashSet<IActorRef>();
		private readonly ConcurrentQueue<Job> _jobQueue = new ConcurrentQueue<Job>();
		private readonly List<RunningJob> _runningJobs = new List<RunningJob>(); 

		public Manager()
		{
			Receive<RegisterWorker>(newWorker =>
			{
				Console.WriteLine("Created new worker {0}", Sender.Path.Name);
				Context.Watch(Sender);
				_workers.Add(Sender);
				if (_jobQueue.Count > 0)
				{
					Sender.Tell(new JobIsReady());
				}
			});

			Receive<RequestJob>(request =>
			{
				var worker = Sender;
				Console.WriteLine("Worker {0} requests for a work", worker.Path.Name);

				//TODO handle no work to be done situation;
				//TODO load tests somehwere else not here

				Job job;
				if (_jobQueue.Count > 0 && _jobQueue.TryDequeue(out job))
				{
					var runnintTest = new RunningJob
					{
						TestFixtureName = job.TestFixture,
						ArtifactsUri = job.ArtifactsUrl,
						AssemblyPath = job.Assembly,
						Worker = worker
					};

					_runningJobs.Add(runnintTest);
					worker.Tell(job, Self);
				}
				else
				{
					worker.Tell(new NoJob());
				}
			});

			Receive<JobCompleted>(_ =>
			{
				_runningJobs.RemoveAll(job => job.Worker.Equals(Sender));

				if (_jobQueue.Count > 0)
				{
					Sender.Tell(new JobIsReady());
				}
			});

			Receive<TestRun>(input =>
			{
				Console.WriteLine("New test run for {0}", input.Assembly);

				var testFixtures = LoadTestFixtures(input.Assembly);
				foreach (var job in testFixtures)
				{
					_jobQueue.Enqueue(job);
				}

				if (_jobQueue.Count > 0)
				{
					foreach (var worker in _workers)
					{
						worker.Tell(new JobIsReady(), Self);
					}
				}
			});

			Receive<TestEvent>(e =>
			{
				// TODO integrate teamcity reporter
				Console.WriteLine("{0} {1} is {2} by '{3}'.", e.Kind, e.FullName, e.Result, e.Worker);

				if (!string.IsNullOrEmpty(e.Output))
				{
					Console.WriteLine("Output:");
					Console.WriteLine(e.Output);
				}
			});

			Receive<Terminated>(t =>
			{
				var worker = t.ActorRef;
				if (IsKnown(worker))
				{
					Console.WriteLine("Killing worker {0}", worker.Path.Name);
					ReaddTaskIfAny(worker);
					_workers.Remove(worker);
				}
			});

			ReceiveAny(any =>
			{
				//readd task on failure to the queue
				var runningTest = any as RunningJob;
				if (runningTest != null)
				{
					_jobQueue.Enqueue(new Job(runningTest.AssemblyPath, runningTest.TestFixtureName, runningTest.ArtifactsUri));
				}
			});
		}

		private bool IsKnown(IActorRef worker)
		{
			return _workers.Contains(worker);
		}

		private void ReaddTaskIfAny(IActorRef worker)
		{
			var task = _runningJobs.FirstOrDefault(job => job.Worker.Equals(worker));
			if (task != null)
			{
				Self.Tell(task, Sender);
			}
		}

		private IEnumerable<Job> LoadTestFixtures(string assemblyPath)
		{
			// TODO zip package with testing assemblies
			// TODO copy zip package to HTTP server dir

			var artifactsUrl = assemblyPath; // TODO URL to http server

			using (var engine = TestEngineActivator.CreateInstance())
			{
				var package = new TestPackage(new[] {assemblyPath});
				package.Settings["ProcessModel"] = "Single";
				var builder = new TestFilterBuilder();
				var filter = builder.GetFilter();

				using (var runner = engine.GetRunner(package))
				{
					var tests = XElement.Load(new XmlNodeReader(runner.Explore(filter)));

					return from d in tests.Descendants()
						let type = d.GetAttribute("type", (string) null)
						let name = d.GetAttribute("fullname", (string) null)
						where name != null && type != null && type == "TestFixture"
						select new Job(assemblyPath, name, artifactsUrl);
				}
			}
		}
	}
}