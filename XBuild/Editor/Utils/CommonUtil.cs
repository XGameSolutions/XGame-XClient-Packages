
using System.IO;

namespace XBuild
{
    public static class CommonUtil
    {
        public static void CheckAndCreateDir(string rootPath, string subPath)
        {
            if (!rootPath.EndsWith("/")) rootPath += "/";
            var list = subPath.Split('/');
            var path = rootPath;
            for (int i = 0; i < list.Length; i++)
            {
                if (!string.IsNullOrEmpty(list[i]))
                {
                    path += "/" + list[i];
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
            }
        }
    }
}