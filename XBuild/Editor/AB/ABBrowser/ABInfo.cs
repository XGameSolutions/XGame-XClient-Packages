

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace XBuild.AB.ABBrowser
{
    public enum ABType
    {
        All,
        Model,
        Scene,
        UI,
        Other,
        Dep
    }

    public class ABInfo
    {
        public ABType type;
        public string name;
        public long size;
        public long depSize;
        private List<string> m_DepABList = new List<string>();
        private List<string> m_RefABList = new List<string>();

        public bool AddDepAB(string abName)
        {
            if (!m_DepABList.Contains(abName))
            {
                m_DepABList.Add(abName);
                return true;
            }
            return false;
        }

        public bool AddRefAB(string abName)
        {
            if (!m_RefABList.Contains(abName))
            {
                m_RefABList.Add(abName);
                return true;
            }
            return false;
        }

        public string GetSizeStr()
        {
            return size == 0 ? "--" : EditorUtility.FormatBytes(size);
        }
        public string GetDepSizeStr()
        {
            return size == 0 ? "--" : EditorUtility.FormatBytes(depSize);
        }
        public string GetTotalSizeStr()
        {
            return size == 0 ? "--" : EditorUtility.FormatBytes(totalSize);
        }

        public long totalSize { get { return size + depSize; } }
        public int depCount { get { return m_DepABList.Count; } }
        public int refCount { get { return m_RefABList.Count; } }
    }
}   