using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StatLight.Client.Tests
{
	public class FixtureBase
	{
		[ClassInitialize]
		public void SetupContext()
		{
			Before_all_tests();
			Because();
		}

		[ClassCleanup]
		public void TearDownContext()
		{
			After_all_tests();
		}

		protected virtual void Because()
		{
		}

		protected virtual void Before_all_tests()
		{
		}

		protected virtual void After_all_tests()
		{
		}
	}
}
