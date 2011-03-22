using System;
using System.Collections.Generic;
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
using StatLight.Core.Serialization;

namespace StatLight.Core.WebServer
{
    public class PostHandler : IPostHandler
    {
        private readonly ILogger _logger;
        private readonly IEventPublisher _eventPublisher;
        private readonly ClientTestRunConfiguration _clientTestRunConfiguration;
        private readonly ResponseFactory _responseFactory;
        private readonly IDictionary<Type, MethodInfo> _publishMethods;

        public PostHandler(ILogger logger, IEventPublisher eventPublisher, ClientTestRunConfiguration clientTestRunConfiguration, ResponseFactory responseFactory)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            if (eventPublisher == null) throw new ArgumentNullException("eventPublisher");
            if (clientTestRunConfiguration == null) throw new ArgumentNullException("clientTestRunConfiguration");

            _logger = logger;
            _eventPublisher = eventPublisher;
            _clientTestRunConfiguration = clientTestRunConfiguration;
            _responseFactory = responseFactory;


            MethodInfo makeGenericMethod = GetType().GetMethod("PublishIt", BindingFlags.Instance | BindingFlags.NonPublic);

            Type clientEventType = typeof(ClientEvent);
            _publishMethods = clientEventType
                .Assembly.GetTypes()
                .Where(w => w.Namespace == clientEventType.Namespace)
                .Where(w => w.Name.EndsWith("ClientEvent", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(key => key, value => makeGenericMethod.MakeGenericMethod(value));

        }


        // Note: do not delete this method - it's used through reflection...
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        private void PublishIt<T>(string xmlMessage)
            where T : ClientEvent
        {
            var result = xmlMessage.Deserialize<T>();
            //DebugLogClientEvent(result);
            _eventPublisher.SendMessage(result);
        }

        public virtual bool TryHandle(Stream messageStream, out string unknownPostData)
        {
            Interlocked.Increment(ref _currentMessagesPostedCount);

            _eventPublisher.SendMessage<MessageReceivedFromClientServerEvent>();

            unknownPostData = null;

            var xmlMessage = GetPostedMessage(messageStream);

            //_logger.Debug(xmlMessage);

            if (Is<SignalTestCompleteClientEvent>(xmlMessage))
            {

                var result = xmlMessage.Deserialize<SignalTestCompleteClientEvent>();
                _eventPublisher.SendMessage(result);
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

                TryWaitingForMessagesToCompletePosting();
                return true;
            }


            
            if (xmlMessage.StartsWith("<", StringComparison.OrdinalIgnoreCase) && xmlMessage.IndexOf(' ') != -1)
            {
                string eventName = xmlMessage.Substring(1, xmlMessage.IndexOf(' ')).Trim();
                if (_publishMethods.Any(w => w.Key.Name == eventName))
                {
                    KeyValuePair<Type, MethodInfo> eventType = _publishMethods.Where(w => w.Key.Name == eventName).SingleOrDefault();
                    eventType.Value.Invoke(this, new[] { xmlMessage });
                    TryWaitingForMessagesToCompletePosting();
                    return true;
                }
            }
            _logger.Debug("Should see this ***********************");
            unknownPostData = xmlMessage;
            return false;
        }

        private int _currentMessagesPostedCount;
        private int? _totalMessagesPostedCount;
        // Browser InstanceId, Total Messages sent count
        private readonly Dictionary<int, int> _browserInstancesComplete = new Dictionary<int, int>();


        public void ResetTestRunStatistics()
        {
            _responseFactory.Reset();
            _browserInstancesComplete.Clear();
            _totalMessagesPostedCount = null;
            _currentMessagesPostedCount = 0;

            //TODO: I think this is only necessary to support the unit tests
            ClientEvent._currentEventCreationOrder = 0;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "TestRunCompletedServerEvent")]
        private void TryWaitingForMessagesToCompletePosting()
        {
            if (_totalMessagesPostedCount.HasValue && _currentMessagesPostedCount >= _totalMessagesPostedCount)
            {
                _logger.Debug("publishing TestRunCompletedServerEvent");
                _eventPublisher.SendMessage(new TestRunCompletedServerEvent());

                ResetTestRunStatistics();
            }
        }

        public static string GetPostedMessage(Stream stream)
        {
            string message;
            using (var reader = new StreamReader(stream))
            {
                var rawString = reader.ReadToEnd();
                message = HttpUtility.UrlDecode(rawString);
            }
            return message;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        private static bool Is<T>(string xmlMessage)
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