
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using System.Linq;
using UnityEditor;

namespace XTianGlyph
{
    public class EditorTable : TreeView, ITianGlyphPanel
    {
        public static EditorTable CreateTable(int column)
        {
            var state = GetDefaultState();
            var mchs = GetDefaultMCHS(column);
            return new EditorTable(state, mchs);
        }

        public static EditorTable CreateTable(int column, TreeViewState state)
        {
            var mchs = GetDefaultMCHS(column);
            return new EditorTable(state, mchs);
        }

        public static EditorTable CreateTable(TreeViewState state, MultiColumnHeaderState mchs)
        {
            return new EditorTable(state, mchs);
        }

        public static TreeViewState GetDefaultState()
        {
            return new TreeViewState();
        }

        public static MultiColumnHeaderState GetDefaultMCHS(int column)
        {
            var m_Columns = new MultiColumnHeaderState.Column[column];
            for (int i = 0; i < column; i++)
            {
                var columnInfo = new MultiColumnHeaderState.Column();
                columnInfo.headerContent = new GUIContent("Column" + i, "");
                columnInfo.minWidth = 30;
                columnInfo.width = 50;
                columnInfo.maxWidth = 100;
                columnInfo.headerTextAlignment = TextAlignment.Left;
                columnInfo.canSort = true;
                columnInfo.autoResize = true;
                m_Columns[i] = columnInfo;
            }
            return new MultiColumnHeaderState(m_Columns);
        }

        public static void SetColumnInfo(MultiColumnHeaderState mchs, int column, float width, float minWidth, float maxWidth,
            string name, string nameTip, bool canSort = true, bool autoResize = true,
            TextAlignment headerTextAlignment = TextAlignment.Left)
        {
            if (column >= 0 && column < mchs.columns.Length)
            {
                var info = mchs.columns[column];
                info.headerContent = new GUIContent(name, nameTip);
                info.minWidth = minWidth;
                info.width = width;
                info.maxWidth = maxWidth;
                info.headerTextAlignment = headerTextAlignment;
                info.canSort = canSort;
                info.autoResize = autoResize;
            }
        }

        private TreeViewItem m_RootItem;
        private System.Action<List<IEditorTableItemInfo>> m_OnSelectionChanged;
        private bool m_SelectedObjects;
        
        public System.Action<List<IEditorTableItemInfo>> OnSelectionChanged { set { m_OnSelectionChanged = value; } }

        public EditorTable(TreeViewState state, MultiColumnHeaderState mchs) : base(state, new MultiColumnHeader(mchs))
        {
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            multiColumnHeader.sortingChanged += OnSortingChanged;
            m_RootItem = new EditorTableItem();
            m_RootItem.children = new List<TreeViewItem>();
        }

        public void SetHeader(int column, float width, float minWidth, float maxWidth,
            string name, string nameTip, bool canSort = true, bool autoResize = true,
            TextAlignment headerTextAlignment = TextAlignment.Left)
        {
            SetColumnInfo(multiColumnHeader.state, column, width, minWidth, maxWidth, name, nameTip, canSort, autoResize,
                headerTextAlignment);
        }



        public void UpdateSelection(IEnumerable<IEditorTableItemInfo> list)
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

        public void UpdateInfoList(IEnumerable<IEditorTableItemInfo> list)
        {
            m_RootItem.children.Clear();
            AddInfoList(list);
        }

        public void AddInfo(IEditorTableItemInfo info)
        {
            m_RootItem.AddChild(new EditorTableItem(info, m_RootItem.depth + 1));
            Reload();
        }

        public void AddInfoList(IEnumerable<IEditorTableItemInfo> list)
        {
            if (list != null)
            {
                foreach (var info in list)
                {
                    m_RootItem.AddChild(new EditorTableItem(info, m_RootItem.depth + 1));
                }
            }
            SetSelection(new List<int>());
            Reload();
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
            for (int i = 0; i < args.GetNumVisibleColumns(); i++)
            {
                CellGUI(args.GetCellRect(i), args.item as EditorTableItem, args.GetColumn(i), ref args);
            }
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds == null) return;
            List<Object> selectedObjects = new List<Object>();
            List<IEditorTableItemInfo> selectedInfo = new List<IEditorTableItemInfo>();
            foreach (var id in selectedIds)
            {
                var item = FindItem(id, rootItem) as EditorTableItem;
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

        private void CellGUI(Rect cellRect, EditorTableItem item, int column, ref RowGUIArgs args)
        {
            var oldColor = GUI.color;
            CenterRectUsingSingleLineHeight(ref cellRect);
            DefaultGUI.Label(cellRect, item.Info.GetColumnString(column), args.selected, args.focused);
            GUI.color = oldColor;
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
            var abList = new List<EditorTableItem>();
            foreach (var item in rootItem.children)
            {
                abList.Add(item as EditorTableItem);
            }
            var orderedItems = InitialOrder(abList, sortedColumns);
            rootItem.children = orderedItems.Cast<TreeViewItem>().ToList();
        }

        IOrderedEnumerable<EditorTableItem> InitialOrder(IEnumerable<EditorTableItem> myTypes, int[] columnList)
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