﻿

using UnityEditor;
using UnityEngine;
using System.Text;
using System.Text.RegularExpressions;

namespace XBuild
{
    public class BuildParams
    {
        private static readonly Regex regex = new Regex(@"\{?(\w+[=|:]\w+,)*\w+[=|:]\w+\}?");
        private static readonly Regex s_IntRex = new Regex(@"\d+");

        private static StringBuilder sb = new StringBuilder();

        public BuildTarget target { get; internal set; }

        public string version;
        public string buildNumber;
        public string productName;
        public string applicationIdentifier;
        private string bundleVersionCode;
        public string appleDeveloperTeamID;
        private string fileNameFormatter;
        public string companyName;
        public string branch;

        public bool isDebug;
        public string startScene;

        public BuildParams()
        {
            productName = BuildConfig.productName;
            fileNameFormatter = BuildConfig.fileNameFormatter;
            applicationIdentifier = BuildConfig.applicationIdentifier;
            version = "0.1.0";
            buildNumber = "";
            appleDeveloperTeamID = BuildConfig.appleDeveloperTeamID;
            branch = "master";
            companyName = BuildConfig.companyName;

            isDebug = false;
            startScene = BuildConfig.startScenePath;
        }

        public override string ToString()
        {
            sb.Length = 0;
            sb.Append("BuildParams:\n");
            sb.AppendFormat("target={0}\n", target);
            sb.AppendFormat("productName={0}\n", productName);
            sb.AppendFormat("version={0}\n", version);
            sb.AppendFormat("buildNumber={0}\n", buildNumber);
            sb.AppendFormat("applicationIdentifier={0}\n", applicationIdentifier);
            sb.AppendFormat("bundleVersionCode={0}\n", bundleVersionCode);
            sb.AppendFormat("appleDeveloperTeamID={0}\n", appleDeveloperTeamID);
            sb.AppendFormat("fileNameFormatter={0}\n", fileNameFormatter);
            sb.AppendFormat("companyName={0}\n", companyName);
            sb.AppendFormat("branch={0}\n", branch);

            sb.AppendFormat("isDebug={0}\n", isDebug);
            sb.AppendFormat("startScene={0}\n", startScene);

            return sb.ToString();
        }

        public int GetVersionCode()
        {
            if (string.IsNullOrEmpty(bundleVersionCode))
            {
                return BuildHelper.ParseVersionCode(version);
            }
            else
            {
                return int.Parse(bundleVersionCode);
            }
        }

        public string GetFileName()
        {
            if (!string.IsNullOrEmpty(fileNameFormatter))
            {
                var fileName = fileNameFormatter;
                if (fileName.IndexOf("{time}") >= 0) fileName = fileName.Replace("{time}", BuildHelper.GetTimeString());
                if (fileName.IndexOf("{version}") >= 0) fileName = fileName.Replace("{version}", version);
                if (fileName.IndexOf("{branch}") >= 0) fileName = fileName.Replace("{branch}", branch);
                return fileName + BuildHelper.GetPackageFileNameExtention(target);
            }
            return productName + BuildHelper.GetPackageFileNameExtention(target);
        }

        public string GetLocationPathName()
        {
            if (target == BuildTarget.StandaloneWindows ||
                target == BuildTarget.iOS)
            {
                return productName;
            }
            else return GetFileName();
        }

        public static BuildParams Parse(string strParams)
        {
            var param = new BuildParams();
            if (string.IsNullOrEmpty(strParams)) return param;
            if (!regex.IsMatch(strParams)) return param;
            strParams = strParams.Replace('{', ' ');
            strParams = strParams.Replace('}', ' ');
            var alls = strParams.Split(',');
            foreach (var str in alls)
            {
                var subs = str.Split(new char[] { '=', ':' });
                if (subs == null || subs.Length != 2) continue;
                var key = subs[0].Trim();
                var value = subs[1].Trim();
                if (key.Equals("productName")) param.productName = value;
                else if (key.Equals("version")) param.version = value;
                else if (key.Equals("applicationIdentifier")) param.applicationIdentifier = value;
                else if (key.Equals("bundleVersionCode") && IsInt(value)) param.bundleVersionCode = value;
                else if (key.Equals("isDebug")) param.isDebug = ParseBool(value);
                else if (key.Equals("appleDeveloperTeamID")) param.appleDeveloperTeamID = value;
                else if (key.Equals("fileNameFormatter")) param.fileNameFormatter = value;
                else if (key.Equals("companyName")) param.companyName = value;
                else if (key.Equals("buildNumber") && IsInt(value)) param.buildNumber = value;
            }
            return param;
        }

        private static bool ParseBool(string value)
        {
            if (!bool.TryParse(value, out bool flag))
            {
                if (int.TryParse(value, out int test)) flag = test == 1;
            }
            return flag;
        }

        private static int ParseInt(string value)
        {
            if (int.TryParse(value, out int test)) return test;
            else return 0;
        }

        private static bool IsInt(string input)
        {
            return s_IntRex.IsMatch(input);
        }
    }
}

