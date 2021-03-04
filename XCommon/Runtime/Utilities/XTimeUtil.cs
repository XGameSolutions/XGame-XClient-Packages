using System;
using System.Globalization;

namespace XCommon.Runtime
{
    public static class XTimeUtil
    {

        public static string GetNowTime()
        {
            DateTime now = DateTime.Now;
            return now.ToString("[HH:mm:ss fff]", DateTimeFormatInfo.InvariantInfo);
        }

        public static string FormatTime(long second)
        {
            return FormatTime((int)second);
        }
        public static string FormatTime(int second)
        {
            var h = 0;
            var m = 0;
            var s = 0;
            if (second < 60) s = second;
            else if (second < 3600)
            {
                m = second / 60;
                s = second % 60;
            }
            else
            {
                h = second / 3600;
                m = second / (3600 * 60);
                s = second % 60;
            }
            return string.Format("{0:d2}:{1:d2}:{2:d2}", h, m, s);
        }
    }
}