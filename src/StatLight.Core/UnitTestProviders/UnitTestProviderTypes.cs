namespace StatLight.Core.UnitTestProviders
{
	public enum UnitTestProviderType
	{
		Undefined,
		MSTest, // Default
		XUnit,
		NUnit,
		UnitDriven,
	}


	//public class UnitTestProviderEventArgs : EventArgs
	//{
	//    private IUnitTestProvider provider;
	//    public IUnitTestProvider Provider { get { return provider; } }

	//    public UnitTestProviderEventArgs(IUnitTestProvider provider)
	//    {
	//        this.provider = provider;
	//    }
	//}

	//public interface IUnitTestProviderActivator
	//{
	//    void BeginUnitTestProviderActivationAsync();
	//    event EventHandler<UnitTestProviderEventArgs> UnitTestProviderActivationCompleted;
	//}

	//public class XUnitUnitTestProvider : IUnitTestProviderActivator
	//{

	//    public void BeginUnitTestProviderActivationAsync()
	//    {
	//        throw new NotImplementedException();
	//    }

	//    public event EventHandler<UnitTestProviderEventArgs> UnitTestProviderActivationCompleted;

	//}

}
