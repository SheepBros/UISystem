using System;

namespace SB.UI
{
    public interface IUIViewHandler
    {
        event Action<string> ScreenChangedEvent;

        void PrecachUIElements(UISceneGraph sceneGraph, Action precached);

        void ChangeScreen(UIScreenNode screenNode);
    }
}