#if BISC8_BETTERFMOD_PRESENT
using FMOD.Studio;
using System.Collections;
using UnityEngine;

internal sealed class FmodFadeManager
{
    private readonly FmodCommands commands;

    internal FmodFadeManager(FmodCommands commands)
    {
        this.commands = commands;
    }

    internal void FadeTo(FmodManagedInstance managed, float targetVolume, float duration)
    {
        if (managed == null || !managed.IsValid)
            return;

        if (managed.FadeCoroutine != null)
            commands.StopManagedCoroutine(managed.FadeCoroutine);

        managed.FadeCoroutine = commands.StartManagedCoroutine(FadeToRoutine(managed, targetVolume, Mathf.Max(0f, duration)));
    }

    internal void FadeOutAndStop(FmodManagedInstance managed, float duration)
    {
        if (managed == null || !managed.IsValid)
            return;

        if (managed.FadeCoroutine != null)
            commands.StopManagedCoroutine(managed.FadeCoroutine);

        managed.FadeCoroutine = commands.StartManagedCoroutine(FadeOutAndStopRoutine(managed, Mathf.Max(0f, duration)));
    }

    private IEnumerator FadeToRoutine(FmodManagedInstance managed, float targetVolume, float duration)
    {
        managed.Instance.getVolume(out float startVolume);

        if (duration <= 0f)
        {
            managed.Instance.setVolume(targetVolume);
            managed.FadeCoroutine = null;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration && managed.IsValid)
        {
            elapsed += Time.deltaTime;
            managed.Instance.setVolume(Mathf.Lerp(startVolume, targetVolume, elapsed / duration));
            yield return null;
        }

        if (managed.IsValid)
            managed.Instance.setVolume(targetVolume);

        managed.FadeCoroutine = null;
    }

    private IEnumerator FadeOutAndStopRoutine(FmodManagedInstance managed, float duration)
    {
        yield return FadeToRoutine(managed, 0f, duration);

        if (managed.IsValid)
            commands.ReleaseInstance(managed, FMOD.Studio.STOP_MODE.IMMEDIATE);
    }
}
#endif
