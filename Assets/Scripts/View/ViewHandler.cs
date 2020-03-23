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

        private IUIAssetManager _assetManager;

        private readonly Dictionary<string, GameObject> _precachedViews = new Dictionary<string, GameObject>();

        private readonly Dictionary<int, Dictionary<string, GameObject>> _currentViews = new Dictionary<int, Dictionary<string, GameObject>>();

        private Dictionary<string, GameObject> _nextViews = new Dictionary<string, GameObject>();

        private bool _precaching;

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

        public void Initialize(IUIAssetManager assetManager)
        {
            _assetManager = assetManager;
        }

        public void PrecacheViews(UISceneGraph sceneGraph, Action uiElementsPrecached)
        {
            if (_precaching)
            {
                return;
            }

            _precaching = true;

            List<UIElement> list = new List<UIElement>();
            foreach (var pair in sceneGraph.UIElements)
            {
                if (pair.Value.Precache && !_precachedViews.ContainsKey(pair.Value.Id))
                {
                    list.Add(pair.Value);
                }
            }

            InstantiateViews(list, _canvasForPrecaching, (id, gameObject) =>
            {
                gameObject.SetActive(false);
                _precachedViews.Add(id, gameObject);
            }, () =>
            {
                _precaching = false;
                uiElementsPrecached?.Invoke();
            });
        }

        public void TransitionScreen(int layer, List<UIElement> elements, Action screenChanged)
        {
            if (_transitionCoroutine != null)
            {
                StopTransitioning();
            }

            Action finished = () =>
            {
                screenChanged?.Invoke();
            };

            _transitionCoroutine = StartCoroutine(TransitionScreenCoroutine(layer, elements, finished));
        }

        private IEnumerator TransitionScreenCoroutine(int layer, List<UIElement> elements, Action screenChanged)
        {
            int highestLayer = GetCurrentHighestLayer();
            if (highestLayer >= 0 && layer == highestLayer)
            {
                yield return DisableCurrentViews(layer, elements);
            }

            yield return PrepareNextViews(elements, _layers[layer].transform);

            yield return EnableNextViews(layer);

            DisableUpperLayer();

            screenChanged?.Invoke();
        }

        private IEnumerator DisableCurrentViews(int layer, List<UIElement> exceptions)
        {
            bool exitAnimationFinished = false;
            if (!_currentViews.TryGetValue(layer, out Dictionary<string, GameObject> views))
            {
                yield break;
            }

            List<GameObject> list = new List<GameObject>();
            foreach (var pair in views)
            {
                UIElement foundItem = exceptions.Find(item =>
                {
                    return item.Id == pair.Key;
                });

                if (foundItem == null)
                {
                    list.Add(pair.Value);
                }
            }

            DoExitAnimation(list, () =>
            {
                exitAnimationFinished = true;
            });

            while (!exitAnimationFinished)
            {
                yield return null;
            }

            foreach (var pair in views)
            {
                pair.Value.GetComponent<IViewExitState>()?.ExitState();
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

            if (_currentViews[layer].Count == 0)
            {
                _currentViews.Remove(layer);
            }
        }

        private IEnumerator PrepareNextViews(List<UIElement> elements, Transform parent)
        {
            bool nextViewsPrepared = false;
            List<UIElement> list = new List<UIElement>(elements);
            InstantiateViews(list, parent, (id, gameObject) =>
            {
                gameObject.SetActive(false);
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

        private IEnumerator EnableNextViews(int layer)
        {
            if (!_currentViews.TryGetValue(layer, out Dictionary<string, GameObject> views))
            {
                views = new Dictionary<string, GameObject>();
                _currentViews.Add(layer, views);
                _layers[layer].SetActive(true);
            }

            foreach (var pair in _nextViews)
            {
                views.Add(pair.Key, pair.Value);
            }

            Transform canvaseTransform = _layers[layer].transform;
            foreach (var pair in views)
            {
                Transform transform = pair.Value.transform;
                transform.SetParent(canvaseTransform);
                transform.Identify();

                pair.Value.SetActive(true);
                pair.Value.GetComponent<IViewEnterState>()?.EnterState();
            }

            _nextViews.Clear();

            bool enterAnimationFinished = false;
            DoEnterAnimation(new List<GameObject>(_nextViews.Values), () =>
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

        private void InstantiateViews(List<UIElement> list, Transform parent, Action<string, GameObject> instatiated, Action finished)
        {
            if (list.Count == 0)
            {
                finished?.Invoke();
                return;
            }

            UIElement element = list[0];
            list.RemoveAt(0);
            InstantiateView(element, parent, (viewObject) =>
            {
                instatiated?.Invoke(element.Id, viewObject);
                InstantiateViews(list, parent, instatiated, finished);
            });
        }

        private void InstantiateView(UIElement element, Transform parent, Action<GameObject> callback)
        {
            if (_precachedViews.TryGetValue(element.Id, out GameObject cachedView))
            {
                Transform viewTransform = cachedView.transform;
                viewTransform.Identify();
                callback?.Invoke(cachedView);
                return;
            }

            _assetManager.LoadAssetAsync<GameObject>(element.Asset, (asset) =>
            {
                GameObject viewObject = Instantiate(asset, parent);
                Transform viewTransform = viewObject.transform;
                viewTransform.Identify();
                callback?.Invoke(viewObject);
            });
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

        private void DisableUpperLayer()
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