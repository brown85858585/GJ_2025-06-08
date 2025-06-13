using System;
using Game.Quests;
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
        public DayModel DayModel { get; private set; }

        public PlayerModel(DayModel dayModel)
        {
            DayModel = dayModel;
            _stamina = 100;
            _score = 0;
            _speed = 5;
            _mood = 100;
        }
        
         
        public void CheckGrounded(Transform position, LayerMask whatIsGround)
        {
            var ray1 = Physics.Raycast(position.position + Vector3.forward* 0.2f+ Vector3.up * 0.2f, Vector3.down,0.4f,
                whatIsGround);
            var ray2 = Physics.Raycast(position.position + Vector3.right* 0.2f+ Vector3.up* 0.2f, Vector3.down,0.4f,
                whatIsGround);
            var ray3 = Physics.Raycast(position.position + Vector3.up * 0.2f, Vector3.down,  0.4f,
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