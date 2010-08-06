
using System.Diagnostics;

namespace StatLight.Core.WebServer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.ServiceModel;
    using System.ServiceModel.Web;
    using System.Threading;
    using System.Web;
    using StatLight.Client.Harness.Events;
    using StatLight.Core.Common;
    using StatLight.Core.Configuration;
    using StatLight.Core.Events;
    using StatLight.Core.Events.Aggregation;
    using StatLight.Core.Properties;
    using StatLight.Core.Serialization;
    using StatLight.Core.WebServer.HelperExtensions;

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class StatLightService : IStatLightService
    {
        private readonly ILogger _logger;
        private readonly IEventAggregator _eventAggregator;
        private readonly ClientTestRunConfiguration _clientTestRunConfiguration;
        private int _currentMessagesPostedCount;
        private int? _totalMessagesPostedCount;
        private readonly ServerTestRunConfiguration _serverTestRunConfiguration;
        private readonly IDictionary<Type, MethodInfo> _publishMethods;

        // Browser InstanceId, Total Messages sent count
        private readonly Dictionary<int, int> _browserInstancesComplete = new Dictionary<int, int>();
        private int _instanceId;

        public string TagFilters
        {
            get { return _clientTestRunConfiguration.TagFilter; }
            set
            {
                _clientTestRunConfiguration.TagFilter = value;
            }
        }

        public StatLightService(ILogger logger, IEventAggregator eventAggregator, ClientTestRunConfiguration clientTestRunConfiguration, ServerTestRunConfiguration serverTestRunConfiguration)
        {
            if (clientTestRunConfiguration == null)
                throw new ArgumentNullException("clientTestRunConfiguration");
            if (serverTestRunConfiguration == null)
                throw new ArgumentNullException("serverTestRunConfiguration");

            _logger = logger;
            _eventAggregator = eventAggregator;

            _clientTestRunConfiguration = clientTestRunConfiguration;
            _serverTestRunConfiguration = serverTestRunConfiguration;

            ResetTestRunStatistics();

            MethodInfo makeGenericMethod = GetType().GetMethod("PublishIt", BindingFlags.Instance | BindingFlags.NonPublic);

            Type clientEventType = typeof(ClientEvent);
            _publishMethods = clientEventType
                .Assembly.GetTypes()
                .Where(w => w.Namespace == clientEventType.Namespace)
                .Where(w => w.Name.EndsWith("ClientEvent", StringComparison.OrdinalIgnoreCase))
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

                    var result = xmlMessage.Deserialize<SignalTestCompleteClientEvent>();
                    _eventAggregator.SendMessage(result);
                    var totalMessagsPostedCount = result.TotalMessagesPostedCount;

                    _logger.Debug("");
                    _logger.Debug("StatLightService.TestComplete() with {0} total messages posted - Currently have {1} registered"
                        .FormatWith(totalMessagsPostedCount, _currentMessagesPostedCount));
                    _logger.Debug(result.WriteDebug());

                    if (!_browserInstancesComplete.ContainsKey(result.BrowserInstanceId))
                        _browserInstancesComplete.Add(result.BrowserInstanceId, totalMessagsPostedCount);

                    if (_browserInstancesComplete.Count == _clientTestRunConfiguration.NumberOfBrowserHosts)
                    {
                        _totalMessagesPostedCount = _browserInstancesComplete.Sum(s => s.Value);
                        _logger.Debug("Awaiting a total of {0} messages - currently have {1}".FormatWith(_totalMessagesPostedCount, _currentMessagesPostedCount));
                    }


                }
                else
                {
                    Action<string> unknownMsg = msg =>
                         {
                             _logger.Error("Unknown message posted...");
                             _logger.Error(xmlMessage);
                         };
                    if (xmlMessage.StartsWith("<", StringComparison.OrdinalIgnoreCase) && xmlMessage.IndexOf(' ') != -1)
                    {
                        string eventName = xmlMessage.Substring(1, xmlMessage.IndexOf(' ')).Trim();
                        if (_publishMethods.Any(w => w.Key.Name == eventName))
                        {
                            KeyValuePair<Type, MethodInfo> eventType = _publishMethods.Where(w => w.Key.Name == eventName).SingleOrDefault();
                            eventType.Value.Invoke(this, new[] { xmlMessage });
                        }
                        else
                        {
                            unknownMsg(xmlMessage);
                        }
                    }
                    else
                    {
                        unknownMsg(xmlMessage);
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

            return _serverTestRunConfiguration.XapToTest.ToStream();
        }


        //Not sure why but can't seem to get this to work... (would be nice to 
        //get done - as it might speed up a StatLight (by not having to request this first before the CrossDomainPolicy)
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        //public Stream ClientAccessPolicy()
        //{
        //    WebOperationContext.Current.OutgoingResponse.ContentType = "text/xml";
        //    return Resources.ClientAccessPolocy.ToStream();
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
            SetOutgoingResponceContentType("text/html");

            var page = Resources.TestPage;
            page = page.Replace("BB86D193-AD39-494A-AEB7-58F948BA5D93", _instanceId.ToString());
            _instanceId++;
            return page.ToStream();
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

        private void WaitingForMessagesToCompletePosting()
        {
            if (_totalMessagesPostedCount.HasValue && _currentMessagesPostedCount >= _totalMessagesPostedCount)
            {
                _logger.Debug("publishing TestRunCompletedServerEvent");
                _eventAggregator.SendMessage(new TestRunCompletedServerEvent());

                ResetTestRunStatistics();
            }
        }

        private void ResetTestRunStatistics()
        {
            _instanceId = 0;
            _browserInstancesComplete.Clear();
            _totalMessagesPostedCount = null;
            _currentMessagesPostedCount = 0;

            //TODO: I think this is only necessary to support the unit tests
            ClientEvent._currentEventCreationOrder = 0;
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
                if (xmlMessage == null) throw new ArgumentNullException("xmlMessage");

                if (xmlMessage.StartsWith("<" + typeof(T).Name + " xmlns", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                return false;
            }
        }
    }
}
