using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SB.UI
{
    public class UIController
    {
        private Dictionary<string, UISceneGraph> _sceneGraphs;

        private UISceneGraph _currentGraph;

        private UIScreenNode _currentScreen;

        private IUIViewHandler _viewHandler;
    }
}