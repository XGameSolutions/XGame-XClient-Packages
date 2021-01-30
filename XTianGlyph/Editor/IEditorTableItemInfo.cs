

using UnityEngine;

namespace XTianGlyph
{
    public interface IEditorTableItemInfo
    {
        int itemId { get; }
        string displayName { get; }
        string assetPath { get; }
        string GetColumnString(int column);
        object GetColumnOrder(int column);
    }
}