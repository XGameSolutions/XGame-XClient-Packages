using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UnityEditor.IMGUI.Controls;
using System;
using XCommon.Editor;
using XCommon.Runtime;

namespace XRemoteDebug
{
    [SerializeField]
    internal class PatchLocalPanel : ITianGlyphPanel
    {
        class Styles
        {
            public static readonly GUIStyle btnInvisible = "InvisibleButton";
            public static readonly GUIContent btnBack = new GUIContent("Back", "Back to previous folder");
            public static readonly GUIContent btnRefresh = new GUIContent("Back", "Back to previous folder");
            public static readonly GUIContent btnUpload = new GUIContent("Upload", "Upload selected files to remote client patch folder");
            public static readonly GUIContent btnCancel = new GUIContent("Cancel", "Cancel all selected files");
            public static readonly GUIContent abDir = new GUIContent("AB Dir", "The directory where assetbundle is in");
            public static readonly GUIContent abSource = new GUIContent("AB Source", "AB read from Directory or AssetDatabase");
            public static readonly GUIContent iconOpenFolder = EditorGUIUtility.IconContent("d_Collab.FolderMoved", "open folder");
            public static readonly GUIContent iconBack = EditorGUIUtility.IconContent("back@2x", "back to previous folder");
            public static readonly Texture iconFilter = EditorGUIUtility.IconContent("d_FilterByType@2x").image;
            public static GUIStyle speedStyle = new GUIStyle(GUI.skin.label);
        }
        [SerializeField] private string m_LocalPath = Application.dataPath;
        [SerializeField] private string[] m_FilterFiles = new string[] { ".meta", ".manifest", ".DS_Store", ".git" };
        private PatchPanel m_Parent;
        private EditorTable m_FileTree;
        private SearchField m_SearchField;
        private string m_SearchText;
        private List<PatchFileInfo> m_FileList = new List<PatchFileInfo>();
        private List<PatchFileInfo> m_WaitingUploadFileInfos = new List<PatchFileInfo>();
        private int m_FileTotalSize;
        private int m_FileTotalCount;
        private int m_FileSelectedSize;
        private int m_FileSelectedCount;

        private int m_UploadTotalSize;
        private int m_UploadTotalCount;
        private int m_UploadCurrSize;
        private int m_UploadFileSize;
        private int m_UploadCurrCount;
        private long m_UploadStartTick;
        private long m_UploadEndTick;
        private int m_UploadRemoteSpeed;
        private bool m_IsUploading = false;
        private bool m_IsUploaded = true;

        private RemoteDebugServer server
        {
            get
            {
                return RemoteDebugWindow.Instance.server;
            }
        }

        private int clientIndex
        {
            get
            {
                return RemoteDebugWindow.Instance.selectedClient == null ? -1 : RemoteDebugWindow.Instance.selectedClient.Index;
            }
        }

        public PatchLocalPanel()
        {
        }

        public void OnEnable(PatchPanel parent)
        {
            m_Parent = parent;
            if (m_SearchField == null) m_SearchField = new SearchField();
        }

        public void Update()
        {
        }

        public void Repaint()
        {
            m_Parent.Repaint();
        }

        public void OnGUI(Rect rect)
        {
            var gap = 0.5f;
            var hig = 18;
            var higStatus = 18;

            OnGUI_Path(new Rect(rect.x, rect.y, rect.width, hig));
            OnGUI_Menu(new Rect(rect.x, rect.y + hig + gap, rect.width, hig));
            OnGUI_Search(new Rect(rect.x, rect.y + 2 * (hig + gap), rect.width, hig));
            OnGUI_Table(new Rect(rect.x, rect.y + 3 * (hig + gap), rect.width,
                rect.height - 3 * (hig + gap) - higStatus));
            OnGUI_Status(new Rect(rect.x, rect.y + rect.height - higStatus, rect.width,
                higStatus));
        }

        public void Reload()
        {
        }

        private void UploadStart()
        {
            if (m_WaitingUploadFileInfos.Count == 0) return;
            m_IsUploading = true;
            m_IsUploaded = false;
            m_UploadTotalSize = 0;
            m_UploadTotalCount = 0;
            m_UploadFileSize = 0;
            m_UploadCurrSize = 0;
            m_UploadCurrCount = 0;
            m_UploadStartTick = DateTime.Now.Ticks;

            foreach (var info in m_WaitingUploadFileInfos)
            {
                m_UploadTotalSize += info.size;
                m_UploadTotalCount++;
            }
        }

        public void UploadNextFile(bool flag = true, string successedFileName = null)
        {
            if (!string.IsNullOrEmpty(successedFileName))
            {
                if (flag)
                {
                    m_UploadCurrCount++;
                    m_UploadCurrSize = 0;
                    m_UploadFileSize += (int)RemoteDebugUtil.GetFileSize(m_LocalPath + "/" + successedFileName);
                    m_Parent.UpdateLogStatus(m_LocalPath + "/" + successedFileName, "");
                    if (m_UploadFileSize == m_UploadTotalSize)
                    {
                        RemoteDebugWindow.Instance.ShowNotification(new GUIContent("Upload All File Success"));
                    }
                }
                else
                {
                    m_Parent.UpdateLogStatus(m_LocalPath + "/" + successedFileName, "failed: not same md5.");
                }
            }
            Debug.LogError("UploadNextFile:" + m_WaitingUploadFileInfos.Count);
            if (m_WaitingUploadFileInfos.Count == 0)
            {
                m_IsUploaded = true;
                m_UploadEndTick = DateTime.Now.Ticks;
                return;
            }
            var info = m_WaitingUploadFileInfos[0];
            m_WaitingUploadFileInfos.RemoveAt(0);
            var filePath = m_LocalPath + "/" + info.name;
            var fileSize = 0L;
            var md5 = RemoteDebugUtil.GetFileMd5(filePath, out fileSize);
            server?.RequestPatchUploadStart(clientIndex, info.name, (int)fileSize, md5);
            server?.RequestPatchUploadFile(clientIndex, filePath);
            m_Parent.AddLog(filePath, (int)fileSize);
            m_IsUploading = true;
        }

        public void FileUploading(string fileName, int size, int speed)
        {
            m_UploadCurrSize = size;
            m_UploadRemoteSpeed = speed;
            m_Parent.UpdateLogSize(m_LocalPath + "/" + fileName, size);
            Repaint();
        }

        private void OnGUI_Path(Rect rect)
        {
            var btnWid = 50;
            var rectTxt = new Rect(rect.x, rect.y, rect.width - btnWid, rect.height);
            m_LocalPath = GUI.TextField(rectTxt, m_LocalPath);

            var rectIcon = new Rect(rect.x + rectTxt.width + 1, rect.y, btnWid, rect.height);
            if (GUI.Button(rectIcon, new GUIContent("Open", "Open Floder")))
            {
                var path = EditorUtility.OpenFolderPanel("Open Floder", "", "");
                if (!string.IsNullOrEmpty(path))
                {
                    m_LocalPath = path;
                    RefreshFileList();
                }
            }
        }

        private void OnGUI_Menu(Rect rect)
        {
            var btnGap = 1;
            var btnWid = 53;
            var index = 0;

            if (GUI.Button(new Rect(rect.x + (index++) * (btnWid + btnGap), rect.y, btnWid, rect.height), Styles.btnBack))
            {
                if (!m_IsUploaded)
                {
                    RemoteDebugWindow.Instance.ShowNotification(new GUIContent("Operation not allowed: uploading ... "));
                    return;
                }
                m_LocalPath = RemoteDebugUtil.GetParentPath(m_LocalPath);
                RefreshFileList();
            }
            if (GUI.Button(new Rect(rect.x + (index++) * (btnWid + btnGap), rect.y, btnWid, rect.height), Styles.btnUpload))
            {
                if (m_WaitingUploadFileInfos.Count <= 0)
                {
                    RemoteDebugWindow.Instance.ShowNotification(new GUIContent("no selected files."));
                }
                else
                {

                    var files = string.Join("\n", m_WaitingUploadFileInfos);
                    if (EditorUtility.DisplayDialog("Upload selected files", "Upload all selected files?\n" + files, "Sure", "Cancel"))
                    {
                        UploadStart();
                        UploadNextFile();
                    }
                }
            }
            if (GUI.Button(new Rect(rect.x + (index++) * (btnWid + btnGap), rect.y, btnWid, rect.height), Styles.btnCancel))
            {
                foreach (var info in m_FileList) info.itemSelected = false;
                m_FileTree.Repaint();
            }

            var filter = new GUIContent(Styles.iconFilter, "Filter Files:\n" + string.Join("|", m_FilterFiles));
            GUI.Button(new Rect(rect.x + rect.width - 25, rect.y, 25, rect.height), filter);
        }

        private void OnGUI_Search(Rect rect)
        {
            var newSearch = m_SearchField.OnGUI(rect, m_SearchText);
            if (string.IsNullOrEmpty(m_SearchText))
            {
                if (!string.IsNullOrEmpty(newSearch))
                {
                    m_SearchText = newSearch;
                    RefreshFileList();
                }
            }
            else if (!m_SearchText.Equals(newSearch))
            {
                m_SearchText = newSearch;
                RefreshFileList();
            }
        }

        private void OnGUI_Table(Rect rect)
        {
            if (m_FileTree == null)
            {
                var column = PatchFileInfo.totalColumn;
                m_FileTree = EditorTable.CreateTable(column);
                for (int i = 0; i < column; i++)
                {
                    m_FileTree.SetColumnHeader(i, PatchFileInfo.GetColumnHeader(i));
                }
                m_FileTree.OnSelectionChanged = OnSelectedFileList;
                m_FileTree.OnDoubleClickedItem = OnDoubleClickedItem;
                RefreshFileList();
                m_FileTree.Reload();
            }
            m_FileTree.OnGUI(rect);
        }

        private void OnGUI_Status(Rect rect)
        {
            if (m_IsUploading)
            {
                var bcolor = Color.gray;
                bcolor.a = 0.1f;
                EditorGUI.DrawRect(rect, bcolor);
                var fcolor = Color.blue;
                fcolor.a = 0.4f;
                var currSize = m_UploadFileSize + m_UploadCurrSize;
                var progress = m_UploadTotalSize == 0 ? 0 : (float)currSize / m_UploadTotalSize;
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width * progress, rect.height), fcolor);
                GUI.Label(rect, string.Format("{0} / {1}, {2} / {3}",
                    EditorUtility.FormatBytes(currSize), EditorUtility.FormatBytes(m_UploadTotalSize),
                     m_UploadCurrCount, m_UploadTotalCount));

                var time = ((m_IsUploaded ? m_UploadEndTick : DateTime.Now.Ticks) - m_UploadStartTick) / 10000000;
                var speed = time == 0 ? 0 : currSize / time;
                Styles.speedStyle.alignment = TextAnchor.MiddleRight;
                GUI.Label(rect, string.Format("{0}/s, {1}/s {2}", EditorUtility.FormatBytes(speed),
                    EditorUtility.FormatBytes(m_UploadRemoteSpeed),
                    XTimeUtil.FormatTime(time)), Styles.speedStyle);
            }
            else
            {
                GUI.Label(rect, string.Format("{0} / {1}, {2} / {3}",
                EditorUtility.FormatBytes(m_FileSelectedSize), EditorUtility.FormatBytes(m_FileTotalSize),
                    m_FileSelectedCount, m_FileTotalCount));
            }
        }

        private void OnSelectedFileList(List<IEditorTableItemInfo> infoList)
        {
            m_WaitingUploadFileInfos.Clear();
            if (infoList != null)
            {
                foreach (var info in infoList)
                {
                    var fileInfo = info as PatchFileInfo;
                    if (fileInfo.type == 2)
                    {
                        m_WaitingUploadFileInfos.Add(fileInfo);
                    }
                }
            }
            UpdateSelectedFileCountAndSize();
        }

        private void OnDoubleClickedItem(IEditorTableItemInfo item)
        {
            if (!m_IsUploaded)
            {
                RemoteDebugWindow.Instance.ShowNotification(new GUIContent("Operation not allowed: uploading ... "));
                return;
            }
            var info = item as PatchFileInfo;
            if (info != null)
            {
                switch (info.type)
                {
                    case 0:
                        m_LocalPath = info.assetPath;
                        RefreshFileList();
                        break;
                    case 1:
                        m_LocalPath = m_LocalPath + "/" + info.name;
                        RefreshFileList();
                        break;
                    case 2:
                        //info.itemSelected = !info.itemSelected;
                        //UpdateSelectedFileCountAndSize();
                        break;
                }
            }
        }

        private void UpdateSelectedFileCountAndSize()
        {
            m_FileSelectedCount = 0;
            m_FileSelectedSize = 0;
            foreach (var info in m_WaitingUploadFileInfos)
            {
                m_FileSelectedCount++;
                m_FileSelectedSize += info.size;
            }
        }

        private void RefreshFileList()
        {
            if (m_FileTree == null) return;
            if (!Directory.Exists(m_LocalPath))
            {
                Debug.LogError("lcoalPath not exist:" + m_LocalPath);
                return;
            }
            m_FileList.Clear();
            m_FileTotalSize = 0;
            m_FileTotalCount = 0;

            var back = new PatchFileInfo();
            var parent = RemoteDebugUtil.GetParentPath(m_LocalPath);
            back.name = "..";
            back.assetPath = parent;
            back.size = 0;
            back.type = 0;
            back.datetime = new FileInfo(parent).LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss");
            back.assetIcon = RemoteDebugStyles.iconFolder;
            m_FileList.Add(back);

            foreach (var path in Directory.GetDirectories(m_LocalPath))
            {
                var folder = Path.GetFileName(path);
                if (IsFilter(path) || IsFilterFolder(folder) || !IsSearchFileName(folder)) continue;
                var fi = new FileInfo(path);
                var info = new PatchFileInfo();
                info.name = folder;
                info.assetPath = path;
                info.size = 0;
                info.type = 1;
                info.datetime = fi.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss");
                info.assetIcon = RemoteDebugStyles.iconFolder;
                m_FileList.Add(info);
                m_FileTotalCount++;
            }
            foreach (var path in Directory.GetFiles(m_LocalPath))
            {
                if (IsFilter(path)) continue;
                var fileName = Path.GetFileName(path);
                if (!IsSearchFileName(fileName)) continue;
                var fi = new FileInfo(path);
                var info = new PatchFileInfo();
                info.name = fileName;
                info.assetPath = path;
                info.size = (int)fi.Length;
                info.type = 2;
                info.datetime = fi.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss");
                info.assetIcon = RemoteDebugStyles.iconFile;
                m_FileList.Add(info);
                m_FileTotalSize += info.size;
                m_FileTotalCount++;
            }
            m_FileTree.UpdateInfoList(m_FileList);
            Repaint();
        }



        private bool IsFilterFolder(string folderName)
        {
            return folderName.StartsWith(".");
        }

        private bool IsFilter(string path)
        {
            foreach (var filter in m_FilterFiles)
            {
                if (path.Contains(filter)) return true;
            }
            return false;
        }

        private bool IsSearchFileName(string fileName)
        {
            return string.IsNullOrEmpty(m_SearchText) ? true : fileName.Contains(m_SearchText);
        }
    }
}