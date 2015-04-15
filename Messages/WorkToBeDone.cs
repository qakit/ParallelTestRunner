using Newtonsoft.Json;

namespace Akka.NUnit.Runtime.Messages
{
	public class WorkToBeDone
	{
		public Job Job { get; private set; }

		[JsonConstructor]
		public WorkToBeDone(Job job)
		{
			Job = job;
		}
	}
}