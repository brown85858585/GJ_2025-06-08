using System;
using UnityEngine;

namespace Game.Models
{
    public class PlayerModel
    {
        private int _stamina;
        private int _score;
        private int _speed;
        private int _mood;

        public bool Grounded;
        public Transform PlayerTransform;
        
        public ItemCategory ItemInHand { get; set; }
        
        public int Stamina
        {
            get => _stamina;
            set => _stamina = Mathf.Clamp(value, 0, 100);
        }
        
        // Наборы внутренних состояний
        public CommonQuestModel CommonQModel { get; private set; }
        public DayModel DayModel { get; private set; }

        public PlayerModel(CommonQuestModel commonQModel, DayModel dayModel)
        {
            CommonQModel = commonQModel;
            DayModel = dayModel;
            _stamina = 100;
            _score = 0;
            _speed = 5;
            _mood = 100;
        }
        
         
        public void CheckGrounded(Transform position, CapsuleCollider _capsuleCollider, LayerMask whatIsGround)
        {
            Grounded = Physics.Raycast(position.position, Vector3.down, _capsuleCollider.height * 0.5f + 0.2f,
                whatIsGround);
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

        public void UpdateOptions()
        {
            switch (DayModel.DifficultLevel)
            {
                case 1:
                    _stamina = 100;
                    _score = 0;
                    _speed = 5;
                    _mood = 100;
                    break;
                case 2:
                    _stamina = 100;
                    _score = 0;
                    _speed = 5;
                    _mood = 100;
                    break;
                case 3:
                    _stamina = 100;
                    _score = 0;
                    _speed = 5;
                    _mood = 100;
                    break;
                case 4:
                    _stamina = 100;
                    _score = 0;
                    _speed = 5;
                    _mood = 100;
                    break;
                case 5:
                    _stamina = 100;
                    _score = 0;
                    _speed = 5;
                    _mood = 100;
                    break;
                case 6:
                    _stamina = 100;
                    _score = 0;
                    _speed = 5;
                    _mood = 100;
                    break;
                case 7:
                    _stamina = 100;
                    _score = 0;
                    _speed = 5;
                    _mood = 100;
                    break;
            }    

        }

    }


}