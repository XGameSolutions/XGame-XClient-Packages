

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace XBuild.AB.ABBrowser
{
    public class ABAssetsInfo : AssetsInfo
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

            if (String.IsNullOrEmpty(abName) && refCount > 0)
            {
                //TODO - refine the parent list to only include those in the current asset list
                var message = name + "\n" + "Is auto included in bundle(s) due to parent(s): \n";
                foreach (var parent in m_RefABList)
                {
                    message += parent + ", ";
                }
                Debug.LogError("msg:"+message);
                message = message.Substring(0, message.Length - 2);//remove trailing comma.
                messages.Add(new MessageSystem.Message(message, MessageType.Info));
            }
            messages.Add(new MessageSystem.Message(name + "\n" + "Path: " + path, MessageType.Info));
            return messages;
        }
    }
}