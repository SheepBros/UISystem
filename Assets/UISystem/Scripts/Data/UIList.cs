using System;
using System.Collections.Generic;

namespace SB.UI
{
    /// <summary>
    /// The list class that has all scene graph data.
    /// </summary>
    [Serializable]
    public class UISceneList
    {
        public Dictionary<string, UISceneGraph> SceneGraphs;
    }
}