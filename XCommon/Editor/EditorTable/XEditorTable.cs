
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using System.Linq;
using UnityEditor;

namespace XCommon.Editor
{
    /// <summary>
    /// 表格
    /// </summary>
    public class XEditorTable : TreeView, ITianGlyphPanel
    {
        class Styles
        {
            public static readonly Texture selected = EditorGUIUtility.IconContent("d_FilterSelectedOnly").image;
        }
        public static XEditorTable CreateTable(int column, bool enabelSelectedObject = false)
        {
            var state = TianGlyphUtil.GetDefaultState();
            var mchs = TianGlyphUtil.GetDefaultMCHS(column);
            return new XEditorTable(state, mchs, enabelSelectedObject);
        }

        public static XEditorTable CreateTable(int column, TreeViewState state, bool enabelSelectedObject = false)
        {
            var mchs = TianGlyphUtil.GetDefaultMCHS(column);
            return new XEditorTable(state, mchs, enabelSelectedObject);
        }

        public static XEditorTable CreateTable(TreeViewState state, MultiColumnHeaderState mchs, bool enabelSelectedObject = false)
        {
            return new XEditorTable(state, mchs, enabelSelectedObject);
        }

        private TreeViewItem m_RootItem;
        private System.Action<List<XIEditorTableItemInfo>> m_OnSelectionChanged;
        private System.Action<XIEditorTableItemInfo> m_OnSingleClickedItem;
        private System.Action<XIEditorTableItemInfo> m_OnDoubleClickedItem;
        private bool m_SelectedObjects;
        public bool showIcon = true;

        public System.Action<List<XIEditorTableItemInfo>> OnSelectionChanged { set { m_OnSelectionChanged = value; } }
        public System.Action<XIEditorTableItemInfo> OnSingleClickedItem { set { m_OnSingleClickedItem = value; } }
        public System.Action<XIEditorTableItemInfo> OnDoubleClickedItem { set { m_OnDoubleClickedItem = value; } }

        public XEditorTable(TreeViewState state, MultiColumnHeaderState mchs, bool selectedObjects = false) : base(state, new MultiColumnHeader(mchs))
        {
            showBorder = true;
            m_SelectedObjects = selectedObjects;
            showAlternatingRowBackgrounds = true;
            multiColumnHeader.sortingChanged += OnSortingChanged;
            m_RootItem = new XEditorTableItem();
            m_RootItem.children = new List<TreeViewItem>();
        }

        public void SetColumnHeader(int column, float width, float minWidth, float maxWidth,
            string name, string nameTip, bool canSort = true, bool autoResize = true,
            TextAlignment headerTextAlignment = TextAlignment.Left)
        {
            TianGlyphUtil.SetColumn(multiColumnHeader.state, column, width, minWidth, maxWidth, name, nameTip, canSort, autoResize,
                headerTextAlignment);
        }

        public void SetColumnHeader(int column, MultiColumnHeaderState.Column info)
        {
            multiColumnHeader.state.columns[column] = info;
        }

        public void EnableSelectedObject(bool flag)
        {
            m_SelectedObjects = flag;
        }

        public void UpdateSelection(IEnumerable<XIEditorTableItemInfo> list)
        {
            var selected = new List<int>();
            foreach (var info in list)
            {
                var id = info.itemId;
                if (FindItem(id, m_RootItem) == null) continue;
                if (!selected.Contains(id))
                {
                    selected.Add(id);
                }
            }
            SetSelection(selected, TreeViewSelectionOptions.FireSelectionChanged);
        }

        public void UpdateInfoList(IEnumerable<XIEditorTableItemInfo> list)
        {
            m_RootItem.children.Clear();
            AddInfoList(m_RootItem, list);
            SetSelection(new List<int>());
            Reload();
        }

        public void AddInfo(XIEditorTableItemInfo info)
        {
            m_RootItem.AddChild(new XEditorTableItem(info, m_RootItem.depth + 1));
            Reload();
        }

        private void AddInfoList(TreeViewItem rootItem, IEnumerable<XIEditorTableItemInfo> list)
        {
            if (list != null)
            {
                foreach (var info in list)
                {
                    var child = new XEditorTableItem(info, rootItem.depth + 1);
                    child.icon = info.assetIcon;
                    rootItem.AddChild(child);
                    if (info.children != null && info.children.Count > 0)
                    {
                        AddInfoList(child, info.children);
                    }
                }
            }
        }

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
            {
                SetSelection(new int[0], TreeViewSelectionOptions.FireSelectionChanged);
            }
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            var rows = base.BuildRows(root);
            SortIfNeeded(root, rows);
            return rows;
        }

        protected override TreeViewItem BuildRoot()
        {
            SetupDepthsFromParentsAndChildren(m_RootItem);
            return m_RootItem;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item as XEditorTableItem;
            for (int i = 0; i < args.GetNumVisibleColumns(); i++)
            {
                CellGUI(args.GetCellRect(i), item, args.GetColumn(i), ref args);
            }
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds == null) return;
            List<Object> selectedObjects = new List<Object>();
            List<XIEditorTableItemInfo> selectedInfo = new List<XIEditorTableItemInfo>();
            foreach (var id in selectedIds)
            {
                var item = FindItem(id, rootItem) as XEditorTableItem;
                if (item != null)
                {
                    if (m_SelectedObjects)
                    {
                        Object o = AssetDatabase.LoadAssetAtPath<Object>(item.Info.assetPath);
                        if (o != null)
                        {
                            selectedObjects.Add(o);
                            Selection.activeObject = o;
                        }
                    }
                    selectedInfo.Add(item.Info);
                }
            }
            if (m_SelectedObjects)
                Selection.objects = selectedObjects.ToArray();
            m_OnSelectionChanged?.Invoke(selectedInfo);
        }

        protected override void SingleClickedItem(int id)
        {
            var item = FindItem(id, rootItem) as XEditorTableItem;
            m_OnSingleClickedItem?.Invoke(item.Info);
        }

        protected override void DoubleClickedItem(int id)
        {
            var item = FindItem(id, rootItem) as XEditorTableItem;
            m_OnDoubleClickedItem?.Invoke(item.Info);
        }

        private void CellGUI(Rect cellRect, XEditorTableItem item, int column, ref RowGUIArgs args)
        {
            CenterRectUsingSingleLineHeight(ref cellRect);
            if (column == 0)
            {
                if (item.Info.itemSelected)
                {
                    var iconRect = new Rect(cellRect.x + 1, cellRect.y + 1, cellRect.height - 2, cellRect.height - 2);
                    GUI.DrawTexture(iconRect, Styles.selected, ScaleMode.ScaleToFit);
                    cellRect.x += cellRect.height - 2;
                }
                if (item.hasChildren || item.depth > 0) cellRect.x += 16;
                if (item.depth > 0) cellRect.x += 14 * item.depth;
                if (showIcon)
                {
                    var iconRect = new Rect(cellRect.x + 1, cellRect.y + 1, cellRect.height - 2, cellRect.height - 2);
                    if (item.icon != null)
                    {
                        GUI.DrawTexture(iconRect, item.icon, ScaleMode.ScaleToFit);
                    }
                    cellRect = new Rect(cellRect.x + iconRect.width + 1, cellRect.y,
                        cellRect.width - iconRect.width, cellRect.height);
                }
                using (new EditorGUI.DisabledGroupScope(item.Info.itemDisabled))
                {
                    DefaultGUI.Label(cellRect, item.Info.GetColumnString(column), args.selected, args.focused);
                }
            }
            else
            {
                DefaultGUI.Label(cellRect, item.Info.GetColumnString(column), args.selected, args.focused);
            }
        }

        private void OnSortingChanged(MultiColumnHeader header)
        {
            SortIfNeeded(rootItem, GetRows());
        }

        void SortIfNeeded(TreeViewItem root, IList<TreeViewItem> rows)
        {
            if (rows.Count <= 1) return;
            if (multiColumnHeader.sortedColumnIndex == -1) return;
            SortByColumn();
            rows.Clear();
            for (int i = 0; i < root.children.Count; i++)
                rows.Add(root.children[i]);
            Repaint();
        }

        void SortByColumn()
        {
            var sortedColumns = multiColumnHeader.state.sortedColumns;
            if (sortedColumns.Length == 0) return;
            var abList = new List<XEditorTableItem>();
            foreach (var item in rootItem.children)
            {
                abList.Add(item as XEditorTableItem);
            }
            var orderedItems = InitialOrder(abList, sortedColumns);
            rootItem.children = orderedItems.Cast<TreeViewItem>().ToList();
        }

        IOrderedEnumerable<XEditorTableItem> InitialOrder(IEnumerable<XEditorTableItem> myTypes, int[] columnList)
        {
            bool ascending = multiColumnHeader.IsSortedAscending(columnList[0]);
            return myTypes.Order(l => l.Info.GetColumnOrder(columnList[0]), ascending);
        }
    }

    static class MyExtensionMethods
    {
        internal static IOrderedEnumerable<T> Order<T, TKey>(this IEnumerable<T> source, System.Func<T, TKey> selector, bool ascending)
        {
            if (ascending)
            {
                return source.OrderBy(selector);
            }
            else
            {
                return source.OrderByDescending(selector);
            }
        }

        internal static IOrderedEnumerable<T> ThenBy<T, TKey>(this IOrderedEnumerable<T> source, System.Func<T, TKey> selector, bool ascending)
        {
            if (ascending)
            {
                return source.ThenBy(selector);
            }
            else
            {
                return source.ThenByDescending(selector);
            }
        }
    }
}