using UnityEngine;

namespace Game.Models
{
    public class PlayerModel
    {
        private int _stamina;
        private int _score;
        private int _speed;
        private int _mood;
        
        public int Stamina
        {
            get => _stamina;
            set => _stamina = Mathf.Clamp(value, 0, 100);
        }
        
        // Наборы внутренних состояний
        public CommonQuestModel CommonQModel { get; private set; }
        public DayModel DayModel { get; private set; }
        public Vector3 MoveDirection { get; set; }

        public PlayerModel(CommonQuestModel commonQModel, DayModel dayModel)
        {
            CommonQModel = commonQModel;
            DayModel = dayModel;
            _stamina = 100;
            _score = 0;
            _speed = 5;
            _mood = 100;
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