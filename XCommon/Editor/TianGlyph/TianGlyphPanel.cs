using UnityEngine;
using UnityEditor;

namespace XCommon.Editor
{
    /// <summary>
    /// 田字形面板
    /// </summary>
    [System.Serializable]
    public class TianGlyphPanel : ITianGlyphPanel
    {
        private class PanelInfo
        {
            internal ITianGlyphPanel panel;
            internal float topOffset;
            internal float bottomOffset;
            internal bool outline = true;
            internal Rect rect;
            internal void SetInfo(ITianGlyphPanel panel, float topOffset, float bottomOffset, bool outline)
            {
                this.panel = panel;
                this.topOffset = topOffset;
                this.bottomOffset = bottomOffset;
                this.outline = outline;
                if (panel != null)
                {
                    panel.Reload();
                }
            }

            internal void OnGUI(Rect rect)
            {
                this.rect = new Rect(rect.x, rect.y + topOffset, rect.width, rect.height - topOffset - bottomOffset);
                panel?.OnGUI(this.rect);
                if (outline) DrawOutline(this.rect, 1f);
            }
        }
        const float k_SplitterWidth = 3f;
        [SerializeField] private float m_HorizontalPercent;
        [SerializeField] private float m_VertialLeftPercent;
        [SerializeField] private float m_VertialRightPercent;


        private Rect m_Position;
        private Rect m_HorizontalRect;
        private Rect m_VertialLeftRect;
        private Rect m_VertialRightRect;
        private bool m_HorzontalResizing;
        private bool m_VertialLeftResizing;
        private bool m_VertialRightResizing;

        private PanelInfo[] m_PanelInfos;
        private EditorWindow m_Parent = null;

        public Rect LeftTopRect { get { return m_PanelInfos[0].rect; } }
        public Rect LeftBottomRect { get { return m_PanelInfos[1].rect; } }
        public Rect RightTopRect { get { return m_PanelInfos[2].rect; } }
        public Rect RightBottomRect { get { return m_PanelInfos[3].rect; } }

        public float SplitterWidth { get { return k_SplitterWidth; } }

        public TianGlyphPanel(EditorWindow parent)
        {
            m_Parent = parent;
            m_HorizontalPercent = 0.4f;
            m_VertialLeftPercent = 0.7f;
            m_VertialRightPercent = 0.85f;
            m_HorizontalRect = new Rect(
                (int)(m_Position.x + m_Position.width * m_HorizontalPercent),
                m_Position.y,
                k_SplitterWidth,
                m_Position.height
            );
            m_VertialLeftRect = new Rect(
                m_Position.x,
                (int)(m_Position.y + m_HorizontalRect.height * m_VertialLeftPercent),
                (m_HorizontalRect.width) - k_SplitterWidth,
                k_SplitterWidth
            );
            m_VertialRightRect = new Rect(
                m_HorizontalRect.x,
                (int)(m_Position.y + m_HorizontalRect.height * m_VertialRightPercent),
                (m_Position.width - m_HorizontalRect.width) - k_SplitterWidth,
                k_SplitterWidth
            );
        }

        public void OnEnable()
        {
            m_PanelInfos = new PanelInfo[4]{
                new PanelInfo(),
                new PanelInfo(),
                new PanelInfo(),
                new PanelInfo()
            };
        }

        public void OnDisable()
        {
        }

        public void SetLeftTopPanel(ITianGlyphPanel panel, bool outline = true, float topOffset = 0f, float bottomOffset = 0f)
        {
            m_PanelInfos[0].SetInfo(panel, topOffset, bottomOffset, outline);
        }

        public void SetLeftBottomPanel(ITianGlyphPanel panel, bool outline = true, float topOffset = 0f, float bottomOffset = 0f)
        {
            m_PanelInfos[1].SetInfo(panel, topOffset, bottomOffset, outline);
        }
        public void SetRightTopPanel(ITianGlyphPanel panel, bool outline = true, float topOffset = 0f, float bottomOffset = 0f)
        {
            m_PanelInfos[2].SetInfo(panel, topOffset, bottomOffset, outline);
        }
        public void SetRigthBottomPanel(ITianGlyphPanel panel, bool outline = true, float topOffsetset = 0f, float bottomOffset = 0f)
        {
            m_PanelInfos[3].SetInfo(panel, topOffsetset, bottomOffset, outline);
        }

        public void Reload()
        {
        }

        public void OnGUI(Rect pos)
        {
            m_Position = pos;

            HandleHorizontalResize();
            HandleVerticalResize();

            var LTRect = new Rect(
                m_Position.x + k_SplitterWidth,
                m_Position.y,
                m_HorizontalRect.x,
                m_VertialLeftRect.y - m_Position.y);
            m_PanelInfos[0].OnGUI(LTRect);

            var LBRect = new Rect(
                LTRect.x,
                LTRect.y + LTRect.height + k_SplitterWidth,
                LTRect.width,
                m_Position.height - LTRect.height - k_SplitterWidth * 2);
            m_PanelInfos[1].OnGUI(LBRect);

            float panelLeft = m_HorizontalRect.x + 2 * k_SplitterWidth;
            float panelWidth = m_VertialRightRect.width - k_SplitterWidth * 3;
            float panelTop = m_Position.y;
            float panelHeight = m_VertialRightRect.y - panelTop;

            var RTRect = new Rect(
                panelLeft,
                panelTop,
                panelWidth,
                panelHeight);
            m_PanelInfos[2].OnGUI(RTRect);

            var RBRect = new Rect(
                panelLeft,
                panelTop + panelHeight + k_SplitterWidth,
                panelWidth,
                (m_Position.height - panelHeight) - k_SplitterWidth * 2);
            m_PanelInfos[3].OnGUI(RBRect);

            if (m_HorzontalResizing || m_VertialLeftResizing || m_VertialRightResizing)
            {
                m_Parent.Repaint();
            }
        }

        private void HandleHorizontalResize()
        {
            m_HorizontalRect.x = (int)(m_Position.width * m_HorizontalPercent);
            m_HorizontalRect.height = m_Position.height;

            EditorGUIUtility.AddCursorRect(m_HorizontalRect, MouseCursor.ResizeHorizontal);
            if (Event.current.type == EventType.MouseDown
                && m_HorizontalRect.Contains(Event.current.mousePosition))
            {
                m_HorzontalResizing = true;
            }
            if (m_HorzontalResizing)
            {
                m_HorizontalPercent = Mathf.Clamp(Event.current.mousePosition.x / m_Position.width, 0.1f, 0.9f);
                m_HorizontalRect.x = (int)(m_Position.width * m_HorizontalPercent);
            }
            if (Event.current.type == EventType.MouseUp)
            {
                m_HorzontalResizing = false;
            }
        }

        private void HandleVerticalResize()
        {
            m_VertialRightRect.x = m_HorizontalRect.x;
            m_VertialRightRect.y = (int)(m_HorizontalRect.height * m_VertialRightPercent);
            m_VertialRightRect.width = m_Position.width - m_HorizontalRect.x;
            m_VertialLeftRect.y = (int)(m_HorizontalRect.height * m_VertialLeftPercent);
            m_VertialLeftRect.width = m_VertialRightRect.width;


            EditorGUIUtility.AddCursorRect(m_VertialRightRect, MouseCursor.ResizeVertical);
            if (Event.current.type == EventType.MouseDown && m_VertialRightRect.Contains(Event.current.mousePosition))
                m_VertialRightResizing = true;

            EditorGUIUtility.AddCursorRect(m_VertialLeftRect, MouseCursor.ResizeVertical);
            if (Event.current.type == EventType.MouseDown && m_VertialLeftRect.Contains(Event.current.mousePosition))
                m_VertialLeftResizing = true;


            if (m_VertialRightResizing)
            {
                m_VertialRightPercent = Mathf.Clamp(Event.current.mousePosition.y / m_HorizontalRect.height, 0.1f, 0.98f);
                m_VertialRightRect.y = (int)(m_HorizontalRect.height * m_VertialRightPercent);
            }
            else if (m_VertialLeftResizing)
            {
                m_VertialLeftPercent = Mathf.Clamp(Event.current.mousePosition.y / m_HorizontalRect.height, 0.1f, 0.98f);
                m_VertialLeftRect.y = (int)(m_HorizontalRect.height * m_VertialLeftPercent);
            }

            if (Event.current.type == EventType.MouseUp)
            {
                m_VertialRightResizing = false;
                m_VertialLeftResizing = false;
            }
        }

        private static void DrawOutline(Rect rect, float size)
        {
            Color color = new Color(0.6f, 0.6f, 0.6f, 1.333f);
            if (EditorGUIUtility.isProSkin)
            {
                color.r = 0.12f;
                color.g = 0.12f;
                color.b = 0.12f;
            }

            if (Event.current.type != EventType.Repaint)
                return;

            Color orgColor = GUI.color;
            GUI.color = GUI.color * color;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, size), EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - size, rect.width, size), EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.y + 1, size, rect.height - 2 * size), EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(new Rect(rect.xMax - size, rect.y + 1, size, rect.height - 2 * size), EditorGUIUtility.whiteTexture);

            GUI.color = orgColor;
        }
    }
}