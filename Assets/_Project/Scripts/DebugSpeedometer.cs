using UnityEngine;

/// DebugSpeedometer — viteza afișată pe ecran, pentru testarea feel-ului (S2/S3).
/// Se pune pe același GameObject cu ArcadeCarController (pe "Car").
public class DebugSpeedometer : MonoBehaviour
{
    public ArcadeCarController car;
    GUIStyle style;

    void Awake()
    {
        if (car == null) car = GetComponent<ArcadeCarController>();
    }

    void OnGUI()
    {
        if (style == null)
        {
            // Copiem stilul Unity în OnGUI, când fontul built-in este disponibil.
            // Elimină avertismentul „No Font Asset has been assigned” din Unity 6.
            style = new GUIStyle(GUI.skin.label);
            style.fontSize = 26;
            style.normal.textColor = Color.white;
        }

        if (car == null) return;
        string txt = Mathf.Abs(car.SpeedKmh).ToString("0") + " km/h";
        if (car.IsDrifting) txt += "   << DRIFT >>";
        GUI.Label(new Rect(24f, 20f, 500f, 44f), txt, style);
    }
}
