.. _extensibility:

*************
Extensibility
*************

Existing Extensions
===================
* `Growl Plugin Extension  <https://github.com/lindsve/Statlight.Growl>`_

   .. image:: _static/growl4windows.jpg

* If you write one up `let me know <http://statlight.codeplex.com/discussions>`_ and I'll publish it here.

.. _basicExtension:

How to create a basic extension
===============================

#. Create a basic Class library
#. Add a reference to StatLight.Core.dll
#. Copy/Paste this following sample extension implementation. (make sure to implement all the necessary ``IListener<T>'s`` on the ``ITestingReportEvents`` interface)

   ::

      using System;
      using StatLight.Client.Harness.Events;
      using StatLight.Core.Events;
      
      namespace SampleExtension
      {
          public class Class1 : ITestingReportEvents
          {
              public void Handle(TestCaseResult message)
              {
                  Console.WriteLine("Hello From Class1");
              }
      
              public void Handle(TraceClientEvent message)
              {
              }
      
              public void Handle(BrowserHostCommunicationTimeoutServerEvent message)
              {
              }
      
              public void Handle(FatalSilverlightExceptionServerEvent message)
              {
              }
      
              public void Handle(UnhandledExceptionClientEvent message)
              {
              }
          }
      }

#. :ref:`Build and Deploy <buildAndDeployExtension>`

How to create a more sophisticated extension
============================================

#. Follow the steps for building a basic extension :ref:`basicExtension`
#. Copy/Paste this following stub.
#. Decide what ``IListener<T>'s`` you want to subscribe to and handle accordingly.
#. To get a list of all messages you can subscribe to Take a look at the classes that end with `ServerEvent here <here https://github.com/staxmanade/StatLight/blob/master/src/StatLight.Core/Events/Events.Server.cs>`_ or `ClientEvent here <https://github.com/staxmanade/StatLight/blob/master/src/StatLight.Core/Events/Events.Client.cs>`_.

   ::

      using System;
      using StatLight.Client.Harness.Events;
      using StatLight.Core.Events;
      
      namespace SampleExtension
      {
          public class Class1 : IShouldBeAddedToEventAggregator
			   IListener<TODO_Pick_an_event_to_listen_to>,
			   IListener<TODO_Pick_another_event_to_listen_to>,
          {
          }
      }

.. _buildAndDeployExtension:

How to deploy your extension.
=============================

#. Build your project
#. Copy **only** your assembly (excluding StatLight.Core.dll and it's dependencies) to the "Extensions" folder where StatLight.exe lives.
#. Your extension should now show up.
#. When you run StatLight you should see some output stating that your extension either failed to load with an exception or if it succeeds you should see something like 

   ::
   
      ********** Extensions **********
      * Adding - SampleExtension.Class1
      ********************************
