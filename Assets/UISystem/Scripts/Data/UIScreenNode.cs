using System;
using System.Collections.Generic;

namespace SB.UI
{
    /// <summary>
    /// UI node class that contains configurations for the screen.
    /// </summary>
    [Serializable]
    public class UIScreenNode
    {
        public string Name;

        public int Layer;

        public bool IsStartNode;

        public string BackTransitionNode;

        public List<string> TransitionNodes;

        public List<string> ElementIdsList;
    }
}