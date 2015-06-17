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
		private readonly IDictionary<string, IActorRef> _workers = new Dictionary<string, IActorRef>();
		private readonly IDictionary<Address, string> _remoteWorkersInfo = new Dictionary<Address, string>(); 
		
		public TestCoordinator()
		{
			Become(Idle);
		}

		private void Idle()
		{
			Receive<RegisterTestActor>(msg =>
			{
				Console.WriteLine("Registering new worker {0}", msg.TestActorPath);
				var workerAddress = Address.Parse(msg.TestActorPath);
				var workerName = string.Format("RemoteTestExecutor{0}", _remoteWorkersInfo.Count + 1);
				
				_remoteWorkersInfo.Add(workerAddress, workerName);
			});

			Receive<RunTests>(msg =>
			{
				foreach (KeyValuePair<Address, string> keyPair in _remoteWorkersInfo)
				{
					Props workerProp =
						Props.Create(() => new TestExecutor()).WithDeploy(Deploy.None.WithScope(new RemoteScope(keyPair.Key)));
					var worker = Context.ActorOf(workerProp);
					_workers.Add(worker.Path.Name, worker);
				}

				if (msg.NumOfLocalWorkers > 0)
				{
					for (int i = 0; i < msg.NumOfLocalWorkers; i++)
					{
						Props workerProp = Props.Create(() => new TestExecutor());
						var worker = Context.ActorOf(workerProp, string.Format("LocalTestExecutor{0}", i + 1));
						_workers.Add(worker.Path.Name, worker);
					}
				}

				var jobs = Runner.LoadFixtures(msg);
				foreach (Job job in jobs)
				{
					_jobQueue.Enqueue(job);
				}

				NotifyJobIsReady();
			});

			Receive<GetStatus>(msg =>
			{
				if (_workers.Count == 0)
					Sender.Tell(new Status(0, 0));
				else
					Sender.Tell(new Status(_runningJobs.Count, _jobQueue.Count));
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
				Console.WriteLine("Work is done by {0} actor", Sender.Path.Name);
				var i = _runningJobs.FindIndex(job => Equals(job.Worker, Sender));
				if (i >= 0)
				{
					_runningJobs.RemoveAt(i);
				}

				NotifyJobIsReady(Sender);
			});

			Receive<Bye>(msg =>
			{
				Console.WriteLine("Worker {0} has been killed", Sender.Path.Name);
				_workers.Remove(Sender.Path.Name);
				Context.Unwatch(Sender);
				Context.Stop(Sender);
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
					foreach (var worker in _workers)
					{
						if (_runningJobs.Count > 0)
						{
							foreach (RunningJob runningJob in _runningJobs)
							{
								if (Equals(runningJob.Worker, worker.Value))
								{
									return;
								}
								worker.Value.Tell(NoJob.Instance);
							}
						}
						else
						{
							worker.Value.Tell(NoJob.Instance);
						}
					}
				}
			}

			if (current != null)
			{
				current.Tell(JobIsReady.Instance);
				return;
			}

			foreach (var worker in _workers)
			{
				worker.Value.Tell(JobIsReady.Instance);
			}
		}
//
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