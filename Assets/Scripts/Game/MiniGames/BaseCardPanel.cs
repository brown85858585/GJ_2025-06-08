using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BaseCardPanel : MonoBehaviour
{
    [SerializeField] private Button buttonQ;
    [SerializeField] private Button buttonE;
    [SerializeField] private GameObject qText;
    [SerializeField] private GameObject eText;
    [SerializeField] private GameObject panelEQ;


    public Button ButtonQ => buttonQ;
    public Button ButtonE => buttonE;
    public GameObject QText => qText;
    public GameObject EText => eText;
    public GameObject PanelEQ => panelEQ;
}
