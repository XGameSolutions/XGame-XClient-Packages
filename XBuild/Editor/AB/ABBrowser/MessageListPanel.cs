
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XBuild.AB;
using XTianGlyph;

namespace XBuild.AB.ABBrowser
{
    public class MessageListPanel : ITianGlyphPanel
    {
        const float k_ScrollbarPadding = 16f;
        const float k_BorderSize = 1f;

        private Vector2 m_ScrollPosition = Vector2.zero;
        private GUIStyle[] m_Style = new GUIStyle[2];
        private Vector2 m_Dimensions = new Vector2(0, 0);
        private List<ABAssetsInfo> m_SelectedAssets;
        private List<MessageSystem.Message> m_Messages;

        public MessageListPanel()
        {
            m_Style[0] = "OL EntryBackOdd";
            m_Style[1] = "OL EntryBackEven";
            m_Style[0].wordWrap = true;
            m_Style[1].wordWrap = true;
            m_Style[0].padding = new RectOffset(32, 0, 1, 4);
            m_Style[1].padding = new RectOffset(32, 0, 1, 4);
            m_Messages = new List<MessageSystem.Message>();
        }

        public void OnGUI(Rect fullPos)
        {
            Rect pos = new Rect(fullPos.x + k_BorderSize, fullPos.y + k_BorderSize, fullPos.width - 2 * k_BorderSize, fullPos.height - 2 * k_BorderSize);
            if (m_Dimensions.y == 0 || m_Dimensions.x != pos.width - k_ScrollbarPadding)
            {
                m_Dimensions.x = pos.width - k_ScrollbarPadding;
                m_Dimensions.y = 0;
                foreach (var message in m_Messages)
                {
                    m_Dimensions.y += m_Style[0].CalcHeight(new GUIContent(message.message), m_Dimensions.x);
                }
            }
            m_ScrollPosition = GUI.BeginScrollView(pos, m_ScrollPosition, new Rect(0, 0, m_Dimensions.x, m_Dimensions.y));
            int counter = 0;
            float runningHeight = 0.0f;
            foreach (var message in m_Messages)
            {
                int index = counter % 2;
                var content = new GUIContent(message.message);
                float height = m_Style[index].CalcHeight(content, m_Dimensions.x);

                GUI.Box(new Rect(0, runningHeight, m_Dimensions.x, height), content, m_Style[index]);
                GUI.DrawTexture(new Rect(0, runningHeight, 32f, 32f), message.icon);
                //TODO - cleanup formatting issues and switch to HelpBox
                //EditorGUI.HelpBox(new Rect(0, runningHeight, m_dimensions.x, height), message.message, (MessageType)message.severity);

                counter++;
                runningHeight += height;
            }
            GUI.EndScrollView();
        }

        public void Reload()
        {
        }

        public void SetSelectedAssets(List<ABAssetsInfo> assets)
        {
            m_SelectedAssets = assets;
            m_Messages.Clear();
            m_Dimensions.y = 0f;
            if (m_SelectedAssets != null)
            {
                foreach (var asset in m_SelectedAssets)
                {
                    m_Messages.AddRange(asset.GetMessages());
                }
            }
        }
    }
}