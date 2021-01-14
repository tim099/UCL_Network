﻿using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
namespace UCL.NetworkLib.Demo {
    [UCL.Core.ATTR.EnableUCLEditor]
    public class UCL_NetworkDemo : MonoBehaviour {
        private const string UrlPattern = "https://docs.google.com/spreadsheets/d/{0}/export?format=csv&gid={1}";

        public UnityEngine.Object m_Folder;
        
        public string m_SaveName = "downloaded.csv";
        
        public string m_TableID = "1QadubNFdjqGJTJtS9dsmQnu0scc0AYKPKy_mhjEj9Qg";
        public string m_SheetID = "1270318686";
        string m_SaveFolder = "";

        public string DownloadPath {
            get { return string.Format(UrlPattern, m_TableID, m_SheetID); }
        }

        public string SavePath { get { return Path.Combine(m_SaveFolder, m_SaveName); } }
#if UNITY_EDITOR
        [UCL.Core.ATTR.UCL_FunctionButton]
        public void ExploreSaveFolder() {
            m_SaveFolder = Core.FileLib.EditorLib.OpenAssetsFolderExplorer(m_SaveFolder);
        }
#endif
        [UCL.Core.ATTR.UCL_RuntimeOnly]
        [UCL.Core.ATTR.UCL_FunctionButton]
        public void WebRequestDemo() {
#if UNITY_EDITOR
            if(m_Folder != null) m_SaveFolder = UnityEditor.AssetDatabase.GetAssetPath(m_Folder);
#endif
            StartCoroutine(WebRequestLoadBundle(DownloadPath, SavePath));
        }
        private IEnumerator WebRequestLoadBundle(string download_path, string save_path) {
            Debug.LogWarning("WebRequestLoad:" + download_path);

            var www = UnityEngine.Networking.UnityWebRequest.Get(download_path);
            UnityWebRequestAsyncOperation request_opt = www.SendWebRequest();

            yield return request_opt;

            if(www.isNetworkError || www.isHttpError) {
                Debug.LogError("LoadByWebRequest Error:" + download_path + ",Error:" + www.error);
            } else {
                var results = www.downloadHandler.data;
                File.WriteAllBytes(save_path, results);
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
#endif
#if UNITY_EDITOR_WIN
                Core.FileLib.WindowsLib.OpenAssetExplorer(save_path);
#endif
            }
        }
    }
}