using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.Event;
using Akka.NUnit.Runtime.Messages;
using Akka.NUnit.Runtime.Reporters;

namespace Akka.NUnit.Runtime
{
	/// <summary>
	/// Manages jobs and workers.
	/// </summary>
	public class Manager : ReceiveActor
	{
		private class RunningJob
		{
			public readonly IActorRef Worker;
			public readonly Job Job;
			public readonly DateTime Started;
			public readonly bool IsLast;

			public RunningJob(IActorRef worker, Job job, bool isLast)
			{
				Worker = worker;
				Job = job;
				IsLast = isLast;
				Started = DateTime.UtcNow;
			}
		}

		protected ILoggingAdapter Log = Context.GetLogger();
		private readonly HashSet<IActorRef> _workers = new HashSet<IActorRef>();
		private readonly ConcurrentQueue<Job> _jobQueue = new ConcurrentQueue<Job>();
		private readonly List<RunningJob> _runningJobs = new List<RunningJob>();
		private IReporter _reporter = NullReporter.Instance;
		
		public Manager()
		{
			Receive<SetReporter>(msg =>
			{
				switch (msg.Kind)
				{
					case ReporterKind.Silent:
						_reporter = NullReporter.Instance;
						break;
					case ReporterKind.Console:
						_reporter = new TextReporter();
						break;
					case ReporterKind.TeamCity:
						_reporter = new TeamCityReporter();
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			});

			Receive<Greet>(_ =>
			{
				Log.Info("New worker {0}", Sender.Path.Name);

				Context.Watch(Sender);
				_workers.Add(Sender);

				Sender.Tell(Greet.Instance, Self);
				NotifyJobIsReady(Sender);
			});

			var firstJobStarted = DateTime.UtcNow;
			var firstJob = true;

			Receive<RequestJob>(request =>
			{
				var worker = Sender;
				Log.Debug("Worker {0} requests job", worker.Path.Name);

				//TODO handle no work to be done situation;
				//TODO load tests somehwere else not here

				Job job;
				if (_jobQueue.Count > 0 && _jobQueue.TryDequeue(out job))
				{
					if (firstJob)
					{
						firstJob = false;
						firstJobStarted = DateTime.UtcNow;
					}

					_runningJobs.Add(new RunningJob(worker, job, _jobQueue.Count == 0));
					worker.Tell(job, Self);
				}
				else
				{
					worker.Tell(NoJob.Instance, Self);
				}
			});

			Receive<JobCompleted>(_ =>
			{
				var i = _runningJobs.FindIndex(job => Equals(job.Worker, Sender));
				if (i >= 0)
				{
					var job = _runningJobs[i];
					_runningJobs.RemoveAt(i);

					var duration = DateTime.UtcNow - job.Started;
					Log.Info("Job duration: {0}s", duration.TotalSeconds);

					if (job.IsLast)
					{
						Log.Info("Total duration: {0}s", (DateTime.UtcNow - firstJobStarted).TotalSeconds);
					}
				}

				NotifyJobIsReady(Sender);
			});

			Receive<RunTests>(msg =>
			{
				Log.Info("New test run of assembly {0}", msg.Assembly);

				// TODO zip package with testing assemblies
				// TODO copy zip package to HTTP server dir

				var jobs = NUnit2Runner.LoadFixtures(msg);

				foreach (var job in jobs)
				{
					_jobQueue.Enqueue(job);
				}

				NotifyJobIsReady();
			});

			Receive<TestEvent>(e => _reporter.Report(e));

			Receive<Bye>(msg =>
			{
				var worker = Sender;
				RemoveWorker(worker);
			});

			Receive<Terminated>(t =>
			{
				if (t.ActorRef == Self)
				{
					Stop();
				}
				else
				{
					RemoveWorker(t.ActorRef);
				}
			});

			Receive<PoisonPill>(_ => Stop());
		}

		private void Stop()
		{
			Log.Info("Killing manager");

			foreach (var worker in _workers)
			{
				worker.Tell(Bye.Shutdown, Self);
			}
		}

		private void NotifyJobIsReady(IActorRef current = null)
		{
		    if (_jobQueue.Count <= 0)
		    {
                foreach (var worker in _workers)
                {
                    worker.Tell(NoJob.Instance, Self);
                }
                return;
		    }

			if (current != null)
			{
				current.Tell(JobIsReady.Instance, Self);
				return;
			}

			foreach (var worker in _workers)
			{
				worker.Tell(JobIsReady.Instance, Self);
			}
		}

		private void RemoveWorker(IActorRef worker)
		{
			if (!_workers.Remove(worker)) return;
			Log.Info("Killing worker {0}", worker.Path.Name);
			ReaddTaskIfAny(worker);
		}

		private void ReaddTaskIfAny(IActorRef worker)
		{
			var task = _runningJobs.FirstOrDefault(job => Equals(job.Worker, worker));
			if (task != null)
			{
				_jobQueue.Enqueue(task.Job);
				NotifyJobIsReady();
			}
		}
	}
}