using System;
using System.Collections.Generic;

namespace SB.UI
{
    [Serializable]
    public class UISceneGraph
    {
        public string SceneName { get; set; }

        public string StartScreenId { get; set; }

        public List<UIScreenNode> ScreenNodes { get; set; }

        public Dictionary<string, UIElement> UIElements { get; set; }

        private Dictionary<string, List<UIElement>> _cachedUIElementsForScreen = new Dictionary<string, List<UIElement>>();

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
    }
}