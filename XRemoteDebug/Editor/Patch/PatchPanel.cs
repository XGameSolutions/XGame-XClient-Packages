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
        private PatchLogPanel m_LogPanel;

        public PatchPanel(RemoteDebugWindow window)
        {
            m_Window = window;
        }

        public ITianGlyphPanel GetStatusPanel()
        {
            return m_LogPanel;
        }

        public void UploadNextFile(bool flag, string successedFileName = null)
        {
            m_LocalPanel.UploadNextFile(flag, successedFileName);
        }

        public void FileUploading(string fileName, long size, int speed)
        {
            m_LocalPanel.FileUploading(fileName, size, speed);
        }

        public void OnEnable()
        {
            if (m_Panel == null) m_Panel = new TianGlyphPanel(false);
            if (m_LocalPanel == null) m_LocalPanel = new PatchLocalPanel();
            if (m_RemotePanel == null) m_RemotePanel = new PatchRemotePanel();
            if (m_LogPanel == null) m_LogPanel = new PatchLogPanel();
            m_LocalPanel.OnEnable(this);
            m_RemotePanel.OnEnable(this);
            m_LogPanel.OnEnable(this);
            m_Panel.OnEnable(this);
            m_Panel.VertialLeftPercent = 1;
            m_Panel.VertialRightPercent = 1;
            m_Panel.SetLeftTopPanel(m_LocalPanel, true);
            m_Panel.SetRightTopPanel(m_RemotePanel, true);
            m_Panel.SetRigthBottomPanel(m_LogPanel, true);
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

        public void AddLog(string filePath, int totalSize)
        {
            m_LogPanel.AddLog(filePath, totalSize);
        }

        public void UpdateLogSize(string filePath, long currSize)
        {
            m_LogPanel.UpdateLogSize(filePath, currSize);
        }

        public void UpdateLogStatus(string filePath, string msg)
        {
            m_LogPanel.UpdateLogStatus(filePath, msg);
        }
    }
}