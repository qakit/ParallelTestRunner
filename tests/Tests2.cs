//using System;
//using NUnit.Framework;
//
//namespace Tests2
//{
//	[TestFixture]
//    public class Fixture1
//    {
//		[Test]
//		public void Test()
//		{
//			Console.WriteLine("Fixture1.Test");
//		}
//
//		[TestCase("a")]
//		[TestCase("b")]
//		public void TestCase(string input)
//		{
//			Console.WriteLine("Fixture1.TestCase({0})", input);
//		}
//    }
//
//	[TestFixture]
//	public class Fixture2
//	{
//		[Test]
//		public void Test()
//		{
//			Console.WriteLine("Fixture2.Test");
//		}
//
//		[TestCase("a")]
//		[TestCase("b")]
//		public void TestCase(string input)
//		{
//			Console.WriteLine("Fixture2.TestCase({0})", input);
//		}
//	}
//
//	[TestFixture(1)]
//	[TestFixture(2)]
//	public class Fixture3
//	{
//		private readonly int _version;
//
//		public Fixture3(int version)
//		{
//			_version = version;
//		}
//
//		[Test]
//		public void Test()
//		{
//			Console.WriteLine("Fixture3({0}).Test", _version);
//		}
//
//		[TestCase("a")]
//		[TestCase("b")]
//		public void TestCase(string input)
//		{
//			Console.WriteLine("Fixture3({0}).TestCase({1})", _version, input);
//		}
//	}
//}
