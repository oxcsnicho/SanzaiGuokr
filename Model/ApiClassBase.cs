using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using RestSharp;
using System.Runtime.Serialization;

namespace SanzaiGuokr.Model
{
    public class ApiClassBase
    {
        protected static bool ProcessError<TException>(IRestResponse response) where TException:MyException
        {
            RestSharp.Deserializers.JsonDeserializer J = new RestSharp.Deserializers.JsonDeserializer();
            TException error;
            try
            {
                error = J.Deserialize<TException>(response);
            }
            catch (SerializationException ex)
            {
                return false;
            }

            if (error == null || error.GetErrorCode() == 0)
                return false;
            else
                throw error;
        }
    }
    public abstract class MyException: Exception
    {
        public abstract int GetErrorCode();
    }
}
