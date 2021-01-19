using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace UCL.NetworkLib {
#if UNITY_EDITOR
    [Core.ATTR.EnableUCLEditor]
#endif
    [CreateAssetMenu(fileName = "New FileDownloader", menuName = "UCL/FileDownloader")]
    public class UCL_FileDownloader : ScriptableObject {
        public string m_DownloadPath = "";
        public string m_SaveFolder = "";
        public string m_FileName = "download.csv";
        public string SavePath { get { return Path.Combine(m_SaveFolder, m_FileName); } }

        [UCL.Core.ATTR.UCL_FunctionButton]
        public void StartDownload() {
#if UNITY_EDITOR
            if(string.IsNullOrEmpty(m_SaveFolder)) {
                var path = UnityEditor.AssetDatabase.GetAssetPath(this);
                m_SaveFolder = Core.FileLib.Lib.RemoveFolderPath(path, 1);
            }
#endif
            UCL.Core.EnumeratorLib.UCL_CoroutineManager.StartCoroutine(WebRequestDownload(m_DownloadPath, delegate(byte[] data) {
                File.WriteAllBytes(SavePath, data);
#if UNITY_EDITOR
                UnityEditor.AssetDatabase.Refresh();
#endif
#if UNITY_EDITOR_WIN
                Core.FileLib.WindowsLib.OpenAssetExplorer(SavePath);
#endif
            }));
        }
        private IEnumerator WebRequestDownload(string download_path, System.Action<byte[]> download_callback) {
            Debug.LogWarning("WebRequestDownload:" + download_path);
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

            if(www.isNetworkError || www.isHttpError) {
                Debug.LogError("LoadByWebRequest Error:" + download_path + ",Error:" + www.error);
            } else {
                var results = www.downloadHandler.data;
                if(download_callback != null) download_callback.Invoke(results);
            }
        }
    }
}