using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UCL.NetworkLib
{
#if UNITY_EDITOR
    [Core.ATTR.EnableUCLEditor]
#endif
    [CreateAssetMenu(fileName = "New CsvDownloader", menuName = "UCL/Downloader/CsvDownloader")]
    public class UCL_CsvDownloader : ScriptableObject
    {
        const string DownloadTemplate = "https://docs.google.com/spreadsheets/d/{0}/export?format=csv&gid={1}";
        public string DownloadPath { get { return string.Format(DownloadTemplate, m_TableId, m_SheetId); } }
        /// <summary>
        /// Table id on Google Spreadsheet.
        /// </summary>
        public string m_TableId = string.Empty;
        public int m_SheetId = 0;
        public string m_SaveFolder = "";
        public string m_FileName = "Localize.csv";
        public string SavePath { get { return Path.Combine(m_SaveFolder, m_FileName); } }

        [UCL.Core.ATTR.UCL_FunctionButton]
        public void StartDownload()
        {
#if UNITY_EDITOR
            if (string.IsNullOrEmpty(m_SaveFolder))
            {
                var path = UCL.Core.EditorLib.AssetDatabaseMapper.GetAssetPath(this);
                m_SaveFolder = Core.FileLib.Lib.RemoveFolderPath(path, 1);
            }
#endif
            UCL.Core.EnumeratorLib.UCL_CoroutineManager.StartCoroutine(UCL.Core.WebRequestLib.Download(DownloadPath, delegate (byte[] data) {
                File.WriteAllBytes(SavePath, data);
#if UNITY_EDITOR
                UCL.Core.EditorLib.AssetDatabaseMapper.Refresh();
#endif
#if UNITY_EDITOR_WIN
                Core.FileLib.WindowsLib.OpenAssetExplorer(SavePath);
#endif
            }));
        }
    }
}