
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using XCommon.Editor;

namespace XRemoteDebug
{
    internal class PatchFileInfo : EditorTableBaseItemInfo
    {
        public int size;
        public int type;
        public string datetime;

        public string GetSizeStr()
        {
            return size == 0 ? "--" : EditorUtility.FormatBytes(size);
        }

        public override string ToString()
        {
            return name;
        }

        #region IEditorTableItemInfo
        public override int itemId { get { return name.GetHashCode(); } }

        public static int totalColumn { get { return 3; } }
        public static MultiColumnHeaderState.Column GetColumnHeader(int column)
        {
            switch (column)
            {
                case 0: return TianGlyphUtil.GetColumn(250, 200, 400, "Name", "");
                case 1: return TianGlyphUtil.GetColumn(55, 55, 100, "Size", "");
                case 2: return TianGlyphUtil.GetColumn(130, 130, 140, "Time", "");
                default: return TianGlyphUtil.GetColumn(75, 50, 100, "Unknow", "");
            }
        }

        public override string GetColumnString(int column)
        {
            switch (column)
            {
                case 0: return name;
                case 1: return GetSizeStr();
                case 2: return datetime;
                default: return "unkown:" + column;
            }
        }
        public override object GetColumnOrder(int column)
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