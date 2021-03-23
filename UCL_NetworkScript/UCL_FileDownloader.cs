using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace UCL.NetworkLib {

    [Core.ATTR.EnableUCLEditor]
    [CreateAssetMenu(fileName = "New FileDownloader", menuName = "UCL/Downloader/FileDownloader")]
    public class UCL_FileDownloader : ScriptableObject {
        public string m_DownloadPath = "";
        public string m_SaveFolder = "";
        public string m_FileName = "download.csv";
        public string SavePath { get { return Path.Combine(m_SaveFolder, m_FileName); } }

        [UCL.Core.ATTR.UCL_FunctionButton]
        public void StartDownload() {
#if UNITY_EDITOR
            if(string.IsNullOrEmpty(m_SaveFolder)) {
                var path = UCL.Core.EditorLib.AssetDatabaseMapper.GetAssetPath(this);
                m_SaveFolder = Core.FileLib.Lib.RemoveFolderPath(path, 1);
            }
#endif
            UCL.Core.EnumeratorLib.UCL_CoroutineManager.StartCoroutine(UCL.Core.WebRequestLib.Download(m_DownloadPath, delegate(byte[] data) {
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