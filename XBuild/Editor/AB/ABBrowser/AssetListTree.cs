using UnityEngine;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using System.Linq;
using UnityEditor;
using XTianGlyph;

namespace XBuild.AB.ABBrowser
{
    public class AssetListTree : TreeView, ITianGlyphPanel
    {
        class AssetTreeItem : TreeViewItem
        {
            private ABAssetsInfo m_Info;
            public ABAssetsInfo Info { get { return m_Info; } }

            public AssetTreeItem() : base(-1, -1) { }
            public AssetTreeItem(ABAssetsInfo info) : base(info.path.GetHashCode(), 0, info.name)
            {
                m_Info = info;
            }
            public AssetTreeItem(ABAssetsInfo info, int depth) : base(info.path.GetHashCode(), depth, info.name)
            {
                m_Info = info;
            }

            public bool ContainsChild(ABAssetsInfo info)
            {
                if (children == null) return false;
                if (info == null) return false;
                var contains = false;
                foreach (var child in children)
                {
                    var c = child as AssetTreeItem;
                    if (c != null && c.Info != null && c.Info.path == info.path)
                    {
                        contains = true;
                        break;
                    }
                }
                return contains;
            }
        }

        private TreeViewItem m_RootItem;

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
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
            };
            var index = 0;
            retVal[index].headerContent = new GUIContent("Asset", "");
            retVal[index].minWidth = 100;
            retVal[index].width = 200;
            retVal[index].maxWidth = 500;
            retVal[index].headerTextAlignment = TextAlignment.Left;
            retVal[index].canSort = true;
            retVal[index].autoResize = true;

            index++;
            retVal[index].headerContent = new GUIContent("Ext", "");
            retVal[index].minWidth = 10;
            retVal[index].width = 50;
            retVal[index].maxWidth = 50;
            retVal[index].headerTextAlignment = TextAlignment.Left;
            retVal[index].canSort = true;
            retVal[index].autoResize = true;

            index++;
            retVal[index].headerContent = new GUIContent("AB", "");
            retVal[index].minWidth = 100;
            retVal[index].width = 200;
            retVal[index].maxWidth = 400;
            retVal[index].headerTextAlignment = TextAlignment.Left;
            retVal[index].canSort = true;
            retVal[index].autoResize = true;

            index++;
            retVal[index].headerContent = new GUIContent("Ref", "");
            retVal[index].minWidth = 20;
            retVal[index].width = 30;
            retVal[index].maxWidth = 50;
            retVal[index].headerTextAlignment = TextAlignment.Left;
            retVal[index].canSort = true;
            retVal[index].autoResize = true;

            index++;
            retVal[index].headerContent = new GUIContent("Size", "");
            retVal[index].minWidth = 20;
            retVal[index].width = 60;
            retVal[index].maxWidth = 100;
            retVal[index].headerTextAlignment = TextAlignment.Left;
            retVal[index].canSort = true;
            retVal[index].autoResize = true;

            index++;
            retVal[index].headerContent = new GUIContent("Width", "");
            retVal[index].minWidth = 20;
            retVal[index].width = 50;
            retVal[index].maxWidth = 100;
            retVal[index].headerTextAlignment = TextAlignment.Left;
            retVal[index].canSort = true;
            retVal[index].autoResize = false;

            index++;
            retVal[index].headerContent = new GUIContent("Height", "");
            retVal[index].minWidth = 20;
            retVal[index].width = 50;
            retVal[index].maxWidth = 100;
            retVal[index].headerTextAlignment = TextAlignment.Left;
            retVal[index].canSort = true;
            retVal[index].autoResize = false;

            index++;
            retVal[index].headerContent = new GUIContent("MaxSize", "");
            retVal[index].minWidth = 20;
            retVal[index].width = 60;
            retVal[index].maxWidth = 100;
            retVal[index].headerTextAlignment = TextAlignment.Left;
            retVal[index].canSort = true;
            retVal[index].autoResize = false;

            return retVal;
        }

        internal enum SortOption
        {
            Asset,
            Extention,
            AB,
            Ref,
            Size,
            Width,
            Height,
            MaxSize,
        }

        SortOption[] m_SortOptions = {
            SortOption.Asset,
            SortOption.Extention,
            SortOption.AB,
            SortOption.Ref,
            SortOption.Size,
            SortOption.Width,
            SortOption.Height,
            SortOption.MaxSize,
        };

        public AssetListTree(TreeViewState state, MultiColumnHeaderState mchs) : base(state, new MultiColumnHeader(mchs))
        {
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            multiColumnHeader.sortingChanged += OnSortingChanged;
            m_RootItem = new AssetTreeItem();
            m_RootItem.children = new List<TreeViewItem>();
        }

        public void UpdateSelection(IEnumerable<ABAssetsInfo> list)
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

        public void UpdateInfoList(IEnumerable<ABAssetsInfo> list)
        {
            m_RootItem.children.Clear();
            if (list != null)
            {
                foreach (var info in list)
                {
                    m_RootItem.AddChild(new AssetTreeItem(info, m_RootItem.depth + 1));
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
                CellGUI(args.GetCellRect(i), args.item as AssetTreeItem, args.GetColumn(i), ref args);
            }
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds == null) return;
            List<Object> selectedObjects = new List<Object>();
            List<ABAssetsInfo> selectedInfo = new List<ABAssetsInfo>();
            foreach (var id in selectedIds)
            {
                var item = FindItem(id, rootItem) as AssetTreeItem;
                if (item != null)
                {
                    Object o = AssetDatabase.LoadAssetAtPath<Object>(item.Info.path);
                    selectedObjects.Add(o);
                    Selection.activeObject = o;
                    selectedInfo.Add(item.Info);
                }
            }
            Selection.objects = selectedObjects.ToArray();
            ABBrowserWindow.Instance.SelectedAssetsList(selectedInfo);
        }

        private void CellGUI(Rect cellRect, AssetTreeItem item, int column, ref RowGUIArgs args)
        {
            var oldColor = GUI.color;
            CenterRectUsingSingleLineHeight(ref cellRect);
            switch (column)
            {
                case 0:
                    DefaultGUI.Label(cellRect, item.Info.name, args.selected, args.focused);
                    break;
                case 1:
                    DefaultGUI.Label(cellRect, item.Info.extension, args.selected, args.focused);
                    break;
                case 2:
                    DefaultGUI.Label(cellRect, item.Info.GetABNameString(), args.selected, args.focused);
                    break;
                case 3:
                    DefaultGUI.Label(cellRect, item.Info.refCount.ToString(), args.selected, args.focused);
                    break;
                case 4:
                    DefaultGUI.Label(cellRect, item.Info.GetSizeString(), args.selected, args.focused);
                    break;
                case 5:
                    DefaultGUI.Label(cellRect, item.Info.GetWidthString(), args.selected, args.focused);
                    break;
                case 6:
                    DefaultGUI.Label(cellRect, item.Info.GetHeightString(), args.selected, args.focused);
                    break;
                case 7:
                    DefaultGUI.Label(cellRect, item.Info.GetMaxSizeString(), args.selected, args.focused);
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
            var abList = new List<AssetTreeItem>();
            foreach (var item in rootItem.children)
            {
                abList.Add(item as AssetTreeItem);
            }
            var orderedItems = InitialOrder(abList, sortedColumns);
            rootItem.children = orderedItems.Cast<TreeViewItem>().ToList();
        }

        IOrderedEnumerable<AssetTreeItem> InitialOrder(IEnumerable<AssetTreeItem> myTypes, int[] columnList)
        {
            SortOption sortOption = m_SortOptions[columnList[0]];
            bool ascending = multiColumnHeader.IsSortedAscending(columnList[0]);
            switch (sortOption)
            {
                case SortOption.Asset:
                    return myTypes.Order(l => l.Info.name, ascending);
                case SortOption.Extention:
                    return myTypes.Order(l => l.Info.extension, ascending);
                case SortOption.AB:
                    return myTypes.Order(l => l.Info.abName, ascending);
                case SortOption.Size:
                    return myTypes.Order(l => l.Info.size, ascending);
                case SortOption.Ref:
                    return myTypes.Order(l => l.Info.refCount, ascending);
                case SortOption.Width:
                    return myTypes.Order(l => l.Info.textureWidth, ascending);
                case SortOption.Height:
                    return myTypes.Order(l => l.Info.textureHeight, ascending);
                case SortOption.MaxSize:
                    return myTypes.Order(l => l.Info.textureMaxSize, ascending);
                default:
                    return myTypes.Order(l => l.Info.name, ascending);
            }
        }
    }
}