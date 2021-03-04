
using UnityEngine;
using UnityEditor;
using XCommon.Editor;

namespace XRemoteDebug
{
    internal class RemoteDebugWindow : EditorWindow, ITianGlyphPanelParent
    {
        enum DebugTab
        {
            Hierarchy,
            Patch,
            Log
        }
        class Styles
        {
            public static readonly GUIStyle invisibleButton = "InvisibleButton";
        }
        private static RemoteDebugWindow s_Instance;
        internal static RemoteDebugWindow Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = GetWindow<RemoteDebugWindow>();
                }
                return s_Instance;
            }
        }

        [SerializeField] private TianGlyphPanel m_Panel;
        [SerializeField] private DebugTab m_SelectedTab;
        private GUIContent m_RefreshTexture;
        private GUIContent m_SaveTexture;
        private EditorTable m_ClientTree;

        private IRemoteDebugPanel[] m_TabPanels;

        private Rect m_Position;
        const float k_MenubarHeight = 0;//= 20f;
        const float k_MenubarXGap = 0;//= 1f;
        const float k_MenubarYGap = 0;//= 4f;
        const string k_IP = "127.0.0.1";
        const float k_AssetsToolbarHeight = 27;

        internal RemoteDebugServer server;
        internal RemoteDebugClientInfo selectedClient;


        [MenuItem("X/XRemoteDebug/DebugWindow")]
        static void ShowWindow()
        {
            s_Instance = null;
            Instance.titleContent = new GUIContent("DebugWindow");
            Instance.Show();
        }

        public void UploadFileSuccess(bool flag, string fileName)
        {
            if (m_SelectedTab == DebugTab.Patch)
                (m_TabPanels[(int)DebugTab.Patch] as PatchPanel).UploadNextFile(flag, fileName);
        }
        public void FileUploading(string fileName, int size, int speed)
        {
            if (m_SelectedTab == DebugTab.Patch)
                (m_TabPanels[(int)DebugTab.Patch] as PatchPanel).FileUploading(fileName, size, speed);
        }

        private void OnEnable()
        {
            m_RefreshTexture = new GUIContent(EditorGUIUtility.FindTexture("Refresh"), "Refresh shader from file");
            m_SaveTexture = new GUIContent(EditorGUIUtility.FindTexture("SaveAs"), "save shader variants to file");

            if (m_Panel == null)
            {
                m_Panel = new TianGlyphPanel();
            }
            m_Panel.OnEnable(this);

            if (m_TabPanels == null)
            {
                m_TabPanels = new IRemoteDebugPanel[3];
                m_TabPanels[0] = new HierarchyPanel(this);
                m_TabPanels[1] = new PatchPanel(this);
                m_TabPanels[2] = new LogPanel(this);
                foreach (var panel in m_TabPanels) panel.OnEnable();
            }
            SwitchPanel();

            server?.Close();
            server = new RemoteDebugServer(k_IP, RemoteDebugConfig.port);
            server.Start();
        }

        private void OnDisable()
        {
            if (server != null)
            {
                server.Close();
            }
        }

        private void Update()
        {
            server?.Update();
            if (m_ClientTree != null)
            {
                if (server.IsClientDirty())
                {
                    m_ClientTree.UpdateInfoList(server.GetClientList());
                    if (selectedClient == null && server.GetClientList().Count > 0)
                    {
                        SelectedClient(server.GetClientList()[0]);
                    }
                }
            }
            m_TabPanels[(int)m_SelectedTab]?.Update();
        }

        private void OnGUI()
        {
            InitPanel();
            OnGUIMenu();
            OnGUIPanel();
        }

        private void OnGUIMenu()
        {
            var rectMenu = new Rect(k_MenubarXGap, k_MenubarYGap,
                position.width - 2 * k_MenubarXGap, k_MenubarHeight);
            var rectRefresh = new Rect(rectMenu.x, rectMenu.y, k_MenubarHeight, k_MenubarHeight);
            if (GUI.Button(rectRefresh, m_RefreshTexture, Styles.invisibleButton))
            {

            }
            var refreshWidth = rectRefresh.x + k_MenubarHeight + k_MenubarXGap;
            var rectPath = new Rect(refreshWidth, rectMenu.y, rectMenu.width - refreshWidth - k_MenubarHeight, k_MenubarHeight);
            //m_VariantsAssetPath = GUI.TextField(rectPath, m_VariantsAssetPath);

            var rectSave = new Rect(rectMenu.width - k_MenubarHeight, rectMenu.y, k_MenubarHeight, k_MenubarHeight);
            if (GUI.Button(rectSave, m_SaveTexture, Styles.invisibleButton))
            {
            }

            var tabLabels = new string[]{
                "Hierarchy",
                "Patch",
                "Log"
            };
            var barWidth = m_Panel.RightTopRect.width;
            var barRect = new Rect(m_Panel.RightTopRect.x, m_Panel.RightTopRect.y - k_AssetsToolbarHeight,
                m_Panel.RightTopRect.width, k_AssetsToolbarHeight);
            var selected = (DebugTab)GUI.Toolbar(barRect, (int)m_SelectedTab, tabLabels);
            if (selected != m_SelectedTab)
            {
                m_SelectedTab = selected;
                SwitchPanel();
            }
        }

        private void SwitchPanel()
        {
            m_Panel.SetRightTopPanel(m_TabPanels[(int)m_SelectedTab], false, k_AssetsToolbarHeight);
            m_TabPanels[(int)m_SelectedTab].OnEnable();

            var statusPanel = m_TabPanels[(int)m_SelectedTab].GetStatusPanel();
            if (statusPanel != null)
                m_Panel.SetRigthBottomPanel(statusPanel, true);
        }

        private void OnGUIPanel()
        {
            var rectPanel = new Rect(0, k_MenubarHeight + 2 * k_MenubarYGap,
                position.width, position.height - k_MenubarHeight - 2 * k_MenubarYGap);
            m_Panel.OnGUI(rectPanel);
        }

        private void InitPanel()
        {
            InitClientTable();
        }

        private void InitClientTable()
        {
            if (m_ClientTree != null) return;
            var column = RemoteDebugClientInfo.totalColumn;
            m_ClientTree = EditorTable.CreateTable(column);
            for (int i = 0; i < column; i++)
            {
                m_ClientTree.SetColumnHeader(i, RemoteDebugClientInfo.GetColumnHeader(i));
            }
            m_ClientTree.OnDoubleClickedItem = OnDoubleClickedItem;
            m_Panel.SetLeftTopPanel(m_ClientTree, false);
        }

        private void OnDoubleClickedItem(IEditorTableItemInfo info)
        {
            SelectedClient(info as RemoteDebugClientInfo);
        }

        private void SelectedClient(RemoteDebugClientInfo info)
        {
            if (selectedClient != null)
            {
                selectedClient.itemSelected = false;
            }
            selectedClient = info;
            if (selectedClient != null)
            {
                selectedClient.itemSelected = true;
            }
            m_TabPanels[(int)m_SelectedTab].OnEnable();
        }
    }
}