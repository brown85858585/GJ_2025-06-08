using System;
using Cysharp.Threading.Tasks;
using Game.Installers;
using Player;
using UnityEngine;
using Utilities;

namespace Scenario
{
    public class Day7FallingScenario : MonoBehaviour
    {
        private ScenarioInstaller _installer;
        private bool _isSlowed = true;

        private void Awake()
        {
            _installer = FindObjectOfType<ScenarioInstaller>();
        }

        private void Update()
        {
            if (_isSlowed)
                (_installer.PlayerController as PlayerController)?.ClampSpeed();
        }

        private void OnTriggerEnter(Collider other)
        {
           var targetMask = LayerMask.GetMask("Player");
           if (LayerChecker.CheckLayerMask(other.gameObject, targetMask))
           {
               StartScenario().Forget();
           }
        }

        private async UniTask StartScenario()
        {
            _installer.PlayerController.SetFallingAnimation();
            
            _isSlowed = false;
            await UniTask.Delay(5000);
            _installer.NextLevelScenario().Forget();
        }
        
    }
}
