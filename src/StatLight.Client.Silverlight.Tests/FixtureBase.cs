using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StatLight.Client.Silverlight.Tests
{
	public class FixtureBase
	{
		[ClassInitialize]
		public void SetupContext()
		{
			Before_each_test();
		}

		[ClassCleanup]
		public void TearDownContext()
		{
			After_each_test();
		}

		protected virtual void Before_each_test()
		{
		}

		protected virtual void After_each_test()
		{
		}
	}
}
