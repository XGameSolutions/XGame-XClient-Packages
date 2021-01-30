
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace XTianGlyph.Tests
{
    class TestInfo : IEditorTableItemInfo
    {
        public string test1;
        public int test2;
        public float test3;

        public string displayName { get { return test1; } }
        public int itemId { get { return test1.GetHashCode(); } }
        public string assetPath { get; set; }

        public static int totalColumn { get { return 3; } }
        public static MultiColumnHeaderState.Column GetColumnHeader(int column)
        {
            switch (column)
            {
                case 0: return TianGlyphUtil.GetColumn(100, 50, 200, "Test1", "This is a test");
                case 1: return TianGlyphUtil.GetColumn(100, 50, 200, "Test2", "This is a test");
                case 2: return TianGlyphUtil.GetColumn(100, 50, 200, "Test3", "This is a test");
                default: return TianGlyphUtil.GetColumn(100, 50, 200, "Unknow", "");
            }
        }
        public string GetColumnString(int column)
        {
            switch (column)
            {
                case 0: return test1;
                case 1: return test2.ToString();
                case 2: return test3.ToString();
                default: return "unkown:" + column;
            }
        }
        public object GetColumnOrder(int column)
        {
            switch (column)
            {
                case 0: return test1;
                case 1: return test2;
                case 2: return test3;
                default: return 0;
            }
        }
    }
    class TestPanel : ITianGlyphPanel
    {
        public void OnGUI(Rect rect)
        {
            GUI.Button(rect, "button");
        }

        public void Reload()
        {
        }
    }

    public class TianGlyphTestWindow : EditorWindow
    {
        TianGlyphPanel m_Panel;

        EditorTable m_RTTable;


        [MenuItem("XTianGlyph/TianGlyphTestWindow")]
        static void ShowWindow()
        {
            var window = GetWindow<TianGlyphTestWindow>();
            window.titleContent = new GUIContent("TianGlyphWindow");
            window.Show();
        }

        private void OnEnable()
        {
            if (m_Panel == null)
            {
                m_Panel = new TianGlyphPanel(this);
            }
            m_Panel.LTOutline = true;
            m_Panel.LBOutline = true;
            m_Panel.RTOutline = true;
            m_Panel.RBOutline = true;
        }

        int count = 0;
        private void Update()
        {
            if (m_RTTable != null && count < 1000)
            {
                m_RTTable.AddInfo(new TestInfo()
                {
                    test1 = "info" + count,
                    test2 = Random.Range(0, 10),
                    test3 = Random.Range(0f, 10f)
                });
                count++;
            }
        }

        private void OnGUI()
        {
            if (m_Panel.LTPanel == null)
            {
                m_Panel.LTPanel = new TestPanel();
            }
            if (m_RTTable == null)
            {
                var column = TestInfo.totalColumn;
                m_RTTable = EditorTable.CreateTable(column);
                for (int i = 0; i < column; i++)
                {
                    m_RTTable.SetColumnHeader(0, TestInfo.GetColumnHeader(i));
                }
                m_RTTable.OnSelectionChanged = SelectedInfo;
                m_Panel.RTPanel = m_RTTable;
                m_Panel.RTPanel.Reload();
            }
            var panelRect = GetPanelArea();
            m_Panel.OnGUI(panelRect);
        }

        private Rect GetPanelArea()
        {
            var padding = 30;
            return new Rect(0, padding, position.width, position.height - padding);
        }

        private void SelectedInfo(List<IEditorTableItemInfo> list)
        {
            Debug.LogError("selected:" + list.Count);
        }
    }
}