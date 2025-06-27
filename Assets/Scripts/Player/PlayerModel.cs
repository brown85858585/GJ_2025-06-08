using Game;
using UniRx;
        using UnityEngine;
        
        namespace Player
        {
            public class PlayerModel
            {
                public readonly ReactiveProperty<int> CurrentScore = new ReactiveProperty<int>();
                
        private int _stamina;
        private int _speed;
        private int _mood;

        public bool Grounded;
        public Transform PlayerTransform;

        public ItemCategory ItemInHand { get; set; }

        public int Stamina
        {
            get => _stamina;
            set => _stamina = Mathf.Clamp(value, 0, 10000);
        }

        public int Score
        {
            get => CurrentScore.Value;
            set => CurrentScore.SetValueAndForceNotify(value);
        }
        public PlayerModel()
        {
            _stamina = 10000;
            CurrentScore.SetValueAndForceNotify(0);
            _speed = 5;
            _mood = 100;
        }


        public void CheckGrounded(Transform position, LayerMask whatIsGround)
        {
            var ray1 = Physics.Raycast(position.position + Vector3.forward * 0.2f + Vector3.up * 0.2f, Vector3.down,
                0.4f,
                whatIsGround);
            var ray2 = Physics.Raycast(position.position + Vector3.right * 0.2f + Vector3.up * 0.2f, Vector3.down, 0.4f,
                whatIsGround);
            var ray3 = Physics.Raycast(position.position + Vector3.up * 0.2f, Vector3.down, 0.4f,
                whatIsGround);
            Grounded = ray1 || ray2 || ray3;
        }


        public void ChangeGrid(Rigidbody rb, float groundDrag)
        {
            if (Grounded)
            {
                rb.drag = groundDrag;
            }
            else
            {
                rb.drag = 0;
            }
        }
    }
}
