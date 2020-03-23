using System.Collections.Generic;
using UnityEngine;

namespace SB.UI
{
    public class UIController : IUIController
    {
        private UISceneList _sceneList;

        private UISceneGraph _currentGraph;

        private UIScreenNode _currentScreen;

        private IViewHandler _viewHandler;

        private bool _precachingUIElements;

        public void Initialize(UISceneList sceneList, IViewHandler viewHandler)
        {
            _sceneList = sceneList;
            _viewHandler = viewHandler;
        }

        public void ChangeSceneGraph(string sceneName)
        {
            if (!_sceneList.SceneGraphs.TryGetValue(sceneName, out UISceneGraph graph))
            {
                Debug.LogError($"There is no graph for {sceneName} scene.");
                return;
            }

            if (_precachingUIElements)
            {
                Debug.LogWarning($"{sceneName} scene is already precaching.");
                return;
            }

            _currentGraph = graph;
            _precachingUIElements = true;
            _viewHandler.PrecacheViews(_currentGraph, () =>
            {
                _precachingUIElements = false;

                UIScreenNode startNode = _currentGraph.GetStartNode();
                RequestScreen(startNode.Name);
            });
        }

        public void RequestScreen(string screenName)
        {
            if (_currentScreen != null && _currentScreen.Name == screenName)
            {
                return;
            }

            UIScreenNode screen = _currentGraph.GetScreenNode(screenName);
            if (screen == null)
            {
                Debug.LogError($"There is no screen named {screenName}");
                return;
            }
            
            List<UIElement> elements = _currentGraph.GetUIElements(screen);
            _viewHandler.TransitionScreen(screen.Layer, elements, () =>
            {
                _currentScreen = _currentGraph.GetScreenNode(screenName);
            });
        }
    }
}