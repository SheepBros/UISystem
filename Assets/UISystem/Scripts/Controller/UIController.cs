using System;
using System.Collections.Generic;
using UnityEngine;
using SB.Async;

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

        /// <inheritdoc cref="IUIController.Load"/>
        public IPromise Load()
        {
            Promise promise = new Promise();
            UIDataIOUtil.Load((data) =>
            {
                _sceneList = data;
                _initialized = true;
                promise.Resolve();
            });
            return promise;
        }

        /// <inheritdoc cref="IUIController.PrecacheSceneUI"/>
        public IPromise PrecacheSceneUI(string sceneName)
        {
            Promise promise = new Promise();
            if (!_initialized)
            {
                promise.Fail(new Exception("UISystem is not initialized"));
                return promise;
            }

            if (!_sceneList.SceneGraphs.TryGetValue(sceneName, out UISceneGraph graph))
            {
                promise.Fail(new Exception($"There is no graph for {sceneName} scene."));
                return promise;
            }

            if (_precachedSceneGraph.ContainsKey(sceneName))
            {
                promise.Resolve();
                return promise;
            }

            return _viewHandler.PrecacheViews(graph).Then(() =>
                {
                    _precachedSceneGraph.Add(sceneName, graph);
                });
        }

        /// <inheritdoc cref="IUIController.ChangeSceneGraph"/>
        public IPromise ChangeSceneGraph(string sceneName, bool precacheIfNot = true)
        {
            Promise promise = new Promise();
            if (!_initialized)
            {
                promise.Fail(new Exception("UISystem is not initialized"));
                return promise;
            }

            if (!_sceneList.SceneGraphs.TryGetValue(sceneName, out UISceneGraph graph))
            {
                promise.Fail(new Exception($"There is no graph for {sceneName} scene."));
                return promise;
            }

            return PrecacheSceneUI(sceneName).Then(() =>
                {
                    _screenStack.Clear();
                    _currentScreen = null;
                    _currentGraph = graph;

                    UIScreenNode startNode = _currentGraph.GetStartNode();
                    RequestScreen(startNode.Name).Then(promise.Resolve);
                });
        }

        /// <inheritdoc cref="IUIController.RequestScreen"/>
        public IPromise RequestScreen(string screenName, object arg = null)
        {
            Promise promise = new Promise();
            if (!_initialized)
            {
                promise.Fail(new Exception("UISystem is not initialized"));
                return promise;
            }

            if (_currentScreen != null && _currentScreen.Name == screenName)
            {
                promise.Resolve();
                return promise;
            }

            UIScreenNode screen = _currentGraph.GetScreenNode(screenName);
            if (screen == null)
            {
                promise.Fail(new Exception($"There is no screen named {screenName}"));
                return promise;
            }
            
            List<UIElement> elements = _currentGraph.GetUIElements(screen);
            return _viewHandler.TransitionScreen(screen.Layer, elements, arg).Then(() =>
                {
                    while (_screenStack.Count > 0 &&
                        _screenStack.Peek().Layer >= screen.Layer)
                    {
                        _screenStack.Pop();
                    }

                    _screenStack.Push(screen);
                    _currentScreen = _currentGraph.GetScreenNode(screenName);
                });
        }

        /// <inheritdoc cref="IUIController.RequestScreen"/>
        public IPromise RequestBackTransition(object arg = null)
        {
            if (_currentScreen == null)
            {
                return new Promise();
            }

            return RequestScreen(_currentScreen.BackTransitionNode, arg);
        }

        /// <inheritdoc cref="IUIController.RequestPreviousScreen"/>
        public IPromise RequestPreviousScreen(object arg = null)
        {
            if (!_initialized || _currentScreen == null)
            {
                return new Promise();
            }

            if (_screenStack.Count <= 1)
            {
                return new Promise();
            }

            _screenStack.Pop();
            return RequestScreen(_screenStack.Peek().Name, arg);
        }

        /// <inheritdoc cref="IUIController.ClearPrecachedViews"/>
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