using SB.Async;

namespace SB.UI
{
    /// <summary>
    /// This is a class to control pre-caching and changing UI or scene.
    /// </summary>
    public interface IUIDataController
    {
        /// <summary>
        /// Loads scene graph list data.
        /// </summary>
        IPromise Load();

        /// <summary>
        /// True, if the UI data is loaded.
        /// </summary>
        bool IsLoaded();

        /// <summary>
        /// Pre-caches UI elements of the scene graph.
        /// </summary>
        /// <param name="sceneName">The scene graph name to pre-cache.</param>
        IPromise PrecacheSceneUI(string sceneName);

        /// <summary>
        /// Clear UI elements of the scene graph.
        /// </summary>
        /// <param name="sceneNameToRemove">The scene graph name to clear.</param>
        void ClearPrecachedViews(string sceneNameToRemove);

        /// <summary>
        /// Try to get the scene graph data.
        /// </summary>
        /// <param name="sceneName">The scene graph name to get.</param>
        /// <returns>False, if the data doesn't exist.</returns>
        bool TryGetSceneGraph(string sceneName, out UISceneGraph graph);
    }
}