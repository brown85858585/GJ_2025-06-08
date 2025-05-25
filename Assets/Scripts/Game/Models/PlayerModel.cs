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

    }


}