using UnityEngine;

public class BubbleBehavior : MonoBehaviour
{
    [Header("Shake")]
    public bool enableShake = true;
    public float shakeIntensity = 0.05f;
    public float shakeSpeed = 10f;

    [Header("Float")]
    public bool enableFloat = true;
    public float floatAmplitude = 0.2f;
    public float floatFrequency = 1f;

    [Header("Blink")]
    public bool enableBlink = true;
    public float blinkInterval = 1.5f;

    private Vector3 startPos;
    private Renderer[] renderers;
    private float blinkTimer = 0f;
    private bool isVisible = true;

    void Start()
    {
        startPos = transform.localPosition;
        renderers = GetComponentsInChildren<Renderer>();
    }

    void Update()
    {
        Vector3 offset = Vector3.zero;

        if (enableShake)
        {
            float shakeX = Mathf.PerlinNoise(Time.time * shakeSpeed, 0f) - 0.5f;
            float shakeY = Mathf.PerlinNoise(0f, Time.time * shakeSpeed) - 0.5f;
            float shakeZ = Mathf.PerlinNoise(Time.time * shakeSpeed, Time.time * shakeSpeed) - 0.5f;
            offset += new Vector3(shakeX, shakeY, shakeZ) * shakeIntensity;
        }

        if (enableFloat)
        {
            float floatY = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
            offset += new Vector3(0f, floatY, 0f);
        }

        transform.localPosition = startPos + offset;

        if (enableBlink)
        {
            blinkTimer += Time.deltaTime;
            if (blinkTimer >= blinkInterval)
            {
                blinkTimer = 0f;
                isVisible = !isVisible;
                foreach (var r in renderers)
                {
                    r.enabled = isVisible;
                }
            }
        }
    }
}
