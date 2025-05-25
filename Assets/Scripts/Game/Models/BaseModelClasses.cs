using System;
using UnityEngine;
using BaseModels;

/// <summary>
/// Summary description for Class1
/// </summary>
namespace BaseModels
{

    public class PlayerController
    {
        Transform Player;
        /*
        void PlayerMove(MonoBehavier monoBehavier)
        {
            //Player
        }
        */
    }


    public class BaseBusController
    {

        DayModel DModel = new DayModel();
        CommonQuestModel QuestModel;
        PlayerModel playerModel;
        MainModel Model;

        BaseBusController()
        {
             QuestModel = new CommonQuestModel();
             playerModel = new PlayerModel(QuestModel, DModel);
             Model = new MainModel(playerModel);
        }

        public void SwitchDay()
        {
            DModel.SwitchDay();
            playerModel.UpdateOptions();
        }
    }


}
