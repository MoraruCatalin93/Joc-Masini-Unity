using UnityEngine;

/// ArcadeCarController v2.1 — TGM Racing (S2).
/// Collider-ele primesc automat frecare zero: grip-ul este controlat de cod, nu de PhysX.
/// Downforce-ul crește cu viteza și nu mai lipește mașina de asfalt la plecare.
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CarInput))]
public class ArcadeCarController : MonoBehaviour
{
    [Header("— Motor —")]
    public float motorForce = 9000f;
    public float maxSpeedKmh = 170f;
    public float brakeForce = 12000f;
    public float reverseFactor = 0.5f;
    public float rollingResistance = 0.06f;

    [Header("— Viraj —")]
    public float steerStrength = 110f;
    public AnimationCurve steerBySpeed = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0.45f));

    [Header("— Grip & drift —")]
    public float grip = 5f;
    public float driftGrip = 1.6f;
    public float downforce = 25f;
    public Vector3 centerOfMassOffset = new Vector3(0f, -0.25f, 0f);
    public float groundRayLength = 0.7f;

    // —— stare publică (pentru HUD / rețea) ——
    public float SpeedKmh { get; private set; }
    public bool IsDrifting { get; private set; }
    public bool IsGrounded { get; private set; }

    Rigidbody rb;
    CarInput carInput;
    PhysicsMaterial zeroFrictionMaterial;
    Vector3 respawnPoint;
    Quaternion respawnRotation;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        carInput = GetComponent<CarInput>();
        rb.centerOfMass = centerOfMassOffset;
        SetupZeroFriction();
        respawnPoint = transform.position;
        respawnRotation = transform.rotation;
    }

    void SetupZeroFriction()
    {
        // Mașina arcade își calculează grip-ul în cod. Frecarea implicită PhysX
        // ar dubla efectul și, împreună cu downforce-ul, ar putea bloca plecarea.
        zeroFrictionMaterial = new PhysicsMaterial("TGM Car — Zero Friction")
        {
            dynamicFriction = 0f,
            staticFriction = 0f,
            bounciness = 0f,
            frictionCombine = PhysicsMaterialCombine.Minimum,
            bounceCombine = PhysicsMaterialCombine.Minimum,
            hideFlags = HideFlags.DontSave
        };

        foreach (Collider col in GetComponentsInChildren<Collider>())
            col.material = zeroFrictionMaterial;
    }

    void OnDestroy()
    {
        if (zeroFrictionMaterial != null) Destroy(zeroFrictionMaterial);
    }

    void FixedUpdate()
    {
        if (carInput == null || rb == null) return;

        if (carInput.ConsumeRespawn()) { Respawn(); return; }

        Vector3 fwd = transform.forward;
        float forwardSpeed = Vector3.Dot(rb.linearVelocity, fwd);
        SpeedKmh = forwardSpeed * 3.6f;

        IsGrounded = Physics.Raycast(transform.position + transform.up * 0.3f, -transform.up, groundRayLength);
        if (!IsGrounded) return;

        float throttle = carInput.Throttle;
        float steer = carInput.Steer;
        bool handbrake = carInput.Handbrake;

        // 1) accelerație / frână / marșarier
        if (throttle > 0.01f)
        {
            if (SpeedKmh < maxSpeedKmh)
                rb.AddForce(fwd * (motorForce * throttle));
        }
        else if (throttle < -0.01f)
        {
            if (forwardSpeed > 1f)
                rb.AddForce(fwd * (brakeForce * throttle));
            else if (SpeedKmh > -maxSpeedKmh * 0.35f)
                rb.AddForce(fwd * (motorForce * reverseFactor * throttle));
        }

        // 2) rezistență la rulare
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(-flatVel * rollingResistance, ForceMode.Acceleration);

        // 3) viraj
        float speedNorm = Mathf.Clamp01(Mathf.Abs(SpeedKmh) / 80f);
        float steerAmount = steer * steerStrength * steerBySpeed.Evaluate(speedNorm);
        if (Mathf.Abs(forwardSpeed) > 0.5f)
        {
            float dir = forwardSpeed >= 0f ? 1f : -1f;
            Quaternion delta = Quaternion.Euler(0f, steerAmount * dir * Time.fixedDeltaTime, 0f);
            rb.MoveRotation(rb.rotation * delta);
        }

        // 4) grip — tăiem viteza laterală (drift la handbrake)
        Vector3 lateralVel = transform.right * Vector3.Dot(rb.linearVelocity, transform.right);
        float lateralSpeed = lateralVel.magnitude;
        float gripNow = handbrake ? driftGrip : grip;
        rb.AddForce(-lateralVel * gripNow, ForceMode.Acceleration);

        // 5) handbrake = frânare ușoară
        if (handbrake && Mathf.Abs(forwardSpeed) > 1f)
            rb.AddForce(-fwd * Mathf.Sign(forwardSpeed) * 8f, ForceMode.Acceleration);

        IsDrifting = handbrake && lateralSpeed > 3.5f;

        // 6) downforce — zero la staționare, crește gradual cu viteza.
        // Versiunea veche aplica valoarea maximă și la 0 km/h, ceea ce putea
        // ține un Rigidbody de 1200 kg lipit de sol din cauza frecării PhysX.
        float downforceFactor = Mathf.Clamp01(flatVel.magnitude / 30f);
        rb.AddForce(-transform.up * downforce * downforceFactor, ForceMode.Acceleration);
    }

    public void SetRespawn(Vector3 point, Quaternion rotation)
    {
        respawnPoint = point;
        respawnRotation = rotation;
    }

    void Respawn()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.position = respawnPoint + Vector3.up * 0.5f;
        rb.rotation = respawnRotation;
    }
}
