# ParallelTestRunner

NUnit distributed test runner powered by Akka.NET

It's tool created to add ability run NUnit tests in parallel using Akka.NET. For now supported ability to run tests in parallel only locally. You can run tests and on different machines but folder structure must be the same on all machines (this problem must be solved in future by adding abililty to distribute necessary files to remote actor).

For now to run tests you need build PTR.Server.exe and run

Syntax.

Run - run tests in specified library. Sample usage run tests.dll;
--localrun=<numberOfLocalActors> - allow system to create more local actors. By default 1 actor created locally
--teamcity - set reporting to TC and allow you run tests using PTR on TC with reporting
--ignore=<category1, category2> - ignore specified category in tests.
--include=<category3, category4> - include specified category in tests (other categories will be skipped)
help - print help.

# TODO

1. [ ] Add support for running tests on different machines
	- Remotly deploy actors (done)
	- Publish artifacts to remote actor

