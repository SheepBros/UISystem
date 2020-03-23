using Newtonsoft.Json;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SB.UI
{
    public class UISystem : MonoBehaviour
    {
        private const string FileNameWitnFolder = "/Resources/UISceneList.json";

        private IUIController _uiController;

        [SerializeField]
        private ViewHandler _viewHandler;

        private void Start()
        {
            MakeSceneList();
            UISceneList sceneList = Load();

            IUIAssetManager assetManager = new SampleAssetManager();
            _viewHandler.Initialize(assetManager);

            UIController uiController = new UIController();
            uiController.Initialize(sceneList, _viewHandler);
            _uiController = uiController;
            _uiController.ChangeSceneGraph("SampleScene");
        }

        private void MakeSceneList()
        {
            UISceneList sceneList = new UISceneList();
            sceneList.SceneGraphs = new Dictionary<string, UISceneGraph>();

            UISceneGraph graph = new UISceneGraph();
            sceneList.SceneGraphs.Add("SampleScene", graph);

            graph.SceneName = "SampleScene";
            graph.StartScreenId = "Main";

            UIScreenNode mainNode = new UIScreenNode();
            UIScreenNode settingsNode = new UIScreenNode();
            UIScreenNode userNode = new UIScreenNode();
            UIScreenNode popupNode = new UIScreenNode();
            graph.ScreenNodes = new List<UIScreenNode>();
            graph.ScreenNodes.Add(mainNode);
            graph.ScreenNodes.Add(settingsNode);
            graph.ScreenNodes.Add(userNode);
            graph.ScreenNodes.Add(popupNode);

            mainNode.Name = "Main";
            mainNode.Layer = 0;
            mainNode.IsStartNode = true;
            mainNode.TransitionNodes = new List<string> { "User", "Settings" };
            mainNode.ElementIdsList = new List<string> { "MainScreen" };

            settingsNode.Name = "Settings";
            settingsNode.Layer = 1;
            settingsNode.IsStartNode = false;
            settingsNode.TransitionNodes = new List<string> { "Main", "User" };
            settingsNode.ElementIdsList = new List<string> { "SettingsScreen", "UserInfo" };

            userNode.Name = "User";
            userNode.Layer = 1;
            userNode.IsStartNode = false;
            userNode.TransitionNodes = new List<string> { "Main", "Settings" };
            userNode.ElementIdsList = new List<string> { "UserScreen", "UserInfo" };

            popupNode.Name = "Popup";
            popupNode.Layer = 3;
            popupNode.IsStartNode = false;
            popupNode.TransitionNodes = new List<string> { "Main", "Settings", "User" };
            popupNode.ElementIdsList = new List<string> { "Popup" };

            graph.UIElements = new Dictionary<string, UIElement>();
            graph.UIElements.Add("Popup", new UIElement()
            {
                Id = "Popup",
                Asset = new UIAsset("common", "Popup.prefab"),
                Precache = false
            });
            graph.UIElements.Add("MainScreen", new UIElement()
            {
                Id = "MainScreen",
                Asset = new UIAsset("main", "MainScreen.prefab"),
                Precache = true
            });
            graph.UIElements.Add("UserInfo", new UIElement()
            {
                Id = "UserInfo",
                Asset = new UIAsset("main", "UserInfo.prefab"),
                Precache = true
            });
            graph.UIElements.Add("SettingsScreen", new UIElement()
            {
                Id = "SettingsScreen",
                Asset = new UIAsset("main", "SettingsScreen.prefab"),
                Precache = true
            });
            graph.UIElements.Add("UserScreen", new UIElement()
            {
                Id = "UserScreen",
                Asset = new UIAsset("main", "UserScreen.prefab"),
                Precache = true
            });

            Save(sceneList);
        }

        public void Save(UISceneList sceneList)
        {
            string serializedData = JsonConvert.SerializeObject(sceneList);
            using (StreamWriter textWriter = new StreamWriter(Application.dataPath + FileNameWitnFolder, false))
            {
                textWriter.Write(serializedData);
            }
        }

        public UISceneList Load()
        {
            UISceneList sceneList;
            using (StreamReader textReader = new StreamReader(Application.dataPath + FileNameWitnFolder))
            {
                string serializedData = textReader.ReadToEnd();
                sceneList = JsonConvert.DeserializeObject<UISceneList>(serializedData);
            }
            return sceneList;
        }
    }
}