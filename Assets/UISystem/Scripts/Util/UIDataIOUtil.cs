using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace SB.UI
{
    public static class UIDataIOUtil
    {
        private const string DataFileName = "UISceneList.json";

        public static void Save(UISceneList sceneList)
        {
            string serializedData = JsonConvert.SerializeObject(sceneList);
            using (StreamWriter textWriter = new StreamWriter($"{Application.dataPath}/Resources/{DataFileName}", false))
            {
                textWriter.Write(serializedData);
            }
        }

        public static void Load(Action<UISceneList> loaded)
        {
            using (StreamReader textReader = new StreamReader($"{Application.dataPath}/Resources/{DataFileName}"))
            {
                string serializedData = textReader.ReadToEnd();
                UISceneList sceneList = JsonConvert.DeserializeObject<UISceneList>(serializedData);
                loaded.Invoke(sceneList);
            }
        }
    }
}