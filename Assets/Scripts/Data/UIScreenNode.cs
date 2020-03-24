using System;
using System.Collections.Generic;

namespace SB.UI
{
    [Serializable]
    public class UIScreenNode
    {
        public string Name;

        public int Layer;

        public bool IsStartNode;

        public List<string> TransitionNodes;

        public List<string> ElementIdsList;
    }
}