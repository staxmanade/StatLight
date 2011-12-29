.. _continuousMode:

****************************
Continuous mode
****************************

What scenario is the continuous mode trying to solve?
=====================================================

If you've used StatLight, and are doing some form of `TDD <http://en.wikipedia.org/wiki/Test-driven_development>`_, then your typical development steps probably look something like.

#. Write Silverlight unit test
#. Build project
#. **Execute StatLight against the test xap and see the test failure**
#. Write code to make test pass
#. Build project
#. **Execute StatLight against the test xap and (hopefully) see the test pass**

The above steps can get very tedious and are steps that, well, you just shouldn't have to do manually. This is where the continuous mode in StatLight will attempt to ease this pain a little.

What is the continuous mode?
============================

The continuous mode is an option you can specify at the command line when running your first test against the xap. It will leave the application in a continuous monitoring mode. On first execution of StatLight it will run the unit tests and report to the console just at is usually does. However, StatLight will not exit, but instead sit in a monitoring mode, looking for a re-compilation (*change in the test xap file*). Once this change is detected, StatLight will automatcially kick off another test run.

So your workflow may now be something like:

#. Write Silverlight unit test
#. Build project
#. **Wait for results** (*or send random twitter message*)
#. Write code to make test pass
#. Build project
#. **Wait for results** (*or another send random twitter message*)

This option truly works best if you have a dual (*or more*) monitor setup. (You can place StatLight out of the way on one monitor, and write your Silverlight code in the other)

Command line args for continuous mode::

   -c, --Continuous           Runs a single test run, and then monitors the
                              xap for build changes and re-runs the tests
                              automatically.

What happens when I have too many tests to make it worth while to run StatLight against the entire test suite?
==============================================================================================================

You can use the :ref:`optionsTagFilters` to specify what tests you would like to use when running StatLight. When in continuous mode this will work, but if your filter is too narrow, or you want to run other types of tests, you have the ability to change the filter by typing the new filter at the command prompt and press enter. StatLight will then re-execute the tests with your new filter. Similarly, if you have a filter defined and want to run ALL tests, just type enter with no filter text and it will clear the filter. 
