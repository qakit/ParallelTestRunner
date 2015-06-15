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
		public static IEnumerable<Job> LoadFixtures(RunTests run)
		{
			var assembly = Assembly.LoadFrom(run.Assembly);

			var fixtures = from t in assembly.GetTypes()
						   where t.HasAttribute<TestFixtureAttribute>()
						   let cat = t.GetAttribute<CategoryAttribute>().IfNotNull(a => a.Name) ?? string.Empty
						   where cat.In(run.Include) && cat.NotIn(run.Exclude)
						   select t;

			return (from type in fixtures select new Job(run.Assembly, type.FullName, run.ReporterActor)).ToList();
		}

		public static TestResult Run(Job job, IReporter reporter)
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

			var assemblyDir = Path.GetDirectoryName(job.Assembly);

			var testPackage = new TestPackage(job.Assembly);
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

				var filter = new SimpleNameFilter(job.TestFixture);

				var runnerFactory = new DefaultTestRunnerFactory();
				using (var runner = runnerFactory.MakeTestRunner(testPackage))
				{
					runner.Load(testPackage);
					var result = runner.Run(listener, filter, true, LoggingThreshold.All);
					return result;
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
