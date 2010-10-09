using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using StatLight.Client.Harness.Events;
using StatLight.Core.Common;
using StatLight.Core.Configuration;
using StatLight.Core.Events;
using StatLight.Core.Events.Aggregation;
using StatLight.Core.Properties;
using StatLight.Core.Serialization;
using StatLight.Core.WebServer.Host.HelperExtensions;

namespace StatLight.Core.WebServer.Host
{
    public class ResponseFactory
    {
        private readonly Func<byte[]> _xapToTestFactory;
        private readonly byte[] _hostXap;
        private readonly string _serializedConfiguration;

        public ResponseFactory(Func<byte[]> xapToTestFactory, byte[] hostXap, ClientTestRunConfiguration clientTestRunConfiguration)
        {
            _xapToTestFactory = xapToTestFactory;
            _hostXap = hostXap;
            _serializedConfiguration = clientTestRunConfiguration.Serialize();
        }

        private static int _htmlPageInstanceId = 0;

        public ResponseFile Get(string localPath)
        {
            if (IsKnown(localPath, StatLightServiceRestApi.CrossDomain))
                return new ResponseFile { FileData = Resources.CrossDomain.ToByteArray(), ContentType = "text/xml" };

            if (IsKnown(localPath, StatLightServiceRestApi.ClientAccessPolicy))
                return new ResponseFile { FileData = Resources.ClientAccessPolicy.ToByteArray(), ContentType = "text/xml" };

            if (IsKnown(localPath, StatLightServiceRestApi.GetHtmlTestPage))
            {
                return GetTestHtmlPage();
            }

            if (IsKnown(localPath, StatLightServiceRestApi.GetXapToTest))
                return new ResponseFile { FileData = _xapToTestFactory(), ContentType = "application/x-silverlight-app" };

            if (IsKnown(localPath, StatLightServiceRestApi.GetTestPageHostXap))
                return new ResponseFile { FileData = _hostXap, ContentType = "application/x-silverlight-app" };

            if (IsKnown(localPath, StatLightServiceRestApi.GetTestRunConfiguration))
                return new ResponseFile { FileData = _serializedConfiguration.ToByteArray(), ContentType = "text/xml" };

            throw new NotImplementedException();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public bool IsKnownFile(string localPath)
        {
            if (IsKnown(localPath, StatLightServiceRestApi.CrossDomain))
                return true;

            if (IsKnown(localPath, StatLightServiceRestApi.ClientAccessPolicy))
                return true;

            if (IsKnown(localPath, StatLightServiceRestApi.GetHtmlTestPage))
                return true;

            if (IsKnown(localPath, StatLightServiceRestApi.GetXapToTest))
                return true;

            if (IsKnown(localPath, StatLightServiceRestApi.GetTestPageHostXap))
                return true;

            if (IsKnown(localPath, StatLightServiceRestApi.GetTestRunConfiguration))
                return true;

            return false;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public static ResponseFile GetTestHtmlPage()
        {
            var page = Resources.TestPage.Replace("BB86D193-AD39-494A-AEB7-58F948BA5D93", _htmlPageInstanceId.ToString(CultureInfo.InvariantCulture));

            Interlocked.Increment(ref _htmlPageInstanceId);

            return new ResponseFile { FileData = page.ToByteArray(), ContentType = "text/html" };
        }

        private static bool IsKnown(string filea, string fileb)
        {
            return string.Equals(filea, fileb, StringComparison.OrdinalIgnoreCase);
        }

        public static void Reset()
        {
            _htmlPageInstanceId = 0;
        }
    }

    public class PostHandler : IHandlePost
    {
        private readonly ILogger _logger;
        private readonly IEventAggregator _eventAggregator;
        private readonly ClientTestRunConfiguration _clientTestRunConfiguration;
        private readonly IDictionary<Type, MethodInfo> _publishMethods;

        public PostHandler(ILogger logger, IEventAggregator eventAggregator, ClientTestRunConfiguration clientTestRunConfiguration)
        {
            _logger = logger;
            _eventAggregator = eventAggregator;
            _clientTestRunConfiguration = clientTestRunConfiguration;


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

        public virtual void Handle(Stream messageStream)
        {
            Interlocked.Increment(ref _currentMessagesPostedCount);

            _eventAggregator.SendMessage<MessageReceivedFromClientServerEvent>();

            var xmlMessage = GetPostedMessage(messageStream);

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



        private int _currentMessagesPostedCount;
        private int? _totalMessagesPostedCount;
        // Browser InstanceId, Total Messages sent count
        private readonly Dictionary<int, int> _browserInstancesComplete = new Dictionary<int, int>();


        internal void ResetTestRunStatistics()
        {
            ResponseFactory.Reset();
            _browserInstancesComplete.Clear();
            _totalMessagesPostedCount = null;
            _currentMessagesPostedCount = 0;

            //TODO: I think this is only necessary to support the unit tests
            ClientEvent._currentEventCreationOrder = 0;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "TestRunCompletedServerEvent")]
        public void TryWaitingForMessagesToCompletePosting()
        {
            if (_totalMessagesPostedCount.HasValue && _currentMessagesPostedCount >= _totalMessagesPostedCount)
            {
                _logger.Debug("publishing TestRunCompletedServerEvent");
                _eventAggregator.SendMessage(new TestRunCompletedServerEvent());

                ResetTestRunStatistics();
            }
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
    }

    public interface IHandlePost
    {
        void Handle(Stream messageStream);
    }


    namespace HelperExtensions
    {
        public static class Extensions
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
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