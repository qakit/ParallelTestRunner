using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Core;
using NUnit.Core.Filters;
using NUnit.Framework;
using NUnit.Util;
using PTR.Core.Extensions;
using PTR.Core.Reporters;

namespace PTR.Core.NUnit
{
	internal static class Runner
	{
		public static IEnumerable<Task> LoadFixtures(Job job, int workersCount)
		{
			var assembly = Assembly.LoadFrom(job.AssemblyPath);
			var fixtures = from t in assembly.GetTypes()
						   where t.HasAttribute<TestFixtureAttribute>()
						   let cat = t.GetAttribute<CategoryAttribute>().IfNotNull(a => a.Name) ?? string.Empty
						   where cat.In(job.Include) && cat.NotIn(job.Exclude)
						   select t; ;

			if (job.Distrubution == Distrubution.Even)
			{
				return (from fixture in fixtures.Split(workersCount) 
						select new Task(
							job.AssemblyPath, 
							(from type in fixture.ToArray() select type.FullName).ToArray(), 
							job.Reporter)).ToList();
			}

			return (from type in fixtures select new Task(job.AssemblyPath, new[] { type.FullName }, job.Reporter)).ToList();
		}

		public static void Run(Task task, IReporter reporter)
		{
			ServiceManager.Services.AddService(new DomainManager());
			ServiceManager.Services.AddService(new ProjectService());
			ServiceManager.Services.AddService(new TestAgency());
			try
			{
				ServiceManager.Services.InitializeServices();
			}
			catch (Exception e)
			{
				Console.WriteLine("exception occurs");
			}

			var assemblyDir = Path.GetDirectoryName(task.Assembly);

			var testPackage = new TestPackage(task.Assembly);
			testPackage.Settings["ProcessModel"] = ProcessModel.Single;
			testPackage.Settings["DomainUsage"] = DomainUsage.Single;
			testPackage.Settings["ShadowCopyFiles"] = false;
			testPackage.Settings["WorkDirectory"] = assemblyDir;

			var outWriter = Console.Out;
			var errorWriter = Console.Error;
			var olddir = Environment.CurrentDirectory;

			try
			{
				var listener = new NUnitEventListener(reporter);

				var filter = new SimpleNameFilter(task.TestFixtures);

				var runnerFactory = new DefaultTestRunnerFactory();
				using (var runner = runnerFactory.MakeTestRunner(testPackage))
				{
					runner.Load(testPackage);
					runner.Run(listener, filter, true, LoggingThreshold.All);
				}
			}
			finally
			{
				ServiceManager.Services.ClearServices();
				Environment.CurrentDirectory = olddir;
				outWriter.Flush();
				errorWriter.Flush();

				Console.SetOut(outWriter);
				Console.SetError(errorWriter);
			}
		}
	}
}
