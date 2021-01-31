

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using XTianGlyph;

namespace XBuild.AB.ABBrowser
{
    public enum ABType
    {
        All,
        Model,
        Scene,
        UI,
        Other,
        Dep
    }

    public class ABInfo : IEditorTableItemInfo
    {
        public ABType type;
        public string name;
        public long size;
        public long depSize;
        private List<string> m_DepABList = new List<string>();
        private List<string> m_RefABList = new List<string>();

        public List<string> RefABList { get { return m_RefABList; } }
        public List<string> DepABList { get { return m_DepABList; } }

        public bool AddDepAB(string abName)
        {
            if (!m_DepABList.Contains(abName))
            {
                m_DepABList.Add(abName);
                return true;
            }
            return false;
        }

        public bool AddRefAB(string abName)
        {
            if (!m_RefABList.Contains(abName))
            {
                m_RefABList.Add(abName);
                return true;
            }
            return false;
        }

        public string GetSizeStr()
        {
            return size == 0 ? "--" : EditorUtility.FormatBytes(size);
        }
        public string GetDepSizeStr()
        {
            return size == 0 ? "--" : EditorUtility.FormatBytes(depSize);
        }
        public string GetTotalSizeStr()
        {
            return size == 0 ? "--" : EditorUtility.FormatBytes(totalSize);
        }

        public long totalSize { get { return size + depSize; } }
        public int depCount { get { return m_DepABList.Count; } }
        public int refCount { get { return m_RefABList.Count; } }

        public string displayName { get { return name; } }
        public int itemId { get { return name.GetHashCode(); } }
        public string assetPath { get; set; }

        public static int totalColumn { get { return 5; } }
        public static MultiColumnHeaderState.Column GetColumnHeader(int column)
        {
            switch (column)
            {
                case 0: return TianGlyphUtil.GetColumn(200, 50, 400, "AB", "");
                case 1: return TianGlyphUtil.GetColumn(75, 50, 100, "Size", "");
                case 2: return TianGlyphUtil.GetColumn(40, 20, 50, "Ref", "");
                case 3: return TianGlyphUtil.GetColumn(40, 20, 50, "Def", "");
                case 4: return TianGlyphUtil.GetColumn(70, 40, 100, "Dep Size", "");
                case 5: return TianGlyphUtil.GetColumn(70, 50, 100, "Total Size", "");
                default: return TianGlyphUtil.GetColumn(75, 50, 100, "Unknow", "");
            }
        }

        public string GetColumnString(int column)
        {
            switch (column)
            {
                case 0: return name;
                case 1: return GetSizeStr();
                case 2: return refCount.ToString();
                case 3: return depCount.ToString();
                case 4: return GetDepSizeStr();
                case 5: return GetTotalSizeStr();
                default: return "unkown:" + column;
            }
        }
        public object GetColumnOrder(int column)
        {
            switch (column)
            {
                case 0: return name;
                case 1: return size;
                case 2: return refCount;
                case 3: return depCount;
                case 4: return depSize;
                case 5: return totalSize;
                default: return name;
            }
        }
    }
}