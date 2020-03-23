namespace SB.UI
{
    public interface IUIController
    {
        void ChangeSceneGraph(string sceneName);

        void RequestScreen(string screenName);
    }
}