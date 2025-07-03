using Cinemachine;
using Cysharp.Threading.Tasks;
using Game.Installers;
using Game.MiniGames.Park;
using Player;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Scenario
{
    public class Day8Scenario : MonoBehaviour
    {
        private ScenarioInstaller _installer;

        private void Awake()
        {
            _installer = FindObjectOfType<ScenarioInstaller>();
        }

        private void Start()
        {
            ParkLevelView parkMiniGame = FindObjectOfType<ParkLevelView>(true);
            MainCanvasUi mainCanvasUi = FindObjectOfType<MainCanvasUi>(true);
            var eventSystem = mainCanvasUi.GetComponentInChildren<EventSystem>().gameObject.transform.parent = null;
            mainCanvasUi.gameObject.SetActive(false);
            parkMiniGame.OnMiniGameStart += OnParkMiniGameStart;
            parkMiniGame.SetStaminaMultiplyer(0);
        }

        private void OnParkMiniGameStart()
        {
            Debug.Log("Park Mini Game Started");
            _installer.InputAdapter.SwitchAdapterToMiniGameMode();
            (_installer.PlayerController as PlayerController)?.StartEndRun();
            StartCamera();
        }

        private void StartCamera()
        {
            var vcam = _installer?.VirtualCamera;
            
            vcam.DestroyCinemachineComponent<CinemachineFramingTransposer>();

            vcam.AddCinemachineComponent<CinemachineComposer>();

            UniTask.Delay(30000).ContinueWith(() =>
            {
                (_installer.PlayerController as PlayerController)?.StopMovement(true);
            });
        }
    }
}
