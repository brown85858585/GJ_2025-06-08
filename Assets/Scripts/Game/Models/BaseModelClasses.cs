using System;
using UnityEngine;

/// <summary>
/// Summary description for Class1
/// </summary>
namespace BaseModels
{

    public class PlayerController
    {
        Transform Player;

        void PlayerMove(MonoBehavier monoBehavier)
        {
            //Player
        }

    }


    public class BaseBusController
    {

        var DModel = new DayModel();
        var QuestModel = new CommonQuestModel();
        var PlayerModel = new PlayerModel(QuestModel, DModel);
        var Model = new MainModel(PlayerModel);



        public void SwitchDay()
        {
            DModel.SwitchDay()
            PlayerModel.UpdateOptions();
        }
    }


}
