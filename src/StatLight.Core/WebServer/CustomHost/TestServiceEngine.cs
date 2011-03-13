using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using StatLight.Core.Common;

namespace StatLight.Core.WebServer.CustomHost
{
    public class TestServiceEngine : IWebServer, IDisposable
    {
        private const string ContentTypeXml = "text/xml";


        private readonly ResponseFactory _responseFactory;
        private readonly ILogger _logger;
        private readonly WebServerLocation _webServerLocation;
        private Task _serverListener;
        private readonly IPostHandler _postHandler;
        private bool Listening { get; set; }


        private HttpListener Server { get; set; }

        public TestServiceEngine(ILogger logger, WebServerLocation webServerLocation, ResponseFactory responseFactory, IPostHandler postHandler)
        {
            _logger = logger;
            _webServerLocation = webServerLocation;
            _postHandler = postHandler;
            _responseFactory = responseFactory;
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
                    HandleUnknownRequest(request, response);
                }
            }
            catch(Exception exception)
            {
                _logger.Debug(exception.ToString());
            }
        }

        private void HandleUnknownRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            SetHttpStatus(response, HttpStatusCode.NotFound);

            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("*****************************************");
            sb.AppendLine("An unknown request was made to the StatLight's web server. You may want to check your test project for what generated the following request.");
            sb.AppendLine("********** Request Information **********");
            sb.AppendLine("{0, 10} : {1}".FormatWith("Url", request.Url));
            sb.AppendLine("{0, 10} : {1}".FormatWith("HttpMethod", request.HttpMethod));
            sb.AppendLine("{0, 10} : {1}".FormatWith("PostData", PostHandler.GetPostedMessage(request.InputStream)));
            sb.AppendLine("*****************************************");
            sb.AppendLine();
            _logger.Warning(sb.ToString());
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "rootDirectory", Justification = "This parameter may be needed in the future.")]
        private void ProcessPostRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            if ((request.Url.Segments.Length > 1) && (request.Url.Segments[1] == StatLightServiceRestApi.PostMessage))
            {
                ServeFunction(request, response);
            }
            else
            {
                HandleUnknownRequest(request, response);
            }
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

        private void ServeFunction(HttpListenerRequest request, HttpListenerResponse response)
        {
            SetContentType(response, ContentTypeXml);
            string unknownPostData;
            if (!_postHandler.TryHandle(request.InputStream, out unknownPostData))
            {
                HandleUnknownRequest(request, response);
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
                    _webServerLocation.BaseUrl.ToString(),
                    _webServerLocation.Port);
                Server.Prefixes.Add(prefix);
                Server.Start();
                Log("Listening on {0}", new object[] { prefix });
                while (Listening)
                {
                    //Log("(Waiting for next request...)", new object[0]);
                    try
                    {
                        HttpListenerContext context = Server.GetContext();
                        HttpListenerResponse response = context.Response;
                        try
                        {
                            HttpListenerRequest request = context.Request;
                            //Log("Received {0} request", new object[] { request.HttpMethod });
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
                            //Log("Sending response", new object[0]);
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
            var msg = exception.ToString();

            // This exception would be cool to get rid of - but I'm not sure how to shut down the process more gracefully
            if (!msg.Contains("The I/O operation has been aborted because of either a thread exit or"))
            {
                _logger.Debug(exception.ToString());
            }
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