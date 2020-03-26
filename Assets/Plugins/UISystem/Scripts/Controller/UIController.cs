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

        private Dictionary<string, UISceneGraph> _precachedSceneGraph = new Dictionary<string, UISceneGraph>();

        private Stack<UIScreenNode> _screenStack = new Stack<UIScreenNode>();

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

        public void PrecacheSceneUI(string sceneName, Action finished)
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

            if (_precachedSceneGraph.ContainsKey(sceneName))
            {
                finished?.Invoke();
                return;
            }

            _viewHandler.PrecacheViews(graph, () =>
            {
                _precachedSceneGraph.Add(sceneName, graph);
                finished?.Invoke();
            });
        }

        public void ChangeSceneGraph(string sceneName, Action finished = null, bool precacheIfNot = true)
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

            Action changeScene = () =>
            {
                _screenStack.Clear();
                _currentScreen = null;
                _currentGraph = graph;

                UIScreenNode startNode = _currentGraph.GetStartNode();
                RequestScreen(startNode.Name, finished);
            };

            if (precacheIfNot)
            {
                PrecacheSceneUI(sceneName, changeScene);
            }
            else
            {
                changeScene();
            }
        }

        public void RequestScreen(string screenName, object arg = null, Action finished = null)
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
            _viewHandler.TransitionScreen(screen.Layer, elements, arg, () =>
            {
                while (_screenStack.Count > 0 &&
                    _screenStack.Peek().Layer >= screen.Layer)
                {
                    _screenStack.Pop();
                }

                _screenStack.Push(screen);
                _currentScreen = _currentGraph.GetScreenNode(screenName);

                finished?.Invoke();
            });
        }

        public void RequestBackTransition(object arg = null, Action finished = null)
        {
            if (_currentScreen == null)
            {
                return;
            }

            RequestScreen(_currentScreen.BackTransitionNode, arg, finished);
        }

        public void RequestPreviousScreen(object arg = null, Action finished = null)
        {
            if (!_initialized || _currentScreen == null)
            {
                return;
            }

            if (_screenStack.Count <= 1)
            {
                return;
            }

            _screenStack.Pop();
            RequestScreen(_screenStack.Peek().Name, arg, finished);
        }

        public void ClearPrecachedViews(string sceneNameToRemove)
        {
            if (!_sceneList.SceneGraphs.TryGetValue(sceneNameToRemove, out UISceneGraph graph))
            {
                return;
            }

            List<UIElement> list = new List<UIElement>();
            foreach (var pair in graph.UIElements)
            {
                if (!_currentGraph.UIElements.ContainsKey(pair.Key))
                {
                    list.Add(pair.Value);
                }
            }

            _precachedSceneGraph.Remove(sceneNameToRemove);
            _viewHandler.ClearCachedViews(list);
        }
    }
}