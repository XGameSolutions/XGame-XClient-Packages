
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using XCommon.Editor;
using XCommon.Runtime;

namespace XRemoteDebug
{
    internal class PatchLogPanel : ITianGlyphPanel
    {
        const float k_ScrollbarPadding = 0f;
        const float k_BorderSize = 1f;

        private Vector2 m_ScrollPosition = Vector2.zero;
        private GUIStyle[] m_Style = new GUIStyle[2];
        private Vector2 m_Dimensions = new Vector2(0, 0);
        private List<PatchLogInfo> m_Logs = new List<PatchLogInfo>();
        private PatchPanel m_Parent;

        public PatchLogPanel()
        {
        }

        public void OnEnable(PatchPanel parent)
        {
            m_Parent = parent;
            m_Style[0] = "OL EntryBackOdd";
            m_Style[1] = "OL EntryBackEven";
            m_Style[0].wordWrap = true;
            m_Style[1].wordWrap = true;
            m_Style[0].padding = new RectOffset(5, 0, 1, 1);
            m_Style[1].padding = new RectOffset(5, 0, 1, 1);
        }

        public void OnGUI(Rect fullPos)
        {
            Rect pos = new Rect(fullPos.x + k_BorderSize, fullPos.y + k_BorderSize, fullPos.width - 2 * k_BorderSize, fullPos.height - 2 * k_BorderSize);
            var maxNameWidth = 0f;
            m_Dimensions.x = pos.width - k_ScrollbarPadding;
            m_Dimensions.y = 0;
            foreach (var message in m_Logs)
            {
                m_Dimensions.y += m_Style[0].CalcHeight(new GUIContent(message.name), m_Dimensions.x);
                var size = m_Style[0].CalcSize(new GUIContent(message.name));
                if (size.x > maxNameWidth) maxNameWidth = size.x;
            }
            m_ScrollPosition = GUI.BeginScrollView(pos, m_ScrollPosition, new Rect(0, 0, m_Dimensions.x, m_Dimensions.y));
            int counter = 0;
            float runningHeight = 0.0f;
            foreach (var log in m_Logs)
            {
                int index = counter % 2;
                var content = new GUIContent(log.name);
                float height = m_Style[index].CalcHeight(content, m_Dimensions.x);
                var rect1 = new Rect(0, runningHeight, maxNameWidth, height);

                GUI.Box(new Rect(0, runningHeight, m_Dimensions.x, height), "", m_Style[index]);
                GUI.Label(rect1, content);

                var rect2 = new Rect(rect1.x + maxNameWidth + 10, rect1.y, 50, height);
                GUI.Label(rect2, log.GetProgress());

                var rect3 = new Rect(rect1.x + maxNameWidth + 10 + 60, rect1.y, 300, height);
                GUI.Label(rect3, log.status);
                //GUI.DrawTexture(new Rect(0, runningHeight, 32f, 32f), message.icon);

                counter++;
                runningHeight += height;
            }
            GUI.EndScrollView();
        }

        public void Reload()
        {
        }

        public void AddLog(string filePath, int totalSize)
        {
            var log = new PatchLogInfo()
            {
                name = Path.GetFileName(filePath),
                path = filePath,
                datetime = XTimeUtil.GetNowTime(),
                totalSize = totalSize
            };
            Debug.LogError("AddLog:" + filePath);
            m_Logs.Add(log);
        }

        public void UpdateLogSize(string filePath, long currSize)
        {
            var log = GetLog(filePath);
            if (log == null)
            {
                Debug.LogError("UpdateLog ERROR:can't find log:" + filePath);
                return;
            }
            log.currSize = currSize;
        }

        public void UpdateLogStatus(string filePath, string msg)
        {
            var log = GetLog(filePath);
            if (log == null)
            {
                Debug.LogError("UpdateLog ERROR:can't find log:" + filePath);
                return;
            }
            log.done = true;
            log.status = msg;
        }

        private PatchLogInfo GetLog(string filePath)
        {
            foreach (var log in m_Logs)
            {
                if (log.path.Equals(filePath)) return log;
            }
            return null;
        }
    }
}