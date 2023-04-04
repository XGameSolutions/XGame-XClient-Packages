using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace XCommon.Runtime
{
    public static class XNetUtil
    {
        private static Regex s_Regex = new Regex(@"^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$");

        public static string GetLocalIPv4()
        {
            try
            {
                var host = System.Net.Dns.GetHostName();
                if (host != null)
                {
                    var ips = System.Net.Dns.GetHostAddresses(host);
                    if (ips.Length > 0)
                    {
                        if (ips.Length == 1) return ips[0].ToString().Trim();
                        foreach (var ip in ips)
                        {
                            var ipstr = ip.ToString().Trim();
                            if (s_Regex.IsMatch(ipstr))
                            {
                                return ipstr;
                            }
                        }
                        return "1.1.1.1";
                    }
                    return "2.2.2.2";
                }
                return "3.3.3.3";
            }
            catch
            {
                return "4.4.4.4";
            }
        }
    }
}