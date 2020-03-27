using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SB.Async;

namespace SB.UI
{
    public class ViewHandler : MonoBehaviour, IViewHandler
    {
        private static ViewHandler _instance;

        [SerializeField]
        private Transform _canvasForPrecaching;

        [SerializeField]
        private List<GameObject> _layers;

        protected IUIAssetManager _assetManager;

        protected readonly Dictionary<UIElement, GameObject> _precachedViews = new Dictionary<UIElement, GameObject>();

        protected readonly Dictionary<int, Dictionary<UIElement, GameObject>> _currentViews = new Dictionary<int, Dictionary<UIElement, GameObject>>();

        protected readonly Dictionary<UIElement, GameObject> _viewsToDisable = new Dictionary<UIElement, GameObject>();

        protected readonly Dictionary<UIElement, GameObject> _nextViews = new Dictionary<UIElement, GameObject>();

        protected readonly List<UISceneGraph> _precaching = new List<UISceneGraph>();

        private Coroutine _transitionCoroutine;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(this);
        }

        protected virtual IPromise<UIElement, GameObject> InstantiateView(UIElement element, Transform parent)
        {
            Promise<UIElement, GameObject> promise = new Promise<UIElement, GameObject>();
            if (_precachedViews.TryGetValue(element, out GameObject cachedView))
            {
                promise.Resolve(element, cachedView);
                return promise;
            }

            _assetManager.LoadAssetAsync<GameObject>(element.Asset, (asset) =>
            {
                GameObject viewObject = Instantiate(asset, parent);
                Transform viewTransform = viewObject.transform;
                promise.Resolve(element, viewObject);
            });
            return promise;
        }

        public void Initialize(IUIAssetManager assetManager)
        {
            _assetManager = assetManager;
        }

        public IPromise PrecacheViews(UISceneGraph sceneGraph)
        {
            if (_precaching.Contains(sceneGraph))
            {
                Promise promise = new Promise();
                promise.Resolve();
                return promise;
            }

            List<UIElement> list = new List<UIElement>();
            foreach (var pair in sceneGraph.UIElements)
            {
                if (pair.Value.Precache && !_precachedViews.ContainsKey(pair.Value))
                {
                    list.Add(pair.Value);
                }
            }

            _precaching.Add(sceneGraph);

            return InstantiateViews(list, _canvasForPrecaching, (element, vieObject) =>
                {
                    vieObject.SetActive(false);
                    _precachedViews.Add(element, vieObject);
                }).Then(() =>
                {
                    _precaching.Remove(sceneGraph);
                });
        }

        public void ClearCachedViews(List<UIElement> uiListToRemove)
        {
            for (int i = 0; i < uiListToRemove.Count; ++i)
            {
                UIElement viewToDestroy = uiListToRemove[i];
                Destroy(_precachedViews[uiListToRemove[i]]);
                _precachedViews.Remove(viewToDestroy);

                foreach (var layerPair in _currentViews)
                {
                    layerPair.Value.Remove(viewToDestroy);
                }
            }
        }

        public IPromise TransitionScreen(int layer, List<UIElement> elements, object arg)
        {
            Promise promise = new Promise();
            if (_transitionCoroutine != null)
            {
                StopTransitioning();
            }

            _transitionCoroutine = StartCoroutine(TransitionScreenCoroutine(layer, elements, arg, promise));
            return promise;
        }

        private IEnumerator TransitionScreenCoroutine(int layer, List<UIElement> elements, object arg, Promise promise)
        {
            int highestLayer = GetCurrentHighestLayer();
            while (highestLayer >= layer)
            {
                yield return ExitCurrentViews(highestLayer, elements);

                highestLayer = GetCurrentHighestLayer();
                if (highestLayer == layer)
                {
                    break;
                }
            }

            yield return PrepareNextViews(elements, _layers[layer].transform);

            yield return EnterNextViews(layer, elements, arg);

            DisablePreviousViews();

            DisableUnusedLayer();

            promise.Resolve();
        }

        private IEnumerator ExitCurrentViews(int layer, List<UIElement> exceptions)
        {
            if (!_currentViews.TryGetValue(layer, out Dictionary<UIElement, GameObject> views))
            {
                yield break;
            }

            foreach (var pair in views)
            {
                if (!exceptions.Contains(pair.Key))
                {
                    _viewsToDisable.Add(pair.Key, pair.Value);
                }
            }

            foreach (var pair in _viewsToDisable)
            {
                views.Remove(pair.Key);
            }

            yield return DoExitAnimation(new List<GameObject>(_viewsToDisable.Values));

            foreach (var pair in _viewsToDisable)
            {
                if (pair.Value != null)
                {
                    pair.Value.GetComponent<IViewExitState>()?.ExitState();
                }
            }

            if (_currentViews[layer].Count == 0)
            {
                _currentViews.Remove(layer);
            }
        }

        private void DisablePreviousViews()
        {
            foreach (var pair in _viewsToDisable)
            {
                DisableOrDestroyView(pair.Key, pair.Value);
            }

            _viewsToDisable.Clear();
        }

        private IEnumerator PrepareNextViews(List<UIElement> elements, Transform parent)
        {
            List<UIElement> elementsToCreate = new List<UIElement>(elements);
            yield return InstantiateViews(elementsToCreate, parent, (id, gameObject) =>
                {
                    _nextViews.Add(id, gameObject);
                });
        }

        private IEnumerator EnterNextViews(int layer, List<UIElement> viewOrderList, object arg)
        {
            if (!_currentViews.TryGetValue(layer, out Dictionary<UIElement, GameObject> views))
            {
                views = new Dictionary<UIElement, GameObject>();
                _currentViews.Add(layer, views);
                _layers[layer].SetActive(true);
            }

            foreach (var pair in _nextViews)
            {
                if (!views.ContainsKey(pair.Key))
                {
                    views.Add(pair.Key, pair.Value);
                }
            }

            Transform canvaseTransform = _layers[layer].transform;
            foreach (var pair in views)
            {
                Transform transform = pair.Value.transform;
                transform.SetParent(canvaseTransform);
                pair.Value.GetComponent<RectTransform>().Identify();

                pair.Value.SetActive(true);
                pair.Value.GetComponent<IViewEnterState>()?.EnterState(arg);
            }

            for (int i = 0; i < viewOrderList.Count; ++i)
            {
                GameObject view = views[viewOrderList[i]];
                view.transform.SetSiblingIndex(i);
            }

            _nextViews.Clear();

            yield return DoEnterAnimation(new List<GameObject>(views.Values));
        }

        private IPromise DoEnterAnimation(List<GameObject> list)
        {
            Promise promise = new Promise();
            for (int i = list.Count - 1; i >= 0; --i)
            {
                DoViewAnimation<IViewEnterAnimation>(list[i]).Then((viewObject) =>
                {
                    list.Remove(viewObject);
                    if (list.Count == 0)
                    {
                        promise.Resolve();
                    }
                });
            }

            return promise;
        }

        private IPromise DoExitAnimation(List<GameObject> list)
        {
            Promise promise = new Promise();
            for (int i = list.Count - 1; i >= 0; --i)
            {
                DoViewAnimation<IViewExitAnimation>(list[i]).Then((viewObject) =>
                {
                    list.Remove(viewObject);
                    if (list.Count == 0)
                    {
                        promise.Resolve();
                    }
                });
            }

            return promise;
        }

        private IPromise<GameObject> DoViewAnimation<TViewAnimation>(GameObject viewObject) where TViewAnimation : IViewAnimation
        {
            Promise<GameObject> promise = new Promise<GameObject>();
            IViewExitAnimation animation = viewObject.GetComponent<IViewExitAnimation>();
            if (animation != null)
            {
                animation.Animate(() =>
                {
                    promise.Resolve(viewObject);
                });
            }
            else
            {
                promise.Resolve(viewObject);
            }

            return promise;
        }

        private void StopTransitioning()
        {
            StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = null;

            if (_nextViews.Count > 0)
            {
                foreach (var pair in _nextViews)
                {
                    GameObject viewObject = pair.Value;
                    IViewExitAnimation animation = viewObject.GetComponent<IViewExitAnimation>();
                    animation?.Stop();

                    DisableOrDestroyView(pair.Key, pair.Value);
                }
            }
        }

        private IPromise InstantiateViews(List<UIElement> list, Transform parent, Action<UIElement, GameObject> instantiated)
        {
            Promise promise = new Promise();
            if (list.Count == 0)
            {
                promise.Resolve();
                return promise;
            }

            UIElement element = list[0];
            list.RemoveAt(0);
            InstantiateView(element, parent).Then((instantiatedElement, vieObject) =>
            {
                list.Remove(instantiatedElement);
                instantiated?.Invoke(instantiatedElement, vieObject);
                InstantiateViews(list, parent, instantiated).Then(promise.Resolve);
            });

            return promise;
        }

        private int GetCurrentHighestLayer()
        {
            int highestValue = -1;
            foreach (var pair in _currentViews)
            {
                highestValue = Mathf.Max(highestValue, pair.Key);
            }

            return highestValue;
        }

        private void DisableUnusedLayer()
        {
            for (int i = 0; i < _layers.Count; ++i)
            {
                if (!_currentViews.ContainsKey(i))
                {
                    _layers[i].SetActive(false);
                }
            }
        }

        private void DisableOrDestroyView(UIElement element, GameObject viewObject)
        {
            if (viewObject == null)
            {
                return;
            }
            
            if (_precachedViews.ContainsKey(element))
            {
                viewObject.SetActive(false);
                viewObject.transform.SetParent(_canvasForPrecaching);
            }
            else
            {
                Destroy(viewObject);
            }
        }
    }
}