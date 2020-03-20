using System;
using System.Collections.Generic;

namespace SB.UI
{
    public interface IViewHandler
    {
        void PrecacheViews(UISceneGraph sceneGraph, Action uiElementsPrecached);

        void TransitionScreen(List<UIElement> elements, Action screenChanged);
    }
}