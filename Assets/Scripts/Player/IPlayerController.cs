using UnityEngine;

namespace Player
{
    public interface IPlayerController
    {
        public void SetPosition(Vector3 position);
        public void ToggleMovement();
        
        public IPlayerDialogue Dialogue { get; }
    }
}