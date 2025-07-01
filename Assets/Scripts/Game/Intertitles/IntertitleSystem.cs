using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Player.Interfaces;
using UnityEngine;

namespace Game.Intertitles
{
    public class IntertitleSystem
    {
        private readonly List<IntertitleCardView> _intertitleCards;
        private IntertitleCardView _currentCardObj;
        private readonly IInputAdapter _inputAdapter;

        public IntertitleSystem(IntertitleConfig intertitleConfig, IInputAdapter inputAdapter)
        {
            _intertitleCards = new List<IntertitleCardView>();
            _inputAdapter = inputAdapter;

            foreach (var entry in intertitleConfig.intertitles)
            {
                _intertitleCards.Add(entry);
            }
        }

        public async UniTask ShowIntertitle(int levelManagerCurrentLevelIndex, CancellationToken cancellationToken)
        {
            await Show(levelManagerCurrentLevelIndex, cancellationToken);
            await ReadKeyPressAsync(cancellationToken);
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

        private async UniTask ReadKeyPressAsync(CancellationToken cancellationToken = default)
        {
            // UniTaskCompletionSource без значения
            
            Debug.Log("IntertitleSystem: Waiting for key press...");
            var tcs = new UniTaskCompletionSource();

            _inputAdapter.OnInteract += Handler;

            try
            {
                await tcs.Task.AttachExternalCancellation(cancellationToken);
            }
            finally
            {
                _inputAdapter.OnInteract -= Handler;
                Object.Destroy(_currentCardObj.gameObject); 
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