namespace SB.UI
{
    public interface IUIController
    {
        void ChangeSceneGraph(string sceneName);

        void RequestUI(string screenName);
    }
}