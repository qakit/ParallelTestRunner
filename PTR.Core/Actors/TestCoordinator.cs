using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Akka.Actor;
using PTR.Core.Messages;
using PTR.Core.NUnit;
using Status = PTR.Core.Messages.Status;

namespace PTR.Core.Actors
{
	public class TestCoordinator : ReceiveActor
	{
		private readonly ConcurrentQueue<Task> _taskQueue = new ConcurrentQueue<Task>();
		private readonly List<RunningTask> _runningTasks = new List<RunningTask>();
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
				//Handle situation then job is running and we connect new node to server
				//we need to register it => create => give task;
				Console.WriteLine("Registering new worker {0}", msg.TestActorPath);
				var workerAddress = Address.Parse(msg.TestActorPath);
				var workerName = string.Format("RemoteTestExecutor{0}", _remoteWorkersInfo.Count + 1);
				
				_remoteWorkersInfo.Add(workerAddress, workerName);
				if (_taskQueue.Count > 0)
				{
					//TODO Extract to separate method as it repeated in few places;
					Props workerProp =
						Props.Create(() => new TestExecutor()).WithDeploy(Deploy.None.WithScope(new RemoteScope(workerAddress)));
					var worker = Context.ActorOf(workerProp);
					_workers.Add(worker.Path.Name, worker);
					worker.Tell(TaskIsReady.Instance);
				}
			});

			Receive<Job>(msg =>
			{
				foreach (KeyValuePair<Address, string> keyPair in _remoteWorkersInfo)
				{
					Props workerProp =
						Props.Create(() => new TestExecutor()).WithDeploy(Deploy.None.WithScope(new RemoteScope(keyPair.Key)));
					var worker = Context.ActorOf(workerProp);
					_workers.Add(worker.Path.Name, worker);
				}

				if (msg.LocalWorkers > 0)
				{
					for (int i = 0; i < msg.LocalWorkers; i++)
					{
						Props workerProp = Props.Create(() => new TestExecutor());
						var worker = Context.ActorOf(workerProp, string.Format("LocalTestExecutor{0}", i + 1));
						_workers.Add(worker.Path.Name, worker);
					}
				}

				if (_workers.Count > 0)
				{
					var tasks = Runner.LoadFixtures(msg, _workers.Count);
					foreach (Task job in tasks)
					{
						_taskQueue.Enqueue(job);
					}
				}

				NotifyTaskIsReady();
			});

			Receive<GetStatus>(msg =>
			{
				if (_workers.Count == 0)
				{
					Sender.Tell(Status.Completed);
				}
				else if (_runningTasks.Count == 0 && _taskQueue.Count == 0)
				{
					Sender.Tell(Status.Completed);
				}
				else
				{
					Sender.Tell(Status.Busy);
				}
			});

			Receive<RequestTask>(msg =>
			{
				Console.WriteLine("{0} requests a job", Sender.Path.Name);
				var sender = Sender;
				var self = Self;

				Task task;
				if (_taskQueue.Count > 0 && _taskQueue.TryDequeue(out task))
				{
					_runningTasks.Add(new RunningTask(sender, task));
					sender.Tell(task, self);
				}
				else
				{
					sender.Tell(NoTask.Instance);
				}
			});

			Receive<TaskCompleted>(_ =>
			{
				Console.WriteLine("Work is done by {0} actor", Sender.Path.Name);
				var i = _runningTasks.FindIndex(job => Equals(job.Worker, Sender));
				if (i >= 0)
				{
					_runningTasks.RemoveAt(i);
				}

				NotifyTaskIsReady(Sender);
			});

			Receive<Bye>(msg =>
			{
				Console.WriteLine("Worker {0} has been killed", Sender.Path.Name);
				_workers.Remove(Sender.Path.Name);
				Context.Unwatch(Sender);
				Context.Stop(Sender);
			});
		}

		private void NotifyTaskIsReady(IActorRef current = null)
		{
			if (_taskQueue.Count <= 0)
			{
				if (current != null)
				{
					current.Tell(NoTask.Instance);
				}
				else
				{
					foreach (var worker in _workers)
					{
						if (_runningTasks.Count > 0)
						{
							foreach (RunningTask runningTask in _runningTasks)
							{
								if (Equals(runningTask.Worker, worker.Value))
								{
									return;
								}
								worker.Value.Tell(NoTask.Instance);
							}
						}
						else
						{
							worker.Value.Tell(NoTask.Instance);
						}
					}
				}
			}

			if (current != null)
			{
				current.Tell(TaskIsReady.Instance);
				return;
			}

			foreach (var worker in _workers)
			{
				worker.Value.Tell(TaskIsReady.Instance);
			}
		}
	}
}