.. _testFilteringOptions:

****************************
Test Filtering Options
****************************



1. If you're using the `Microsoft.Silverlight.Testing <http://code.msdn.microsoft.com/silverlightut>`_ framework.
=================================================================================================================

You can use the below ``-t`` option when running StatLight against your xap and your tag expression will be passed to the built in Microsoft Silverlight Testing filtering engine. 

::

  -t, --TagFilters[=VALUE]   The tag filter expression used to filter
                               executed tests. (See Microsoft.Silverligh-
                               t.Testing filter format for how to generate
                               complicated filter expressions) Only available
                               with MSTest.

The tag filtering is based off of a subset of the `Extended Backus Naur Form <http://en.wikipedia.org/wiki/Extended_Backus%E2%80%93Naur_Form>`_. If you run your test project (w/out StatLight, the U.I. that is shown by the Microsoft.Silverlight.Testing tool gives some examples of how to set expressions).

2. Specify the method(s) to run.
================================

If your not using the `Microsoft.Silverlight.Testing <http://code.msdn.microsoft.com/silverlightut>`_ framework, or if you are and would prefer to use this option, you can specify the full name of a method you would like to test separated by semicolons.
EX:

::

    StatLight.exe -x=<pathToXap.xap> 
	    --MethodsToTest="MyNamespace.SubNamespace.MyClassName.Should_test_something_method;NS1.SubNS1.Class1.TestMethod1"
