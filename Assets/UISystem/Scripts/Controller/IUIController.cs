using System;

namespace SB.UI
{
    public interface IUIController
    {
        void Load(Action loaded);

        void PrecacheSceneUI(string sceneName, Action finished);

        void ChangeSceneGraph(string sceneName, Action finished = null, bool precacheIfNot = true);

        void RequestScreen(string screenName, object arg = null, Action finished = null);

        void RequestBackTransition(object arg = null, Action finished = null);

        void RequestPreviousScreen(object arg = null, Action finished = null);

        void ClearPrecachedViews(string sceneNameToRemove);
    }
}