using System;
using System.IO;
using System.Net;
using System.Text;
using Vindinium.Common.Entities;
using Vindinium.Common.Services;

namespace Vindinium.Client.Logic
{
    public class ApiCaller : IApiCaller
    {
        private static readonly ILogger Logger = new Logger();

        #region IApiCaller Members

        public void Call(IApiRequest apiRequest, IApiResponse apiResponse)
        {
            Logger.Trace("Call");
            try
            {
                HttpWebRequest request = CreateApiClient(apiRequest);

                DateTime started = DateTime.Now;
                using (WebResponse response = request.GetResponse())
                {
                    LogResponseTime(started);
                    ResponseAsApiResponse(response, apiResponse);
                }
            }
            catch (WebException exception)
            {
                ExceptionAsApiResponse(exception, apiResponse);
            }
        }

        #endregion

        private void ResponseAsApiResponse(WebResponse webResponse, IApiResponse apiResponse)
        {
            Logger.Trace("ResponseAsApiResponse");
            apiResponse.HasError = false;
            apiResponse.ErrorMessage = null;
            apiResponse.Text = ReadResponseStream(webResponse);
        }

        private static string ReadResponseStream(WebResponse response)
        {
            Logger.Trace("ReadResponseStream");
            Stream stream = null;
            try
            {
                stream = response.GetResponseStream();

                if (stream == null) return null;

                using (var reader = new StreamReader(stream))
                {
                    stream = null;

                    string serverResponseText = reader.ReadToEnd();

                    Logger.Trace(serverResponseText);
                    return serverResponseText;
                }
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }
            }
        }

        private static void WriteRequestStream(WebRequest request, string text)
        {
            Logger.Trace("WriteRequestStream, {0}", text);
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            request.ContentLength = buffer.Length;
            Stream stream = null;
            try
            {
                stream = request.GetRequestStream();
                if (stream == null) return;
                using (var writer = new StreamWriter(stream))
                {
                    stream = null;
                    writer.Write(text);
                }
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }
            }
        }

        private void LogResponseTime(DateTime startedTime)
        {
            TimeSpan diff = DateTime.Now.Subtract(startedTime);
            Logger.Trace("LogResponseTime: {0,5} miliseconds", diff.TotalMilliseconds);
        }

        private HttpWebRequest CreateApiClient(IApiRequest apiRequest)
        {
            Logger.Trace("CreateApiClient {0} {1}", apiRequest.Uri, apiRequest.Parameters);
            var request = (HttpWebRequest) WebRequest.Create(apiRequest.Uri);
            request.KeepAlive = false;
            request.Method = "Post";
            request.ContentType = "application/x-www-form-urlencoded";
            WriteRequestStream(request, apiRequest.Parameters);
            return request;
        }

        private void ExceptionAsApiResponse(WebException exception, IApiResponse apiResponse)
        {
            Logger.Trace("ExceptionAsApiResponse - Status: {0}", exception.Status);
            apiResponse.HasError = true;
            apiResponse.Text = null;
            apiResponse.ErrorMessage = ReadResponseStream(exception.Response);
        }
    }
}