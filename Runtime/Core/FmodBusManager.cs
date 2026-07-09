#if BISC8_BETTERFMOD_PRESENT
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

internal sealed class FmodBusManager
{
    private IFmodBackend backend;

    internal FmodBusManager(IFmodBackend backend)
    {
        this.backend = backend;
    }

    internal void SetBackend(IFmodBackend backend)
    {
        this.backend = backend;
    }

    internal void SetBusVolume(string path, float volume)
    {
        if (TryGetBus(path, out Bus bus))
            bus.setVolume(Mathf.Clamp01(volume));
    }

    internal float GetBusVolume(string path)
    {
        if (!TryGetBus(path, out Bus bus))
            return 0f;

        bus.getVolume(out float volume);
        return volume;
    }

    internal void SetBusPaused(string path, bool paused)
    {
        if (TryGetBus(path, out Bus bus))
            bus.setPaused(paused);
    }

    internal void StopBus(string path)
    {
        if (TryGetBus(path, out Bus bus))
            bus.stopAllEvents(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    internal void SetVcaVolume(string path, float volume)
    {
        if (TryGetVca(path, out VCA vca))
            vca.setVolume(Mathf.Clamp01(volume));
    }

    internal float GetVcaVolume(string path)
    {
        if (!TryGetVca(path, out VCA vca))
            return 0f;

        vca.getVolume(out float volume);
        return volume;
    }

    private bool TryGetBus(string path, out Bus bus)
    {
        bus = default;

        if (string.IsNullOrWhiteSpace(path))
            return false;

        try
        {
            bus = RuntimeManager.GetBus(path);
            return bus.isValid();
        }
        catch (BusNotFoundException)
        {
            LogWarning("[FMOD] Bus not found: " + path);
            return false;
        }
    }

    private bool TryGetVca(string path, out VCA vca)
    {
        vca = default;

        if (string.IsNullOrWhiteSpace(path))
            return false;

        try
        {
            vca = RuntimeManager.GetVCA(path);
            return vca.isValid();
        }
        catch (VCANotFoundException)
        {
            LogWarning("[FMOD] VCA not found: " + path);
            return false;
        }
    }

    private void LogWarning(string message)
    {
        if (backend.LogWarnings)
            Debug.LogWarning(message);
    }
}
#endif
