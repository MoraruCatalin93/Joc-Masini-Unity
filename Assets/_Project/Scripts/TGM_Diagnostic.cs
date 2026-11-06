using UnityEngine;
using UnityEngine.InputSystem;

/// TGM DIAGNOSTIC v3 — înlocuiește complet conținutul vechiului TGM_Diagnostic.cs
/// ▶ Play → ține W 3 sec → apasă K → apasă L → ține J 2 sec → spune-mi ce vezi.
[DisallowMultipleComponent]
public class TGM_Diagnostic : MonoBehaviour
{
    public ArcadeCarController car;
    public CarInput carInput;
    public Rigidbody rb;

    GUIStyle s, ok, bad;
    bool stylesInit;
    float y;
    int fixedCount, errorCount;
    string firstError = "";

    void Awake()
    {
        if (car == null) car = GetComponent<ArcadeCarController>();
        if (carInput == null) carInput = GetComponent<CarInput>();
        if (rb == null) rb = GetComponent<Rigidbody>();
        Application.logMessageReceived += OnLog;
    }

    void OnLog(string cond, string st, LogType t)
    {
        if (t == LogType.Error || t == LogType.Exception || t == LogType.Assert)
        {
            errorCount++;
            if (firstError == "") firstError = cond.Length > 100 ? cond.Substring(0, 100) : cond;
        }
    }

    void FixedUpdate() { fixedCount++; }

    void Update()
    {
        var kb = Keyboard.current; if (kb == null || rb == null) return;
        if (kb.kKey.wasPressedThisFrame) rb.linearVelocity = transform.forward * 12f; // TEST K: viteză directă
        if (kb.lKey.wasPressedThisFrame) rb.AddForce(Vector3.up * 9000f);            // TEST L: forță în sus
        if (kb.jKey.isPressed) rb.AddForce(transform.forward * 12000f);              // TEST J: forță înainte (a mea)
    }

    void InitStyles()
    {
        if (stylesInit) return;
        stylesInit = true;
        Font f = null;
        try { f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); } catch { }
        s = new GUIStyle(); s.fontSize = 20; s.font = f; s.normal.textColor = Color.white;
        ok = new GUIStyle(s); ok.normal.textColor = Color.green;
        bad = new GUIStyle(s); bad.normal.textColor = Color.red;
    }

    void OnGUI()
    {
        InitStyles();
        y = 70f;
        GUI.Label(new Rect(24, y, 1100, 30), "== DIAGNOSTIC v3: ține W; apasă K; apasă L; ține J ==", s); y += 30;

        Line("A. Erori în Console:", errorCount == 0,
             errorCount == 0 ? "ZERO" : errorCount + "! → " + firstError);

        bool e1 = car != null && car.enabled;
        bool e2 = carInput != null && carInput.enabled;
        Line("B. Componente bifate ON:", e1 && e2,
             "Controller: " + (car == null ? "LIPSEȘTE" : (e1 ? "ON" : "OFF!!!")) +
             " | Input: " + (carInput == null ? "LIPSEȘTE" : (e2 ? "ON" : "OFF!!!")));

        Line("C. FixedUpdate (fizica):", fixedCount > 3, fixedCount + " apeluri (normal: crește constant)");

        Line("D. Fixed Timestep:", Time.fixedDeltaTime > 0.001f && Time.fixedDeltaTime < 0.2f,
             Time.fixedDeltaTime.ToString("0.000") + "s (normal: 0.020)");

        Line("E. Simulation Mode:", Physics.simulationMode == SimulationMode.FixedUpdate,
             Physics.simulationMode.ToString() + " (normal: FixedUpdate)");

        Line("F. Kinematic:", rb != null && !rb.isKinematic,
             rb == null ? "-" : (rb.isKinematic ? "DA!!! → debifează Is Kinematic" : "nu"));

        Line("G. Constraints:", rb != null && rb.constraints == RigidbodyConstraints.None,
             rb == null ? "-" : rb.constraints.ToString() + " (normal: None)");

        Line("H. Throttle (W):", carInput != null && carInput.Throttle > 0.05f,
             carInput != null ? carInput.Throttle.ToString("0.00") : "-");

        if (rb != null)
        {
            GUI.Label(new Rect(24, y, 1100, 30), "Viteză: " + rb.linearVelocity.magnitude.ToString("0.00") + " m/s", s); y += 30;
        }

        GUI.Label(new Rect(24, y, 1100, 70),
            "TESTE (spune-mi ce face mașina la fiecare):\n" +
            "K = viteză directă | L = forță în sus (sare?) | J = forță înainte (independentă de controller)", s); y += 55;

        if (errorCount > 0)
        {
            GUI.Label(new Rect(24, y, 1100, 60), ">>> PRIMA EROARE (asta e cauza):\n" + firstError, bad);
        }
    }

    void Line(string label, bool good, string detail)
    {
        GUI.Label(new Rect(24, y, 1100, 30), label + "  " + detail, good ? ok : bad);
        y += 30;
    }
}
