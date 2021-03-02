

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using XCommon.Editor;

namespace XBuild.AB.ABBrowser
{
    internal class ABAssetsInfo : AssetsInfo, IEditorTableItemInfo
    {
        internal static Color k_LightGrey = Color.grey * 1.5f;
        private MessageSystem.MessageState m_AssetMessages = new MessageSystem.MessageState();

        public string GetSizeString()
        {
            return size == 0 ? "--" : EditorUtility.FormatBytes(size);
        }
        public string GetWidthString()
        {
            return textureWidth == 0 ? "--" : textureWidth.ToString();
        }
        public string GetHeightString()
        {
            return textureHeight == 0 ? "--" : textureHeight.ToString();
        }
        public string GetMaxSizeString()
        {
            return textureMaxSize == 0 ? "--" : textureMaxSize.ToString();
        }

        public string GetABNameString()
        {
            if (string.IsNullOrEmpty(abName)) return "auto";
            else return abName;
        }

        internal Color GetColor()
        {
            if (System.String.IsNullOrEmpty(abName))
                return k_LightGrey;
            else
                return Color.white;
        }


        public string displayName { get { return name; } }
        public int itemId { get { return path.GetHashCode(); } }
        public bool itemDisable { get; set; }
        public bool itemSelected { get; set; }
        public string assetPath { get { return path; } }
        public Texture2D assetIcon { get { return AssetDatabase.GetCachedIcon(path) as Texture2D; } }
        public Texture2D assetDisableIcon { get; set; }
        public List<IEditorTableItemInfo> children { get; set; }

        public static int totalColumn { get { return 8; } }
        public static MultiColumnHeaderState.Column GetColumnHeader(int column)
        {
            switch (column)
            {
                case 0: return TianGlyphUtil.GetColumn(200, 100, 500, "Asset", "");
                case 1: return TianGlyphUtil.GetColumn(50, 10, 50, "Ext", "");
                case 2: return TianGlyphUtil.GetColumn(200, 100, 400, "AB", "");
                case 3: return TianGlyphUtil.GetColumn(30, 20, 50, "Ref", "");
                case 4: return TianGlyphUtil.GetColumn(60, 20, 100, "Size", "");
                case 5: return TianGlyphUtil.GetColumn(50, 20, 100, "Width", "");
                case 6: return TianGlyphUtil.GetColumn(50, 20, 100, "Height", "");
                case 7: return TianGlyphUtil.GetColumn(60, 20, 100, "MaxSize", "");
                default: return TianGlyphUtil.GetColumn(100, 50, 400, "Unknow", "");
            }
        }

        public string GetColumnString(int column)
        {
            switch (column)
            {
                case 0: return name;
                case 1: return extension;
                case 2: return GetABNameString();
                case 3: return refCount.ToString();
                case 4: return GetSizeString();
                case 5: return GetWidthString();
                case 6: return GetHeightString();
                case 7: return GetMaxSizeString();
                default: return "unkown:" + column;
            }
        }
        public object GetColumnOrder(int column)
        {
            switch (column)
            {
                case 0: return name;
                case 1: return extension;
                case 2: return abName;
                case 3: return refCount;
                case 4: return size;
                case 5: return textureWidth;
                case 6: return textureHeight;
                case 7: return textureMaxSize;
                default: return name;
            }
        }

        internal bool IsMessageSet(MessageSystem.MessageFlag flag)
        {
            return m_AssetMessages.IsSet(flag);
        }
        internal void SetMessageFlag(MessageSystem.MessageFlag flag, bool on)
        {
            m_AssetMessages.SetFlag(flag, on);
        }
        internal MessageType HighestMessageLevel()
        {
            return m_AssetMessages.HighestMessageLevel();
        }
        internal IEnumerable<MessageSystem.Message> GetMessages()
        {
            List<MessageSystem.Message> messages = new List<MessageSystem.Message>();
            if (IsMessageSet(MessageSystem.MessageFlag.SceneBundleConflict))
            {
                // var message = name + "\n";
                // if (isScene)
                //     message += "Is a scene that is in a bundle with non-scene assets. Scene bundles must have only one or more scene assets.";
                // else
                //     message += "Is included in a bundle with a scene. Scene bundles must have only one or more scene assets.";
                // messages.Add(new MessageSystem.Message(message, MessageType.Error));
            }
            if (IsMessageSet(MessageSystem.MessageFlag.DependencySceneConflict))
            {
                var message = name + "\n";
                message += MessageSystem.GetMessage(MessageSystem.MessageFlag.DependencySceneConflict).message;
                messages.Add(new MessageSystem.Message(message, MessageType.Error));
            }
            if (IsMessageSet(MessageSystem.MessageFlag.AssetsDuplicatedInMultBundles))
            {
                // var bundleNames = AssetBundleModel.Model.CheckDependencyTracker(this);
                // string message = name + "\n" + "Is auto-included in multiple bundles:\n";
                // foreach (var bundleName in bundleNames)
                // {
                //     message += bundleName + ", ";
                // }
                // message = message.Substring(0, message.Length - 2);//remove trailing comma.
                // messages.Add(new MessageSystem.Message(message, MessageType.Warning));
            }

            if (refCount > 0)
            {
                if (String.IsNullOrEmpty(abName))
                {
                    var message = name + "\n" + "Is auto included in bundle(s) due to parent(s): \n";
                    foreach (var parent in m_RefABList)
                    {
                        message += "  " + parent + "\n";
                    }
                    message = message.Substring(0, message.Length - 1);
                    messages.Add(new MessageSystem.Message(message, MessageType.Warning));
                }
                else if (refCount > 1)
                {
                    var message = name + "\n" + "Is ref by parent(s): \n";
                    foreach (var parent in m_RefABList)
                    {
                        message += "  " + parent + "\n";
                    }
                    message = message.Substring(0, message.Length - 1);
                    messages.Add(new MessageSystem.Message(message, MessageType.Info));
                }
            }
            messages.Add(new MessageSystem.Message(name + "\n" + "Path: " + path, MessageType.Info));
            return messages;
        }
    }
}