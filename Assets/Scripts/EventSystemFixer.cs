using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

[ExecuteAlways]
public class EventSystemFixer : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void FixEventSystemBeforeSceneLoad()
    {
        SanitizeEventSystems();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void FixEventSystemAfterSceneLoad()
    {
        SanitizeEventSystems();
    }

    private void Awake()
    {
        SanitizeEventSystems();
    }

    private void OnEnable()
    {
        SanitizeEventSystems();
    }

    private void Update()
    {
        SanitizeEventSystems();
    }

    public static void SanitizeEventSystems()
    {
        EventSystem[] eventSystems = FindObjectsOfType<EventSystem>(true);

        foreach (var es in eventSystems)
        {
            if (es == null) continue;

            StandaloneInputModule[] legacyModules = es.GetComponents<StandaloneInputModule>();
            foreach (var legacy in legacyModules)
            {
                if (legacy != null)
                {
                    legacy.enabled = false; // Disable immediately to stop UpdateModule execution
                    if (Application.isPlaying)
                    {
                        Destroy(legacy);
                    }
                    else
                    {
                        DestroyImmediate(legacy);
                    }
                }
            }

            InputSystemUIInputModule newModule = es.GetComponent<InputSystemUIInputModule>();
            if (newModule == null)
            {
                es.gameObject.AddComponent<InputSystemUIInputModule>();
            }
        }
    }
}
