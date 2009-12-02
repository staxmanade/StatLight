
namespace StatLight.Core.WebServer
{
    using System.IO;
	using System.ServiceModel;
	using System.ServiceModel.Web;

    [ServiceContract]
	public interface IStatLightService
	{
		string TagFilters { get; set; }

		//TODO: this is causing a security exception inside silverlight for some reason???..
		// falling back to the flash crossdomain.xml
		//[OperationContract]
		//[WebGet(UriTemplate = "ClientAccessPolicy.xml")]
		//Stream ClientAccessPolicy();

		[OperationContract]
		[WebGet(UriTemplate = "crossdomain.xml", BodyStyle = WebMessageBodyStyle.Bare)]
		string GetCrossDomainPolicy();

		[OperationContract]
		[WebInvoke(UriTemplate = StatLightServiceRestApi.PostMessage, Method = "POST", BodyStyle = WebMessageBodyStyle.Bare)]
		void PostMessage(Stream stream);

		[OperationContract]
		[WebGet(UriTemplate = StatLightServiceRestApi.GetXapToTest, BodyStyle = WebMessageBodyStyle.Bare)]
		Stream GetTestXap();

		[OperationContract]
		[WebGet(UriTemplate = StatLightServiceRestApi.GetHtmlTestPage, BodyStyle = WebMessageBodyStyle.Bare)]
		Stream GetHtmlTestPage();

		[OperationContract]
		[WebGet(UriTemplate = StatLightServiceRestApi.GetTestPageHostXap, BodyStyle = WebMessageBodyStyle.Bare)]
		Stream GetTestPageHostXap();

		[OperationContract]
		[WebGet(UriTemplate = StatLightServiceRestApi.SignalTestComplete, BodyStyle = WebMessageBodyStyle.Bare)]
		void SignalTestComplete(int totalMessagesPostedCount);

		[OperationContract]
		[WebGet(UriTemplate = StatLightServiceRestApi.GetTestRunConfiguration, BodyStyle = WebMessageBodyStyle.Bare)]
		TestRunConfiguration GetTestRunConfiguration();
	}
}