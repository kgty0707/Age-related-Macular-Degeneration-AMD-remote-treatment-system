---
name: Using the AutoDetectUtility class
slug: 'autodetect-utility'
---

# Using the AutoDetectUtility class

The `Haply.HardwareAPI.Unity` namespace contains the `AutoDetectUtility` class. This class is used internally to populate the device dropdowns in the **Haptic Thread** and **Handle Thread** inspectors.

It can also be used directly to detect connected hardware for use in your UI or other app logic.

## Example

```csharp
using UnityEngine;
using Haply.HardwareAPI.Unity;

public class AutoDetectExample : MonoBehaviour
{
    private void Awake ()
    {
        // Register callbacks for device detection events

        AutoDetectUtility.OnDetectInverse3 += e => Debug.Log($"Inverse3: {e.id:X4} {e.handedness} ({e.name})");
        AutoDetectUtility.OnDetectHandle += e => Debug.Log($"Handle: {e.id:X4}");
    }

    private void Update()
    {
        // Device detection events originate outside of
        // the main Unity thread, so we need to call Poll,
        // which safely empties the concurrent event queue

        AutoDetectUtility.Poll();
    }
}
```

## Troubleshooting

Please note that `AutoDetectUtility` will fail to detect any device already bound to a **Haptic Thread** or **Handle Thread** in the loaded scene.