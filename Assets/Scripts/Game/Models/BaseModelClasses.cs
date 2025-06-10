using Game.Quests;

namespace Game.Models
{
    public class BaseBusController
    {
        private DayModel DModel = new DayModel();
        private QuestsModel QuestModel;
        private PlayerModel _playerModel;
        private MainModel Model;

        BaseBusController(PlayerModel playerModel)
        {
             QuestModel = new QuestsModel();
             _playerModel = playerModel;
             Model = new MainModel(playerModel);
        }

        public void SwitchDay()
        {
            DModel.SwitchDay();
            _playerModel.UpdateOptions();
        }
    }


}
