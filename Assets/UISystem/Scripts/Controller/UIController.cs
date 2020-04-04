using System;
using System.Collections.Generic;
using SB.Async;

namespace SB.UI
{
    public class UIController : IUIController
    {
        private IUIDataController _dataController;

        private IViewHandler _viewHandler;

        private UISceneGraph _currentGraph;

        private UIScreenNode _currentScreen;

        private Stack<UIScreenNode> _screenStack = new Stack<UIScreenNode>();

        public UIController(IUIDataController dataController, IViewHandler viewHandler)
        {
            _dataController = dataController;
            _viewHandler = viewHandler;
        }

        /// <inheritdoc cref="IUIController.ChangeSceneGraph"/>
        public IPromise ChangeSceneGraph(string sceneName, bool precacheIfNot = true)
        {
            Promise promise = new Promise();
            if (!_dataController.IsLoaded())
            {
                promise.Fail(new Exception("UISystem is not initialized"));
                return promise;
            }

            if (!_dataController.TryGetSceneGraph(sceneName, out UISceneGraph graph))
            {
                promise.Fail(new Exception($"There is no graph for {sceneName} scene."));
                return promise;
            }

            return _dataController.PrecacheSceneUI(sceneName).Then(() =>
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
            if (!_dataController.IsLoaded())
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
            if (!_dataController.IsLoaded() || _currentScreen == null)
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
    }
}