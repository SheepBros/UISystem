using SB.Async;

namespace SB.UI
{
    /// <summary>
    /// This has a responsibility to load UI data from local.
    /// </summary>
    public interface IUIDataIOController
    {
        /// <summary>
        /// Loads scene graph list data.
        /// </summary>
        IPromise<UISceneList> Load();

        /// <summary>
        /// Save scene graph list data.
        /// </summary>
        /// <param name="sceneList">Scene list data.</param>
        void Save(UISceneList sceneList);
    }
}