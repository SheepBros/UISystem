using System;

namespace SB.UI
{
    public interface IUIAssetManager
    {
        void LoadAssetAsync<T>(UIAsset definition, Action<T> loaded) where T : UnityEngine.Object;

        bool IsLoad(UIAsset definition);

        void UnloadAsset(UIAsset asset);
    }
}