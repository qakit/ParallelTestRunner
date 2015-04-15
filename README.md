# TODO

1. [x] loading of test fixtures reflecting assemly
1. [ ] stupid command line interface
	- must be but can be hardcoded for now
1. [ ] Logging messages using akka infrastructure logLevel configuration
1. [x] Separate master and worker modules in different console apps (libs)

1. [ ] Master configuration
	- can have a worker(s) so pass number of workes using config or args
1. [ ] Worker configuration (args or config file)
	- set master url (if not set then shutdown)
	- number of workers (default is 1)
1. [ ] downgrade NUNIt engine to 2.6.2
1. [ ] easy configuration of distributed environment
	- must be
1. [ ] downloading of artifacts (zip package with assemblies)
	- don't download artifacts on agent which is placed on same machine where manager placed
	- don't download artifacts each time (just once, cache path somehow)
	- 
1. [x] running of single test fixture in worker
1. [x] simple console reporter
1. [ ] integrate teamcity reporter into master (from command line args --teamcity like in nunit);
1. [ ] running of multiple assemblies
	- don't needed for now. Assuming situation with single assembly