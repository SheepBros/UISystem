using System;

namespace SB.UI
{
    public interface IUIController
    {
        void Load(Action loaded);

        void ChangeSceneGraph(string sceneName);

        void RequestScreen(string screenName);
    }
}