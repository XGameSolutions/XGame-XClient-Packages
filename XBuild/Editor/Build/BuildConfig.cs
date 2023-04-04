

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace XBuild
{
    [Serializable]
    [ExcludeFromPresetAttribute]
    public class BuildConfig : ScriptableObject
    {
        public const string configName = "BuildConfig";
        public const string configPath = "Assets/XPlugins/XBuild/Editor/Resources/BuildConfig.asset";
        [SerializeField] private string m_CodeName = "xgame";
        [SerializeField] private string m_ABDir = "ResAB/";
        [SerializeField] private string m_ABDirRelativeToDataPath = "/../ResAB/";
        [SerializeField] private string m_StartScenePath = "Assets/Plugins/start.unity";
        [SerializeField] private string m_StartSceneInitObjectPath = "init";
        [SerializeField] private string m_ProductName = "xgame";
        [SerializeField] private string m_CompanyName = "monitor1394";
        [SerializeField] private string m_FileNameFormatter = "xgame_{time}_{version}_{branch}";
        [SerializeField] private string m_ApplicationIdentifier = "com.monitor1394.xgame";
        [SerializeField] private string m_AppleDeveloperTeamID = "";

        public static string codeName { get { return Instance.m_CodeName; } }
        public static string abDir { get { return Instance.m_ABDir; } }
        public static string abDirRelativeToDataPath { get { return Instance.m_ABDirRelativeToDataPath; } }
        public static string startScenePath { get { return Instance.m_StartScenePath; } }
        public static string startSceneInitObjectPath { get { return Instance.m_StartSceneInitObjectPath; } }
        public static string productName { get { return Instance.m_ProductName; } }
        public static string companyName { get { return Instance.m_CompanyName; } }
        public static string fileNameFormatter { get { return Instance.m_FileNameFormatter; } }
        public static string applicationIdentifier { get { return Instance.m_ApplicationIdentifier; } }
        public static string appleDeveloperTeamID { get { return Instance.m_AppleDeveloperTeamID; } }



        private static BuildConfig s_Instance;
        private static BuildConfig Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = Resources.Load<BuildConfig>(configName);
                    if (s_Instance == null)
                    {
                        s_Instance = ScriptableObject.CreateInstance(configName) as BuildConfig;
                        CommonUtil.CheckAndCreateDir(Application.dataPath, "XPlugins/XBuild/Editor/Resources/");
                        AssetDatabase.CreateAsset(s_Instance, BuildConfig.configPath);
                    }
                }
                return s_Instance;
            }
        }
    }
}