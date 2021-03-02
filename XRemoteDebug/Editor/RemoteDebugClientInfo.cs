
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using XCommon.Editor;
using XCommon.Runtime;

namespace XRemoteDebug
{
    internal class RemoteDebugClientInfo : XSocket, IEditorTableItemInfo
    {
        private string m_Name;

        public string name
        {
            get { return string.IsNullOrEmpty(m_Name) ? "<new client>" : m_Name; }
            set { m_Name = value; }
        }
        #region Msg
        public List<string> msgList = new List<string>();
        public string unhandledMsg = "";

        #endregion

        #region GameInfo
        public List<HierarchyItemInfo> objectList = new List<HierarchyItemInfo>();
        public Dictionary<string, HierarchyItemInfo> objectDic = new Dictionary<string, HierarchyItemInfo>();
        public List<PatchFileInfo> remotePatchFileList = new List<PatchFileInfo>();
        public string remoteCurrentFolder;
        #endregion

        #region IEditorTableItemInfo
        public string displayName { get { return name; } }
        public int itemId { get { return name.GetHashCode(); } }
        public bool itemDisable { get; set; }
        public bool itemSelected { get; set; }
        public string assetPath { get; set; }
        public Texture2D assetIcon { get; set; }
        public Texture2D assetDisableIcon { get; set; }
        public List<IEditorTableItemInfo> children { get; set; }

        public static int totalColumn { get { return 1; } }
        public static MultiColumnHeaderState.Column GetColumnHeader(int column)
        {
            switch (column)
            {
                case 0: return TianGlyphUtil.GetColumn(200, 150, 300, "Client", "");
                default: return TianGlyphUtil.GetColumn(75, 50, 100, "Unknow", "");
            }
        }

        public string GetColumnString(int column)
        {
            switch (column)
            {
                case 0: return name;
                default: return "unkown:" + column;
            }
        }
        public object GetColumnOrder(int column)
        {
            switch (column)
            {
                case 0: return name;
                default: return name;
            }
        }
        #endregion
    }
}