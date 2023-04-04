using UnityEngine;
using XCommon.Editor;
using System.Collections.Generic;

namespace XRemoteDebug
{
    internal class HierarchyPanel : IRemoteDebugPanel
    {
        private RemoteDebugWindow m_Window;
        private XEditorTable m_ClientObjectsTree;

        public HierarchyPanel(RemoteDebugWindow window)
        {
            m_Window = window;
        }

        public ITianGlyphPanel GetStatusPanel()
        {
            return null;
        }

        public void OnEnable()
        {
            var client = RemoteDebugWindow.Instance.selectedClient;
            if (client == null) return;
            RemoteDebugWindow.Instance.server.RequestRootObjects(client.Index);
        }

        public void Update()
        {
            if (m_ClientObjectsTree != null && m_Window.selectedClient != null)
            {
                if (m_Window.server.IsClientObjectsDirty())
                {
                    m_ClientObjectsTree.UpdateInfoList(m_Window.server.GetClientObjectsInfos(m_Window.selectedClient.Index));
                }
            }
        }

        public void OnGUI(Rect rect)
        {
            InitClientObjectsTable();
            m_ClientObjectsTree.OnGUI(rect);
        }

        public void Reload()
        {
        }

        private void InitClientObjectsTable()
        {
            if (m_ClientObjectsTree != null) return;
            var column = HierarchyItemInfo.totalColumn;
            m_ClientObjectsTree = XEditorTable.CreateTable(column);
            for (int i = 0; i < column; i++)
            {
                m_ClientObjectsTree.SetColumnHeader(i, HierarchyItemInfo.GetColumnHeader(i));
            }
            m_ClientObjectsTree.OnSelectionChanged = OnSelectedClientObjectList;
            m_ClientObjectsTree.Reload();
        }

        private void OnSelectedClientObjectList(List<XIEditorTableItemInfo> infoList)
        {
            if (infoList.Count > 0)
            {
                // m_SelectedClient = infoList[0] as ClientInfo;
                //m_Server.RequestRootObjects(m_SelectedClient.Index);
            }
            else
            {
                //m_SelectedClient = null;
            }
        }
    }
}