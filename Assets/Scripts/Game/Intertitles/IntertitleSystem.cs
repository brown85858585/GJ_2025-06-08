using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Player;
using Player.Interfaces;
using UnityEngine;

namespace Game.Intertitles
{
    public class IntertitleSystem
    {
        private readonly IInputAdapter _inputAdapter;
        
        private readonly List<IntertitleCardView> _intertitleCards;
        private readonly List<ScoreIntertitleCardView> _scoreIntertitleCards;
        private IntertitleCardView _currentCardObj;
        private ScoreIntertitleCardView _currentCardScoreObj;

        public IntertitleSystem(IntertitleConfig intertitleConfig, IInputAdapter inputAdapter)
        {
            _inputAdapter = inputAdapter;

            _intertitleCards = new List<IntertitleCardView>();
            foreach (var entry in intertitleConfig.intertitles)
            {
                _intertitleCards.Add(entry);
            }
            
            _scoreIntertitleCards = new List<ScoreIntertitleCardView>();
            foreach (var scoreCard in intertitleConfig.scoreIntertitles)
            {
                _scoreIntertitleCards.Add(scoreCard);
            }
        }

        public async UniTask ShowScoreIntertitle(int levelManagerCurrentLevelIndex, PlayerModel playerModel, CancellationToken cancellationToken)
        {
            await ShowScore(levelManagerCurrentLevelIndex, playerModel, cancellationToken);
            await ReadKeyPressAsync(_currentCardScoreObj.gameObject, cancellationToken);
        }

        private async UniTask ShowScore(
            int levelManagerCurrentLevelIndex,
            PlayerModel playerModel,
            CancellationToken cancellationToken = default)
        {
            _currentCardScoreObj = Object.Instantiate(_scoreIntertitleCards[levelManagerCurrentLevelIndex]);
            _currentCardScoreObj.Tweener.Show();
            playerModel.Score -= _currentCardScoreObj.RequiredScore;
            _currentCardScoreObj.SetScore(playerModel.Score);
            await UniTask.Delay(300, cancellationToken: cancellationToken);
        }

        public async UniTask ShowIntertitle(int levelManagerCurrentLevelIndex, CancellationToken cancellationToken)
        {
            await Show(levelManagerCurrentLevelIndex, cancellationToken);
            await ReadKeyPressAsync(_currentCardObj.gameObject, cancellationToken);
        }

        private async UniTask Show(
            int levelManagerCurrentLevelIndex,
            CancellationToken cancellationToken = default)
        {
            _currentCardObj = Object.Instantiate(_intertitleCards[levelManagerCurrentLevelIndex]);
            _currentCardObj.SequentialTextFader.gameObject.SetActive(true);

            var tcs = new UniTaskCompletionSource();

            _currentCardObj.SequentialTextFader.OnComplete += Handler;

            try
            {
                await tcs.Task.AttachExternalCancellation(cancellationToken);
            }
            finally
            {
                _currentCardObj.SequentialTextFader.OnComplete -= Handler;
            }

            return;

            void Handler()
            {
                tcs.TrySetResult();
            }
        }

        private async UniTask ReadKeyPressAsync(GameObject cardObj, CancellationToken cancellationToken = default)
        {
            // UniTaskCompletionSource без значения
            
            Debug.Log("IntertitleSystem: Waiting for key press...");
            var tcs = new UniTaskCompletionSource();

            _inputAdapter.OnGameInteract += Handler;

            try
            {
                await tcs.Task.AttachExternalCancellation(cancellationToken);
            }
            finally
            {
                _inputAdapter.OnGameInteract -= Handler;
                Object.Destroy(cardObj); 
            }

            return;

            void Handler(bool pressed)
            {
                Debug.Log($"IntertitleSystem: Handler called with pressed={pressed}");
                if (pressed)
                {
                    tcs.TrySetResult();
                }
            }
        }

        private UniTask ShowIntertitle(int levelManagerCurrentLevelIndex)
        {
            _currentCardObj = Object.Instantiate(_intertitleCards[levelManagerCurrentLevelIndex]);
            _currentCardObj.SequentialTextFader.gameObject.SetActive(true);
            var tcs = new UniTaskCompletionSource();
            _currentCardObj.SequentialTextFader.OnComplete += Handler;

            return tcs.Task;
            
            void Handler()
            {
                _currentCardObj.SequentialTextFader.OnComplete -= Handler; // важно снять подписку
                tcs.TrySetResult();
            }
        }
    }
}