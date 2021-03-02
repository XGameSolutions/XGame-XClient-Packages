using UnityEngine;
using XCommon.Editor;

namespace XRemoteDebug
{
    internal class PatchPanel : IRemoteDebugPanel, ITianGlyphPanelParent
    {
        private RemoteDebugWindow m_Window;
        private TianGlyphPanel m_Panel;
        private PatchLocalPanel m_LocalPanel;
        private PatchRemotePanel m_RemotePanel;

        public PatchPanel(RemoteDebugWindow window)
        {
            m_Window = window;
        }

        public void UploadNextFile(bool flag, string successedFileName = null)
        {
            m_LocalPanel.UploadNextFile(flag, successedFileName);
        }

        public void OnEnable()
        {
            if (m_Panel == null) m_Panel = new TianGlyphPanel(this, false);
            if (m_LocalPanel == null) m_LocalPanel = new PatchLocalPanel(this);
            if (m_RemotePanel == null) m_RemotePanel = new PatchRemotePanel(this);
            m_LocalPanel.OnEnable();
            m_RemotePanel.OnEnable();
            m_Panel.OnEnable();
            m_Panel.VertialLeftPercent = 1;
            m_Panel.VertialRightPercent = 1;
            m_Panel.SetLeftTopPanel(m_LocalPanel, true);
            m_Panel.SetRightTopPanel(m_RemotePanel, true);
        }

        public void Update()
        {
            m_LocalPanel?.Update();
            m_RemotePanel?.Update();
        }

        public void Repaint()
        {
            m_Window.Repaint();
        }

        public void OnGUI(Rect rect)
        {
            //GUI.Button(rect, "patch");
            m_Panel.OnGUI(rect);
        }

        public void Reload()
        {
        }
    }
}