﻿using System;
using System.Threading;
using NUnit.Framework;

namespace Tests
{
	[TestFixture]
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
			Console.WriteLine("SHOULD BE RUNNED BEFORE EACH TEST");
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

		[Test]
		public void Test()
		{
			Thread.Sleep(100);
			Console.WriteLine("Fixture1.Test");
		}

		[TestCase("a")]
		[TestCase("b")]
		public void TestCase(string input)
		{
			Thread.Sleep(100);
			Console.WriteLine("{0}.TestCase({1})", FixtureName, input);
		}
    }

	[TestFixture]
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

	[TestFixture(1)]
	[TestFixture(2)]
	public class Fixture3
	{
		private readonly int _version;

		public Fixture3(int version)
		{
			_version = version;
		}

		[Test]
		public void Test()
		{
			Thread.Sleep(100);
			Console.WriteLine("Fixture3({0}).Test", _version);
		}

		[TestCase("a")]
		[TestCase("b")]
		public void TestCase(string input)
		{
			Thread.Sleep(100);
			Console.WriteLine("Fixture3({0}).TestCase({1})", _version, input);
		}
	}
}