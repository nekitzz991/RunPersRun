using System.Runtime.InteropServices;
using UnityEngine;

public static class WebGLBeforeUnloadBridge
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void RegisterBeforeUnloadSave(string gameObjectName, string callbackMethod);
#endif

    public static void Register(MonoBehaviour target, string callbackMethod)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (target == null || string.IsNullOrEmpty(callbackMethod))
        {
            return;
        }

        RegisterBeforeUnloadSave(target.gameObject.name, callbackMethod);
#endif
    }
}
