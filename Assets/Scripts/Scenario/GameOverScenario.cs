using System.Threading;
using Cysharp.Threading.Tasks;
using Game.Intertitles;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scenario
{
    public class GameOverScenario
    {
        private IntertitleSystem _intertitleSystem;

        public GameOverScenario(IntertitleSystem intertitleSystem)
        {
            _intertitleSystem = intertitleSystem;
        }


        public async UniTask StartScenario()
        {
            MainCanvasUi mainCanvasUi = GameObject.FindObjectOfType<MainCanvasUi>(true);
            
            mainCanvasUi.gameObject.SetActive(false);
            void ReloadGame()
            {
                var ddScene = SceneManager.GetSceneByName("DontDestroyOnLoad");
                if (ddScene.IsValid())
                {
                    foreach (var go in ddScene.GetRootGameObjects())
                    {
                        Object.Destroy(go);
                    }
                }
                SceneManager.LoadScene("SampleUIScene", LoadSceneMode.Single);
            }

            await _intertitleSystem.ShowIntertitle(8, CancellationToken.None);
            ReloadGame();
        }
    }
}
