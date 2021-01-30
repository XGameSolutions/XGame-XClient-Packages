using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace XTianGlyph
{
    public static class TianGlyphUtil
    {
        public static MultiColumnHeaderState.Column GetColumn(float width, float minWidth, float maxWidth,
            string name, string nameTooltip = null, bool canSort = true, bool autoResize = true,
            TextAlignment headerTextAlignment = TextAlignment.Left)
        {
            var info = new MultiColumnHeaderState.Column();
            info.headerContent = new GUIContent(name, nameTooltip);
            info.minWidth = minWidth;
            info.width = width;
            info.maxWidth = maxWidth;
            info.headerTextAlignment = headerTextAlignment;
            info.canSort = canSort;
            info.autoResize = autoResize;
            return info;
        }

        public static void SetColumn(MultiColumnHeaderState mchs, int column, float width, float minWidth, float maxWidth,
            string name, string nameTip, bool canSort = true, bool autoResize = true,
            TextAlignment headerTextAlignment = TextAlignment.Left)
        {
            if (column >= 0 && column < mchs.columns.Length)
            {
                var info = mchs.columns[column];
                info.headerContent = new GUIContent(name, nameTip);
                info.minWidth = minWidth;
                info.width = width;
                info.maxWidth = maxWidth;
                info.headerTextAlignment = headerTextAlignment;
                info.canSort = canSort;
                info.autoResize = autoResize;
            }
        }
    }
}
