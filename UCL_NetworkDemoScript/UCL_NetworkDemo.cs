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
        string SaveFolder {
            get {
                string path = Application.persistentDataPath;
#if UNITY_EDITOR
                if(m_Folder != null) path = UCL.Core.EditorLib.AssetDatabaseMapper.GetAssetPath(m_Folder);
#endif
                return path;
            }
        }
        public string DownloadPath {
            get { return string.Format(UrlPattern, m_TableID, m_SheetID); }
        }

        public string SavePath { get { return Path.Combine(SaveFolder, m_SaveName); } }

        [UCL.Core.ATTR.UCL_RuntimeOnly]
        [UCL.Core.ATTR.UCL_FunctionButton]
        public void WebRequestDemo() {
            StartCoroutine(WebRequestLoad(DownloadPath, SavePath));
        }
        private IEnumerator WebRequestLoad(string download_path, string save_path) {
            Debug.LogWarning("WebRequestLoad:" + download_path);
            long file_size = 0;
            ///Get Header
            using(var headRequest = UnityWebRequest.Head(download_path)) {
                yield return headRequest.SendWebRequest();
                if(headRequest.responseCode == 200) {
                    var contentLength = headRequest.GetResponseHeader("CONTENT-LENGTH");
                    long.TryParse(contentLength, out file_size);
                } else {
                    Debug.LogError("WebRequestLoadBundle headRequest.error:" + headRequest.error);
                    yield break;
                }
            }
            Debug.LogWarning("WebRequestLoad file_size:" + file_size);

            var www = UnityEngine.Networking.UnityWebRequest.Get(download_path);
            UnityWebRequestAsyncOperation request_opt = www.SendWebRequest();

            yield return request_opt;
            switch (www.result)
            {
                case UnityWebRequest.Result.Success:
                    {
                        var results = www.downloadHandler.data;
                        File.WriteAllBytes(save_path, results);
#if UNITY_EDITOR
                        UCL.Core.EditorLib.AssetDatabaseMapper.Refresh();
#endif
#if UNITY_EDITOR_WIN
                        Core.FileLib.WindowsLib.OpenAssetExplorer(save_path);
#endif
                        break;
                    }
                default://Error
                    {
                        Debug.LogError("LoadByWebRequest Error Path:" + download_path +",Result:"+ www.result + ",Error:" + www.error);
                        break;
                    }
            }
        }
#if UNITY_EDITOR
        Core.EnumeratorLib.EnumeratorPlayer m_Player = null;

        [UCL.Core.ATTR.UCL_FunctionButton]
        public void EditorDownload() {
            EditorStopCoroutine();
            m_Player = Core.EnumeratorLib.UCL_CoroutineManager.StartCoroutine(WebRequestLoad(DownloadPath, SavePath));
        }
        [UCL.Core.ATTR.UCL_FunctionButton]
        public void EditorStopCoroutine() {
            Core.EnumeratorLib.UCL_CoroutineManager.StopCoroutine(m_Player);
        }
#endif
    }
}