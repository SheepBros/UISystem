using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SB.UI
{
    public class UIController
    {
        private Dictionary<string, UISceneGraph> _sceneGraphs;

        private UISceneGraph _currentGraph;

        private UIScreenNode _currentScreen;

        private IIVViewHandler _viewHandler;

        private bool _precachingUIElements;

        private bool _changingScreen;

        public UIController(Dictionary<string, UISceneGraph> sceneGraphs, IIVViewHandler viewHandler)
        {
            _sceneGraphs = sceneGraphs;
            _viewHandler = viewHandler;
        }

        public void ChangeSceneGraph(string sceneName)
        {
            if (!_sceneGraphs.TryGetValue(sceneName, out UISceneGraph graph))
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
            if (_changingScreen || _currentScreen.Name == screenName)
            {
                return;
            }

            UIScreenNode screen = _currentGraph.GetScreenNode(screenName);
            if (screen == null)
            {
                Debug.LogError($"There is no screen named {screenName}");
                return;
            }
            
            _changingScreen = true;
            List<UIElement> elements = _currentGraph.GetUIElements(screen);
            _viewHandler.TransitionScreen(elements, () =>
            {
                _changingScreen = false;
                _currentScreen = _currentGraph.GetScreenNode(screenName);
            });
        }
    }
}