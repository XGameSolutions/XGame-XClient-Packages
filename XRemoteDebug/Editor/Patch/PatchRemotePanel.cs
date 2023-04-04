using UnityEngine;
using UnityEditor;
using XCommon.Editor;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace XRemoteDebug
{
    internal class PatchRemotePanel : ITianGlyphPanel
    {
        class Styles
        {
            public static readonly GUIContent btnBack = new GUIContent("Back", "Back to previous remote client folder");
            public static readonly GUIContent btnRefresh = new GUIContent("Refresh", "Refresh folder");
            public static readonly GUIContent btnDelete = new GUIContent("Delete", "Delete all selected files");
            public static readonly GUIContent btnCancel = new GUIContent("Cancel", "Cancel all selected files");
            public static readonly GUIContent iconBack = EditorGUIUtility.IconContent("back@2x", "back to previous folder");
            public static readonly Texture iconFilter = EditorGUIUtility.IconContent("d_FilterByType@2x").image;
        }

        private PatchPanel m_Parent;
        private XEditorTable m_FileTree;
        private string m_RemotePath;
        private SearchField m_SearchField;
        private string m_SearchText;
        private List<PatchFileInfo> m_SelectedFileInfos = new List<PatchFileInfo>();

        private RemoteDebugServer server
        {
            get
            {
                return RemoteDebugWindow.Instance.server;
            }
        }

        private int clientIndex
        {
            get
            {
                return RemoteDebugWindow.Instance.selectedClient == null ? -1 : RemoteDebugWindow.Instance.selectedClient.Index;
            }
        }
        private RemoteDebugClientInfo client
        {
            get
            {
                return RemoteDebugWindow.Instance.selectedClient;
            }
        }

        public PatchRemotePanel()
        {
        }

        public void OnEnable(PatchPanel parent)
        {
            m_Parent = parent;
            if (m_SearchField == null) m_SearchField = new SearchField();
            server?.RequestPatchFiles(clientIndex);
        }

        public void Update()
        {
            RefreshFileList();
        }

        public void Repaint()
        {
            m_Parent.Repaint();
        }

        public void OnGUI(Rect rect)
        {
            var gap = 0.5f;
            var hig = 18;

            OnGUI_Path(new Rect(rect.x, rect.y, rect.width, hig));
            OnGUI_Menu(new Rect(rect.x, rect.y + hig + gap, rect.width, hig));
            OnGUI_Search(new Rect(rect.x, rect.y + 2 * (hig + gap), rect.width, hig));
            OnGUI_Table(new Rect(rect.x, rect.y + 3 * (hig + gap), rect.width,
                rect.height - 3 * (hig + gap)));
        }

        public void Reload()
        {
        }

        private void OnGUI_Path(Rect rect)
        {
            GUI.Label(rect, client != null ? client.remoteCurrentFolder : "");
        }

        private void OnGUI_Menu(Rect rect)
        {
            var btnGap = 1;
            var btnWid = 53;
            var index = 0;

            if (GUI.Button(new Rect(rect.x + (index++) * (btnWid + btnGap), rect.y, btnWid, rect.height), Styles.btnBack))
            {
                server?.RequestPatchBack(clientIndex);
            }
            if (GUI.Button(new Rect(rect.x + (index++) * (btnWid + btnGap), rect.y, btnWid, rect.height), Styles.btnRefresh))
            {
                server?.RequestPatchFiles(clientIndex);
            }
            if (GUI.Button(new Rect(rect.x + (index++) * (btnWid + btnGap), rect.y, btnWid, rect.height), Styles.btnDelete))
            {
                    if (m_SelectedFileInfos.Count <= 0)
                    {
                        RemoteDebugWindow.Instance.ShowNotification(new GUIContent("no selected files."));
                    }
                    else
                    {
                        var files = string.Join("\n", m_SelectedFileInfos);
                        if (EditorUtility.DisplayDialog("Delete selected files", "Delete all selected files?\n"+files, "Sure", "Cancel"))
                        {
                            server?.RequestPatchDelete(clientIndex,string.Join("|", m_SelectedFileInfos));
                        }
                    }
            }
            if (GUI.Button(new Rect(rect.x + (index++) * (btnWid + btnGap), rect.y, btnWid, rect.height), Styles.btnCancel))
            {
                if (client != null)
                {
                    foreach (var info in client.remotePatchFileList) info.itemSelected = false;
                    m_FileTree.Repaint();
                }
            }
        }

        private void OnGUI_Search(Rect rect)
        {
            var newSearch = m_SearchField.OnGUI(rect, m_SearchText);
            if (string.IsNullOrEmpty(m_SearchText))
            {
                if (!string.IsNullOrEmpty(newSearch))
                {
                    m_SearchText = newSearch;
                    server?.RequestPatchSearch(clientIndex, m_SearchText);
                }
            }
            else if (!m_SearchText.Equals(newSearch))
            {
                m_SearchText = newSearch;
                server?.RequestPatchSearch(clientIndex, m_SearchText);
            }
        }

        private void OnGUI_Table(Rect rect)
        {
            if (m_FileTree == null)
            {
                var column = PatchFileInfo.totalColumn;
                m_FileTree = XEditorTable.CreateTable(column);
                for (int i = 0; i < column; i++)
                {
                    m_FileTree.SetColumnHeader(i, PatchFileInfo.GetColumnHeader(i));
                }
                m_FileTree.OnSelectionChanged = OnSelectedFileList;
                m_FileTree.OnDoubleClickedItem = OnDoubleClickedItem;
                RefreshFileList();
                m_FileTree.Reload();
            }
            m_FileTree.OnGUI(rect);
        }

        private void OnSelectedFileList(List<XIEditorTableItemInfo> infoList)
        {
            
            m_SelectedFileInfos.Clear();
            if (infoList != null)
            {
                foreach (var info in infoList)
                {
                    var fileInfo = info as PatchFileInfo;
                    if (fileInfo.type == 2)
                    {
                        m_SelectedFileInfos.Add(fileInfo);
                    }
                }
            }
        }

        private void OnDoubleClickedItem(XIEditorTableItemInfo item)
        {
            var info = item as PatchFileInfo;
            if (info != null)
            {
                switch (info.type)
                {
                    case 0:
                        server?.RequestPatchBack(clientIndex);
                        break;
                    case 1:
                        server?.RequestPatchOpenFolder(clientIndex, info.name);
                        break;
                    case 2:
                        //info.itemSelected = !info.itemSelected;
                        break;
                }
            }
        }

        private void RefreshFileList()
        {
            var client = RemoteDebugWindow.Instance.selectedClient;
            if (client == null) return;
            if (!RemoteDebugWindow.Instance.server.IsPatchFileListDirty()) return;
            m_FileTree.UpdateInfoList(client.remotePatchFileList);
        }
    }
}