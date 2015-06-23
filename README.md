ParallelTestRunner
==================
NUnit distributed test runner powered by Akka.NET.

Table of Contents
-----------------

* [Description](#description)
* [Usage](#usage)
  * [Commands](#commands)

Description
-----------
ParallelTestRanner - tool created to add abilily run NUnit tests in parallel using Akka.NET. For now supported abililty to run tests in parallel only locally. But you can run tests and on different machines, but folder structure must be the same on all machines (this problem will be solved in near future by adding abililty to distribute necessary files to remote actor).

Usage
-----
###Commands
####PTR.Server
* `run` - run tests in specified library. Usage: `run tests.dll`. **Required**;
* `--localrun=<numberOfLocalActorsToRun>` - specify how many local actors should be created during running tests (1 by default). Usage: `--localrun=2`;
* `--mode=<inprocess, separate>` - specifies how local actors must be runned (`inprocess` - actors will be runned in PTR.Server process, `separate` - actors will be runned using PTR.Agent.exe (remote deploy). Usage: `--mode=separate`;
* `--teamcity` - enable reporting in TeamCity style (PTR can be integrated with TC);
* `--ignore=<category1, category2>` - ignore specified category(ies) during running tests. Usage: `--ignore=TestCategory1,TestCategory2`;
* `--include=<category3, category4>` - include specified category(ies) during running tests. If specified only this(these) category(ies) will be included in test run (other will be skipped). Usage: `--include=TestCategory3,TestCategory4`;
* `help` - prints help in console.

####PTR.Agent
* `--port` - specifies port on which current agent system must be runned. If not specified value of `akka.remote.helios.tcp.port` value in app.config will be used.;
* `--ip` - specifies ip (hostname) which will be used to run agent system. If not specified default system IP will be used;
* `--masterIp` - specifies ip address of the PTR.Server. If not specified value of `akka.remote.helios.tcp.master-hostname` in app.config will be used;
* `--masterPort` - spcifies port of the PTR.Server. If no specified value of `akka.remote.helios.tcp.master-port` in app.config will be used;

TODO
----

* [ ] Add support for running tests on different machines:
  * [X] Remotly deploy actors;
  * Publish artifacts to remote actor.

