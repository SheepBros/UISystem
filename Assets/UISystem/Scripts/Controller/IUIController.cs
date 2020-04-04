using SB.Async;

namespace SB.UI
{
    /// <summary>
    /// This is a class to control pre-caching and changing UI or scene.
    /// </summary>
    public interface IUIController
    {
        /// <summary>
        /// Loads scene graph list data.
        /// </summary>
        IPromise Load();

        /// <summary>
        /// Pre-caches UI elements of the scene graph.
        /// </summary>
        /// <param name="sceneName">The scene graph name to pre-cache.</param>
        IPromise PrecacheSceneUI(string sceneName);

        /// <summary>
        /// Change a scene graph.
        /// </summary>
        /// <param name="sceneName">The scene graph name to load.</param>
        /// <param name="precacheIfNot">If true, pre-cache first if the UI elements are not cached yet.</param>
        IPromise ChangeSceneGraph(string sceneName, bool precacheIfNot = true);

        /// <summary>
        /// Requests to change a screen.
        /// </summary>
        /// <param name="screenName">The screen name to change.</param>
        /// <param name="arg">The arguments to send to UI elements of the next screen.</param>
        IPromise RequestScreen(string screenName, object arg = null);

        /// <summary>
        /// Requests to change to the back screen of the screen that is defined in the data.
        /// </summary>
        /// <param name="arg">The arguments to send to UI elements of the next screen.</param>
        IPromise RequestBackTransition(object arg = null);

        /// <summary>
        /// Requests to change to the previous screen where the player was on.
        /// </summary>
        /// <param name="arg">The arguments to send to UI elements of the previous screen.</param>
        IPromise RequestPreviousScreen(object arg = null);

        /// <summary>
        /// Clear UI elements of the scene graph.
        /// </summary>
        /// <param name="sceneNameToRemove">The scene graph name to clear.</param>
        void ClearPrecachedViews(string sceneNameToRemove);
    }
}