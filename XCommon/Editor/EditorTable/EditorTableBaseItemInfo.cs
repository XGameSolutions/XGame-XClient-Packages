

using System;
using System.Collections.Generic;
using UnityEngine;

namespace XCommon.Editor
{
    public class EditorTableBaseItemInfo : IEditorTableItemInfo
    {
        protected string m_Name;

        public virtual string name
        {
            get { return string.IsNullOrEmpty(m_Name) ? "<empty>" : m_Name; }
            set { m_Name = value; }
        }
        public virtual string uuid { get; set; }
        public virtual int itemId { get; set; }
        public virtual bool itemDisabled { get; set; }
        public virtual bool itemSelected { get; set; }
        public virtual string displayName { get { return name; } }
        public virtual string assetPath { get; set; }
        public virtual Texture2D assetIcon { get; set; }
        public virtual List<IEditorTableItemInfo> children { get; set; }
        public virtual string GetColumnString(int column)
        {
            throw new Exception("GetColumnString() need to override");
        }
        public virtual object GetColumnOrder(int column)
        {
            throw new Exception("GetColumnString() need to override");
        }

        // public static int totalColumn { get { return 3; } }
        // public static MultiColumnHeaderState.Column GetColumnHeader(int column)
        // {
        //     switch (column)
        //     {
        //         case 0: return TianGlyphUtil.GetColumn(100, 50, 200, "Test1", "This is a test");
        //         case 1: return TianGlyphUtil.GetColumn(100, 50, 200, "Test2", "This is a test");
        //         case 2: return TianGlyphUtil.GetColumn(100, 50, 200, "Test3", "This is a test");
        //         default: return TianGlyphUtil.GetColumn(100, 50, 200, "Unknow", "");
        //     }
        // }
    }
}