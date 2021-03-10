
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using XCommon.Runtime;

namespace XRemoteDebug
{
    public class RemoteDebugClient : XMonoSingleton<RemoteDebugClient>
    {
        enum ClientState
        {
            Disconnected,
            Connecting,
            ConnectFailed,
            Connected
        }
        class Styles
        {
            public static GUIContent title = new GUIContent("RemoteDebug", "RemoteDebug");
            public static GUIContent connect = new GUIContent("Connect", "Connect to remote debug server");
            public static GUIContent connectAgain = new GUIContent("Connect Again", "Connect to remote debug server again");
            public static GUIContent disconnect = new GUIContent("Disconnect", "Disconnect with debug server");
        }
        private XSocket m_Client;
        private ClientState m_State = ClientState.Disconnected;
        private string m_ServerName = "";
        private List<byte[]> m_MsgList = new List<byte[]>();
        private string m_CurrentFolderPath;
        private string m_SearchText;

        private bool m_ReceiveFile;
        private string m_ReceiveFileName;
        private string m_ReceiveFileMd5;
        private int m_ReceiveFileSize;
        private string m_ReceiveFilePath;
        private FileStream m_ReceiveFileStream;
        private bool m_ShowServerList;
        private float m_LastHeartbeatTime;

        const string k_IP = "127.0.0.1";
        const int k_Port = 6666;

        public bool showEntry = true;

        private RemoteDebugClient()
        {
        }

        private string PatchFolder
        {
            get
            {
                if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    return string.Format("{0}/patch/", Application.persistentDataPath);
                }
                else
                {
                    return string.Format("{0}/patch/", Application.dataPath);
                }
            }
        }

        protected override void OnInit()
        {
            m_Client = new XSocket();
            m_Client.Name = "Client";
            m_Client.OnConnectCallback = OnConnect;
            m_Client.OnReceiveCallback = OnReceive;
            m_Client.OnCloseCallback = OnClose;
        }

        public void Connect(string ip, int port)
        {
            m_Client.Connect(ip, port);
        }

        public void SendInfo()
        {
            m_Client.Send(GetInfoMsg());
        }

        private void OnGUI()
        {
            if(!showEntry) return;
            var rect = RemoteDebugConfig.clientRect;
            switch (m_State)
            {
                case ClientState.Disconnected:
                    GUI_ServerList(rect);
                    break;
                case ClientState.Connecting:
                    GUI.Label(rect, "连接中...");
                    break;
                case ClientState.ConnectFailed:
                    GUI.Label(rect, "连接失败:" + m_ServerName);
                    rect.y += GUI.skin.font.fontSize + 5;
                    GUI_ServerList(rect);
                    break;
                case ClientState.Connected:
                    GUI.Label(rect, "已连接：" + m_ServerName);
                    rect.y += GUI.skin.font.fontSize + 5;
                    if (GUI.Button(rect, Styles.disconnect))
                    {
                        m_State = ClientState.Disconnected;
                        if (m_Client != null)
                        {
                            m_Client.Close();
                        }
                    }
                    break;
            }

        }

        private void GUI_ServerList(Rect rect)
        {
            if (GUI.Button(rect, Styles.connect))
            {
                m_ShowServerList = !m_ShowServerList;

            }
            if (m_ShowServerList)
            {
                m_ShowServerList = true;
                var count = 0;
                var port = RemoteDebugConfig.port;
                foreach (var info in RemoteDebugConfig.serverList)
                {
                    var btnRect = new Rect(rect.x + rect.width + 10, rect.y + (count++) * 32, rect.width, 30);
                    if (GUI.Button(btnRect, info.name))
                    {
                        ConnectServer(info.name, info.ip, port);
                    }
                }
            }
        }

        private void ConnectServer(string name, string ip, int port)
        {
            m_ServerName = name;
            m_State = ClientState.Connecting;
            m_Client.Connect(k_IP, k_Port);
            //SendInfo();
        }

        private void OnConnect(bool success, string error)
        {
            Debug.Log("DebugClient-OnConnect:" + success + "," + error);
            if (success)
            {
                m_Client.BeginReceive();
                m_State = ClientState.Connected;
                var msg = string.Format("{0}#0", (int)RemoteDebugMsg.ConnectServer);
                m_MsgList.Add(System.Text.Encoding.UTF8.GetBytes(msg));
            }
            else
            {
                m_State = ClientState.ConnectFailed;
                Debug.LogError("OnConnect ERROR:" + error);
            }
        }

        private void OnClose(XSocket socket)
        {
            m_State = ClientState.ConnectFailed;
            m_ShowServerList = false;
        }

        private void OnReceive(XSocket socket, byte[] buffer, int len)
        {
            var buf = new byte[len];
            for (int i = 0; i < len; i++) buf[i] = buffer[i];
            m_MsgList.Add(buf);
        }

        private void Update()
        {
            HandleMsg();
            HandleReceiveFile();
            Heartbeat();
        }

        private void Heartbeat()
        {
            if (m_State != ClientState.Connected) return;
            if (Time.time - m_LastHeartbeatTime > 1)
            {
                m_LastHeartbeatTime = Time.time;
                m_Client.Send(string.Format("{0}#{1}$", (int)RemoteDebugMsg.Heartbeat, 0));
            }
        }

        private void HandleMsg()
        {
            if (m_MsgList.Count == 0) return;
            var bytes = m_MsgList[0];
            m_MsgList.RemoveAt(0);
            if (bytes == null) return;
            if (m_ReceiveFile)
            {
                m_LastSpeedByte += bytes.Length;
                m_ReceiveFileStream.Write(bytes, 0, bytes.Length);
                m_ReceiveFileStream.Flush();
                return;
            }
            var content = System.Text.Encoding.UTF8.GetString(bytes);
            var temp = content.Split('#');
            if (temp.Length < 2)
            {
                Debug.LogError("Error msg:" + content);
                return;
            }
            var index = 0;
            if (!int.TryParse(temp[0], out index))
            {
                Debug.LogError("Error msg:" + content);
                return;
            }
            var msgType = (RemoteDebugMsg)index;
            var msgContent = temp[1];
            switch (msgType)
            {
                case RemoteDebugMsg.ConnectServer:
                    SendInfo();
                    break;
                case RemoteDebugMsg.Hierarchy_RootObjects:
                    HandleMsg_GetRootObjects(msgContent);
                    break;
                case RemoteDebugMsg.Hierarchy_SubObjects:
                    HandleMsg_GetSubObjects(msgContent);
                    break;
                case RemoteDebugMsg.Patch_RemoteFiles:
                    HandleMsg_Patch_GetFileList();
                    break;
                case RemoteDebugMsg.Patch_RemoteBack:
                    HandleMsg_Patch_RemoteBack();
                    break;
                case RemoteDebugMsg.Patch_RemoteOpenFolder:
                    HandleMsg_Patch_RemoteOpenFolder(msgContent);
                    break;
                case RemoteDebugMsg.Patch_RemoteSearch:
                    HandleMsg_Patch_RemoteSearch(msgContent);
                    break;
                case RemoteDebugMsg.Patch_RemoteDelete:
                    HandleMsg_Patch_RemoteDelete(msgContent);
                    break;
                case RemoteDebugMsg.Patch_LocalUploadStart:
                    HandleMsg_Patch_LocalUploadStart(msgContent);
                    break;
                default:
                    Debug.LogError("Unkown msg:" + msgType);
                    break;
            }
        }

        private float m_LastCheckTime = 0;
        private float m_LastSpeedTime = 0;
        private int m_LastSpeedByte = 0;
        private int m_ReceiveSpeed = 0;
        private void HandleReceiveFile()
        {
            if (!m_ReceiveFile) return;
            if (Time.time - m_LastSpeedTime >= 1)
            {
                m_LastSpeedTime = Time.time;
                m_ReceiveSpeed = m_LastSpeedByte;
                m_LastSpeedByte = 0;
            }
            if (Time.time - m_LastCheckTime > 0.1f)
            {
                m_LastCheckTime = Time.time;
                var currSize = RemoteDebugUtil.GetFileSize(m_ReceiveFilePath);
                if (currSize == m_ReceiveFileSize)
                {
                    m_ReceiveFileStream.Flush();
                    m_ReceiveFileStream.Close();
                    m_ReceiveFile = false;
                    var md5 = RemoteDebugUtil.GetFileMd5(m_ReceiveFilePath);
                    if (md5.Equals(m_ReceiveFileMd5))
                    {
                        HandleMsg_Patch_GetFileList();
                        m_Client.Send(string.Format("{0}#{1}|{2}$", (int)RemoteDebugMsg.Patch_LocalUploadEnd, m_ReceiveFileName, 0));
                    }
                    else
                    {
                        m_Client.Send(string.Format("{0}#{1}|{2}$", (int)RemoteDebugMsg.Patch_LocalUploadEnd, m_ReceiveFileName, 1));
                    }
                }
                else
                {
                    m_Client.Send(string.Format("{0}#{1}|{2}|{3}$",
                        (int)RemoteDebugMsg.Patch_LocalUpload, m_ReceiveFileName, currSize, m_ReceiveSpeed));
                }
            }
        }

        private string GetInfoMsg()
        {
            return string.Format("{0}#{1}|{2}|{3}$", (int)RemoteDebugMsg.BaseInfo, SystemInfo.deviceName, SystemInfo.operatingSystem,
                (int)Application.platform);
        }

        private void HandleMsg_GetRootObjects(string content)
        {
            var path = content;
            //Debug.LogError("HandlerMsg_GetRootObjects:" + path);
            var scene = SceneManager.GetActiveScene();
            foreach (var obj in scene.GetRootGameObjects())
            {
                m_Client.Send(string.Format("{0}#{1}|{2}|{3}$",
                    (int)RemoteDebugMsg.Hierarchy_RootObjects, scene.name, obj.name, obj.activeSelf));
                SyncSubObjects(obj.transform, obj.activeSelf);
            }
            foreach (var obj in gameObject.scene.GetRootGameObjects())
            {
                m_Client.Send(string.Format("{0}#{1}|{2}|{3}$",
                    (int)RemoteDebugMsg.Hierarchy_RootObjects, "DontDestroyOnLoad", obj.name, obj.activeSelf));
                SyncSubObjects(obj.transform, obj.activeSelf);
            }
        }

        private void SyncSubObjects(Transform go, bool parentActive)
        {
            if (go.childCount == 0) return;
            var path = GetTransformPath(go);
            for (int i = 0; i < go.childCount; i++)
            {
                var child = go.GetChild(i);
                var active = parentActive && child.gameObject.activeSelf;
                m_Client.Send(string.Format("{0}#{1}|{2}|{3}|{4}$",
                    (int)RemoteDebugMsg.Hierarchy_SubObjects, path, child.name, child.gameObject.activeSelf, active));
                SyncSubObjects(child, active);
            }
        }

        private string GetTransformPath(Transform go)
        {
            if (go.transform.parent != null)
            {
                return GetTransformPath(go.transform.parent) + "/" + go.name;
            }
            else
            {
                return go.name;
            }
        }

        private void HandleMsg_GetSubObjects(string content)
        {
            var obj = GameObject.Find(content);
        }

        private void HandleMsg_Patch_GetFileList()
        {
            if (string.IsNullOrEmpty(m_CurrentFolderPath))
            {
                m_CurrentFolderPath = PatchFolder;
                if (!Directory.Exists(m_CurrentFolderPath))
                {
                    m_CurrentFolderPath = RemoteDebugUtil.GetParentPath(PatchFolder);
                }
            }
            if (!Directory.Exists(m_CurrentFolderPath))
            {
                Debug.LogError("not exist folder:" + m_CurrentFolderPath);
                return;
            }
            m_Client.Send(string.Format("{0}#{1}$",
                (int)RemoteDebugMsg.Patch_RemoteCurrentFolder,
                m_CurrentFolderPath
            ));
            var parent = RemoteDebugUtil.GetParentPath(m_CurrentFolderPath);

            m_Client.Send(string.Format("{0}#{1}|{2}|{3}|{4}$",
                (int)RemoteDebugMsg.Patch_RemoteFiles,
                0,
                "..",
                0,
                RemoteDebugUtil.GetFileLastWriteTime(parent)
            ));
            foreach (var path in Directory.GetDirectories(m_CurrentFolderPath))
            {
                var folder = Path.GetFileName(path);
                if (!IsSearchFileName(folder)) continue;
                m_Client.Send(string.Format("{0}#{1}|{2}|{3}|{4}$",
                    (int)RemoteDebugMsg.Patch_RemoteFiles,
                    1,
                    folder,
                    0,
                    RemoteDebugUtil.GetFileLastWriteTime(path)
                ));
            }
            foreach (var path in Directory.GetFiles(m_CurrentFolderPath))
            {
                var fileName = Path.GetFileName(path);
                if (!IsSearchFileName(fileName)) continue;
                var fi = new FileInfo(path);
                m_Client.Send(string.Format("{0}#{1}|{2}|{3}|{4}$",
                    (int)RemoteDebugMsg.Patch_RemoteFiles,
                    2,
                    fileName,
                    fi.Length,
                    fi.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss")
                ));
            }
        }

        private void HandleMsg_Patch_RemoteBack()
        {
            m_CurrentFolderPath = RemoteDebugUtil.GetParentPath(m_CurrentFolderPath);
            HandleMsg_Patch_GetFileList();
        }
        private void HandleMsg_Patch_RemoteOpenFolder(string content)
        {
            if (!m_CurrentFolderPath.EndsWith(content))
                m_CurrentFolderPath = m_CurrentFolderPath + "/" + content;
            if (m_CurrentFolderPath.EndsWith("/"))
                m_CurrentFolderPath = m_CurrentFolderPath.Substring(0, m_CurrentFolderPath.Length - 2);
            HandleMsg_Patch_GetFileList();
        }
        private void HandleMsg_Patch_RemoteSearch(string content)
        {
            m_SearchText = content;
            HandleMsg_Patch_GetFileList();
        }

        private void HandleMsg_Patch_RemoteDelete(string content)
        {
            var fileNames = content.Split('|');
            foreach (var name in fileNames)
            {
                var path = m_CurrentFolderPath + "/" + name;
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                else
                {
                    SendError("File no exist:" + name);
                }
            }
            HandleMsg_Patch_GetFileList();
        }
        private void HandleMsg_Patch_LocalUploadStart(string content)
        {
            var temp = content.Split('|');
            m_ReceiveFileName = temp[0];
            m_ReceiveFileSize = int.Parse(temp[1]);
            m_ReceiveFileMd5 = temp[2];
            m_ReceiveFile = true;
            m_ReceiveFilePath = m_CurrentFolderPath + "/" + m_ReceiveFileName;
            m_ReceiveFileStream = new FileStream(m_ReceiveFilePath, FileMode.Create);
        }

        private bool IsSearchFileName(string fileName)
        {
            return string.IsNullOrEmpty(m_SearchText) ? true : fileName.Contains(m_SearchText);
        }

        private void SendError(string msg)
        {
            m_Client.Send(string.Format("{0}#{1}$", (int)RemoteDebugMsg.Error, msg));
        }
    }
}