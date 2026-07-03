#if FMOD_PRESENT
using FMODUnity;
using UnityEngine;

internal sealed class FmodParameterManager
{
    internal void SetParameter(FMOD.Studio.EventInstance instance, string parameter, float value)
    {
        if (string.IsNullOrWhiteSpace(parameter) || !instance.isValid())
            return;

        instance.setParameterByName(parameter, value);
    }

    internal float GetParameter(FMOD.Studio.EventInstance instance, string parameter)
    {
        if (string.IsNullOrWhiteSpace(parameter) || !instance.isValid())
            return 0f;

        instance.getParameterByName(parameter, out float value);
        return value;
    }

    internal void SetParameterLabel(FMOD.Studio.EventInstance instance, string parameter, string label)
    {
        if (string.IsNullOrWhiteSpace(parameter) || string.IsNullOrWhiteSpace(label) || !instance.isValid())
            return;

        instance.setParameterByNameWithLabel(parameter, label);
    }

    internal void SetGlobalParameter(string parameter, float value)
    {
        if (string.IsNullOrWhiteSpace(parameter))
            return;

        RuntimeManager.StudioSystem.setParameterByName(parameter, value);
    }

    internal float GetGlobalParameter(string parameter)
    {
        if (string.IsNullOrWhiteSpace(parameter))
            return 0f;

        RuntimeManager.StudioSystem.getParameterByName(parameter, out float value);
        return value;
    }
}
#endif
