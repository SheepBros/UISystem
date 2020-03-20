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
        public string SceneName;

        public string StartScreenId;

        public List<UIScreenNode> ScreenNodes = new List<UIScreenNode>();

        public List<UIElement> UIElements = new List<UIElement>();
    }
}