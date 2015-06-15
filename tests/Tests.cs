using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using NUnit.Framework;

namespace Tests
{
	[TestFixture, Category("Test")]
    public class Fixture1
	{
		private string FixtureName;
		[TestFixtureSetUp]
		public void TestFixtureSetUp()
		{
			FixtureName = "TTT";
			Console.WriteLine("THIS SETUP SHOULD BE RUNNED ONCE");
		}

		[SetUp]
		public void SetUP()
		{
			Debug.Print("SHOULD BE RUNNED BEFORE EACH TEST");
		}

		[TearDown]
		public void TearDown()
		{
			Console.WriteLine("SHOULD BE RUNNED AFTER EACH TEST");
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			FixtureName = "";
			Console.WriteLine("THIS TEARDOWN SHOULD BE RUNNED ONCE");
		}

		public IEnumerable TestData
		{
			get
			{
				yield return new TestCaseData("Test A", new Action(() => Console.WriteLine("ACTION")));
				yield return new TestCaseData("Test B", new Action(() => Console.WriteLine("ACTION2"))).SetName("THis is test b");
			}
		}

		[TestCaseSource("TestData")]
		public void EnumTest(string test, Action todo)
		{
			Thread.Sleep(1000);
			Console.WriteLine(test);
			todo();
		}

		[Test]
		public void Test()
		{
			Thread.Sleep(100);
			Console.WriteLine("Fixture1.Test");
//			MessageBox.Show("Test");
		}

		[TestCase("a")]
		[TestCase("b")]
		public void TestCase(string input)
		{
			Thread.Sleep(100);
			Console.WriteLine("{0}.TestCase({1})", FixtureName, input);
		}

		[Test]
		public void FailTest()
		{
			Assert.AreEqual(1, 2);
		}

		[Test]
		public void ExceptionTest()
		{
			throw new Exception("BLAAA");
		}
    }

	[TestFixture, Category("Test2")]
	public class Fixture2
	{
		[Test]
		public void Test()
		{
			Thread.Sleep(100);
			Console.WriteLine("Fixture2.Test");
		}

		[TestCase("a")]
		[TestCase("b")]
		public void TestCase(string input)
		{
			Thread.Sleep(100);
			Console.WriteLine("Fixture2.TestCase({0})", input);
		}
	}

	//[TestFixture(1)]
	//[TestFixture(2)]
	//public class Fixture3
	//{
	//	private readonly int _version;

	//	public Fixture3(int version)
	//	{
	//		_version = version;
	//	}

	//	[Test]
	//	public void Test()
	//	{
	//		Thread.Sleep(100);
	//		Console.WriteLine("Fixture3({0}).Test", _version);
	//	}

	//	[TestCase("a")]
	//	[TestCase("b")]
	//	public void TestCase(string input)
	//	{
	//		Thread.Sleep(100);
	//		Console.WriteLine("Fixture3({0}).TestCase({1})", _version, input);
	//	}
	//}
}
