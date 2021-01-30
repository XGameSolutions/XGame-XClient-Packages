using UnityEditor.IMGUI.Controls;

namespace XTianGlyph
{
    class EditorTableItem : TreeViewItem
    {
        private IEditorTableItemInfo m_Info;
        public IEditorTableItemInfo Info { get { return m_Info; } }

        public EditorTableItem() : base(-1, -1) { }
        public EditorTableItem(IEditorTableItemInfo info) : base(info.itemId, 0, info.displayName)
        {
            m_Info = info;
        }
        public EditorTableItem(IEditorTableItemInfo info, int depth) : base(info.itemId, depth, info.displayName)
        {
            m_Info = info;
        }

        public bool ContainsChild(IEditorTableItemInfo info)
        {
            if (children == null) return false;
            if (info == null) return false;
            var contains = false;
            foreach (var child in children)
            {
                var c = child as EditorTableItem;
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