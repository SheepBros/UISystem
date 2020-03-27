using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        private readonly Dictionary<UIElement, GameObject> _precachedViews = new Dictionary<UIElement, GameObject>();

        private readonly Dictionary<int, Dictionary<UIElement, GameObject>> _currentViews = new Dictionary<int, Dictionary<UIElement, GameObject>>();

        private readonly Dictionary<UIElement, GameObject> _viewsToDisable = new Dictionary<UIElement, GameObject>();

        private readonly Dictionary<UIElement, GameObject> _nextViews = new Dictionary<UIElement, GameObject>();

        private readonly List<UISceneGraph> _precaching = new List<UISceneGraph>();

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

        protected virtual void InstantiateView(UIElement element, Transform parent, Action<GameObject> callback)
        {
            _assetManager.LoadAssetAsync<GameObject>(element.Asset, (asset) =>
            {
                GameObject viewObject = Instantiate(asset, parent);
                Transform viewTransform = viewObject.transform;
                callback?.Invoke(viewObject);
            });
        }

        public void Initialize(IUIAssetManager assetManager)
        {
            _assetManager = assetManager;
        }

        public void PrecacheViews(UISceneGraph sceneGraph, Action finished)
        {
            if (_precaching.Contains(sceneGraph))
            {
                return;
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

            InstantiateViews(list, _canvasForPrecaching, (id, gameObject) =>
            {
                gameObject.SetActive(false);
                _precachedViews.Add(id, gameObject);
                _precaching.Remove(sceneGraph);
            }, () =>
            {
                finished?.Invoke();
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

        public void TransitionScreen(int layer, List<UIElement> elements, object arg, Action screenChanged)
        {
            if (_transitionCoroutine != null)
            {
                StopTransitioning();
            }

            Action finished = () =>
            {
                screenChanged?.Invoke();
            };

            _transitionCoroutine = StartCoroutine(TransitionScreenCoroutine(layer, elements, arg, finished));
        }

        private IEnumerator TransitionScreenCoroutine(int layer, List<UIElement> elements, object arg, Action screenChanged)
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

            screenChanged?.Invoke();
        }

        private IEnumerator ExitCurrentViews(int layer, List<UIElement> exceptions)
        {
            bool exitAnimationFinished = false;
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

            DoExitAnimation(new List<GameObject>(_viewsToDisable.Values), () =>
            {
                exitAnimationFinished = true;
            });

            while (!exitAnimationFinished)
            {
                yield return null;
            }

            foreach (var pair in _viewsToDisable)
            {
                pair.Value.GetComponent<IViewExitState>()?.ExitState();
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
                if (_precachedViews.ContainsKey(pair.Key))
                {
                    pair.Value.SetActive(false);
                    pair.Value.transform.SetParent(_canvasForPrecaching);
                }
                else
                {
                    Destroy(pair.Value);
                }
            }

            _viewsToDisable.Clear();
        }

        private IEnumerator PrepareNextViews(List<UIElement> elements, Transform parent)
        {
            bool nextViewsPrepared = false;
            List<UIElement> elementsToCreate = new List<UIElement>(elements);
            InstantiateViews(elementsToCreate, parent, (id, gameObject) =>
            {
                _nextViews.Add(id, gameObject);
            }, () =>
            {
                nextViewsPrepared = true;
            });

            while (!nextViewsPrepared)
            {
                yield return null;
            }
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

            bool enterAnimationFinished = false;
            DoEnterAnimation(new List<GameObject>(views.Values), () =>
            {
                enterAnimationFinished = true;
            });

            while (!enterAnimationFinished)
            {
                yield return null;
            }
        }

        private void DoEnterAnimation(List<GameObject> list, Action finished)
        {
            for (int i = list.Count - 1; i >= 0; --i)
            {
                DoViewAnimation<IViewEnterAnimation>(list[i], (viewObject) =>
                {
                    list.Remove(viewObject);
                    if (list.Count == 0)
                    {
                        finished?.Invoke();
                    }
                });
            }
        }

        private void DoExitAnimation(List<GameObject> list, Action finished)
        {
            for (int i = list.Count - 1; i >= 0; --i)
            {
                DoViewAnimation<IViewExitAnimation>(list[i], (viewObject) =>
                {
                    list.Remove(viewObject);
                    if (list.Count == 0)
                    {
                        finished?.Invoke();
                    }
                });
            }
        }

        private void DoViewAnimation<TViewAnimation>(GameObject viewObject, Action<GameObject> finished) where TViewAnimation : IViewAnimation
        {
            IViewExitAnimation animation = viewObject.GetComponent<IViewExitAnimation>();
            if (animation != null)
            {
                animation.Animate(() =>
                {
                    finished?.Invoke(viewObject);
                });
            }
            else
            {
                finished?.Invoke(viewObject);
            }
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

                    if (_precachedViews.ContainsKey(pair.Key))
                    {
                        pair.Value.SetActive(false);
                        pair.Value.transform.SetParent(_canvasForPrecaching);
                    }
                    else
                    {
                        Destroy(pair.Value);
                    }
                }
            }
        }

        private void InstantiateViews(List<UIElement> list, Transform parent, Action<UIElement, GameObject> instatiated, Action finished)
        {
            if (list.Count == 0)
            {
                finished?.Invoke();
                return;
            }

            UIElement element = list[0];
            list.RemoveAt(0);
            Action<GameObject> callback = (viewObject) =>
            {
                instatiated?.Invoke(element, viewObject);
                InstantiateViews(list, parent, instatiated, finished);
            };

            if (_precachedViews.TryGetValue(element, out GameObject cachedView))
            {
                Transform viewTransform = cachedView.transform;
                callback(viewTransform.gameObject);
                return;
            }

            InstantiateView(element, parent, callback);
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
    }
}