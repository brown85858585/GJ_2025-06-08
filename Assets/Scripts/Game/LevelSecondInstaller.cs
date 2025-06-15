using Game.Models;
using Player;
using UnityEngine;

namespace Game
{
    public class LevelSecondInstaller : MonoBehaviour
    {
        public void Initialize( PlayerController playerController, PlayerModel playerModel)
        {
            playerController.SetPosition(playerModel.PlayerTransform, this.transform.position + Vector3.up);
        }
    }
}