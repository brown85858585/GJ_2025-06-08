using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MainCanvasUi : MonoBehaviour
    {
        [SerializeField] private Button nextLevelButton;
        
        public Button NextLevelButton => nextLevelButton;
    }
}