using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Akka.Actor;
using PTR.Core.Messages;
using PTR.Core.NUnit;
using Status = PTR.Core.Messages.Status;

namespace PTR.Core.Actors
{
	public class TestCoordinator : ReceiveActor
	{
		private class AgentInfo
		{
			public Address Address;
			public string Name;
			public IActorRef Actor;
		}

		private readonly ConcurrentQueue<Task> _taskQueue = new ConcurrentQueue<Task>();
		private readonly List<RunningTask> _runningTasks = new List<RunningTask>();
		private readonly IDictionary<string, IActorRef> _workers = new Dictionary<string, IActorRef>();
		private readonly List<AgentInfo> _remoteWorkersInfo = new List<AgentInfo>();
		private readonly List<AgentProcess> ProcessList = new List<AgentProcess>(); 

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
				_remoteWorkersInfo.Add(new AgentInfo
				{
					Address = workerAddress,
					Name = workerName
				});

				if (_taskQueue.Any())
				{
					CreateRemoteWorkers();
					NotifyTaskIsReady();
				}
			});

			Receive<Init>(msg =>
			{
				if (msg.LocalWorkers > 0)
				{
					if (msg.RunningMode == RunningMode.Inprocess)
					{
						for (int i = 0; i < msg.LocalWorkers; i++)
						{
							Props workerProp = Props.Create(() => new TestExecutor());
							var worker = Context.ActorOf(workerProp, string.Format("LocalTestExecutor{0}", i + 1));
							_workers.Add(worker.Path.Name, worker);
						}
					}
					else
					{
						StartAgent(msg.LocalWorkers);
					}
				}
			});

			Receive<Job>(msg =>
			{
				CreateRemoteWorkers();
				
				var tasks = Runner.LoadFixtures(msg, _workers.Count);
				foreach (Task job in tasks)
				{
					_taskQueue.Enqueue(job);
				}

				NotifyTaskIsReady();
			});

			Receive<GetStatus>(msg =>
			{
				if (_runningTasks.Count == 0 && _taskQueue.Count == 0)
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

				var i = _remoteWorkersInfo.FindIndex(t => t.Actor != null && Equals(t.Actor.Path, Sender.Path));
				if (i >= 0)
				{
					var info = _remoteWorkersInfo[i];
					info.Actor = null;
					i = ProcessList.FindIndex(p => p.Address == info.Address);
					if (i >= 0)
					{
						ProcessList[i].Process.Kill();
						ProcessList.RemoveAt(i);
					}
				}
			});
		}

		private void CreateRemoteWorkers()
		{
			foreach (var info in _remoteWorkersInfo.Where(t => t.Actor == null))
			{
				Props workerProp =
					Props.Create(() => new TestExecutor()).WithDeploy(Deploy.None.WithScope(new RemoteScope(info.Address)));
				var worker = Context.ActorOf(workerProp);
				_workers.Add(worker.Path.Name, worker);
				worker.Tell(new Greet("Connected to server"));
				info.Actor = worker;
			}
		}

		private void StartAgent(int numberOfLocalWorkers)
		{
			for (int i = 0; i < numberOfLocalWorkers; i++)
			{
				int port = 8091;
				if (ProcessList.Count > 0)
				{
					while (ProcessList.Any(p => p.Port == port))
					{
						port++;
					}
				}
				
				var ip = Utils.GetIpAddress();
				var cmdArgs = string.Format("--port={0} --ip={1}", port, ip);
				var process = Process.Start("PTR.Agent.exe", cmdArgs);
				var address = Address.Parse(string.Format("akka.tcp://{0}@{1}:{2}/",
					"RemoteSystem",
					ip,
					port));
				
				ProcessList.Add(new AgentProcess
				{
					Port = port,
					Process = process,
					Address = address
				});
			}
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