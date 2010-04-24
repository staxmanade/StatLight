using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using StatLight.Client.Harness.Events;
using StatLight.Core.Configuration;
using StatLight.Core.Events.Aggregation;
using StatLight.Core.Reporting.Messages;
using StatLight.Core.WebServer.HelperExtensions;

namespace StatLight.Core.WebServer
{
    using System;
    using System.IO;
    using System.Linq;
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using System.Web;
    using StatLight.Core.Common;
    using StatLight.Core.Events;
    using StatLight.Core.Properties;
    using StatLight.Core.Serialization;
    using System.Threading;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class StatLightService : IStatLightService
    {
        private readonly FileInfo _xapTestFile;
        private readonly ILogger _logger;
        private readonly IEventAggregator _eventAggregator;
        private readonly ClientTestRunConfiguration _clientTestRunConfiguration;
        private int _currentMessagesPostedCount;
        private int _totalMessagesPostedCount;
        private readonly ServerTestRunConfiguration _serverTestRunConfiguration;
        private readonly IDictionary<Type, MethodInfo> _publishMethods;

        public string TagFilters
        {
            get { return _clientTestRunConfiguration.TagFilter; }
            set
            {
                _clientTestRunConfiguration.TagFilter = value;
            }
        }

        public StatLightService(ILogger logger, IEventAggregator eventAggregator, string xapTestFile,
            ClientTestRunConfiguration clientTestRunConfiguration, ServerTestRunConfiguration serverTestRunConfiguration)
        {
            if (clientTestRunConfiguration == null)
                throw new ArgumentNullException("clientTestRunConfiguration");
            if (serverTestRunConfiguration == null)
                throw new ArgumentNullException("serverTestRunConfiguration");

            _logger = logger;
            _eventAggregator = eventAggregator;

            if (!File.Exists(xapTestFile))
            {
                throw new FileNotFoundException("File could not be found. [{0}]".FormatWith(xapTestFile));
            }

            _clientTestRunConfiguration = clientTestRunConfiguration;
            _serverTestRunConfiguration = serverTestRunConfiguration;

            _logger.Debug("StatLightService.ctor() - Initializing StatLightService with xapTestFile[{0}]".FormatWith(xapTestFile));

            _xapTestFile = new FileInfo(xapTestFile);

            ResetTestRunStatistics();

            MethodInfo makeGenericMethod = GetType().GetMethod("PublishIt", BindingFlags.Instance | BindingFlags.NonPublic);

            Type clientEventType = typeof(ClientEvent);
            _publishMethods = clientEventType
                .Assembly.GetTypes()
                .Where(w => w.Namespace == clientEventType.Namespace)
                .Where(w => w.Name.EndsWith("ClientEvent"))
                .ToDictionary(key => key, value => makeGenericMethod.MakeGenericMethod(value));
        }

        private void PublishIt<T>(string xmlMessage)
            where T : ClientEvent
        {
            var result = xmlMessage.Deserialize<T>();
            //DebugLogClientEvent(result);
            _eventAggregator.SendMessage(result);
        }

        public void PostMessage(Stream stream)
        {
            try
            {
                Interlocked.Increment(ref _currentMessagesPostedCount);

                _eventAggregator.SendMessage<MessageReceivedFromClientServerEvent>();

                var xmlMessage = GetPostedMessage(stream);

                //_logger.Debug(xmlMessage);

                if (xmlMessage.Is<SignalTestCompleteClientEvent>())
                {
                    Interlocked.Decrement(ref _currentMessagesPostedCount);

                    var result = xmlMessage.Deserialize<SignalTestCompleteClientEvent>();
                    _eventAggregator.SendMessage(result);
                    var totalMessagsPostedCount = result.TotalMessagesPostedCount;

                    _logger.Debug("");
                    _logger.Debug("StatLightService.TestComplete() with {0} total messages posted - Currently have {1} registered".FormatWith(totalMessagsPostedCount, _currentMessagesPostedCount));

                    _logger.Debug("SignalTestCompleteClientEvent");
                    _logger.Debug("     {");
                    _logger.Debug("         Failed = {0}".FormatWith(result.Failed));
                    _logger.Debug("         TotalMessagesPostedCount = {0}".FormatWith(result.TotalMessagesPostedCount));
                    _logger.Debug("         TotalTestsCount = {0}".FormatWith(result.TotalTestsCount));
                    _logger.Debug("         TotalFailureCount = {0}".FormatWith(result.TotalFailureCount));
                    _logger.Debug("     }");

                    _totalMessagesPostedCount = totalMessagsPostedCount;
                }
                else
                {
                    Action<string> _unknownMsg = (msg) =>
                         {
                             _logger.Error("Unknown message posted...");
                             _logger.Error(xmlMessage);
                         };
                    if (xmlMessage.StartsWith("<") && xmlMessage.IndexOf(' ') != -1)
                    {
                        string eventName = xmlMessage.Substring(1, xmlMessage.IndexOf(' ')).Trim();
                        if (_publishMethods.Any(w => w.Key.Name == eventName))
                        {
                            KeyValuePair<Type, MethodInfo> eventType = _publishMethods.Where(w => w.Key.Name == eventName).SingleOrDefault();
                            eventType.Value.Invoke(this, new[] { xmlMessage });
                        }
                        else
                        {
                            _unknownMsg(xmlMessage);
                        }
                    }
                    else
                    {
                        _unknownMsg(xmlMessage);
                    }
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

        private void DebugLogClientEvent(ClientEvent clientEvent)
        {
            var type = clientEvent.GetType();
            Action<string> log = msg => _logger.Debug(msg);

            var properties = type.GetProperties();

            log(type.Name);
            log("  {");
            foreach (var propertyInfo in properties)
            {
                var value = propertyInfo.GetValue(clientEvent, null);
                var msg = "    {0,24}: {1}".FormatWith(propertyInfo.Name, value);
                log(msg);
            }
            log("  }");
        }

        //public Stream ClientAccessPolicy()
        //{
        //    WebOperationContext.Current.OutgoingResponse.ContentType = "text/xml";
        //    return Resources.ClientAccessPolocy.ToStream();
        //}

        public string GetCrossDomainPolicy()
        {
            _logger.Debug("StatLightService.GetCrossDomainPolicy()");

            SetOutgoingResponceContentType("text/xml");

            return Resources.CrossDomain;
        }

        public Stream GetHtmlTestPage()
        {
            _logger.Debug("StatLightService.GetHtmlTestPage()");

            SetOutgoingResponceContentType("text/html");

            return Resources.TestPage.ToStream();
        }

        private void SetOutgoingResponceContentType(string contentType)
        {
            if (WebOperationContext.Current != null)
                WebOperationContext.Current.OutgoingResponse.ContentType = contentType;
        }

        public Stream GetTestPageHostXap()
        {
            _logger.Debug("StatLightService.GetTestPageHostXap()");
            return _serverTestRunConfiguration.HostXap.ToStream();
        }

        private void WaitingForMessagesToCompletePosting()
        {
            if (_totalMessagesPostedCount == _currentMessagesPostedCount)
            {
                _logger.Debug("publishing TestRunCompletedServerEvent");
                _eventAggregator.SendMessage(new TestRunCompletedServerEvent());

                ResetTestRunStatistics();
            }
        }

        private void ResetTestRunStatistics()
        {
            _totalMessagesPostedCount = 0;
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

        public ClientTestRunConfiguration GetTestRunConfiguration()
        {
            return _clientTestRunConfiguration;
        }

    }

    namespace HelperExtensions
    {
        public static class Extensions
        {
            public static bool Is<T>(this string xmlMessage)
            {
                if (xmlMessage.StartsWith("<" + typeof(T).Name + " xmlns"))
                {
                    return true;
                }
                return false;
            }
        }
    }
}
