
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using XCommon.Editor;
using XCommon.Runtime;

namespace XRemoteDebug
{
    internal class HierarchyItemInfo : XSocket, IEditorTableItemInfo
    {
        public HierarchyItemInfo(string name, string path)
        {
            m_Name = name;
            m_Path = path;
            children = new List<IEditorTableItemInfo>();
        }
        private string m_Name;
        private string m_Path;

        public string name
        {
            get { return string.IsNullOrEmpty(m_Name) ? "<new client>" : m_Name; }
            set { m_Name = value; }
        }
        public string path { get { return m_Path; } }

        #region IEditorTableItemInfo
        public string displayName { get { return name; } }
        public int itemId { get { return m_Path.GetHashCode(); } }
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
                case 0: return TianGlyphUtil.GetColumn(500, 150, 1000, "Objects", "");
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