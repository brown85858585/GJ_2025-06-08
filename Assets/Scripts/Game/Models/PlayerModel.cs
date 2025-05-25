using System;
using UnityEngine;

/// <summary>
/// Summary description for Class1
/// </summary>
namespace BaseModels
{
    public class PlayerModel
    {

        int stamina;
        int score;
        int speed;
        int mood;
        // Наборы внутренних состояний
        public CommonQuestModel CommonQModel { get; private set; }
        public DayModel DayModel { get; private set; }

        public PlayerModel(CommonQuestModel commonQModel, DayModel dayModel)
        {
            CommonQModel = commonQModel;
            DayModel = dayModel;
            stamina = 100;
            score = 0;
            speed = 5;
            mood = 100;
        }

        public UpdateOptions()
        {
            switch (DayModel.DiffcultLevel)
            {
                case 1:
                    stamina = 100;
                    score = 0;
                    speed = 5;
                    mood = 100;
                    break;
                case 2:
                    stamina = 100;
                    score = 0;
                    speed = 5;
                    mood = 100;
                    break;
                case 3:
                    stamina = 100;
                    score = 0;
                    speed = 5;
                    mood = 100;
                    break;
                case 4:
                    stamina = 100;
                    score = 0;
                    speed = 5;
                    mood = 100;
                    break;
                case 5:
                    stamina = 100;
                    score = 0;
                    speed = 5;
                    mood = 100;
                    break;
                case 6:
                    stamina = 100;
                    score = 0;
                    speed = 5;
                    mood = 100;
                    break;
                case 7:
                    stamina = 100;
                    score = 0;
                    speed = 5;
                    mood = 100;
                    break;
            }    

        }

    }


}