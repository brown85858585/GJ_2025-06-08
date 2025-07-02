using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseCardPanel : MonoBehaviour
{
    [SerializeField] private Button buttonQ;
    [SerializeField] private Button buttonE;
    [SerializeField] private Image check;
    [SerializeField] private Image crossCheck;


    public Button ButtonQ => buttonE;
    public Button ButtonE => buttonE;
    public Image Ñheck => check;
    public Image ÑrossCheck => crossCheck;
}
