namespace Game.Models
{
    public class DayModel
    {

        private int DayNum;

        public CommonQuestModel CommonQModel;

        public int  DifficultLevel{get { return DayNum; } }



			public void SwitchDay()
        {

            DayNum++;
            CommonQModel.DiffcultLevel = DayNum;

        }
    }


}