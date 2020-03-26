using System;
using System.Collections.Generic;

namespace SB.UI
{
    public interface IViewHandler
    {
        void PrecacheViews(UISceneGraph sceneGraph, Action finished);

        void ClearCachedViews(List<UIElement> uiListToRemove);

        void TransitionScreen(int layer, List<UIElement> elements, object arg, Action screenChanged);
    }
}