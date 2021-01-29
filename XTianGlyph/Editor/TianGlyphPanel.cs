using UnityEngine;
using UnityEditor;

namespace XTianGlyph
{
    /// <summary>
    /// 田字形面板
    /// </summary>
    [System.Serializable]
    public class TianGlyphPanel : ITianGlyphPanel
    {
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

        private EditorWindow m_Parent = null;

        public ITianGlyphPanel LTPanel { get; set; }
        public ITianGlyphPanel LBPanel { get; set; }
        public ITianGlyphPanel RTPanel { get; set; }
        public ITianGlyphPanel RBPanel { get; set; }
        public Rect LTRect { get; private set; }
        public Rect LBRect { get; private set; }
        public Rect RTRect { get; private set; }
        public Rect RBRect { get; private set; }
        public float LTOffset { get; set; }
        public float LBOffset { get; set; }
        public float RTOffset { get; set; }
        public float RBOffset { get; set; }
        public bool LTOutline { get; set; }
        public bool LBOutline { get; set; }
        public bool RTOutline { get; set; }
        public bool RBOutline { get; set; }
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

        public void Reload()
        {
        }

        public void OnGUI(Rect pos)
        {
            m_Position = pos;

            HandleHorizontalResize();
            HandleVerticalResize();

            LTRect = new Rect(
                m_Position.x + k_SplitterWidth,
                m_Position.y + LTOffset,
                m_HorizontalRect.x,
                m_VertialLeftRect.y - m_Position.y - LTOffset);
            LTPanel?.OnGUI(LTRect);
            if (LTOutline)
            {
                DrawOutline(LTRect, 1f);
            }


            LBRect = new Rect(
                LTRect.x,
                LTRect.y + LTRect.height + k_SplitterWidth,
                LTRect.width,
                m_Position.height - LTRect.height - k_SplitterWidth * 2 + LBOffset);
            LBPanel?.OnGUI(LBRect);
            if (LBOutline)
            {
                DrawOutline(LBRect, 1f);
            }

            float panelLeft = m_HorizontalRect.x + 2 * k_SplitterWidth;
            float panelWidth = m_VertialRightRect.width - k_SplitterWidth * 3;
            float searchHeight = 0f;
            float panelTop = m_Position.y + searchHeight;
            float panelHeight = m_VertialRightRect.y - panelTop;

            RTRect = new Rect(
                panelLeft,
                panelTop + RTOffset,
                panelWidth,
                panelHeight - RTOffset);
            RTPanel?.OnGUI(RTRect);
            if (RTOutline)
            {
                DrawOutline(RTRect, 1f);
            }

            RBRect = new Rect(
                panelLeft,
                panelTop + panelHeight + k_SplitterWidth,
                panelWidth,
                (m_Position.height - panelHeight) - k_SplitterWidth * 2 + RBOffset);
            RBPanel?.OnGUI(RBRect);
            if (RBOutline)
            {
                DrawOutline(RBRect, 1f);
            }

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