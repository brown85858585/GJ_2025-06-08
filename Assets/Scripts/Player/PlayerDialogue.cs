namespace Player
{
    public class PlayerDialogue : IPlayerDialogue
    {
        private readonly PlayerDialogueView _dialogueView;

        public PlayerDialogue(PlayerDialogueView dialogueView)
        {
            _dialogueView = dialogueView;
        }

        public void OpenDialogue(string dialogueText)
        {
            _dialogueView.gameObject.SetActive(true);
            _dialogueView.SetText(dialogueText);
        }
        
        public void CloseDialogue()
        {
            _dialogueView.gameObject.SetActive(false);
        }
    }
}