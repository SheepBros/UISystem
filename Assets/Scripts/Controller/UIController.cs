using System;
using System.Collections.Generic;
using UnityEngine;

namespace SB.UI
{
    public class UIController : IUIController
    {
        private IViewHandler _viewHandler;

        private UISceneList _sceneList;

        private UISceneGraph _currentGraph;

        private UIScreenNode _currentScreen;

        private bool _precachingUIElements;

        private bool _initialized;

        public UIController(IViewHandler viewHandler)
        {
            _viewHandler = viewHandler;
        }

        public void Load(Action loaded)
        {
            UIDataIOUtil.Load((data) =>
            {
                _sceneList = data;
                _initialized = true;
                loaded?.Invoke();
            });
        }

        public void ChangeSceneGraph(string sceneName)
        {
            if (!_initialized)
            {
                return;
            }

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

            ClearPrecachedViews(graph);

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
            if (!_initialized)
            {
                return;
            }

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

        private void ClearPrecachedViews(UISceneGraph newSceneGraph)
        {
            if (_currentGraph == null)
            {
                return;
            }

            List<UIElement> exceptions = new List<UIElement>();
            foreach (var pair in _currentGraph.UIElements)
            {
                if (newSceneGraph.UIElements.ContainsKey(pair.Key))
                {
                    exceptions.Add(pair.Value);
                }
            }

            _viewHandler.ClearCachedViews(exceptions);
        }
    }
}