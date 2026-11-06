using UnityEngine;
using UnityEngine.InputSystem;

/// Interfața de input a mașinii.
/// Pregătită pentru multiplayer: în rețea va exista o a doua implementare
/// care citește din pachetele de rețea, nu din tastatură (schimbăm doar asta).
public interface ICarInput
{
    float Throttle { get; }   // -1 (frână/marșarier) .. +1 (accelerație)
    float Steer { get; }      // -1 (stânga) .. +1 (dreapta)
    bool Handbrake { get; }
    bool ConsumeRespawn();
}

/// Citește tastatura + gamepad-ul prin noul Input System.
/// Zero de setat în editor — funcționează imediat.
[DisallowMultipleComponent]
public class CarInput : MonoBehaviour, ICarInput
{
    [Header("Ce input folosești")]
    public bool useKeyboard = true;
    public bool useGamepad = true;

    public float Throttle { get; private set; }
    public float Steer { get; private set; }
    public bool Handbrake { get; private set; }

    bool respawnFlag;

    void Update()
    {
        float throttle = 0f, steer = 0f;
        bool handbrake = false;

        if (useKeyboard && Keyboard.current != null)
        {
            var kb = Keyboard.current;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) throttle += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) throttle -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) steer += 1f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) steer -= 1f;
            handbrake |= kb.spaceKey.isPressed;
            if (kb.rKey.wasPressedThisFrame) respawnFlag = true;
        }

        if (useGamepad && Gamepad.current != null)
        {
            var gp = Gamepad.current;
            throttle += gp.rightTrigger.ReadValue() - gp.leftTrigger.ReadValue();
            steer += gp.leftStick.x.ReadValue();
            handbrake |= gp.buttonEast.isPressed;              // B / Circle
            if (gp.buttonNorth.wasPressedThisFrame) respawnFlag = true; // Y / Triangle
        }

        Throttle = Mathf.Clamp(throttle, -1f, 1f);
        Steer = Mathf.Clamp(steer, -1f, 1f);
        Handbrake = handbrake;
    }

    public bool ConsumeRespawn()
    {
        bool v = respawnFlag;
        respawnFlag = false;
        return v;
    }
}
