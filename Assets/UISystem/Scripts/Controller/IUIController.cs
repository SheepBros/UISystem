using SB.Async;

namespace SB.UI
{
    public interface IUIController
    {
        IPromise Load();

        IPromise PrecacheSceneUI(string sceneName);

        IPromise ChangeSceneGraph(string sceneName, bool precacheIfNot = true);

        IPromise RequestScreen(string screenName, object arg = null);

        IPromise RequestBackTransition(object arg = null);

        IPromise RequestPreviousScreen(object arg = null);

        void ClearPrecachedViews(string sceneNameToRemove);
    }
}