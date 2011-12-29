.. StatLight documentation master file, created by
   sphinx-quickstart on Wed Dec 28 16:31:14 2011.
   You can adapt this file completely to your liking, but it should at least
   contain the root `toctree` directive.

*************************************
Welcome to StatLight's documentation!
*************************************

Project Description
===================
StatLight is a tool developed for automating the setup, execution, and gathering results of Silverlight unit tests. StatLight helps to speed up the feedback cycles while practicing TDD/BDD/(insert your test style here) during Silverlight development.

Important Links!
=================
* Project Home `StatLight.Codeplex.com <http://statlight.codeplex.com>`_
* Source Control `GitHub <http://github.com/staxmanade/statlight>`_
* Hot off the build! `DOWNLOAD the latest build TeamCity <http://teamcity.codebetter.com/project.html?projectId=project119&tab=projectOverview>`_

How does it work?
=================
StatLight is a console application that creates an in memory web server. It starts up a web browser that will request from the web server a page containing your test xap. By executing all the tests in the browser and communicating test results back to the web server. The console now has the ability to publish those results in manners not allowed in the Silverlight sandbox.

3rd Party Extensions
====================
NOTE: *third party extensions are leveraging StatLight and are subject to their own project release cycles.*

* Visual Studio Extension: Unit Test Result Viewer for StatLight Codeplex Source
   .. image:: _static/ToolboxScreenshotSingle0.1.png
* `Growl Plugin Extension  <https://github.com/lindsve/Statlight.Growl>`_
   .. image:: _static/growl4windows.jpg
* `AgUnit ReSharper <http://AgUnit.CodePlex.com>`_ plugin allowing you to run your tests from Visual Studio. (Backed by StatLight)

How does it work?
=================
* Blog: `Automated Silverlight Unit Testing Using StatLight <http://www.wintellect.com/CS/blogs/jlikness/archive/2010/01/09/automated-silverlight-unit-testing-using-statlight.aspx>`_
* MSDN Webcast: `geekSpeak: Silverlight Testing with Jeremy Likness (Level 200) <http://channel9.msdn.com/shows/geekSpeak/geekSpeak-Recording-Silverlight-Testing-with-Jeremy-Likness>`_
* Blog: `Running Silverlight unit tests in TeamCity using StatLight <http://pontusmunck.com/2010/05/13/running-silverlight-unit-tests-in-teamcity-using-statlight/>`_
* Blog: `Silverlight Unit Tests and Continuous Integration - StatLight (MSBuild) <http://www.keith-woods.com/Blog/post/Silverlight-Unit-Test-and-Continuous-Integration-StatLight.aspx>`_
* Blog: `TFS 2010: Executing Silverlight unit tests during build <http://www.nielshebling.de/?p=167>`_


Contents:

.. toctree::
   :maxdepth: 2

   FAQ.rst
   DocumentationHome.rst
   ContinuousIntegration.rst
   ContinuousMode.rst
   TestFilteringOptions.rst


Indices and tables
==================

* :ref:`genindex`
* :ref:`modindex`
* :ref:`search`

