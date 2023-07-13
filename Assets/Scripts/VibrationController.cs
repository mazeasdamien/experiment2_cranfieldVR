using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class VibrationController : MonoBehaviour
{
    private List<InputDevice> devices = new List<InputDevice>();

    void Start()
    {
        // Get all connected devices
        InputDevices.GetDevices(devices);
    }

    public void TurnOnHaptic()
    {
        // Vibrate all connected devices
        foreach (InputDevice device in devices)
        {
            // Check if the device has haptic capabilities
            HapticCapabilities capabilities;
            if (device.TryGetHapticCapabilities(out capabilities))
            {
                if (capabilities.supportsImpulse)
                {
                    uint channel = 0;
                    device.SendHapticImpulse(channel, 1, 1);
                }
            }
        }
    }

    public void TurnOffHaptic()
    {
        // Stop vibration on all connected devices
        foreach (InputDevice device in devices)
        {
            // Check if the device has haptic capabilities
            HapticCapabilities capabilities;
            if (device.TryGetHapticCapabilities(out capabilities))
            {
                if (capabilities.supportsImpulse)
                {
                    device.StopHaptics();
                }
            }
        }
    }
}
