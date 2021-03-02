using System.IO;
using System.Text;

namespace XRemoteDebug
{
    public static class RemoteDebugUtil
    {
        private static StringBuilder s_StringBuilder = new StringBuilder();
        public static string GetParentPath(string path)
        {
            if (path.EndsWith("/")) path = path.Substring(0, path.Length - 2);
            var end = path.LastIndexOf('/');
            return path.Substring(0, end);
        }

        public static long GetFileSize(string filePath)
        {
            var file = new FileInfo(filePath);
            return file.Exists ? file.Length : 0;
        }

        public static string GetFileLastWriteTime(string path)
        {
            try
            {
                var info = new FileInfo(path);
                return info.Exists ? info.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss") : "--";
            }
            catch
            {
                return "--";
            }
        }

        public static string GetFileMd5(string filePath, out long fileSize)
        {
            fileSize = 0;
            if (!File.Exists(filePath)) return null;
            try
            {
                var fs = new FileStream(filePath, FileMode.Open);
                fileSize = fs.Length;
                var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                var bytes = md5.ComputeHash(fs);
                fs.Close();
                s_StringBuilder.Length = 0;
                for (int i = 0; i < bytes.Length; i++)
                {
                    s_StringBuilder.Append(bytes[i].ToString("x2"));
                }
                return s_StringBuilder.ToString();
            }
            catch (System.Exception e)
            {
                throw new System.Exception("GetFileMd5 failed, ERROR:" + e.Message);
            }
        }
        public static string GetFileMd5(string filePath)
        {
            var fileSize = 0L;
            return GetFileMd5(filePath, out fileSize);
        }
    }
}