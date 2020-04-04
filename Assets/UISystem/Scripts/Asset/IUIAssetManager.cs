using System;

namespace SB.UI
{
    /// <summary>
    /// Basic asset manager interface for UISystem.
    /// </summary>
    public interface IUIAssetManager
    {
        void LoadAssetAsync<T>(UIAsset definition, Action<T> loaded) where T : UnityEngine.Object;

        bool IsLoad(UIAsset definition);

        void UnloadAsset(UIAsset asset);
    }
}