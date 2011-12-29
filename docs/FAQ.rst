F.A.Q.
======

* I have code in my App.xaml.cs Application_Startup beyond the typical setup of the test project EX:``UnitTestSystem.CreateTestPage();`` and it doesn't appear to be executed

 StatLight is the host "App" in the Silverlight plugin. StatLight dynamically loads your assemblies into the appdomain. This means if you have any custom logic in you App.xaml.cs's app startup method, it will get bypassed and never executed because it's StatLight's App.xaml.cs code that's actually executed.
 
 Look to place this sort of code in a test classes's setup method or possibly an assembly setup method. (EX: MSTest supports the ``AssemblyInitialize`` attribute)
   ::

        [AssemblyInitialize]
        public void AssemblyInitialize()
        {
            // To some special one time setup logic here.
        }

* I have a C.I. server that runs, but doesn't report any tests.

 If your test project runs on the command line (dev machine) just fine; however, when run on a C.I. server it executes StatLight and reports zero tests. Take a look at the :ref:`uiTests` section.
