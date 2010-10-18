
using StatLight.Core.WebServer.CustomHost;
using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;
using StatLight.Core.Common;
using StatLight.Core.Configuration;
using StatLight.Core.Properties;

namespace StatLight.Core.WebServer.WcfHost
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class StatLightService : IStatLightService
    {
        private readonly ILogger _logger;
        private readonly ClientTestRunConfiguration _clientTestRunConfiguration;
        private readonly ServerTestRunConfiguration _serverTestRunConfiguration;

        private readonly IPostHandler _postHandler;

        public string TagFilters
        {
            get { return _clientTestRunConfiguration.TagFilter; }
            set
            {
                _clientTestRunConfiguration.TagFilter = value;
            }
        }

        public StatLightService(ILogger logger, ClientTestRunConfiguration clientTestRunConfiguration, ServerTestRunConfiguration serverTestRunConfiguration, IPostHandler postHandler)
        {
            if (postHandler == null)
                throw new ArgumentNullException("postHandler");

            _logger = logger;
            _clientTestRunConfiguration = clientTestRunConfiguration;
            _serverTestRunConfiguration = serverTestRunConfiguration;
            _postHandler = postHandler;

            _postHandler.ResetTestRunStatistics();
        }


        public void PostMessage(Stream stream)
        {
            try
            {
                string unknownPostData;
                if (!_postHandler.TryHandle(stream, out unknownPostData))
                {
                    _logger.Error("Unknown message posted...");
                    _logger.Error(unknownPostData);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error deserializing LogMessage...");
                _logger.Error(ex.ToString());
                throw;
            }
        }

        public Stream GetTestXap()
        {
            _logger.Debug("StatLightService.GetTestXap()");

            return _serverTestRunConfiguration.XapToTest.ToStream();
        }


        //Not sure why but can't seem to get this to work... (would be nice to 
        //get done - as it might speed up a StatLight (by not having to request this first before the CrossDomainPolicy)
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        //public Stream ClientAccessPolicy()
        //{
        //    WebOperationContext.Current.OutgoingResponse.ContentType = "text/xml";
        //    return Resources.ClientAccessPolicy.ToStream();
        //}

        public Stream GetCrossDomainPolicy()
        {
            _logger.Debug("StatLightService.GetCrossDomainPolicy()");

            SetOutgoingResponceContentType("text/xml");

            return Resources.CrossDomain.ToStream();
        }

        public Stream GetHtmlTestPage()
        {
            _logger.Debug("StatLightService.GetHtmlTestPage()");
            var page = ResponseFactory.GetTestHtmlPage();

            SetOutgoingResponceContentType(page.ContentType);

            return page.FileData.ToStream();
        }

        private static void SetOutgoingResponceContentType(string contentType)
        {
            if (WebOperationContext.Current != null)
                WebOperationContext.Current.OutgoingResponse.ContentType = contentType;
        }

        public Stream GetTestPageHostXap()
        {
            _logger.Debug("StatLightService.GetTestPageHostXap()");
            return _serverTestRunConfiguration.HostXap.ToStream();
        }


        public ClientTestRunConfiguration GetTestRunConfiguration()
        {
            return _clientTestRunConfiguration;
        }

    }

}
