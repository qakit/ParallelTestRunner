using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Akka.Actor;
using PTR.Core.NUnit;

namespace PTR.Core.Actors
{
	public class TestCoordinator : ReceiveActor
	{
		private readonly ConcurrentQueue<Job> _jobQueue = new ConcurrentQueue<Job>();
		private readonly List<RunningJob> _runningJobs = new List<RunningJob>();
		private readonly HashSet<IActorRef> _workers = new HashSet<IActorRef>();
		private IActorRef Reporter { get; set; }
		
		public TestCoordinator()
		{
			Become(Idle);
		}

		private void Idle()
		{
			Receive<RegisterTestActor>(msg => Console.WriteLine((string) msg.TestActorPath));

			Receive<RunTests>(msg =>
			{
				var executor = Context.ActorOf(Props.Create(() => new TestExecutor()), "TextExecutor");
				var executor2 = Context.ActorOf(Props.Create(() => new TestExecutor()), "TextExecutor2");
				_workers.Add(executor);
				_workers.Add(executor2);

				var jobs = Runner.LoadFixtures(msg);
				foreach (Job job in jobs)
				{
					_jobQueue.Enqueue(job);
				}

				NotifyJobIsReady();
			});

			Receive<RequestJob>(msg =>
			{
				Console.WriteLine("{0} requests a job", Sender.Path.Name);
				var sender = Sender;
				var self = Self;

				Job job;
				if (_jobQueue.Count > 0 && _jobQueue.TryDequeue(out job))
				{
					_runningJobs.Add(new RunningJob(sender, job));
					sender.Tell(job, self);
				}
				else
				{
					sender.Tell(NoJob.Instance);
				}
			});

			Receive<JobCompleted>(_ =>
			{
				Console.WriteLine((string) "Work is done by {0} actor", (object) Sender.Path.Name);
				var i = _runningJobs.FindIndex(job => Equals(job.Worker, Sender));
				if (i >= 0)
				{
					_runningJobs.RemoveAt(i);
				}

				NotifyJobIsReady(Sender);
			});

			Receive<Bye>(msg =>
			{
				var actor = Sender;
				Console.WriteLine("-------------------------------------------");
				Console.WriteLine((string) "Removing actor {0}", (object) actor.Path.Name);
				Context.Unwatch(actor);
				Context.Stop(actor);
				_workers.Remove(actor);
			});
		}

		private void NotifyJobIsReady(IActorRef current = null)
		{
			if (_jobQueue.Count <= 0)
			{
				if (current != null)
				{
					current.Tell(NoJob.Instance);
				}
				else
				{
					foreach (IActorRef worker in _workers)
					{
						if (_runningJobs.Count > 0)
						{
							foreach (RunningJob runningJob in _runningJobs)
							{
								if (runningJob.Worker == worker)
								{
									return;
								}
								worker.Tell(NoJob.Instance);
							}
						}
						else
						{
							worker.Tell(NoJob.Instance);
						}
					}
				}
			}

			if (current != null)
			{
				current.Tell(JobIsReady.Instance);
				return;
			}

			foreach (IActorRef worker in _workers)
			{
				worker.Tell(JobIsReady.Instance);
			}
		}

//		protected override SupervisorStrategy SupervisorStrategy()
//		{
//			return new OneForOneStrategy(
//				maxNrOfRetries: 10, 
//				withinTimeRange: TimeSpan.FromSeconds(30),
//				localOnlyDecider: x =>
//				{
//					//TODO complete code here
//					if(x is NotImplementedException)
//						return Directive.Resume;
//					else return Directive.Restart;
//				});
//		}
	}
}