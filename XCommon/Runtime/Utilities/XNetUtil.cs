using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace XCommon.Runtime
{
    public static class XNetUtil
    {
        public static string GetLocalIP()
        {
            try
            {
                var host = System.Net.Dns.GetHostName();
                if (host != null)
                {
                    var ips = System.Net.Dns.GetHostAddresses(host);
                    if (ips.Length > 0)
                    {
                        return ips[0].ToString();
                    }
                }
                return "0.0.0.0";
            }
            catch
            {
                return "0.0.0.0";
            }
        }
    }
}