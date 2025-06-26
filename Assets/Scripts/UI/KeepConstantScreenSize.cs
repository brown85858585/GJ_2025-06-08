using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class KeepConstantScreenSize : MonoBehaviour
{
    [Tooltip("Камера, чье FOV мы учитываем; если не указана — будет MainCamera")]
    public Camera cam;

    // Запоминаем стартовые параметры
    private float initDist;
    private float initFovTan;
    private Vector3 initScale;

    void Start()
    {
        if (cam == null) cam = Camera.main;
        // исходная дистанция точечной лейки до камеры
        initDist = Vector3.Distance(cam.transform.position, transform.position);
        // тангенс половины начального FOV
        initFovTan = Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad * 0.5f);
        initScale   = transform.localScale;
    }

    void LateUpdate()
    {
        // текущая дистанция и FOV
        float currDist   = Vector3.Distance(cam.transform.position, transform.position);
        float currFovTan = Mathf.Tan(cam.fieldOfView * Mathf.Deg2Rad * 0.5f);

        // коэффициент, сохраняющий постоянный угловой размер:
        //   scaleFactor = (currDist * currFovTan) / (initDist * initFovTan)
        float scaleFactor = (currDist * currFovTan) / (initDist * initFovTan);

        transform.localScale = initScale * scaleFactor;
    }
}