using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using StatLight.Core.Common;

namespace StatLight.Core.WebServer.Host
{
    public class TestServiceEngine : IWebServer, IDisposable
    {
        private const string ContentTypeXml = "text/xml";

        private const string PrefixUrlFormat = "http://localhost:{0}/";

        private readonly ResponseFactory _responseFactory;

        private readonly ILogger _logger;
        private Task _serverListener;
        private readonly IPostHandler _postHandler;

        public TestServiceEngine(ILogger logger, string machineName, int port, ResponseFactory responseFactory, IPostHandler postHandler)
        {
            _logger = logger;
            _postHandler = postHandler;
            ServerName = machineName;
            Port = port;
            _responseFactory = responseFactory;
        }


        private static string GetLocalPath(HttpListenerRequest request)
        {
            string filePath = request.Url.LocalPath;
            if (!(string.IsNullOrEmpty(filePath) || (filePath[0] != '/')))
            {
                filePath = filePath.Substring(1, filePath.Length - 1);
            }
            return filePath;
        }

        private void Log(string value, params object[] o)
        {
            string text = (o != null && o.Length == 0) ? value : string.Format(CultureInfo.InvariantCulture, value, o);
            _logger.Debug(text);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Need to keep requests going.")]
        private void ProcessGetRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            try
            {
                var localPath = GetLocalPath(request);
                if (_responseFactory.IsKnownFile(localPath))
                {
                    var responseFile = _responseFactory.Get(localPath);

                    SetHttpStatus(response, HttpStatusCode.OK);
                    SetContentType(response, responseFile.ContentType);
                    ServeString(response, responseFile.FileData);
                }
                else
                {
                    SetHttpStatus(response, HttpStatusCode.NotFound);
                }
            }
            catch (HttpListenerException)
            {
            }
            catch
            {
            }
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "rootDirectory", Justification = "This parameter may be needed in the future.")]
        private void ProcessPostRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            if ((request.Url.Segments.Length > 1) && (request.Url.Segments[1] == StatLightServiceRestApi.PostMessage))
            {
                ServeFunction(request, response);
            }
        }


        private void ServeFunction(HttpListenerRequest request, HttpListenerResponse response)
        {
            _logger.Debug(request.Url.ToString());

            SetContentType(response, ContentTypeXml);
            //using (var sw = new StreamWriter(response.OutputStream))
            {
                _postHandler.Handle(request.InputStream);
                //sw.Write(Functions.ProcessFunction(data, response, postData));
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Need to keep the thread processing regardless.")]
        public void ServeRequests()
        {
            Listening = true;
            Server = new HttpListener();
            try
            {
                string prefix = string.Format(
                    CultureInfo.InvariantCulture,
                    PrefixUrlFormat,
                    Port);
                Server.Prefixes.Add(prefix);
                Server.Start();
                if (!string.IsNullOrEmpty(RootDirectory))
                {
                    Log("Starting server for {0}", new object[] { RootDirectory });
                }
                else
                {
                    Log("Starting server without a root directory", new object[0]);
                }
                Log("Listening on {0}", new object[] { prefix });
                while (Listening)
                {
                    Log("(Waiting for next request...)", new object[0]);
                    try
                    {
                        HttpListenerContext context = Server.GetContext();
                        HttpListenerResponse response = context.Response;
                        try
                        {
                            HttpListenerRequest request = context.Request;
                            Log("Received {0} request", new object[] { request.HttpMethod });
                            if (request.HttpMethod == "GET")
                            {
                                ProcessGetRequest(request, response);
                            }
                            else if (request.HttpMethod == "POST")
                            {
                                ProcessPostRequest(request, response);
                            }
                        }
                        catch (Exception e)
                        {
                            LogException(e);
                        }
                        finally
                        {
                            Log("Sending response", new object[0]);
                            response.Close();
                        }
                    }
                    catch (Exception e)
                    {
                        LogException(e);
                    }
                }
            }
            catch (Exception e)
            {
                LogException(e);
            }
        }

        private void LogException(Exception exception)
        {
            _logger.Debug(exception.ToString());
        }

        private static void ServeString(HttpListenerResponse response, byte[] value)
        {
            response.Headers.Add("Cache-Control", "no-cache");
            using (Stream output = response.OutputStream)
            {
                output.Write(value, 0, value.Length);
            }
        }

        private static void SetContentType(HttpListenerResponse response, string contentType)
        {
            response.Headers.Add("Content-type", contentType);
        }

        private static void SetHttpStatus(HttpListenerResponse response, HttpStatusCode status)
        {
            response.StatusCode = (int)status;
        }

        public string HostName
        {
            get
            {
                return (ServerName + ":" + Port);
            }
        }

        private bool Listening { get; set; }

        public int Port { get; private set; }

        public string RootDirectory { get; set; }

        private HttpListener Server { get; set; }

        public string ServerName { get; private set; }

        public string TagExpression { get; set; }

        public string TestRunPrefix { get; set; }

        public void Start()
        {
            _serverListener = new Task(ServeRequests);
            _serverListener.Start();

        }

        public void Stop()
        {
            Listening = false;
            Server.Close();
            Server = null;
        }


        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Server != null)
                {
                    Stop();
                }

                if (_serverListener != null)
                    _serverListener.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}