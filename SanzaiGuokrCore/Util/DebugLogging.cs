﻿using System;
using System.Collections.Generic;
using System.Text;

#if DEBUG
using System.Diagnostics;
#endif

namespace SanzaiGuokr.Util
{
    public class DebugLogging
    {
        public static List<string> data = new List<string>();
        const int LengthLimit = 256;
        public static void Append(string type, string request, string response)
        {
            if (request == null)
                return;
            if (request.Contains("/api/userinfo"))
                request = "";

            if (request.Length > LengthLimit)
                request = request.Substring(0, 256);
            if (response.Length > LengthLimit)
                response = response.Substring(0, 256);

            var res = string.Format("{0}||{1}||{2}||{3}\n", DateTime.Now, type, request, response);

#if DEBUG
            Debug.WriteLine(res);
#endif
            data.Add(res);

        }

        public static void Append(string p, RestSharp.IRestResponse response)
        {
            StringBuilder sb = new StringBuilder();

            var request = response.Request;
            if (request != null)
            {
                sb.Append(request.Resource);
                if (request.Parameters != null && request.Parameters.Count > 0)
                {
                    sb.Append("?");
                    foreach (var item in request.Parameters)
                        sb.AppendFormat("{0}={1}&", item.Name, item.Value);
                }
            }

            Append(p, sb.ToString(), response.Content);
        }

        public static string Flush()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in data)
                sb.Append(item);
            return sb.ToString();
        }
        public static void Clear()
        {
            data.Clear();
        }
    }
}
