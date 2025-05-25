using System;
using UnityEngine;

/// <summary>
/// Summary description for Class1
/// </summary>
namespace BaseModels
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