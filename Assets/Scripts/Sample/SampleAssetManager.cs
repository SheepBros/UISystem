using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace SB.UI
{
    /// <summary>
    /// This is just a sample class.
    /// It loads asssets from the resources folder.
    /// </summary>
    public class SampleAssetManager : IUIAssetManager
    {
        private Dictionary<string, AssetBundle> _loadedAssetBundles = new Dictionary<string, AssetBundle>();

        private Dictionary<UIAsset, object> _loadedAssets = new Dictionary<UIAsset, object>();

        public void LoadAssetAsync<T>(UIAsset definition, Action<T> loaded) where T : UnityEngine.Object
        {
            if (_loadedAssets.TryGetValue(definition, out object asset))
            {
                loaded.Invoke((T)asset);
                return;
            }

            if (_loadedAssetBundles.TryGetValue(definition.Bundle, out AssetBundle assetBundle))
            {
                LoadAssetAsync<T>(assetBundle, definition, loaded);
                return;
            }

            string path = $"{Application.streamingAssetsPath}//{definition.Bundle}";
            StreamReader streamReader = new StreamReader(path);
            AssetBundleCreateRequest assetBundleLoadRequest = AssetBundle.LoadFromStreamAsync(streamReader.BaseStream);
            assetBundleLoadRequest.completed += (async) =>
            {
                _loadedAssetBundles.Add(definition.Bundle, assetBundleLoadRequest.assetBundle);
                LoadAssetAsync<T>(assetBundleLoadRequest.assetBundle, definition, loaded);
            };
        }

        public bool IsLoad(UIAsset definition)
        {
            return _loadedAssets.ContainsKey(definition);
        }

        private void LoadAssetAsync<T>(AssetBundle assetBundle, UIAsset definition, Action<T> loaded) where T :UnityEngine.Object
        {
            AssetBundleRequest assetLoadRequest = assetBundle.LoadAssetAsync<T>(definition.Name);
            assetLoadRequest.completed += (async) =>
            {
                _loadedAssets.Add(definition, assetLoadRequest.asset);
                loaded.Invoke((T)assetLoadRequest.asset);
            };
        }
    }
}