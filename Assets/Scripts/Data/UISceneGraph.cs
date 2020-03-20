using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SB.UI
{
    [Serializable]
    public class UISceneGraph
    {
        public string SceneName { get; private set; }

        public string StartScreenId { get; private set; }

        public List<UIScreenNode> ScreenNodes { get; private set; } = new List<UIScreenNode>();

        public Dictionary<string, UIElement> UIElements { get; private set; } = new Dictionary<string, UIElement>();

        private Dictionary<string, List<UIElement>> _cachedUIElementsForScreen = new Dictionary<string, List<UIElement>>();

        [JsonConstructor]
        public UISceneGraph(string sceneName, string startScreenId,
            List<UIScreenNode> screenNodes, Dictionary<string, UIElement> uiElements)
        {
            SceneName = sceneName;
            StartScreenId = startScreenId;
            ScreenNodes = screenNodes;
            UIElements = uiElements;
        }

        public void SetStartNode(string startScreenId)
        {
            for (int i = 0; i < ScreenNodes.Count; ++i)
            {
                ScreenNodes[i].IsStartNode = startScreenId == ScreenNodes[i].Name;
            }
        }

        public UIScreenNode GetStartNode()
        {
            return ScreenNodes.Find(item => { return item.IsStartNode; });
        }

        public UIScreenNode GetScreenNode(string screenName)
        {
            return ScreenNodes.Find(item => { return item.Name == screenName; });
        }

        public List<UIElement> GetUIElements()
        {
            return new List<UIElement>(UIElements.Values);
        }

        public List<UIElement> GetUIElements(UIScreenNode node)
        {
            if (_cachedUIElementsForScreen.TryGetValue(node.Name, out List<UIElement> list))
            {
                return list;
            }

            list = new List<UIElement>();
            foreach (string name in node.ElementIdsList)
            {
                if (UIElements.TryGetValue(name, out UIElement value))
                {
                    list.Add(value);
                }
            }

            _cachedUIElementsForScreen.Add(node.Name, list);
            return list;
        }

        public void Save()
        {
            const string FileNameWitnFolder = "/Resources/UISceneGraph.json";
            string serilizedData = JsonConvert.SerializeObject(this);
            using (StreamWriter textWriter = new StreamWriter(Application.dataPath + FileNameWitnFolder, false))
            {
                textWriter.Write(serilizedData);
            }
        }
    }
}