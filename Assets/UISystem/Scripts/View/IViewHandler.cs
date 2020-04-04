using System.Collections.Generic;
using SB.Async;

namespace SB.UI
{
    /// <summary>
    /// The handler interface that manages to pre-cache views and enable or disable views.
    /// </summary>
    public interface IViewHandler
    {
        /// <summary>
        /// Pre-caches UI elements that are defined in the scene graph.
        /// </summary>
        /// <param name="sceneGraph">The scene graph data.</param>
        IPromise PrecacheViews(UISceneGraph sceneGraph);

        /// <summary>
        /// Remove all UI elements precached.
        /// </summary>
        /// <param name="uiListToRemove">The UI list to remove.</param>
        void ClearCachedViews(List<UIElement> uiListToRemove);

        /// <summary>
        /// Change a screen.
        /// </summary>
        /// <param name="layer">The canvas layer for views to be placed.</param>
        /// <param name="elements">The UI elements data list to show.</param>
        /// <param name="arg">The arguments to send to views.</param>
        IPromise TransitionScreen(int layer, List<UIElement> elements, object arg);
    }
}