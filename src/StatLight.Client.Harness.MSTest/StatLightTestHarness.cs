using System;
using Microsoft.Silverlight.Testing.Harness;

#if July2009 || October2009 || November2009
using CompositeWorkItem = Microsoft.Silverlight.Testing.UnitTesting.Harness.CompositeWorkItem;
using UnitTestHarness = Microsoft.Silverlight.Testing.UnitTesting.Harness.UnitTestHarness;
#else
using CompositeWorkItem = Microsoft.Silverlight.Testing.Harness.CompositeWorkItem;
using UnitTestHarness = Microsoft.Silverlight.Testing.Harness.UnitTestHarness;
#endif


namespace StatLight.Client.Harness.Hosts.MSTest
{
	public class StatLightTestHarness : UnitTestHarness
	{
		private CompositeWorkItem _harnessTasks;
		internal const string HarnessName = "NON UI Unit Testing";

		//public static AgUnitVsttTestHarness Create(IEnumerable<TestAssembly> testAssemblies)
		//{
		//    var unitTestSettings = new UnitTestSettings();
		//    var testHarness = new AgUnitVsttTestHarness();
		//    var agUnitVsttLogProvider = new AgUnitVsttLogProvider(logger, testAssemblies);

		//    unitTestSettings.LogProviders.Add(agUnitVsttLogProvider);
		//    unitTestSettings.TestHarness = testHarness;

		//    var agUnitVsttTestRunFilter = new AgUnitVsttTestRunFilter(testAssemblies);

		//    var assemblies = new AssemblyLoader().LoadAssemblies();
		//    foreach (var assembly in assemblies)
		//    {
		//        testHarness.EnqueueTestAssembly(assembly, agUnitVsttTestRunFilter);
		//    }

		//    testHarness.Initialize(unitTestSettings);
		//    testHarness.TestHarnessCompleted += (s, a) => logger.TestRunFinished();

		//    return testHarness;
		//}

		public StatLightTestHarness()
		{
			CreateHarnessTasks();
		}

		private void CreateHarnessTasks()
		{
			Console.WriteLine("CreateHarnessTasks");
			_harnessTasks = new CompositeWorkItem();
			_harnessTasks.Complete += HarnessComplete;
		}

		private void HarnessComplete(object sender, EventArgs e)
		{
			Console.WriteLine("HarnessComplete");
			_harnessTasks = null;
		}

		protected override bool RunNextStep()
		{
			Console.WriteLine("RunNextStep");
			ProcessLogMessages();
			if (RootCompositeWorkItem == null)
			{
				throw new InvalidOperationException("UnitTestHarness_RunNextStep_NoCompositeWorkItemsExist");
			}
			return RootCompositeWorkItem.Invoke();
		}

		public override void RestartRunDispatcher()
		{

			Console.WriteLine(base.TestMethodCount);
			Console.WriteLine("RestartRunDispatcher");
			if (_harnessTasks == null)
			{
				CreateHarnessTasks();
			}
			RunDispatcher = new RunDispatcher(RunNextStep);
			RunDispatcher.Complete += RunDispatcherComplete;
			Console.WriteLine("Before Run()");
			RunDispatcher.Run();
			Console.WriteLine("After Run()");
		}


	}
}