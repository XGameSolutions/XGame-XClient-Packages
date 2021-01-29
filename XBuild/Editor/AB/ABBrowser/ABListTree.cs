using UnityEngine;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using System.Linq;
using XTianGlyph;

namespace XBuild.AB.ABBrowser
{
    public class ABListTree : TreeView, ITianGlyphPanel
    {
        class ABTreeItem : TreeViewItem
        {
            private ABInfo m_Info;
            public ABInfo Info { get { return m_Info; } }

            public ABTreeItem() : base(-1, -1) { }
            public ABTreeItem(ABInfo info) : base(info.name.GetHashCode(), 0, info.name)
            {
                m_Info = info;
            }
            public ABTreeItem(ABInfo info, int depth) : base(info.name.GetHashCode(), depth, info.name)
            {
                m_Info = info;
            }

            public bool ContainsChild(ABInfo info)
            {
                if (children == null) return false;
                if (info == null) return false;
                var contains = false;
                foreach (var child in children)
                {
                    var c = child as ABTreeItem;
                    if (c != null && c.Info != null && c.Info.name == info.name)
                    {
                        contains = true;
                        break;
                    }
                }
                return contains;
            }
        }
        private TreeViewItem m_RootItem;
        private bool m_IsDep;

        internal static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
        {
            return new MultiColumnHeaderState(GetColumns());
        }
        private static MultiColumnHeaderState.Column[] GetColumns()
        {
            var retVal = new MultiColumnHeaderState.Column[] {
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
            };
            var index = 0;
            retVal[index].headerContent = new GUIContent("AB", "");
            retVal[index].minWidth = 50;
            retVal[index].width = 100;
            retVal[index].maxWidth = 400;
            retVal[index].headerTextAlignment = TextAlignment.Left;
            retVal[index].canSort = true;
            retVal[index].autoResize = true;

            index++;
            retVal[index].headerContent = new GUIContent("Size", "");
            retVal[index].minWidth = 50;
            retVal[index].width = 75;
            retVal[index].maxWidth = 100;
            retVal[index].headerTextAlignment = TextAlignment.Left;
            retVal[index].canSort = true;
            retVal[index].autoResize = true;

            index++;
            retVal[index].headerContent = new GUIContent("Ref", "");
            retVal[index].minWidth = 20;
            retVal[index].width = 40;
            retVal[index].maxWidth = 50;
            retVal[index].headerTextAlignment = TextAlignment.Left;
            retVal[index].canSort = true;
            retVal[index].autoResize = true;

            index++;
            retVal[index].headerContent = new GUIContent("Dep", "");
            retVal[index].minWidth = 20;
            retVal[index].width = 40;
            retVal[index].maxWidth = 50;
            retVal[index].headerTextAlignment = TextAlignment.Left;
            retVal[index].canSort = true;
            retVal[index].autoResize = true;

            index++;
            retVal[index].headerContent = new GUIContent("Dep Size", "");
            retVal[index].minWidth = 40;
            retVal[index].width = 70;
            retVal[index].maxWidth = 100;
            retVal[index].headerTextAlignment = TextAlignment.Left;
            retVal[index].canSort = true;
            retVal[index].autoResize = false;

            index++;
            retVal[index].headerContent = new GUIContent("Total Size", "");
            retVal[index].minWidth = 50;
            retVal[index].width = 75;
            retVal[index].maxWidth = 100;
            retVal[index].headerTextAlignment = TextAlignment.Left;
            retVal[index].canSort = true;
            retVal[index].autoResize = false;

            return retVal;
        }

        internal enum SortOption
        {
            Name,
            Size,
            Ref,
            Dep,
            DepSize,
            TotalSize,
        }

        SortOption[] m_SortOptions = {
            SortOption.Name,
            SortOption.Size,
            SortOption.Ref,
            SortOption.Dep,
            SortOption.DepSize,
            SortOption.TotalSize,
        };

        public ABListTree(TreeViewState state, MultiColumnHeaderState mchs, bool isDep) : base(state, new MultiColumnHeader(mchs))
        {
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            multiColumnHeader.sortingChanged += OnSortingChanged;
            m_RootItem = new ABTreeItem();
            m_RootItem.children = new List<TreeViewItem>();
            m_IsDep = isDep;
        }

        public void UpdateSelection(IEnumerable<ABInfo> list)
        {
            var selected = new List<int>();
            foreach (var info in list)
            {
                var id = info.name.GetHashCode();
                if (FindItem(id, m_RootItem) == null) continue;
                if (!selected.Contains(id))
                {
                    selected.Add(id);
                }
            }
            SetSelection(selected, TreeViewSelectionOptions.FireSelectionChanged);
        }

        public void UpdateInfoList(IEnumerable<ABInfo> list)
        {
            m_RootItem.children.Clear();
            if (list != null)
            {
                foreach (var info in list)
                {
                    m_RootItem.AddChild(new ABTreeItem(info, m_RootItem.depth + 1));
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
                CellGUI(args.GetCellRect(i), args.item as ABTreeItem, args.GetColumn(i), ref args);
            }
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds == null) return;
            //List<Object> selectedObjects = new List<Object>();
            List<ABInfo> selectedInfo = new List<ABInfo>();
            foreach (var id in selectedIds)
            {
                var item = FindItem(id, rootItem) as ABTreeItem;
                if (item != null)
                {
                    selectedInfo.Add(item.Info);
                }
            }
            ABBrowserWindow.Instance.SelectedABList(selectedInfo, m_IsDep);
        }

        private void CellGUI(Rect cellRect, ABTreeItem item, int column, ref RowGUIArgs args)
        {
            var oldColor = GUI.color;
            CenterRectUsingSingleLineHeight(ref cellRect);
            switch (column)
            {
                case 0:
                    DefaultGUI.Label(cellRect, item.Info.name, args.selected, args.focused);
                    break;
                case 1:
                    DefaultGUI.Label(cellRect, item.Info.GetSizeStr(), args.selected, args.focused);
                    break;
                case 2:
                    DefaultGUI.Label(cellRect, item.Info.refCount.ToString(), args.selected, args.focused);
                    break;
                case 3:
                    DefaultGUI.Label(cellRect, item.Info.depCount.ToString(), args.selected, args.focused);
                    break;
                case 4:
                    DefaultGUI.Label(cellRect, item.Info.GetDepSizeStr(), args.selected, args.focused);
                    break;
                case 5:
                    DefaultGUI.Label(cellRect, item.Info.GetTotalSizeStr(), args.selected, args.focused);
                    break;
                default:
                    break;
            }
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
            var abList = new List<ABTreeItem>();
            foreach (var item in rootItem.children)
            {
                abList.Add(item as ABTreeItem);
            }
            var orderedItems = InitialOrder(abList, sortedColumns);
            rootItem.children = orderedItems.Cast<TreeViewItem>().ToList();
        }

        IOrderedEnumerable<ABTreeItem> InitialOrder(IEnumerable<ABTreeItem> myTypes, int[] columnList)
        {
            SortOption sortOption = m_SortOptions[columnList[0]];
            bool ascending = multiColumnHeader.IsSortedAscending(columnList[0]);
            switch (sortOption)
            {
                case SortOption.Name:
                    return myTypes.Order(l => l.Info.name, ascending);
                case SortOption.Size:
                    return myTypes.Order(l => l.Info.size, ascending);
                case SortOption.Ref:
                    return myTypes.Order(l => l.Info.refCount, ascending);
                case SortOption.Dep:
                    return myTypes.Order(l => l.Info.depCount, ascending);
                case SortOption.DepSize:
                    return myTypes.Order(l => l.Info.depSize, ascending);
                case SortOption.TotalSize:
                    return myTypes.Order(l => l.Info.totalSize, ascending);
                default:
                    return myTypes.Order(l => l.Info.name, ascending);
            }
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