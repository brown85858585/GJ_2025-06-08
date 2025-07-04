using UnityEngine;

namespace Player
{
    public interface IPlayerController
    {
        public void SetPosition(Vector3 position);
        public void SetRotation(Quaternion rotation);
        public void ToggleMovement(bool state);

        public void SetFallingAnimation();

        public PlayerModel Model { get; }
        public IPlayerMovement Movement { get; }
        public IPlayerDialogue Dialogue { get; }
    }
}