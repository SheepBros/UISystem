using System;
using System.Collections.Generic;

namespace SB.UI
{
    public interface IViewHandler
    {
        void PrecacheViews(UISceneGraph sceneGraph, Action uiElementsPrecached);

        void ClearCachedViews(List<UIElement> exceptions);

        void TransitionScreen(int layer, List<UIElement> elements, Action screenChanged);
    }
}