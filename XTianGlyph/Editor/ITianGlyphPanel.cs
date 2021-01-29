using UnityEngine;

namespace XTianGlyph
{
    /// <summary>
    /// 田字形面板的子面板
    /// </summary>
    public interface ITianGlyphPanel
    {
        void OnGUI(Rect rect);
        void Reload();
    }
}