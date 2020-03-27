using System;
using System.Collections.Generic;
using SB.Async;

namespace SB.UI
{
    public interface IViewHandler
    {
        IPromise PrecacheViews(UISceneGraph sceneGraph);

        void ClearCachedViews(List<UIElement> uiListToRemove);

        IPromise TransitionScreen(int layer, List<UIElement> elements, object arg);
    }
}