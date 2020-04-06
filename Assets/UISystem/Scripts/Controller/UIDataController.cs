using System;
using System.Collections.Generic;
using SB.Async;

namespace SB.UI
{
    public class UIDataController : IUIDataController
    {
        private IUIDataIOController _uiDataIOController;

        private IViewHandler _viewHandler;

        private UISceneList _sceneList;

        private Dictionary<string, UISceneGraph> _precachedSceneGraph = new Dictionary<string, UISceneGraph>();

        private bool _initialized;

        public UIDataController(IUIDataIOController uiDataIOController, IViewHandler viewHandler)
        {
            _uiDataIOController = uiDataIOController;
            _viewHandler = viewHandler;
        }

        /// <inheritdoc cref="IUIController.Load"/>
        public IPromise Load()
        {
            Promise promise = new Promise();
            _uiDataIOController.Load().Then((data) =>
            {
                _sceneList = data;
                _initialized = true;
                promise.Resolve();
            });
            return promise;
        }

        /// <inheritdoc cref="IUIDataController.IsLoad"/>
        public bool IsLoaded()
        {
            return _initialized;
        }

        /// <inheritdoc cref="IUIDataController.PrecacheSceneUI"/>
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

        /// <inheritdoc cref="IUIDataController.ClearPrecachedViews"/>
        public void ClearPrecachedViews(string sceneNameToRemove)
        {
            if (!_sceneList.SceneGraphs.TryGetValue(sceneNameToRemove, out UISceneGraph graph))
            {
                return;
            }

            _precachedSceneGraph.Remove(sceneNameToRemove);

            List<UIElement> list = new List<UIElement>();
            foreach (var graphPair in _precachedSceneGraph)
            {
                foreach (var viewPiarToRemvoe in graph.UIElements)
                {
                    if (!graphPair.Value.UIElements.ContainsKey(viewPiarToRemvoe.Key))
                    {
                        list.Add(viewPiarToRemvoe.Value);
                    }
                }
            }

            _viewHandler.ClearCachedViews(list);
        }

        /// <inheritdoc cref="IUIDataController.TryGetSceneGraph"/>
        public bool TryGetSceneGraph(string sceneName, out UISceneGraph graph)
        {
            if (_sceneList.SceneGraphs.TryGetValue(sceneName, out graph))
            {
                return true;
            }

            return false;
        }
    }
}