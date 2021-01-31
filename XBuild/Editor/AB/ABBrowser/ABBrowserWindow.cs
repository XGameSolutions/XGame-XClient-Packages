

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using XTianGlyph;

namespace XBuild.AB.ABBrowser
{
    public class ABBrowserWindow : EditorWindow
    {
        class Styles
        {
            public static readonly GUIStyle btnInvisible = "InvisibleButton";
            public static readonly GUIContent btnSearchAB = new GUIContent("AB", "search AB");
            public static readonly GUIContent btnSearchAssets = new GUIContent("Assets", "search assets");
            public static readonly GUIContent[] searchBtns = new GUIContent[] { btnSearchAB, btnSearchAssets };
        }
        private static ABBrowserWindow s_Instance;
        internal static ABBrowserWindow Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = GetWindow<ABBrowserWindow>();
                }
                return s_Instance;
            }
        }

        [SerializeField] private TianGlyphPanel m_Panel;
        [SerializeField] private ABType m_SelectedABType;
        [SerializeField] private AssetsType m_SelectedAssetType;
        [SerializeField] private BuildTarget m_SelectedPlatform = BuildTarget.StandaloneWindows64;
        [SerializeField] private int m_SelectedPlatformIndex;
        [SerializeField] private int m_SelectedSearchIndex;
        [SerializeField] private int m_SelectedDepIndex;

        private EditorTable m_ABTree;
        private EditorTable m_ABDepTree;
        private EditorTable m_AssetTree;
        private MessageListPanel m_MessagePanel;
        const float k_MenubarPadding = 46.5f;
        const float k_DepToolbarHeight = 27f;
        const float k_ABToolbarHeight = 27f;
        const float k_AssetsToolbarHeight = 34f;

        private GUIContent m_RefreshTexture;
        private Texture2D m_AndroidTexture;
        private Texture2D m_IOSTexture;
        private Texture2D m_StandaloneTexture;
        private GUIContent[] m_PlatformTextures;
        private SearchField m_SearchField;
        private List<ABInfo> m_SelectedABInfos;


        [MenuItem("XBuild/AB-Browser")]
        static void ShowWindow()
        {
            s_Instance = null;
            Instance.titleContent = new GUIContent("ABBrowser");
            Instance.Show();
        }

        private void OnEnable()
        {
            ABDatabase.RefreshAB(m_SelectedPlatform);
            var panelPos = GetPanelArea();
            if (m_Panel == null)
            {
                m_Panel = new TianGlyphPanel(this);
            }
            m_Panel.OnEnable();
            m_RefreshTexture = new GUIContent(EditorGUIUtility.FindTexture("Refresh"), "Refresh AB list");
            m_StandaloneTexture = EditorGUIUtility.FindTexture("BuildSettings.Standalone@2x");
            m_IOSTexture = EditorGUIUtility.FindTexture("BuildSettings.iPhone@2x");
            m_AndroidTexture = EditorGUIUtility.FindTexture("BuildSettings.Android@2x");
            m_PlatformTextures = new GUIContent[] {
                new GUIContent(m_StandaloneTexture,"PC AB"),
                new GUIContent(m_IOSTexture,"iOS AB"),
                new GUIContent(m_AndroidTexture,"Android AB")
            };
            m_SearchField = new SearchField();
        }

        private void OnDisable()
        {
        }

        private void Update()
        {
            ABDatabase.Update();
            if (m_ABTree != null)
            {
                if (ABDatabase.IsABDirty())
                {
                    m_ABTree.UpdateInfoList(ABDatabase.GetABInfoList(m_SelectedABType));
                }
                if (ABDatabase.IsAssetDirty())
                {
                    m_AssetTree.UpdateInfoList(ABDatabase.GetAssetInfoList(m_SelectedAssetType));
                }
            }
        }

        private void OnGUI()
        {
            InitPanelTree();
            GUIButton();
            GUITianGlyphPanel();
            GUISearchAndAsset();
        }

        private void GUIButton()
        {
            GUILayout.BeginHorizontal();
            var platform = GUILayout.SelectionGrid(m_SelectedPlatformIndex, m_PlatformTextures, 3,
                GUILayout.Width(k_ABToolbarHeight * 3.3f), GUILayout.Height(k_ABToolbarHeight));
            if (platform != m_SelectedPlatformIndex)
            {
                m_SelectedPlatformIndex = platform;
                switch (m_SelectedPlatformIndex)
                {
                    case 0: m_SelectedPlatform = BuildTarget.StandaloneWindows64; break;
                    case 1: m_SelectedPlatform = BuildTarget.iOS; break;
                    case 2: m_SelectedPlatform = BuildTarget.Android; break;
                }
                ABDatabase.RefreshAB(m_SelectedPlatform);
            }
            var clicked = GUILayout.Button(m_RefreshTexture, GUILayout.Width(k_ABToolbarHeight), GUILayout.Height(k_ABToolbarHeight));
            if (clicked)
            {
                ABDatabase.RefreshAB(m_SelectedPlatform);
            }
            var tabLabels = new string[]{
                "All - " + ABDatabase.GetABTypeSizeStr(ABType.All),
                "Model - "+ ABDatabase.GetABTypeSizeStr(ABType.Model),
                "Scene - "+ ABDatabase.GetABTypeSizeStr(ABType.Scene),
                "UI - "+ ABDatabase.GetABTypeSizeStr(ABType.UI),
                "Other - "+ ABDatabase.GetABTypeSizeStr(ABType.Other),
                "Dep - "+ ABDatabase.GetABTypeSizeStr(ABType.Dep)
            };
            var barWidth = position.width - 5 * k_ABToolbarHeight + 7.4f;
            var selected = (ABType)GUILayout.Toolbar((int)m_SelectedABType, tabLabels, GUILayout.Height(k_ABToolbarHeight),
                GUILayout.Width(barWidth));
            if (selected != m_SelectedABType)
            {
                m_SelectedABType = selected;
                m_ABTree.UpdateInfoList(ABDatabase.GetABInfoList(m_SelectedABType));
            }
            GUILayout.EndHorizontal();
        }

        private void GUITianGlyphPanel()
        {
            var panelRect = GetPanelArea();
            m_Panel.OnGUI(panelRect);
        }

        private void GUISearchAndAsset()
        {
            var searchHeight = 16.8f;
            var btnWidth = 98;
            m_SelectedSearchIndex = GUILayout.SelectionGrid(m_SelectedSearchIndex, Styles.searchBtns, 2,
                GUILayout.Width(btnWidth), GUILayout.Height(searchHeight - 2));
            var rect = new Rect(m_Panel.LeftTopRect.x + btnWidth, m_Panel.LeftTopRect.y - searchHeight + 0.5f,
                m_Panel.LeftTopRect.width - btnWidth + 2 * m_Panel.SplitterWidth, searchHeight);
            if (m_SelectedSearchIndex == 0)
            {
                var newSearch = m_SearchField.OnGUI(rect, ABDatabase.abSearchString);
                if (ABDatabase.abSearchString != newSearch)
                {
                    ABDatabase.abSearchString = newSearch;
                    m_ABTree.UpdateInfoList(ABDatabase.GetABInfoList(m_SelectedABType));
                    Repaint();
                }
            }
            else
            {
                var newSearch = m_SearchField.OnGUI(rect, ABDatabase.assetsSearchString);
                if (ABDatabase.assetsSearchString != newSearch)
                {
                    ABDatabase.assetsSearchString = newSearch;
                    m_AssetTree.UpdateInfoList(ABDatabase.GetAssetInfoList(m_SelectedAssetType));
                    Repaint();
                }
            }
            var tabLabels = new string[]{
                "All\n" + ABDatabase.GetAssetTypeSizeStr(AssetsType.None),
                "Material\n"+ ABDatabase.GetAssetTypeSizeStr(AssetsType.Material),
                "Texture\n"+ ABDatabase.GetAssetTypeSizeStr(AssetsType.Texture),
                "Prefab\n"+ ABDatabase.GetAssetTypeSizeStr(AssetsType.Prefab),
                "Shader\n"+ ABDatabase.GetAssetTypeSizeStr(AssetsType.Shader),
                "Asset\n"+ ABDatabase.GetAssetTypeSizeStr(AssetsType.Asset),
                "Other\n"+ ABDatabase.GetAssetTypeSizeStr(AssetsType.Other),
            };
            var barWidth = m_Panel.RightTopRect.width;
            var barRect = new Rect(m_Panel.RightTopRect.x, m_Panel.RightTopRect.y - k_AssetsToolbarHeight,
                m_Panel.RightTopRect.width, k_AssetsToolbarHeight - 0.5f);
            var selected = (AssetsType)GUI.Toolbar(barRect, (int)m_SelectedAssetType, tabLabels);
            if (selected != m_SelectedAssetType)
            {
                m_SelectedAssetType = selected;
                m_AssetTree.UpdateInfoList(ABDatabase.GetAssetInfoList(m_SelectedAssetType));
            }

            barWidth = m_Panel.LeftBottomRect.width;
            barRect = new Rect(m_Panel.LeftBottomRect.x, m_Panel.LeftBottomRect.y - k_DepToolbarHeight,
                m_Panel.LeftBottomRect.width, k_DepToolbarHeight - 0.5f);
            var selectedDep = GUI.Toolbar(barRect, m_SelectedDepIndex, new string[] { "Dep", "Ref" });
            if (selectedDep != m_SelectedDepIndex)
            {
                m_SelectedDepIndex = selectedDep;
                UpdateDepInfoList();
            }
        }

        private void InitPanelTree()
        {
            InitABTable();
            InitABDepTable();
            InitAssetsTable();
            if (m_MessagePanel == null)
            {
                m_MessagePanel = new MessageListPanel();
                m_Panel.SetRigthBottomPanel(m_MessagePanel);
            }
        }

        private void InitABTable()
        {
            if (m_ABTree != null) return;
            var column = ABInfo.totalColumn;
            m_ABTree = EditorTable.CreateTable(column);
            for (int i = 0; i < column; i++)
            {
                m_ABTree.SetColumnHeader(i, ABInfo.GetColumnHeader(i));
            }
            m_ABTree.OnSelectionChanged = OnSelectedABList;
            m_Panel.SetLeftTopPanel(m_ABTree, false);
        }

        private void InitABDepTable()
        {
            if (m_ABDepTree != null) return;
            var column = ABInfo.totalColumn;
            m_ABDepTree = EditorTable.CreateTable(column);
            for (int i = 0; i < column; i++)
            {
                m_ABDepTree.SetColumnHeader(i, ABInfo.GetColumnHeader(i));
            }
            m_ABDepTree.OnSelectionChanged = OnSelectedABDepList;
            m_Panel.SetLeftBottomPanel(m_ABDepTree, false, k_DepToolbarHeight);
        }

        private void InitAssetsTable()
        {
            if (m_AssetTree != null) return;
            var column = ABAssetsInfo.totalColumn;
            m_AssetTree = EditorTable.CreateTable(column);
            for (int i = 0; i < column; i++)
            {
                m_AssetTree.SetColumnHeader(i, ABAssetsInfo.GetColumnHeader(i));
            }
            m_AssetTree.OnSelectionChanged = OnSelectedAssetsList;
            m_Panel.SetRightTopPanel(m_AssetTree, false, k_AssetsToolbarHeight / 2);
        }


        private void OnSelectedABList(List<IEditorTableItemInfo> infoList)
        {
            m_SelectedABInfos = infoList.ConvertAll<ABInfo>(info => info as ABInfo);
            ABDatabase.RefreshAssets(m_SelectedABInfos);
            m_AssetTree.UpdateInfoList(null);
            UpdateDepInfoList();
        }

        private void UpdateDepInfoList()
        {
            if (m_SelectedDepIndex == 0)
            {
                m_ABDepTree.UpdateInfoList(ABDatabase.GetABDepInfoList(m_SelectedABInfos));
            }
            else
            {
                m_ABDepTree.UpdateInfoList(ABDatabase.GetABRefInfoList(m_SelectedABInfos));
            }
        }

        private void OnSelectedABDepList(List<IEditorTableItemInfo> infoList)
        {
            var list = infoList.ConvertAll<ABInfo>(info => info as ABInfo);
            ABDatabase.RefreshAssets(list);
            m_AssetTree.UpdateInfoList(null);
        }

        private void OnSelectedAssetsList(List<IEditorTableItemInfo> assetsList)
        {
            var list = assetsList.ConvertAll<ABAssetsInfo>(info => info as ABAssetsInfo);
            m_MessagePanel.SetSelectedAssets(list);
        }

        private Rect GetPanelArea()
        {
            return new Rect(0, k_MenubarPadding, position.width, position.height - k_MenubarPadding);
        }
    }
}