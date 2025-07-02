using System;

namespace Game.Installers
{
    public class ScenarioInstaller
    {
        public void NextLevelScenario(Action nextLevelStart)
        {
            nextLevelStart?.Invoke();
        }
    }
}