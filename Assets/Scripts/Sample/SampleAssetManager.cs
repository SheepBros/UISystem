using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SB.UI
{
    /// <summary>
    /// This is just a sample class.
    /// It loads asssets from the resources folder.
    /// </summary>
    public class UIAssetManager : IUIAssetManager
    {
        private Dictionary<UIAsset, object> _loadedAssets = new Dictionary<UIAsset, object>();

        public void LoadAssetAsync<T>(UIAsset definition, Action<T> loaded) where T : UnityEngine.Object
        {
            if (_loadedAssets.TryGetValue(definition, out object asset))
            {
                loaded.Invoke((T)asset);
                return;
            }

            Resources.LoadAsync<T>("").completed += (async) =>
            {
                ResourceRequest request = (ResourceRequest)async;
                _loadedAssets.Add(definition, request.asset);
                loaded.Invoke((T)request.asset);
            };
        }

        public bool IsLoad(UIAsset definition)
        {
            return _loadedAssets.ContainsKey(definition);
        }
    }
}