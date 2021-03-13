/******************************************/
/*                                        */
/*     Copyright (c) 2020 monitor1394     */
/*     https://github.com/monitor1394     */
/*                                        */
/******************************************/

using System;
using System.IO;
using UnityEngine;
using XCommon.Runtime;

namespace XRemoteDebug
{
    [Serializable]
    public class ServerInfo
    {
        [SerializeField] public string name;
        [SerializeField] public string ip;
        public ServerInfo(string name, string ip)
        {
            this.name = name;
            this.ip = ip;
        }
    }

    [Serializable]
    [ExcludeFromPresetAttribute]
    public class RemoteDebugConfig : ScriptableObject
    {

        public const string configName = "RemoteDebugConfig";
        public const string configPath = "Assets/XPlugins/XRemoteDebug/Resources/RemoteDebugConfig.asset";
        private static RemoteDebugConfig s_Instance;
        private static RemoteDebugConfig Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = Resources.Load<RemoteDebugConfig>(configName);
                    if (s_Instance == null)
                    {
                        s_Instance = ScriptableObject.CreateInstance(configName) as RemoteDebugConfig;
#if UNITY_EDITOR
                        XFileUtil.CheckAndCreateDir(Application.dataPath, "XPlugins/XRemoteDebug/Resources");
                        UnityEditor.AssetDatabase.CreateAsset(s_Instance, RemoteDebugConfig.configPath);
#endif
                    }
                }
                return s_Instance;
            }
        }


        [SerializeField] private float m_ClientX = 10;
        [SerializeField] private float m_ClientY = 10;
        [SerializeField] private float m_ClientWidth = 100;
        [SerializeField] private float m_ClientHeight = 50;
        [SerializeField] private int m_SocketUploadFileBufferSize = 1024 * 1024;


        [SerializeField] private int m_Port = 6666;
        [SerializeField]
        private ServerInfo[] m_ServerInfos = new ServerInfo[] {
            new ServerInfo("本地服","127.0.0.1"),
        };




        public static Rect clientRect { get { return new Rect(Instance.m_ClientX, Instance.m_ClientY, Instance.m_ClientWidth, Instance.m_ClientHeight); } }
        public static float clientHeight { get { return Instance.m_ClientHeight; } }
        public static int port { get { return Instance.m_Port; } }
        public static ServerInfo[] serverList { get { return Instance.m_ServerInfos; } }
        public static int socketUploadFileBufferSize { get { return Instance.m_SocketUploadFileBufferSize; } }

    }
}