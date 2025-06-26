using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Knot.Localization;
using Knot.Localization.Components;

public class CurrentCart : MonoBehaviour
{

    [SerializeField] private KnotLocalizedTextMeshProUGUI content;
    [SerializeField] private KnotLocalizedTextMeshProUGUI senderName;

    public KnotLocalizedTextMeshProUGUI ContentText => content;
    public KnotLocalizedTextMeshProUGUI SanderText => senderName;



}
