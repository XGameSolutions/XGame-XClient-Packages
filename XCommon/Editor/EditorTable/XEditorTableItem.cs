using UnityEditor.IMGUI.Controls;

namespace XCommon.Editor
{
    class XEditorTableItem : TreeViewItem
    {
        private XIEditorTableItemInfo m_Info;
        public XIEditorTableItemInfo Info { get { return m_Info; } }

        public XEditorTableItem() : base(-1, -1) { }
        public XEditorTableItem(XIEditorTableItemInfo info) : base(info.itemId, 0, info.displayName)
        {
            m_Info = info;
        }
        public XEditorTableItem(XIEditorTableItemInfo info, int depth) : base(info.itemId, depth, info.displayName)
        {
            m_Info = info;
        }

        public bool ContainsChild(XIEditorTableItemInfo info)
        {
            if (children == null) return false;
            if (info == null) return false;
            var contains = false;
            foreach (var child in children)
            {
                var c = child as XEditorTableItem;
                if (c != null && c.Info != null && c.Info.itemId == info.itemId)
                {
                    contains = true;
                    break;
                }
            }
            return contains;
        }
    }
}