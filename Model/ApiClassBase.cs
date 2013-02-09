using System;
using System.Runtime.Serialization;
using RestSharp;
using SanzaiGuokr.Util;
using SanzaiGuokr.ViewModel;

namespace SanzaiGuokr.Model
{
    public class ApiClassBase
    {
        protected static bool ProcessError<TException>(IRestResponse response) where TException : MyException
        {
            RestSharp.Deserializers.JsonDeserializer J = new RestSharp.Deserializers.JsonDeserializer();
#if DEBUG
            DebugLogging.Append("ExternalCall", response);
#else
            if (ViewModelLocator.ApplicationSettingsStatic.DebugMode)
            {
                DebugLogging.Append("ExternalCall", response);
            }
#endif
            if (!response.Content.Contains("err"))
                return false;

            TException error;
            try
            {
                error = J.Deserialize<TException>(response);
            }
            catch (Exception ex)
            {
                return false;
            }

            if (error == null || error.GetErrorCode() == 0)
                return false;
            else
                throw error;
        }

        protected static RestRequest NewJsonRequest()
        {
            var req = new RestRequest();
            req.RequestFormat = DataFormat.Json;
            req.Parameters.Add(new Parameter() { Name = "Accept-Encoding", Value = "gzip", Type = ParameterType.HttpHeader });
            req.OnBeforeDeserialization = resp =>
            {
                resp.ContentType = "application/json";
            };
            return req;
        }
        protected static RestRequest NewJsonRequestCallback()
        {
            var req = new RestRequest();
            req.RequestFormat = DataFormat.Json;
            req.Parameters.Add(new Parameter() { Name = "Accept-Encoding", Value = "gzip", Type = ParameterType.HttpHeader });
            req.OnBeforeDeserialization = resp =>
            {
                resp.ContentType = "application/json";
                if (resp.Content.Length == 0)
                    return;
                if (resp.Content[0] == '{')
                    return;
                int ind = resp.Content.IndexOf('(');
                if (ind > 0)
                    resp.Content = resp.Content.Substring(ind + 1, resp.Content.Length - 2 - ind);
            };
            return req;
        }
    }
    public abstract class MyException : Exception
    {
        public abstract int GetErrorCode();
        public abstract string GetErrorMessage();
    }
}
