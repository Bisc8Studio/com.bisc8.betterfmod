using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Ponte simples para Animation Events. Os metodos publicos nao usam overload,
/// para evitar o aviso do Unity sobre funcoes duplicadas no MonoBehaviour.
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

    [Header("Defaults")]
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

    public void PlayDefault()
    {
        PlayEvent(defaultEventId, false, false, false);
    }

    public void PlayById(string eventId)
    {
        PlayEvent(eventId, false, false, false);
    }

    public void PlayLoopDefault()
    {
        PlayEvent(defaultEventId, true, false, false);
    }

    public void PlayLoopById(string eventId)
    {
        PlayEvent(eventId, true, false, false);
    }

    public void Play3DDefault()
    {
        PlayEvent(defaultEventId, false, true, false);
    }

    public void Play3DById(string eventId)
    {
        PlayEvent(eventId, false, true, false);
    }

    public void PlayAttachedDefault()
    {
        PlayEvent(defaultEventId, false, true, true);
    }

    public void PlayAttachedById(string eventId)
    {
        PlayEvent(eventId, false, true, true);
    }

    public void PlayAttachedRadiusDefault()
    {
        PlayEvent(defaultEventId, false, true, true, true);
    }

    public void PlayAttachedRadiusById(string eventId)
    {
        PlayEvent(eventId, false, true, true, true);
    }

    public void PlayFadeInDefault()
    {
        if (!CanExecute())
            return;

        string eventId = ResolveEventId(defaultEventId);
        if (string.IsNullOrWhiteSpace(eventId))
            return;

        lastHandle = FmodB8.Event(eventId)
            .FadeIn(Mathf.Max(0f, defaultFadeTime))
            .Play();
    }

    public void PlayFadeInById(string eventId)
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

    public void StopReleaseDefault()
    {
        StopEvent(defaultEventId, true);
    }

    public void StopReleaseById(string eventId)
    {
        StopEvent(eventId, true);
    }

    public void StopImmediateDefault()
    {
        StopEvent(defaultEventId, false);
    }

    public void StopImmediateById(string eventId)
    {
        StopEvent(eventId, false);
    }

    public void FadeOutLast()
    {
        if (!CanExecute())
            return;

        if (lastHandle != null && lastHandle.IsValid)
            lastHandle.FadeOut(Mathf.Max(0f, defaultFadeTime));
        else
            StopEvent(defaultEventId, true);
    }

    public void PauseDefault()
    {
        PauseEvent(defaultEventId);
    }

    public void PauseById(string eventId)
    {
        PauseEvent(eventId);
    }

    public void ResumeDefault()
    {
        ResumeEvent(defaultEventId);
    }

    public void ResumeById(string eventId)
    {
        ResumeEvent(eventId);
    }

    public void TogglePauseDefault()
    {
        TogglePauseEvent(defaultEventId);
    }

    public void TogglePauseById(string eventId)
    {
        TogglePauseEvent(eventId);
    }

    public void SetParameterLabelDefault()
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

    public void AddEmitterDefault()
    {
        SetEmitterEnabled(defaultEmitterKey, true);
    }

    public void AddEmitterByKey(string emitterKey)
    {
        SetEmitterEnabled(emitterKey, true);
    }

    public void RemoveEmitterDefault()
    {
        SetEmitterEnabled(defaultEmitterKey, false);
    }

    public void RemoveEmitterByKey(string emitterKey)
    {
        SetEmitterEnabled(emitterKey, false);
    }

    public void PlayEmitterDefault()
    {
        PlayEmitterByKey(defaultEmitterKey);
    }

    public void PlayEmitterByKey(string emitterKey)
    {
        if (!CanExecute())
            return;

        FmodEmitterCustom emitter = ResolveEmitter(emitterKey);
        if (emitter != null)
            emitter.Play();
    }

    public void StopEmitterDefault()
    {
        StopEmitterByKey(defaultEmitterKey);
    }

    public void StopEmitterByKey(string emitterKey)
    {
        if (!CanExecute())
            return;

        FmodEmitterCustom emitter = ResolveEmitter(emitterKey);
        if (emitter != null)
            emitter.Stop(defaultFadeTime > 0f);
    }

    private void PlayEvent(string eventId, bool loop, bool as3D, bool attach, bool useRadius = false)
    {
        if (!CanExecute())
            return;

        string resolvedId = ResolveEventId(eventId);
        if (string.IsNullOrWhiteSpace(resolvedId))
            return;

        if (loop)
        {
            lastHandle = FmodB8.PlayLoop(resolvedId);
            return;
        }

        if (!as3D)
        {
            lastHandle = FmodB8.Play(resolvedId);
            return;
        }

        FmodEventBuilder builder = FmodB8.Event(resolvedId).As3D();

        if (attach)
            builder.FollowTransform(transform);
        else
            builder.Position(transform.position);

        if (useRadius)
            builder.Radius(Mathf.Max(0.01f, defaultRadius));

        lastHandle = builder.Play();
    }

    private void StopEvent(string eventId, bool fade)
    {
        if (!CanExecute())
            return;

        if (lastHandle != null && lastHandle.IsValid && IsDefaultOrEmpty(eventId))
        {
            if (fade)
                lastHandle.Stop(true, Mathf.Max(0f, defaultFadeTime));
            else
                lastHandle.Stop(false);

            return;
        }

        string resolvedId = ResolveEventId(eventId);
        if (!string.IsNullOrWhiteSpace(resolvedId))
            FmodB8.Stop(resolvedId, fade, Mathf.Max(0f, defaultFadeTime));
    }

    private void PauseEvent(string eventId)
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

    private void ResumeEvent(string eventId)
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

    private void TogglePauseEvent(string eventId)
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
