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
* `run` - run tests in specified library. Usage: `run tests.dll`. **Required**;
* `--localrun=<numberOfLocalActorsToRun>` - specify how many local actors should be created during running tests (1 by default). Usage: `--localrun=2`;
* `--teamcity` - enable reporting in TeamCity style (PTR can be integrated with TC);
* `--ignore=<category1, category2>` - ignore specified category(ies) during running tests. Usage: `--ignore=TestCategory1,TestCategory2`;
* `--include=<category3, category4>` - include specified category(ies) during running tests. If specified only this(these) category(ies) will be included in test run (other will be skipped). Usage: `--include=TestCategory3,TestCategory4`;
* `help` - prints help in console.


TODO
----

* [ ] Add support for running tests on different machines:
  * [X] Remotly deploy actors;
  * Publish artifacts to remote actor.

