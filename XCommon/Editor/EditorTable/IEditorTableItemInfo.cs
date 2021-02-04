

using UnityEngine;

namespace XCommon.Editor
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