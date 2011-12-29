.. _continuousIntegrationOptions:

******************************
Continuous Integration Options
******************************

.. _uiTests:

U.I. Tests
======================================

If you are running UI tests (EX: using the TestPanel in the Microsoft.Silverlight.Testing framework) the server's "agent" process must run with access to the desktop.

* In older versions of windows you can do this making the service process run under the **Local System account** and selecting the **Allow service to interact with the desktop**

 .. image:: _static/ServiceConfig.PNG

* In newer versions of windows (or if you need to run the service with a domain account). You will have to look at how to run the agent's service application under a domain account. 

 * Auto-login an account to the console
 * Setup the integration service process to run at start. (This will be different for TeamCity, TFS, CruiseControl.Net (or other))
 * Take a look at the following to get your build agents running under a domain account http://lostechies.com/keithdahlby/2011/08/13/allowing-a-windows-service-to-interact-with-desktop-without-localsystem/

TeamCity Integration 
=============================

* Checkout a TeamCity plugin build for StatLight - `Download <https://bitbucket.org/metaman/teamcitydotnetcontrib/downloads>`_ Thanks to `@WMMac <http://twitter.com/MWMac>`_

TeamCity has an extensibility that allows you to communicate to the server through the console (std out) with special commands. The TeamCity agent will capture the commands and publish the results within TeamCity.

If you first get StatLight console running on your desktop and do a regular test run. Then do another run by giving it the "--teamcity" parameter. Notice the difference in the output?

You can then create a **Command Line Build Runner** in TeamCity.

* Command executable: ``"<Path to statlight.exe>"``
* Command parameters: ``"-x=%system.teamcity.build.checkoutDir%\PathToXap\SilverlightClient.Tests.xap --teamcity"``

The following blog has a more thorough set of setup steps.
Blog: `Running Silverlight unit tests in TeamCity using StatLight <http://pontusmunck.com/2010/05/13/running-silverlight-unit-tests-in-teamcity-using-statlight/>`_


CruiseControl.net
=================
CC.net support is not directly supported, however due to some generous work from the community you should be get up and running by reading the following discussion thread.

`Link to thread <http://statlight.codeplex.com/Thread/View.aspx?ThreadId=233432>`_

TFS
===

There is one TFS option that is built-in, but it's not necessarily the best solution.

* Take a look at using the ``--ReportOutputFileType=MSGenericTest`` flag in conjunction with the ``-r:<path>`` (you can read more detail here http://www.nielshebling.de/?p=167)
* Consider using the ``--ReportOutputFileType=TRX`` (I'm assuming TFS works with this standard MSTest format)

Other options are hacky work arounds - but could work for you.

* Take a look at the following project http://statlightteambuild.codeplex.com
* Another option (may work, but not positive of the outcome) by doing some xml transformations http://statlight.codeplex.com/discussions/258195
