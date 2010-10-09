using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using StatLight.Core.Common;

namespace StatLight.Core.WebServer.Host
{
    /// <summary>
    /// The underlying service implementation.
    /// </summary>
    public class TestServiceEngine : IWebServer, IDisposable
    {
        /// <summary>
        /// The content type to use for XML documents.
        /// </summary>
        private const string ContentTypeXml = "text/xml";

        /// <summary>
        /// The URL format used for the prefix.
        /// </summary>
        private const string PrefixUrlFormat = "http://localhost:{0}/";

        private readonly ResponseFactory _responseFactory;

        /// <summary>
        /// Initializes a new instance of the test service.
        /// </summary>
        public TestServiceEngine(ILogger logger, string machineName, int port, ResponseFactory responseFactory, PostHandler postHandler)
        {
            _logger = logger;
            _postHandler = postHandler;
            ServerName = machineName;
            Port = port;
            _responseFactory = responseFactory;
        }


        /// <summary>
        /// Returns the local path from an HTTP listener request.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <returns>Returns the local path.</returns>
        private static string GetLocalPath(HttpListenerRequest request)
        {
            string filePath = request.Url.LocalPath;
            if (!(string.IsNullOrEmpty(filePath) || (filePath[0] != '/')))
            {
                filePath = filePath.Substring(1, filePath.Length - 1);
            }
            return filePath;
        }

        /// <summary>
        /// Logs to the debug stream a message.
        /// </summary>
        /// <param name="value">The format or value.</param>
        /// <param name="o">Optional set of parameter obejcts.</param>
        private void Log(string value, params object[] o)
        {
            string text = (o != null && o.Length == 0) ? value : string.Format(CultureInfo.InvariantCulture, value, o);
            _logger.Debug(text);
        }

        /// <summary>
        /// Process a GET request.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <param name="response">The response object.</param>
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

        /// <summary>
        /// Process a POST request.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <param name="response">The response object.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "rootDirectory", Justification = "This parameter may be needed in the future.")]
        private void ProcessPostRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            string results;
            //using (Stream input = request.InputStream)
            //{
            //    using (var reader = new StreamReader(input))
            //    {
            //        results = reader.ReadToEnd();
            //    }
            //}
            using (var reader = new StreamReader(request.InputStream))
            {
                results = reader.ReadToEnd();
            }
            if ((request.Url.Segments.Length > 1) && (request.Url.Segments[1] == StatLightServiceRestApi.PostMessage))
            {
                ServeFunction(request, response, results);
            }
        }


        ///// <summary>
        ///// Save a file in the directory.
        ///// </summary>
        ///// <param name="directory">The directory to save in.</param>
        ///// <param name="request">The request object.</param>
        ///// <param name="response">The repsonse object.</param>
        //private void ServeFile(string directory, HttpListenerRequest request, HttpListenerResponse response)
        //{
        //    string filePath = GetLocalPath(request);
        //    Log("Requested file {0}", new object[] { filePath });
        //    string path = Path.Combine(directory, filePath);
        //    response.Headers.Add("Cache-Control", "no-cache");
        //    if (Directory.Exists(path) && DirectoryIsWithinDirectory(path, directory))
        //    {
        //        ListDirectoryContents(response, path);
        //    }
        //    else if (!File.Exists(path))
        //    {
        //        Log("404 - File not found -" + path, new object[0]);
        //        SetHttpStatus(response, HttpStatusCode.NotFound);
        //    }
        //    else if (!FileIsWithinDirectory(path, directory))
        //    {
        //        SetHttpStatus(response, HttpStatusCode.Unauthorized);
        //        Log("401 - Unauthorized file location " + path, new object[0]);
        //    }
        //    else
        //    {
        //        Log("Writing file {0}", new object[] { path });
        //        SetContentType(response, GetMimeType(path));
        //        using (Stream output = response.OutputStream)
        //        {
        //            using (FileStream input = File.OpenRead(path))
        //            {
        //                response.ContentLength64 = input.Length;
        //                int read = 0;
        //                byte[] buffer = new byte[0x800];
        //                do
        //                {
        //                    read = input.Read(buffer, 0, buffer.Length);
        //                    output.Write(buffer, 0, read);
        //                }
        //                while (read > 0);
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// Serve a function without any POST data.
        ///// </summary>
        ///// <param name="request">The request object.</param>
        ///// <param name="response">The response object.</param>
        //private void ServeFunction(HttpListenerRequest request, HttpListenerResponse response)
        //{
        //    ServeFunction(request, response, null);
        //}

        /// <summary>
        /// Serve a function.
        /// </summary>
        /// <param name="request">The request object.</param>
        /// <param name="response">The response object.</param>
        /// <param name="postData">The raw HTTP POST data as a string.</param>
        private void ServeFunction(HttpListenerRequest request, HttpListenerResponse response, string postData)
        {
            _logger.Debug(request.Url.ToString());

            SetContentType(response, ContentTypeXml);
            using (var sw = new StreamWriter(response.OutputStream))
            {
                _postHandler.Handle(postData);
                //throw new NotImplementedException();
                //sw.Write(Functions.ProcessFunction(data, response, postData));
            }
        }

        /// <summary>
        /// Serve requests.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Need to keep the thread processing regardless.")]
        public void ServeRequests()
        {
            //_listenerThread = new Thread(new ThreadStart(StartShutdownMonitorThread));
            //_listenerThread.Start();
            //Current = this;
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

        /// <summary>
        /// Serve a string back through the stream.
        /// </summary>
        /// <param name="response">The response object.</param>
        /// <param name="value">The string value to write.</param>
        private static void ServeString(HttpListenerResponse response, byte[] value)
        {
            response.Headers.Add("Cache-Control", "no-cache");
            using (Stream output = response.OutputStream)
            {
                output.Write(value, 0, value.Length);
                //using (StreamWriter sw = new StreamWriter(output))
                //{
                //    sw.Write();
                //    sw.Write(value.ToStringFromByteArray());
                //}
            }
        }

        /// <summary>
        /// Sets the content type on the HTTP response.
        /// </summary>
        /// <param name="response">The response object.</param>
        /// <param name="contentType">The content type.</param>
        private static void SetContentType(HttpListenerResponse response, string contentType)
        {
            response.Headers.Add("Content-type", contentType);
        }

        /// <summary>
        /// Sets the HTTP status.
        /// </summary>
        /// <param name="response">The response object.</param>
        /// <param name="status">The status to set.</param>
        private static void SetHttpStatus(HttpListenerResponse response, HttpStatusCode status)
        {
            response.StatusCode = (int)status;
        }

        ///// <summary>
        ///// Start the shutdown monitoring thread that will gracefully shut down
        ///// by performing the final HTTP request after a shutdown has been
        ///// initiated.
        ///// </summary>
        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Improving thread robustness.")]
        //private void StartShutdownMonitorThread()
        //{
        //    while (Listening && (DateTime.Now < _shutdownTime))
        //    {
        //        Thread.Sleep(100);
        //    }
        //    if (Listening)
        //    {
        //        Listening = false;
        //    }
        //    try
        //    {
        //        for (int i = 0; i < 3; i++)
        //        {
        //            throw new NotImplementedException();
        //            //TestServiceHelper.PingService(HostName);
        //        }
        //    }
        //    catch
        //    {
        //    }
        //}

        ///// <summary>
        ///// Gets or sets the current test service reference.
        ///// </summary>
        //internal static TestServiceEngine Current { get; set; }

        /// <summary>
        /// Gets the name name with port.
        /// </summary>
        public string HostName
        {
            get
            {
                return (ServerName + ":" + Port);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the service is currently
        /// listening.
        /// </summary>
        public bool Listening { get; private set; }

        ///// <summary>
        ///// Gets or sets the log file path.
        ///// </summary>
        //public string LogFile { get; private set; }

        /// <summary>
        /// Gets the port used for listening.
        /// </summary>
        public int Port { get; private set; }

        ///// <summary>
        ///// Gets or sets the test run result.
        ///// </summary>
        //public TestRunResult Result { get; set; }

        /// <summary>
        /// Gets or sets the root directory to use for hosting.
        /// </summary>
        public string RootDirectory { get; set; }

        /// <summary>
        /// Gets or sets the HTTP listener object.
        /// </summary>
        private HttpListener Server { get; set; }

        /// <summary>
        /// Gets the server name used for listening.
        /// </summary>
        public string ServerName { get; private set; }

        /// <summary>
        /// Gets or sets the tag expression in use for the run.
        /// </summary>
        public string TagExpression { get; set; }

        /// <summary>
        /// Gets or sets the test run prefix.
        /// </summary>
        public string TestRunPrefix { get; set; }


        private readonly ILogger _logger;
        private Task _serverListener;
        private readonly PostHandler _postHandler;

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