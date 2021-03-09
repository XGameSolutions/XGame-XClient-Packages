
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using XCommon.Editor;

namespace XRemoteDebug
{
    internal class PatchLogInfo
    {
        public string name;
        public string path;
        public string status;
        public bool done;


        public long currSize;
        public long totalSize;
        public string datetime;


        public string GetProgress()
        {
            if(done) return "100%";
            return totalSize > 0 ? string.Format("{0:f0}%", currSize * 1.0f / totalSize * 100) : "0%";
        }

        public override string ToString()
        {
            return name;
        }
    }
}