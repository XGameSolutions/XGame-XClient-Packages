using UnityEngine;

namespace XCommon.Editor
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