using System;
using UnityEngine;

/// <summary>
/// Summary description for Class1
/// </summary>
namespace BaseModels
{
    public class DayModel
    {

        private int DayNum;

        public CommonQuestModel CommonQModel;

        public int  DiffcultLevel{get { return DayNum; } }



			public void SwitchDay()
        {

            DayNum++;
            CommonQModel.DiffcultLevel = DayNum;

        }
    }


}