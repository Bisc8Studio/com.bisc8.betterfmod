using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Ponte simples para chamar FMODB8 por Animation Events.
/// Os metodos publicos usam apenas void ou string para evitar parametros
/// aleatorios de int/float/Object na janela de Animation Event.
/// </summary>
[AddComponentMenu("FMODB8/FMODB8 Event Anin")]
public class FmodAninEvent : MonoBehaviour
{
    public enum ConditionType
    {
        None,
        InScene
    }

    [System.Serializable]
    public sealed class ConditionEntry
    {
        public ConditionType type;
        public bool invert;
        public string value;
    }

    [System.Serializable]
    public sealed class ConditionGroup
    {
        public List<ConditionEntry> entries = new List<ConditionEntry>();
    }

    [Header("Animation Defaults")]
    [SerializeField] private string defaultEventId;
    [SerializeField] private string defaultEmitterKey;
    [SerializeField] private string defaultParameter;
    [SerializeField] private string defaultLabel;
    [SerializeField] private float defaultRadius = 5f;
    [SerializeField] private float defaultFadeTime = 1f;
    [SerializeField] private bool warnWhenMissing = true;

    [Header("Conditions")]
    [SerializeField] private List<ConditionGroup> conditions = new List<ConditionGroup>();

    private FmodHandle lastHandle;

    public void Play()
    {
        Play(defaultEventId);
    }

    public void Play(string eventId)
    {
        if (!CanExecute())
            return;

        string resolvedId = ResolveEventId(eventId);
        if (string.IsNullOrWhiteSpace(resolvedId))
            return;

        lastHandle = FmodB8.Play(resolvedId);
    }

    public void PlayLoop()
    {
        PlayLoop(defaultEventId);
    }

    public void PlayLoop(string eventId)
    {
        if (!CanExecute())
            return;

        string resolvedId = ResolveEventId(eventId);
        if (string.IsNullOrWhiteSpace(resolvedId))
            return;

        lastHandle = FmodB8.PlayLoop(resolvedId);
    }

    public void Play3D()
    {
        Play3D(defaultEventId);
    }

    public void Play3D(string eventId)
    {
        if (!CanExecute())
            return;

        string resolvedId = ResolveEventId(eventId);
        if (string.IsNullOrWhiteSpace(resolvedId))
            return;

        lastHandle = FmodB8.Event(resolvedId)
            .As3D()
            .Position(transform.position)
            .Play();
    }

    public void PlayAttached()
    {
        PlayAttached(defaultEventId);
    }

    public void PlayAttached(string eventId)
    {
        if (!CanExecute())
            return;

        string resolvedId = ResolveEventId(eventId);
        if (string.IsNullOrWhiteSpace(resolvedId))
            return;

        lastHandle = FmodB8.Event(resolvedId)
            .As3D()
            .FollowTransform(transform)
            .Play();
    }

    public void PlayAttachedRadius()
    {
        PlayAttachedRadius(defaultEventId);
    }

    public void PlayAttachedRadius(string eventId)
    {
        if (!CanExecute())
            return;

        string resolvedId = ResolveEventId(eventId);
        if (string.IsNullOrWhiteSpace(resolvedId))
            return;

        lastHandle = FmodB8.Event(resolvedId)
            .As3D()
            .FollowTransform(transform)
            .Radius(Mathf.Max(0.01f, defaultRadius))
            .Play();
    }

    public void PlayFadeIn()
    {
        PlayFadeIn(defaultEventId);
    }

    public void PlayFadeIn(string eventId)
    {
        if (!CanExecute())
            return;

        string resolvedId = ResolveEventId(eventId);
        if (string.IsNullOrWhiteSpace(resolvedId))
            return;

        lastHandle = FmodB8.Event(resolvedId)
            .FadeIn(Mathf.Max(0f, defaultFadeTime))
            .Play();
    }

    public void Stop()
    {
        StopRelease(defaultEventId);
    }

    public void Stop(string eventId)
    {
        StopRelease(eventId);
    }

    public void StopRelease()
    {
        StopRelease(defaultEventId);
    }

    public void StopRelease(string eventId)
    {
        if (!CanExecute())
            return;

        if (lastHandle != null && lastHandle.IsValid && IsDefaultOrEmpty(eventId))
        {
            lastHandle.Stop(true, Mathf.Max(0f, defaultFadeTime));
            return;
        }

        string resolvedId = ResolveEventId(eventId);
        if (!string.IsNullOrWhiteSpace(resolvedId))
            FmodB8.Stop(resolvedId, true, Mathf.Max(0f, defaultFadeTime));
    }

    public void StopImmediate()
    {
        StopImmediate(defaultEventId);
    }

    public void StopImmediate(string eventId)
    {
        if (!CanExecute())
            return;

        if (lastHandle != null && lastHandle.IsValid && IsDefaultOrEmpty(eventId))
        {
            lastHandle.Stop(false);
            return;
        }

        string resolvedId = ResolveEventId(eventId);
        if (!string.IsNullOrWhiteSpace(resolvedId))
            FmodB8.Stop(resolvedId, false);
    }

    public void FadeOut()
    {
        if (!CanExecute())
            return;

        if (lastHandle != null && lastHandle.IsValid)
            lastHandle.FadeOut(Mathf.Max(0f, defaultFadeTime));
        else
            StopRelease(defaultEventId);
    }

    public void Pause()
    {
        Pause(defaultEventId);
    }

    public void Pause(string eventId)
    {
        if (!CanExecute())
            return;

        if (lastHandle != null && lastHandle.IsValid && IsDefaultOrEmpty(eventId))
        {
            lastHandle.Pause();
            return;
        }

        string resolvedId = ResolveEventId(eventId);
        if (!string.IsNullOrWhiteSpace(resolvedId))
            FmodB8.Pause(resolvedId);
    }

    public void Resume()
    {
        Resume(defaultEventId);
    }

    public void Resume(string eventId)
    {
        if (!CanExecute())
            return;

        if (lastHandle != null && lastHandle.IsValid && IsDefaultOrEmpty(eventId))
        {
            lastHandle.Resume();
            return;
        }

        string resolvedId = ResolveEventId(eventId);
        if (!string.IsNullOrWhiteSpace(resolvedId))
            FmodB8.Resume(resolvedId);
    }

    public void TogglePause()
    {
        TogglePause(defaultEventId);
    }

    public void TogglePause(string eventId)
    {
        if (!CanExecute())
            return;

        if (lastHandle != null && lastHandle.IsValid && IsDefaultOrEmpty(eventId))
        {
            lastHandle.TogglePause();
            return;
        }

        string resolvedId = ResolveEventId(eventId);
        if (!string.IsNullOrWhiteSpace(resolvedId))
            FmodB8.TogglePause(resolvedId);
    }

    public void SetParameterLabel()
    {
        if (!CanExecute())
            return;

        if (string.IsNullOrWhiteSpace(defaultParameter) || string.IsNullOrWhiteSpace(defaultLabel))
            return;

        if (lastHandle != null && lastHandle.IsValid)
            lastHandle.SetParameterLabel(defaultParameter, defaultLabel);
        else if (!string.IsNullOrWhiteSpace(defaultEventId))
            FmodB8.SetParameterLabel(defaultEventId, defaultParameter, defaultLabel);
    }

    public void GetState()
    {
        GetState(defaultEventId);
    }

    public void GetState(string eventId)
    {
        if (!CanExecute())
            return;

        string resolvedId = ResolveEventId(eventId);
        if (!string.IsNullOrWhiteSpace(resolvedId))
            FmodB8.GetState(resolvedId);
    }

    public void AddEmitter()
    {
        AddEmitter(defaultEmitterKey);
    }

    public void AddEmitter(string emitterKey)
    {
        SetEmitterEnabled(emitterKey, true);
    }

    public void RemoveEmitter()
    {
        RemoveEmitter(defaultEmitterKey);
    }

    public void RemoveEmitter(string emitterKey)
    {
        SetEmitterEnabled(emitterKey, false);
    }

    public void PlayEmitter()
    {
        PlayEmitter(defaultEmitterKey);
    }

    public void PlayEmitter(string emitterKey)
    {
        if (!CanExecute())
            return;

        FmodEmitterCustom emitter = ResolveEmitter(emitterKey);
        if (emitter != null)
            emitter.Play();
    }

    public void StopEmitter()
    {
        StopEmitter(defaultEmitterKey);
    }

    public void StopEmitter(string emitterKey)
    {
        if (!CanExecute())
            return;

        FmodEmitterCustom emitter = ResolveEmitter(emitterKey);
        if (emitter != null)
            emitter.Stop(defaultFadeTime > 0f);
    }

    public void PauseEmitter()
    {
        PauseEmitter(defaultEmitterKey);
    }

    public void PauseEmitter(string emitterKey)
    {
        if (!CanExecute())
            return;

        FmodEmitterCustom emitter = ResolveEmitter(emitterKey);
        if (emitter != null)
            emitter.Pause(true);
    }

    public void ResumeEmitter()
    {
        ResumeEmitter(defaultEmitterKey);
    }

    public void ResumeEmitter(string emitterKey)
    {
        if (!CanExecute())
            return;

        FmodEmitterCustom emitter = ResolveEmitter(emitterKey);
        if (emitter != null)
            emitter.Pause(false);
    }

    private void SetEmitterEnabled(string emitterKey, bool enabled)
    {
        if (!CanExecute())
            return;

        FmodEmitterCustom emitter = ResolveEmitter(emitterKey);
        if (emitter != null)
            emitter.enabled = enabled;
    }

    private FmodEmitterCustom ResolveEmitter(string emitterKey)
    {
        string resolvedKey = string.IsNullOrWhiteSpace(emitterKey) ? defaultEmitterKey : emitterKey;

        if (FmodB8EmitterKey.TryResolve(resolvedKey, out FmodEmitterCustom emitter))
            return emitter;

        if (warnWhenMissing)
            Debug.LogWarning("[FMODB8] Emitter key not found for animation event: " + resolvedKey, this);

        return null;
    }

    private string ResolveEventId(string eventId)
    {
        string resolvedId = string.IsNullOrWhiteSpace(eventId) ? defaultEventId : eventId;

        if (string.IsNullOrWhiteSpace(resolvedId) && warnWhenMissing)
            Debug.LogWarning("[FMODB8] Animation event has no FMOD event id.", this);

        return resolvedId;
    }

    private bool IsDefaultOrEmpty(string eventId)
    {
        return string.IsNullOrWhiteSpace(eventId) || eventId == defaultEventId;
    }

    private bool CanExecute()
    {
        if (conditions == null || conditions.Count == 0)
            return true;

        foreach (ConditionGroup group in conditions)
        {
            if (!EvaluateConditionGroup(group))
                return false;
        }

        return true;
    }

    private bool EvaluateConditionGroup(ConditionGroup group)
    {
        if (group == null || group.entries == null || group.entries.Count == 0)
            return true;

        bool hasActiveEntry = false;

        foreach (ConditionEntry entry in group.entries)
        {
            if (entry == null || entry.type == ConditionType.None)
                continue;

            hasActiveEntry = true;

            if (EvaluateConditionEntry(entry))
                return true;
        }

        return !hasActiveEntry;
    }

    private bool EvaluateConditionEntry(ConditionEntry entry)
    {
        bool result = true;

        if (entry.type == ConditionType.InScene)
        {
            Scene activeScene = SceneManager.GetActiveScene();
            result = activeScene.name == entry.value || activeScene.buildIndex.ToString() == entry.value;
        }

        return entry.invert ? !result : result;
    }

    private void OnValidate()
    {
        defaultRadius = Mathf.Max(0.01f, defaultRadius);
        defaultFadeTime = Mathf.Max(0f, defaultFadeTime);
    }
}
