using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloweGameOptions : MonoBehaviour
{

    [Header("Curent View")]
    [SerializeField] public GameObject UsedPrefab; // Скорость горизонтального движения
    [Header("Water Animation")]
    [SerializeField] public float waterMinY = -750f; // Минимальная позиция воды
    [SerializeField] public float waterMaxY = -150f;  // Максимальная позиция воды
    [SerializeField] public float waterHorizontalRange = 50f; // Диапазон горизонтального движения
    [SerializeField] public float waterHorizontalSpeed = 2f; // Скорость горизонтального движения

    [Header("Game Settings")]
    public float indicatorSpeed = 100f;
    public float trackHeight = 300f; // Высота зоны для воды
    public float zoneHeight = 75f;
    public int maxAttempts = 3;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
