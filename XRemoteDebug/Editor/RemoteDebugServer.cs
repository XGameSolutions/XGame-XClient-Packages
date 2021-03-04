
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XCommon.Runtime;

namespace XRemoteDebug
{
    internal class RemoteDebugServer
    {
        private class IconStyles
        {
            public static readonly Texture2D iconGameObject = EditorGUIUtility.IconContent("GameObject Icon").image as Texture2D;
            public static readonly Texture2D iconGameObjectOn = EditorGUIUtility.IconContent("d_GameObject Icon").image as Texture2D;
        }
        private XSocketServer<RemoteDebugClientInfo> m_Server;
        private Dictionary<int, RemoteDebugClientInfo> m_ClientDic = new Dictionary<int, RemoteDebugClientInfo>();
        private Dictionary<int, string> m_ClientUnhandledMsgDic = new Dictionary<int, string>();
        private bool m_IsClientDirty;
        private bool m_IsClientObjectsDirty;
        private bool m_IsPatchFileListDirty;

        public RemoteDebugServer(string ip, int port)
        {
            m_Server = new XSocketServer<RemoteDebugClientInfo>(ip, port);
            m_Server.OnClientConnectCallback = OnClientConnect;
            m_Server.OnClientCloseCallback = OnClientClose;
            m_Server.OnClientReceiveCallback = OnClientReceive;
        }

        public void Start()
        {
            m_Server.Start();
        }

        public void Close()
        {
            m_Server.Close();
        }

        public bool IsClientDirty()
        {
            if (m_IsClientDirty)
            {
                m_IsClientDirty = false;
                return true;
            }
            return false;
        }
        public bool IsClientObjectsDirty()
        {
            if (m_IsClientObjectsDirty)
            {
                m_IsClientObjectsDirty = false;
                return true;
            }
            return false;
        }
        public bool IsPatchFileListDirty()
        {
            if (m_IsPatchFileListDirty)
            {
                m_IsPatchFileListDirty = false;
                return true;
            }
            return false;
        }

        public List<RemoteDebugClientInfo> GetClientList()
        {
            return m_ClientDic.Values.ToList();
        }

        public List<HierarchyItemInfo> GetClientObjectsInfos(int clientIndex)
        {
            return m_ClientDic[clientIndex].objectList;
        }

        public void RequestRootObjects(int clientIndex)
        {
            if (!m_ClientDic.ContainsKey(clientIndex)) return;
            m_ClientDic[clientIndex].objectDic.Clear();
            m_ClientDic[clientIndex].objectList.Clear();
            m_Server.SendToClient(clientIndex, string.Format("{0}#", (int)RemoteDebugMsg.Hierarchy_RootObjects));
            m_IsClientObjectsDirty = true;
        }

        public void RequestSubObjects(int clientIndex, string path)
        {
            if (!m_ClientDic.ContainsKey(clientIndex)) return;
            m_Server.SendToClient(clientIndex, string.Format("{0}#{1}", (int)RemoteDebugMsg.Hierarchy_SubObjects, path));
        }
        public void RequestPatchFiles(int clientIndex)
        {
            if (!m_ClientDic.ContainsKey(clientIndex)) return;
            m_ClientDic[clientIndex].remotePatchFileList.Clear();
            m_IsPatchFileListDirty = true;
            m_Server.SendToClient(clientIndex, string.Format("{0}#", (int)RemoteDebugMsg.Patch_RemoteFiles));
        }
        public void RequestPatchBack(int clientIndex)
        {
            if (!m_ClientDic.ContainsKey(clientIndex)) return;
            m_ClientDic[clientIndex].remotePatchFileList.Clear();
            m_IsPatchFileListDirty = true;
            m_Server.SendToClient(clientIndex, string.Format("{0}#", (int)RemoteDebugMsg.Patch_RemoteBack));
        }
        public void RequestPatchDelete(int clientIndex, string deleteFileNames)
        {
            if (!m_ClientDic.ContainsKey(clientIndex)) return;
            m_ClientDic[clientIndex].remotePatchFileList.Clear();
            m_IsPatchFileListDirty = true;
            m_Server.SendToClient(clientIndex, string.Format("{0}#{1}", (int)RemoteDebugMsg.Patch_RemoteDelete, deleteFileNames));
        }
        public void RequestPatchOpenFolder(int clientIndex, string folder)
        {
            if (!m_ClientDic.ContainsKey(clientIndex)) return;
            m_ClientDic[clientIndex].remotePatchFileList.Clear();
            m_IsPatchFileListDirty = true;
            m_Server.SendToClient(clientIndex, string.Format("{0}#{1}", (int)RemoteDebugMsg.Patch_RemoteOpenFolder, folder));
        }

        public void RequestPatchSearch(int clientIndex, string search)
        {
            if (!m_ClientDic.ContainsKey(clientIndex)) return;
            m_ClientDic[clientIndex].remotePatchFileList.Clear();
            m_IsPatchFileListDirty = true;
            m_Server.SendToClient(clientIndex, string.Format("{0}#{1}", (int)RemoteDebugMsg.Patch_RemoteSearch, search));
        }
        public void RequestPatchUploadStart(int clientIndex, string fileName, int length, string md5)
        {
            if (!m_ClientDic.ContainsKey(clientIndex)) return;
            m_Server.SendToClient(clientIndex, string.Format("{0}#{1}|{2}|{3}", (int)RemoteDebugMsg.Patch_LocalUploadStart,
                fileName, length, md5));
        }

        public void RequestPatchUploadFile(int clientIndex, string filePath)
        {
            if (!m_ClientDic.ContainsKey(clientIndex)) return;
            m_Server.SendFileToClient(clientIndex, filePath, RemoteDebugConfig.socketUploadFileBufferSize);
        }

        public void Update()
        {
            foreach (var client in m_ClientDic.Values)
            {
                if (client.msgList.Count > 0)
                {
                    foreach (var msg in client.msgList)
                    {
                        var dealMsg = client.unhandledMsg + msg;
                        var temp = dealMsg.Split('$');
                        if (!string.IsNullOrEmpty(temp[temp.Length - 1])) client.unhandledMsg = temp[temp.Length - 1];
                        else client.unhandledMsg = "";
                        for (int i = 0; i < temp.Length - 1; i++)
                        {
                            HandleMsg(client, temp[i]);
                        }
                    }
                    client.msgList.Clear();
                }
            }
        }
        private void OnClientConnect(RemoteDebugClientInfo client)
        {
            Debug.Log("DebugServer:OnClientConnect--" + client);

            m_ClientDic[client.Index] = client;
            m_IsClientDirty = true;
        }

        private void OnClientClose(int index)
        {
            Debug.Log("DebugServer:OnClientClose--" + index);
            m_ClientDic.Remove(index);
            m_IsClientDirty = true;
        }

        private void OnClientReceive(int index, string content)
        {
            if (!m_ClientDic.ContainsKey(index))
            {
                Debug.LogError("Unkown client:" + index);
                return;
            }
            m_ClientDic[index].msgList.Add(content);
        }

        private void HandleMsg(RemoteDebugClientInfo client, string msg)
        {
            //Debug.Log("HandleMsg:" + msg);
            var temp = msg.Split('#');
            if (temp.Length < 2)
            {
                Debug.LogError("Error msg:" + msg);
                return;
            }
            var msgIndex = 0;
            if (!int.TryParse(temp[0], out msgIndex))
            {
                Debug.LogError("Error msg:" + msg);
                return;
            }
            var msgType = (RemoteDebugMsg)msgIndex;
            var msgContent = temp[1];
            switch (msgType)
            {
                case RemoteDebugMsg.Error:
                    RemoteDebugWindow.Instance.ShowNotification(new GUIContent(msgContent));
                    Debug.LogError("ERROR:" + msgContent);
                    break;
                case RemoteDebugMsg.BaseInfo:
                    MsgInfo(client, msgContent);
                    break;
                case RemoteDebugMsg.Hierarchy_RootObjects:
                    MsgRootObjects(client, msgContent);
                    break;
                case RemoteDebugMsg.Hierarchy_SubObjects:
                    MsgSubObjects(client, msgContent);
                    break;
                case RemoteDebugMsg.Patch_RemoteFiles:
                    MsgPathFileList(client, msgContent);
                    break;
                case RemoteDebugMsg.Patch_RemoteCurrentFolder:
                    MsgPatchCurrentFolder(client, msgContent);
                    break;
                case RemoteDebugMsg.Patch_LocalUpload:
                    MsgPatchUpload(client, msgContent);
                    break;
                case RemoteDebugMsg.Patch_LocalUploadEnd:
                    MsgPatchUploadEnd(client, msgContent);
                    break;
                default:
                    Debug.LogError("Unhandle msg:" + msgType);
                    break;
            }
        }

        private void MsgInfo(RemoteDebugClientInfo client, string info)
        {
            // Debug.Log("MsgInfo:" + info);
            var temp = info.Split('|');
            var os = temp[1];
            var platform = (RuntimePlatform)int.Parse(temp[2]);
            client.name = temp[0];
            switch (platform)
            {
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.WindowsEditor:
                    client.assetIcon = EditorGUIUtility.FindTexture("BuildSettings.Standalone");
                    break;
                case RuntimePlatform.Android:
                    client.assetIcon = EditorGUIUtility.FindTexture("BuildSettings.Android");
                    break;
                case RuntimePlatform.IPhonePlayer:
                    client.assetIcon = EditorGUIUtility.FindTexture("BuildSettings.iPhone");
                    break;
            }

            m_IsClientDirty = true;
        }

        private void MsgRootObjects(RemoteDebugClientInfo client, string info)
        {
            //Debug.Log("MsgRootObjects:" + info);
            var temp = info.Split('|');
            var root = temp[0];
            var name = temp[1];
            var active = bool.Parse(temp[2]);
            if (!client.objectDic.ContainsKey(root))
            {
                var objInfo = new HierarchyItemInfo(root, root);
                objInfo.assetIcon = EditorGUIUtility.FindTexture("BuildSettings.Editor.Small");
                client.objectDic[root] = objInfo;
                client.objectList.Add(objInfo);
            }
            var chdInfo = new HierarchyItemInfo(name, name);
            chdInfo.itemDisable = !active;
            chdInfo.assetIcon = IconStyles.iconGameObject;
            chdInfo.assetDisableIcon = IconStyles.iconGameObjectOn;
            client.objectDic[name] = chdInfo;
            client.objectDic[root].children.Add(chdInfo);
            m_IsClientObjectsDirty = true;
        }

        private void MsgSubObjects(RemoteDebugClientInfo client, string content)
        {
            var temp = content.Split('|');
            var path = temp[0];
            var name = temp[1];
            var active = bool.Parse(temp[2]);
            //Debug.Log("MsgSubObjects:" + path + "," + name);
            if (!client.objectDic.ContainsKey(path))
            {
                var parent = new HierarchyItemInfo(path, path);
                parent.assetIcon = IconStyles.iconGameObject;
                parent.assetDisableIcon = IconStyles.iconGameObjectOn;
                client.objectDic[path] = parent;
            }
            var child = new HierarchyItemInfo(name, path + "/" + name);
            child.itemDisable = !active;
            child.assetIcon = IconStyles.iconGameObject;
            child.assetDisableIcon = IconStyles.iconGameObjectOn;
            client.objectDic[child.path] = child;
            client.objectDic[path].children.Add(child);
        }

        private void MsgPathFileList(RemoteDebugClientInfo client, string content)
        {
            var temp = content.Split('|');
            var type = int.Parse(temp[0]);
            var name = temp[1];
            var size = int.Parse(temp[2]);
            var time = temp[3];
            client.remotePatchFileList.Add(new PatchFileInfo()
            {
                type = type,
                name = name,
                size = size,
                datetime = time,
                assetIcon = type == 1 ? RemoteDebugStyles.iconFolder : RemoteDebugStyles.iconFile
            });
            m_IsPatchFileListDirty = true;
        }
        private void MsgPatchCurrentFolder(RemoteDebugClientInfo client, string content)
        {
            client.remoteCurrentFolder = content;
            client.remotePatchFileList.Clear();
            m_IsPatchFileListDirty = true;
        }

        private void MsgPatchUploadEnd(RemoteDebugClientInfo client, string content)
        {
            var temp = content.Split('|');
            var fileName = temp[0];
            var flag = temp[1] == "0";
            RemoteDebugWindow.Instance.UploadFileSuccess(flag, fileName);
        }

        private void MsgPatchUpload(RemoteDebugClientInfo client, string content)
        {
            var temp = content.Split('|');
            var fileName = temp[0];
            var fileSize = int.Parse(temp[1]);
            var speed = int.Parse(temp[2]);
            RemoteDebugWindow.Instance.FileUploading(fileName, fileSize, speed);
        }
    }
}