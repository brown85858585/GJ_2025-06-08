using Game.Quests;

namespace Game.Models
{
    public class DayModel
    {

        private int DayNum;

        public QuestsModel CommonQModel;

        public int  DifficultLevel{get { return DayNum; } }



			public void SwitchDay()
        {

            DayNum++;
            CommonQModel.DifficultyLevel = DayNum;

        }
    }


}