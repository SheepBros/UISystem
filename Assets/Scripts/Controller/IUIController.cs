namespace SB.UI
{
    public interface IUIController
    {
        void ChangeScene(string sceneName);

        void RequestScreen(string screenName);
    }
}