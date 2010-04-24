
using StatLight.Core.Configuration;
using StatLight.Core.Events.Aggregation;

namespace StatLight.Core.WebServer
{
	using System;
	using System.ServiceModel;
	using System.ServiceModel.Description;
	using StatLight.Core.Common;
	using System.ServiceModel.Web;

	public class StatLightServiceHost : IDisposable, IWebServer
	{
		private readonly ILogger _logger;
		private ServiceHost _serviceHost;
		private IStatLightService _serviceInstance;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "xap")]
		public StatLightServiceHost(ILogger logger, IEventAggregator eventAggregator, string xapPath, Uri baseAddress, ClientTestRunConfiguration clientTestRunConfiguration, ServerTestRunConfiguration serverTestRunConfiguration)
            : this(logger, new StatLightService(logger, eventAggregator, xapPath, clientTestRunConfiguration, serverTestRunConfiguration), baseAddress)
		{ }

		public StatLightServiceHost(ILogger logger, StatLightService statLightService, Uri baseAddress)
		{
			_logger = logger;

			_logger.Debug("Initializing StatLightServiceHost at baseAddress[{0}]".FormatWith(baseAddress));

			_serviceInstance = statLightService;

			_serviceHost = new WebServiceHost(statLightService, baseAddress);
			_serviceHost.AddServiceEndpoint(typeof(IStatLightService), new WebHttpBinding(), "");


			//_serviceHost = new ServiceHost(statLightService, baseAddress);

			//var contract = ContractDescription.GetContract(typeof(IStatLightService), typeof(StatLightService));
			//var endpointAddress = new EndpointAddress(baseAddress);

			//var serviceEndpoint = new ServiceEndpoint(contract, new WebHttpBinding(WebHttpSecurityMode.None), endpointAddress);
			//serviceEndpoint.Behaviors.Add(new WebHttpBehavior());
			//var httpServiceMetaDataBehavior = new ServiceMetadataBehavior { HttpGetEnabled = true };
			//httpServiceMetaDataBehavior.MetadataExporter.ExportContract(contract);

			//_serviceHost.Description.Behaviors.Add(httpServiceMetaDataBehavior);
			//_serviceHost.Description.Endpoints.Add(serviceEndpoint);
			//_serviceHost.Authorization.PrincipalPermissionMode = PrincipalPermissionMode.None;
		}

		public IStatLightService StatLightServiceInstance
		{
			get { return _serviceInstance; }
		}

		public CommunicationState State
		{
			get { return _serviceHost.State; }
		}

		public void Start()
		{
			Open();
		}

		public void Stop()
		{
			Close();
		}

		private void Open()
		{
			_logger.Debug("StatLightServiceHost.Open()");
			_serviceHost.Open();
		}

		public void Abort()
		{
			_logger.Debug("StatLightServiceHost.Abort()");
			_serviceHost.Abort();
		}

		private void Close()
		{
			_logger.Debug("StatLightServiceHost.Close()");
			_serviceHost.Close();
		}

		#region IDisposable Members


		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_serviceHost.Close();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
	}

}
