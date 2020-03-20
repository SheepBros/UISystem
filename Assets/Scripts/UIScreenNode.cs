using System;
using System.Collections.Generic;

namespace SB.UI
{
    [Serializable]
    public class UIScreenNode
    {
        public string Name;

        public List<int> TransitionNodes = new List<int>();

        public List<int> ElementIdsList = new List<int>();

        public bool IsStartNode;
    }
}