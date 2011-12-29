.. _optionsDefined:

****************************
Options Defined
****************************


.. _optionsTagFilters:

-t | --TagFilters
=================

If you're using the `Microsoft.Silverlight.Testing <http://code.msdn.microsoft.com/silverlightut>`_ framework. You can use the below ``-t`` option when running StatLight against your xap and your tag expression will be passed to the built in Microsoft Silverlight Testing filtering engine. 

::

  -t, --TagFilters[=VALUE]   The tag filter expression used to filter
                               executed tests. (See Microsoft.Silverligh-
                               t.Testing filter format for how to generate
                               complicated filter expressions) Only available
                               with MSTest.

The tag filtering is based off of a subset of the `Extended Backus Naur Form <http://en.wikipedia.org/wiki/Extended_Backus%E2%80%93Naur_Form>`_. If you run your test project (w/out StatLight, the U.I. that is shown by the Microsoft.Silverlight.Testing tool gives some examples of how to set expressions).


.. _optionsBrowserWindow:

-b | --BrowserWindow
====================

Sets the display visibility and/or size of the StatLight browser window. 

Several uses:

#. Leave blank to keep browser window hidden. (Note: does not work with U.I. tests)
#. Specify this flag to have the browser window shown with the default height/width.
#. Using the following pattern [M|m|n][HEIGHTxWIDTH] you can specify the window state [M|Maximized|m|Minimized|N|Normal][HEIGHTxWIDTH] to be able to specify a specific size. EX: ``-b:N800x600`` a normal window with a width of 800 and height of 600.


.. _optionsMethodsToTest:

--MethodsToTest
===============

Semicolon seperated list of full method names, including namespace and class, to execute.

EX: ``--methodsToTest="RootNamespace.ChildNamespace.ClassName.MethodUnderTest;RootNamespace.ChildNamespace.ClassName.Method2UnderTest;"``

Sample:

::

    StatLight.exe -x=<pathToXap.xap> 
	    --MethodsToTest="MyNamespace.SubNamespace.MyClassName.Should_test_something_method;NS1.SubNS1.Class1.TestMethod1"

Sets the display visibility and/or size of the StatLight browser window. Leave blank to keep browser window hidden. Specify this flag to have the browser window shown with the default height/width. Or using the following pattern {"[M|m|n][HEIGHTxWIDTH]"} you can specify the window state {"[M|Maximized|m|Minimized|N|Normal][HEIGHTxWIDTH]"} to be able to specify a specific size. EX: ``-b:N800x600`` a normal window with a width of 800 and height of 600.


.. _optionOverrideTestProvider:

--OverrideTestProvider
======================

By default StatLight is will try it's best to figure out what testing provider your xap should be tested with EX: [MSTest | XUnit | NUnit | UnitDriven | etc].

There are cases that you will have to help StatLight out. This option allows you to explicitly tell StatLight which provider to use.

This comes in handy if you have a custom provider EX:`Raven Custom provider <https://github.com/bennage/Raven-Custom-Silverlight-Unit-Test-Provider>`.

The ``--MSTestWithCustomProvider`` option will instruct StatLight not use any of it's own providers and search your test assemblies for an ``IUnitTestProvider`` (EX:`Raven Custom provider <https://github.com/bennage/Raven-Custom-Silverlight-Unit-Test-Provider>`_


.. _optionsQueryString:

--QueryString
=============

**My test requires some external configuration** EX: a port or url to my test web service.

When StatLight starts up an instance of a browser, the url the browser requests from the StatLight web server to load the test page is appended with your querystring options. StatLight doesn't use any of the querystring options itself, but allows you to access these parameters from within your silverlight testing xap.

You can use the ``--QueryString="MyValue1=12345&MyValue2=7890"`` option to pass configuration into your silverlight tests.

::.

   var myValue = HtmlPage.Document.QueryString["MyValue1"];

One scenario this becomes useful is hosting a separate web service on a different port. You can first start up the web service instance with a build script. Then pass the port or location to the service into the StatLight test where your test code can fish the configuration out of the QueryString.
