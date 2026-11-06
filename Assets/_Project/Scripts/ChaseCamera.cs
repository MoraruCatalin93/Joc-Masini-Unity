using UnityEngine;

/// ChaseCamera — camera care urmărește mașina, smooth (TGM Racing, S2).
/// Se pune pe Main Camera. Trage obiectul "Car" în câmpul Target.
public class ChaseCamera : MonoBehaviour
{
    [Header("Țintă")]
    public Transform target;

    [Header("Poziționare")]
    public float distance = 6.5f;
    public float height = 2.6f;

    [Header("Smoothing (mai mare = mai rigid)")]
    public float positionLerp = 5f;
    public float rotationLerp = 7f;

    [Header("Privește în fața mașinii")]
    public float lookAhead = 3f;

    [Header("FOV kick la viteză (senzația de viteză)")]
    public float baseFov = 60f;
    public float extraFovAtSpeed = 12f;
    public float fovSpeedReference = 40f; // m/s la care se atinge FOV-ul maxim

    Rigidbody targetRb;
    Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (target != null) targetRb = target.GetComponent<Rigidbody>();
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 vel = targetRb != null ? targetRb.linearVelocity : Vector3.zero;
        Vector3 flatVel = new Vector3(vel.x, 0f, vel.z);

        // La viteză mare camera stă în spatele DIRECȚIEI DE MERS (nu al mașinii)
        // — la drift vezi mașina alunecând lateral, very NFS 😎
        Vector3 dir = flatVel.magnitude > 6f ? flatVel.normalized : target.forward;

        Vector3 wantedPos = target.position - dir * distance + Vector3.up * height;
        float tPos = 1f - Mathf.Exp(-positionLerp * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, wantedPos, tPos);

        Vector3 lookPoint = target.position + Vector3.up * 1.1f + dir * lookAhead;
        Quaternion wantedRot = Quaternion.LookRotation(lookPoint - transform.position);
        float tRot = 1f - Mathf.Exp(-rotationLerp * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, wantedRot, tRot);

        if (cam != null)
        {
            float speedNorm = Mathf.Clamp01(flatVel.magnitude / fovSpeedReference);
            cam.fieldOfView = Mathf.Lerp(baseFov, baseFov + extraFovAtSpeed, speedNorm);
        }
    }
}
