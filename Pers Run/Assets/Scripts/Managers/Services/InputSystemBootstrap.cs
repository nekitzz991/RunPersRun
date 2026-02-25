using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

public static class InputSystemBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (!EnhancedTouchSupport.enabled)
        {
            EnhancedTouchSupport.Enable();
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        ConfigureEventSystem();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ConfigureEventSystem();
    }

    private static void ConfigureEventSystem()
    {
        EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem = eventSystemObject.GetComponent<EventSystem>();
        }

        eventSystem.sendNavigationEvents = true;

        var inputSystemModule = eventSystem.GetComponent<InputSystemUIInputModule>();
        if (inputSystemModule == null)
        {
            inputSystemModule = eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }
        inputSystemModule.enabled = true;

        if (inputSystemModule.actionsAsset == null)
        {
            inputSystemModule.AssignDefaultActions();
        }

        var modules = eventSystem.GetComponents<BaseInputModule>();
        for (int i = 0; i < modules.Length; i++)
        {
            if (modules[i] == null || modules[i] == inputSystemModule)
            {
                continue;
            }

            modules[i].enabled = false;
            if (modules[i] is StandaloneInputModule)
            {
                Object.Destroy(modules[i]);
            }
        }
    }
}
