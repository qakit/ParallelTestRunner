using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Akka.Actor;
using Akka.NUnit.Runtime.Messages;
using Akka.NUnit.Runtime.Reporters;
using NUnit.Core;
using NUnit.Core.Filters;
using NUnit.Framework;
using NUnit.Util;

namespace Akka.NUnit.Runtime
{
	/// <summary>
	/// Runs and loads tests for NUnit.Framework v2.
	/// </summary>
	internal static class NUnit2Runner
	{
		public static IEnumerable<Job> LoadFixtures(RunTests run)
		{
			var assembly = Assembly.LoadFrom(run.Assembly);

			var fixtures = from t in assembly.GetTypes()
				where t.HasAttribute<TestFixtureAttribute>()
				let cat = t.GetAttribute<CategoryAttribute>().IfNotNull(a => a.Name) ?? string.Empty
				where cat.In(run.Include) && cat.NotIn(run.Exclude)
				select t;

			return (from type in fixtures select new Job(run.Assembly, type.FullName, run.Assembly)).ToList();
		}

		private static bool In(this string cat, string[] cats)
		{
			return cats == null || cats.Length == 0
			       || cats.Any(s => string.Equals(s, cat, StringComparison.InvariantCultureIgnoreCase));
		}

		private static bool NotIn(this string cat, string[] cats)
		{
			return cats == null || cats.All(s => !string.Equals(s, cat, StringComparison.InvariantCultureIgnoreCase));
		}

		private static string[] GetTests(Type type)
		{
			return (
				from method in type.GetMethods()
				where method.HasAttribute<TestAttribute>() ||
				      method.HasAttribute<TestCaseAttribute>() ||
				      method.HasAttribute<TestCaseSourceAttribute>()
				select type.FullName + "." + method.Name
				).ToArray();
		}

		public static TestResult Run(Job job, IActorRef sender, IActorRef self)
		{
			ServiceManager.Services.AddService(new DomainManager());
			ServiceManager.Services.AddService(new ProjectService());
			ServiceManager.Services.AddService(new TestAgency());
			ServiceManager.Services.InitializeServices();

			var assemblyDir = Path.GetDirectoryName(job.Assembly);

			var testPackage = new TestPackage(job.Assembly);
			testPackage.Settings["ProcessModel"] = ProcessModel.Single;
			testPackage.Settings["DomainUsage"] = DomainUsage.Single;
			testPackage.Settings["ShadowCopyFiles"] = false;
			testPackage.Settings["WorkDirectory"] = assemblyDir;

			// TODO enable config injection if really needed
//			var configPath = new FileInfo(Path.Combine(assemblyDir, Path.GetFileName(job.Assembly) + ".config"));
//			var configMap = new ExeConfigurationFileMap {ExeConfigFilename = configPath.FullName};
//			var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
//
//			foreach (var key in config.AppSettings.Settings.AllKeys)
//			{
//				ConfigurationManager.AppSettings[key] = config.AppSettings.Settings[key].Value;
//			}

			var outWriter = Console.Out;
			var errorWriter = Console.Error;
			var olddir = Environment.CurrentDirectory;

			try
			{
				var listener = new CompositeEventListener(new EventListener[]
				{
					new EventListenerImpl(self.Path.Name, e => sender.Tell(e, self)),
					new NUnitEventListener(outWriter, errorWriter)
				});

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
				Environment.CurrentDirectory = olddir;
				outWriter.Flush();
				errorWriter.Flush();

				Console.SetOut(outWriter);
				Console.SetError(errorWriter);
			}
		}
	}
}
