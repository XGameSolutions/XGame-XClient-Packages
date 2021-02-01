/******************************************/
/*                                        */
/*     Copyright (c) 2020 monitor1394     */
/*     https://github.com/monitor1394     */
/*                                        */
/******************************************/

using System.Collections.Generic;

namespace XBuild.AB
{
    public enum AssetsCategory
    {
        None,
        Model,
        Scene,
        UI,
        Other
    }
    public enum AssetsType
    {
        None,
        Material,
        Texture,
        Prefab,
        Shader,
        Asset,
        Other
    }

    public class AssetsInfo
    {
        public AssetsCategory category;
        public AssetsType type;
        public string name;
        public string extension;
        public string path;
        public string abName;
        public long size;
        public int textureWidth;
        public int textureHeight;
        public int textureMaxSize;
        public List<string> m_RefABList = new List<string>();

        public int refCount { get { return m_RefABList.Count; } }

        public void AddRefAB(string abName)
        {
            if (!m_RefABList.Contains(abName))
            {
                m_RefABList.Add(abName);
            }
        }

        public string GetFirstRefAB()
        {
            return m_RefABList.Count > 0 ? m_RefABList[0] : null;
        }
    }
}