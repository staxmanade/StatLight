using System.Windows;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StatLight.IntegrationTests.Silverlight
{
	[TestClass]
	[Tag("MessageBox")]
	[Tag("UI")]
	public class When_a_modal_MessageBox_is_displayed
	{
		[TestMethod]
		public void messageBox_overload_1()
		{
			MessageBox.Show("Some text");
		}

		[TestMethod]
		public void messageBox_overload_1_MessageBoxButton_OK()
		{
			MessageBox.Show("Some text", "some caption", MessageBoxButton.OK);
		}

		[TestMethod]
		public void messageBox_overload_1_MessageBoxButton_OKCancel()
		{
			MessageBox.Show("Some text", "some caption", MessageBoxButton.OKCancel);
		}
	}
}