using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Registra um FmodEmitterCustom com uma chave legivel para Animation Events.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(FmodEmitterCustom))]
[AddComponentMenu("FMODB8/FMODB8 Emmiter Key")]
public class FmodB8EmitterKey : MonoBehaviour
{
    private static readonly Dictionary<string, FmodB8EmitterKey> Registry = new();

    [SerializeField] private string key;

    private FmodEmitterCustom emitter;
    private string registeredKey;

    public string Key
    {
        get { return string.IsNullOrWhiteSpace(key) ? name : key; }
    }

    public FmodEmitterCustom Emitter
    {
        get { return emitter != null ? emitter : GetComponent<FmodEmitterCustom>(); }
    }

    public static bool TryResolve(string emitterKey, out FmodEmitterCustom resolvedEmitter)
    {
        resolvedEmitter = null;

        if (string.IsNullOrWhiteSpace(emitterKey))
            return false;

        if (!Registry.TryGetValue(emitterKey, out FmodB8EmitterKey registeredKey) || registeredKey == null)
        {
            Registry.Remove(emitterKey);
            return false;
        }

        resolvedEmitter = registeredKey.Emitter;
        return resolvedEmitter != null;
    }

    private void Awake()
    {
        CacheEmitter();
        Register();
    }

    private void OnEnable()
    {
        CacheEmitter();
        Register();
    }

    private void OnDestroy()
    {
        Unregister();
    }

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(key))
            key = name;

        CacheEmitter();

        if (Application.isPlaying && isActiveAndEnabled)
            Register();
    }

    private void CacheEmitter()
    {
        if (emitter == null)
            emitter = GetComponent<FmodEmitterCustom>();
    }

    private void Register()
    {
        string resolvedKey = Key;

        if (string.IsNullOrWhiteSpace(resolvedKey))
            return;

        if (!string.IsNullOrWhiteSpace(registeredKey)
            && registeredKey != resolvedKey
            && Registry.TryGetValue(registeredKey, out FmodB8EmitterKey previousKey)
            && previousKey == this)
        {
            Registry.Remove(registeredKey);
        }

        Registry[resolvedKey] = this;
        registeredKey = resolvedKey;
    }

    private void Unregister()
    {
        if (string.IsNullOrWhiteSpace(registeredKey))
            return;

        if (Registry.TryGetValue(registeredKey, out FmodB8EmitterKey currentKey) && currentKey == this)
            Registry.Remove(registeredKey);

        registeredKey = null;
    }
}
