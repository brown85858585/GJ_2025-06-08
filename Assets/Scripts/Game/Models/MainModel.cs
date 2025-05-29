namespace Game.Models
{
    public class MainModel
    {

        public PlayerModel CurrentPlayerModel { get; set; }
        public MainModel(PlayerModel playerModel)
        {
            CurrentPlayerModel = playerModel;
        }

        // BaseSettings....

    }


}