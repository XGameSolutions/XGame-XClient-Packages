

using System.Collections.Generic;
using UnityEngine;

namespace XCommon.Editor
{
    public interface IEditorTableItemInfo
    {
        int itemId { get; }
        bool itemDisable { get; }
        string displayName { get; }
        string assetPath { get; }
        Texture2D assetIcon { get; }
        Texture2D assetDisableIcon { get; }
        List<IEditorTableItemInfo> children { get; }
        string GetColumnString(int column);
        object GetColumnOrder(int column);
    }
}