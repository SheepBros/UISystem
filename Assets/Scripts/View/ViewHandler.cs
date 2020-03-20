using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SB.UI
{
    public class ViewHandler : MonoBehaviour, IViewHandler
    {
        private IUIAssetManager _assetManager;

        private readonly Dictionary<string, GameObject> _precachedViews = new Dictionary<string, GameObject>();

        private readonly Dictionary<string, GameObject> _currentViews = new Dictionary<string, GameObject>();

        private bool _precaching;

        private bool _transferring;

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

            InstantiateViews(list, (id, gameObject) =>
            {
                _precachedViews.Add(id, gameObject);
            }, () =>
            {
                _precaching = false;
                uiElementsPrecached?.Invoke();
            });
        }


        public void TransitionScreen(List<UIElement> elements, Action screenChanged)
        {
            if (_transferring)
            {
                return;
            }

            _transferring = true;
            Action finished = () =>
            {
                _transferring = false;
                screenChanged?.Invoke();
            };

            StartCoroutine(TransitionScreenCoroutine(elements, finished));
        }

        private IEnumerator TransitionScreenCoroutine(List<UIElement> elements, Action screenChanged)
        {
            bool nextViewsPrepared = false;
            List<UIElement> list = new List<UIElement>(elements);
            Dictionary<string, GameObject> nextViews = new Dictionary<string, GameObject>();
            InstantiateViews(list, (id, gameObject) =>
            {
                nextViews.Add(id, gameObject);
            }, () =>
            {
                nextViewsPrepared = true;
            });

            while (!nextViewsPrepared)
            {
                yield return null;
            }

            bool exitAnimationFinished = false;
            DoExitAnimation(() =>
            {
                exitAnimationFinished = true;
            });

            while (!exitAnimationFinished)
            {
                yield return null;
            }

            foreach (var pair in _currentViews)
            {
                pair.Value.GetComponent<IViewExitState>()?.ExitState();
            }

            _currentViews.Clear();

            foreach (var pair in nextViews)
            {
                _currentViews.Add(pair.Key, pair.Value);
            }

            foreach (var pair in _currentViews)
            {
                pair.Value.SetActive(true);
                pair.Value.GetComponent<IViewEnterState>()?.EnterState();
            }

            bool enterAnimationFinished = false;
            DoEnterAnimation(() =>
            {
                enterAnimationFinished = true;
            });

            while (!enterAnimationFinished)
            {
                yield return null;
            }

            screenChanged?.Invoke();
        }

        private void DoEnterAnimation(Action finished)
        {
            List<GameObject> list = new List<GameObject>(_currentViews.Values);
            for (int i = list.Count; i >= 0; --i)
            {
                DoViewAnimation<ViewEnterAnimation>(list[i], (viewObject) =>
                {
                    list.Remove(viewObject);
                    if (list.Count == 0)
                    {
                        finished?.Invoke();
                    }
                });
            }
        }

        private void DoExitAnimation(Action finished)
        {
            List<GameObject> list = new List<GameObject>(_currentViews.Values);
            for (int i = list.Count; i >= 0; --i)
            {
                DoViewAnimation<ViewExitAnimation>(list[i], (viewObject) =>
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
            viewObject.GetComponent<ViewExitAnimation>()?.Animate(() =>
            {
                finished?.Invoke(viewObject);
            });
        }

        private void InstantiateViews(List<UIElement> list, Action<string, GameObject> instatiated, Action finished)
        {
            if (list.Count == 0)
            {
                finished?.Invoke();
                return;
            }

            UIElement element = list[0];
            list.RemoveAt(0);
            InstantiateViews(element, (viewObject) =>
            {
                instatiated?.Invoke(element.Id, viewObject);
                InstantiateViews(list, instatiated, finished);
            });
        }

        private void InstantiateViews(UIElement element, Action<GameObject> callback)
        {
            _assetManager.LoadAssetAsync<GameObject>(element.Asset, (asset) =>
            {
                GameObject viewObject = Instantiate(asset);
                callback?.Invoke(viewObject);
            });
        }
    }
}