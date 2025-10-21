using ALWTTT.Managers;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ALWTTT.Utils
{
    public class SceneChanger : MonoBehaviour
    {
        private GameManager GameManager => GameManager.Instance;
        private UIManager UIManager => UIManager.Instance;

        private enum SceneType
        {
            MainMenu,
            BandSetup,
            SectorMap,
            Ship,
            Gig,
            GameOver
        }

        public void OpenMainMenuScene()
        {
            StartCoroutine(DelaySceneChange(SceneType.MainMenu));
        }

        public void OpenBandSetupScene()
        {
            StartCoroutine(DelaySceneChange(SceneType.BandSetup));
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

        public void OpenGameOverScene()
        {
            StartCoroutine(DelaySceneChange(SceneType.GameOver));
        }

        // TODO: Refactor
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

                case SceneType.BandSetup:
                    UIManager.ChangeScene(GameManager.SceneData.bandSetupSceneIndex);
                    UIManager.SetCanvas(UIManager.GigCanvas, false, true);
                    UIManager.SetCanvas(UIManager.RewardCanvas, false, true);

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
                case SceneType.GameOver:
                    UIManager.ChangeScene(GameManager.SceneData.gameOverSceneIndex);
                    UIManager.SetCanvas(UIManager.GigCanvas, false, true);
                    UIManager.SetCanvas(UIManager.RewardCanvas, false, true);

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