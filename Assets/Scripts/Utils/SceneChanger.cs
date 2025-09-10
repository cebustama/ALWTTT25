using ALWTTT.Managers;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace ALWTTT.Utils
{
    public class SceneChanger : MonoBehaviour
    {
        private GameManager GameManager => GameManager.Instance;
        private UIManager UIManager => UIManager.Instance;

        private enum SceneType
        {
            MainMenu,
            SectorMap,
            Ship,
            Gig
        }

        public void OpenMainMenuScene()
        {
            StartCoroutine(DelaySceneChange(SceneType.MainMenu));
        }

        public void OpenMapScene()
        {
            StartCoroutine(DelaySceneChange(SceneType.SectorMap));
        }

        public void OpenShipScene()
        {
            StartCoroutine(DelaySceneChange(SceneType.Ship));
        }

        public void OpenGigScene()
        {
            StartCoroutine(DelaySceneChange(SceneType.Gig));
        }

        private IEnumerator DelaySceneChange(SceneType type)
        {
            // Close inventory canvas in case it's open
            UIManager.SetCanvas(UIManager.InventoryCanvas, false, true);
            yield return StartCoroutine(UIManager.Fade(true));

            switch (type)
            {
                case SceneType.MainMenu:
                    UIManager.ChangeScene(GameManager.SceneData.mainMenuSceneIndex);
                    UIManager.SetCanvas(UIManager.GigCanvas, false, true);
                    UIManager.SetCanvas(UIManager.RewardCanvas, false, true);
                    // TODO: InformationCanvas, false

                    GameManager.InitGameplayData();
                    GameManager.SetInitialDeck();
                    break;

                case SceneType.SectorMap:
                    UIManager.ChangeScene(GameManager.SceneData.sectorMapSceneIndex);
                    UIManager.SetCanvas(UIManager.GigCanvas, false, true);
                    UIManager.SetCanvas(UIManager.RewardCanvas, false, true);
                    // TODO: InformationCanvas, true
                    break;

                case SceneType.Ship:
                    UIManager.ChangeScene(GameManager.SceneData.shipInteriorSceneIndex);
                    UIManager.SetCanvas(UIManager.GigCanvas, false, true);
                    UIManager.SetCanvas(UIManager.RewardCanvas, false, true);
                    // TODO: InformationCanvas, true
                    break;

                case SceneType.Gig:
                    UIManager.ChangeScene(GameManager.SceneData.gigSceneIndex);
                    UIManager.SetCanvas(UIManager.GigCanvas, false, true);
                    UIManager.SetCanvas(UIManager.RewardCanvas, false, true);
                    // TODO: InformationCanvas, true
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public void ExitApp()
        {
            GameManager.OnExitApp();
            Application.Quit();
        }
    }
}