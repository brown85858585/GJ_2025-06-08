namespace Game.Models
{
    public class BaseBusController
    {
        private DayModel DModel = new DayModel();
        private CommonQuestModel QuestModel;
        private PlayerModel playerModel;
        private MainModel Model;

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
