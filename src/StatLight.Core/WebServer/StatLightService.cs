using StatLight.Core.Events.Aggregation;
using StatLight.Core.Reporting.Messages;

namespace StatLight.Core.WebServer
{
    using System;
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using System.Web;
    using StatLight.Core.Common;
    using StatLight.Core.Events;
    using StatLight.Core.Properties;
    using StatLight.Core.Serialization;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class StatLightService : IStatLightService
    {
        private readonly FileInfo _xapTestFile;
        private readonly ILogger _logger;
        private readonly IEventAggregator _eventAggregator;
        private readonly TestRunConfiguration _testRunConfiguration;
        private int _currentMessagesPostedCount;
        private int _totalMessagesPostedCount;
        private readonly ServerTestRunConfiguration _serverTestRunConfiguration;

        public string TagFilters
        {
            get { return _testRunConfiguration.TagFilter; }
            set
            {
                _testRunConfiguration.TagFilter = value;
            }
        }

        public StatLightService(ILogger logger, IEventAggregator eventAggregator, string xapTestFile,
            TestRunConfiguration testRunConfiguration, ServerTestRunConfiguration serverTestRunConfiguration)
        {
            if (testRunConfiguration == null)
                throw new ArgumentNullException("testRunConfiguration");
            if (serverTestRunConfiguration == null)
                throw new ArgumentNullException("serverTestRunConfiguration");

            _logger = logger;
            _eventAggregator = eventAggregator;

            if (!File.Exists(xapTestFile))
            {
                throw new FileNotFoundException("File could not be found. [{0}]".FormatWith(xapTestFile));
            }

            _testRunConfiguration = testRunConfiguration;
            _serverTestRunConfiguration = serverTestRunConfiguration;

            _logger.Debug("StatLightService.ctor() - Initializing StatLightService with xapTestFile[{0}]".FormatWith(xapTestFile));

            _xapTestFile = new FileInfo(xapTestFile);

            ResetTestRunStatistics();
        }

        public void PostMessage(Stream stream)
        {
            _currentMessagesPostedCount++;

            try
            {
                var xmlMessage = GetPostedMessage(stream);
                _logger.Debug(xmlMessage);

                if (xmlMessage.Contains(typeof(MobilOtherMessageType).Name))
                {
                    var result = xmlMessage.Deserialize<MobilOtherMessageType>();
                    _eventAggregator.SendMessage(new TestHarnessOtherMessageEvent { Payload = result });

                    //TODO: Remove the logging here...

                    if (result.MessageType == LogMessageType.Error
                        && !result.Message.Contains("KeyType=TestGranularity, ValueType=TestScenario"))
                        _logger.Error(result.TraceMessage());
                }
                else if (xmlMessage.Contains(typeof(MobilScenarioResult).Name))
                {
                    var result = xmlMessage.Deserialize<MobilScenarioResult>();
                    _eventAggregator.SendMessage(new TestResultEvent { Payload = result });
                }
                else
                {
                    _logger.Error("Unknown message posted...");
                    _logger.Error(xmlMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Error deserializing LogMessage...");
                _logger.Error(ex.ToString());
                throw;
            }

            WaitingForMessagesToCompletePosting();
        }

        public Stream GetTestXap()
        {
            _logger.Debug("StatLightService.GetTestXap()");

            return _xapTestFile.OpenRead();
        }

        //public Stream ClientAccessPolicy()
        //{
        //    WebOperationContext.Current.OutgoingResponse.ContentType = "text/xml";
        //    return Resources.ClientAccessPolocy.ToStream();
        //}

        public string GetCrossDomainPolicy()
        {
            _logger.Debug("StatLightService.GetCrossDomainPolicy()");

            if (WebOperationContext.Current != null)
                WebOperationContext.Current.OutgoingResponse.ContentType = "text/xml";

            return Resources.CrossDomain;
        }

        public Stream GetHtmlTestPage()
        {
            _logger.Debug("StatLightService.GetHtmlTestPage()");
            if (WebOperationContext.Current != null)
                WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";

            return Resources.TestPage.ToStream();
        }

        public Stream GetTestPageHostXap()
        {
            _logger.Debug("StatLightService.GetTestPageHostXap()");
            return _serverTestRunConfiguration.HostXap.ToStream();
        }

        public void SignalTestComplete(int totalMessagesPostedCount)
        {
            _logger.Debug("");
            _logger.Debug("StatLightService.TestComplete() with {0} total messages posted - Currently have {1} registered".FormatWith(totalMessagesPostedCount, _currentMessagesPostedCount));
            _totalMessagesPostedCount = totalMessagesPostedCount;

            WaitingForMessagesToCompletePosting();
        }

        private void WaitingForMessagesToCompletePosting()
        {
            if (_totalMessagesPostedCount == _currentMessagesPostedCount)
            {
                _eventAggregator.SendMessage(new TestRunCompletedEvent());

                ResetTestRunStatistics();
            }
        }

        private void ResetTestRunStatistics()
        {
            this._totalMessagesPostedCount = 0;
            _currentMessagesPostedCount = 0;
        }

        private static string GetPostedMessage(Stream stream)
        {
            string message;
            using (var reader = new StreamReader(stream))
            {
                var rawString = reader.ReadToEnd();
                message = HttpUtility.UrlDecode(rawString);
            }
            return message;
        }

        public TestRunConfiguration GetTestRunConfiguration()
        {
            return _testRunConfiguration;
        }

    }
}
