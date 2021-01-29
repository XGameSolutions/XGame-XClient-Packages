
using UnityEditor;
using UnityEngine;

namespace XTianGlyph.Tests
{
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

        [MenuItem("TianGlyph/TianGlyphTestWindow")]
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

        private void OnGUI()
        {
            if (m_Panel.LTPanel == null)
            {
                m_Panel.LTPanel = new TestPanel();
            }

            var panelRect = GetPanelArea();
            m_Panel.OnGUI(panelRect);
        }

        private Rect GetPanelArea()
        {
            var padding = 30;
            return new Rect(0, padding, position.width, position.height - padding);
        }
    }
}