using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace UCL.NetworkLib {
    #region BundleHandle
    public class BundleHandle {
        public BundleHandle() {
            m_LoadEnd = false;
            m_LoadError = false;
        }


        /// <summary>
        /// Don't call this function!!!FileLoader Only
        /// </summary>
        public void LoadEnd() {
            m_LoadEnd = true;
            m_LoadEndAct?.Invoke();
        }
        public string GetText() {
            if(!m_LoadEnd) return "Error,File not LoadEnd yet!!";
            return m_UnityWebRequest.downloadHandler.text;
        }
        public void UnloadBundle() {
            if(m_AssetBundle == null) {
                return;
            }

            UCL_WebRequestManager.Instance.UnloadBundle(m_LoadPath);
            m_AssetBundle = null;
        }
        public string m_LoadPath;
        public System.Action m_LoadEndAct;
        public bool m_LoadError;
        public bool m_LoadEnd;
        public UnityWebRequest m_UnityWebRequest;
        public AssetBundle m_AssetBundle;
    }
    #endregion
    public class UCL_WebRequestManager : UCL.Core.UCL_Singleton<UCL_WebRequestManager> {

        Dictionary<string, AssetBundle> m_BundleDic = new Dictionary<string, AssetBundle>();

        public BundleHandle LoadBundle(string path, System.Action LoadEndAct = null) {
            BundleHandle file = new BundleHandle();
            file.m_LoadEndAct = LoadEndAct;
            file.m_LoadPath = path;
            if(m_BundleDic.ContainsKey(path)) {
                file.m_AssetBundle = m_BundleDic[path];
                file.LoadEnd();
                return file;
            }

            StartCoroutine(WebRequestLoadBundle(path, file));
            return file;
        }
        public void UnloadBundle(string path) {
            if(!m_BundleDic.ContainsKey(path)) {
                return;
            }
            var bundle = m_BundleDic[path];
            m_BundleDic.Remove(path);
            bundle.Unload(true);
        }
        private IEnumerator WebRequestLoadBundle(string path, BundleHandle file) {
            Debug.LogWarning("WebRequestLoad:" + path);

            var www = UnityEngine.Networking.UnityWebRequestAssetBundle.GetAssetBundle(path);
            //var www = UnityEngine.Networking.UnityWebRequest.Get(path);
            file.m_UnityWebRequest = www;
            //DownloadHandlerAssetBundle handler = new DownloadHandlerAssetBundle(www.url, uint.MaxValue);
            //www.downloadHandler = handler;
            yield return www.SendWebRequest();

            if(www.isNetworkError || www.isHttpError) {
                Debug.LogError("LoadByWebRequest Error:" + path + ",Error:" + www.error);
                file.m_LoadError = true;
            } else {
                var bundle = (www.downloadHandler as DownloadHandlerAssetBundle).assetBundle;
                Debug.LogWarning("bundle:" + bundle.name);
                file.m_AssetBundle = bundle;
                m_BundleDic.Add(path, bundle);

#if UNITY_EDITOR_OSX || UNITY_EDITOR
                //var objs = bundle.LoadAllAssets(typeof(GameObject));//typeof(Renderer)

                {
                    var mats = bundle.LoadAllAssets(typeof(Material));
                    foreach(Material mat in mats) {
                        var shaderName = mat.shader.name;
                        var shaderInRuntime = Shader.Find(shaderName);
                        if(shaderInRuntime != null) {
                            mat.shader = shaderInRuntime;
                            Debug.Log(string.Format("Found the shader: {0} used in mat: {1}", shaderName, mat.name));
                        } else {
                            Debug.Log(string.Format("Cant not find the shader: {0} used in mat: {1}", shaderName, mat.name));
                        }
                    }
                }
                {
                    var objs = bundle.LoadAllAssets(typeof(GameObject));
                    List<Renderer> rs = new List<Renderer>();
                    foreach(GameObject obj in objs) {
                        var rends = obj.GetComponentsInChildren<Renderer>();
                        foreach(var r in rends) {
                            rs.Add(r);
                        }
                    }

                    //var rs = bundle.LoadAllAssets(typeof(Renderer));//

                    foreach(Renderer r in rs) {
                        var mats = r.sharedMaterials;
                        foreach(Material mat in mats) {
                            var shaderName = mat.shader.name;
                            var shaderInRuntime = Shader.Find(shaderName);
                            if(shaderInRuntime != null) {
                                mat.shader = shaderInRuntime;
                                Debug.Log(string.Format("Found the shader: {0} used in mat: {1}", shaderName, mat.name));
                            } else {
                                Debug.Log(string.Format("Cant not find the shader: {0} used in mat: {1}", shaderName, mat.name));
                            }
                        }
                    }
                }


#endif
            }
            file.LoadEnd();
        }
    }
}

