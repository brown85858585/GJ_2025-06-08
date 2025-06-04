using UnityEngine;

public class UIFaceCamera : MonoBehaviour
{
    public Camera targetCamera;

    private void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (targetCamera == null) return;

        
        transform.rotation = targetCamera.transform.rotation;
    }
}