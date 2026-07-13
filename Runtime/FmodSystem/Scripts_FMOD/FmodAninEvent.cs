using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Ponte para chamar comandos FMODB8 por Animation Events.
/// Mantem os nomes legados e adiciona atalhos configuraveis no componente.
/// </summary>
[AddComponentMenu("FMODB8/FMODB8 Event Anin")]
public class FmodAninEvent : MonoBehaviour
{
    public enum ConditionType
    {
        None,
        InSceneBuildIndex,
        InSceneName,
        GameObjectActive,
        BehaviourEnabled
    }

    [System.Serializable]
    public sealed class ConditionEntry
    {
        public ConditionType type;
        public bool invert;
        public int intValue;
        public string stringValue;
        public GameObject gameObject;
        public Behaviour behaviour;
    }

    [System.Serializable]
    public sealed class ConditionGroup
    {
        public List<ConditionEntry> entries = new List<ConditionEntry>();
    }

    [Header("Defaults")]
    [SerializeField] private string defaultEventId;
    [SerializeField] private string defaultParameter;
    [SerializeField] private string defaultLabel;
    [SerializeField] private Transform defaultTarget;
    [SerializeField] private float defaultRadius = 5f;
    [SerializeField] private float defaultFadeTime = 1f;
    [SerializeField] private bool warnWhenMissing = true;

    [Header("Emitter")]
    [SerializeField] private FmodEmitterCustom defaultEmitter;

    [Header("Conditions")]
    [SerializeField] private List<ConditionGroup> conditions = new List<ConditionGroup>();

    private FmodHandle lastHandle;

    public void Play()
    {
        PlayOneShot(defaultEventId);
    }

    public void PlayEvent()
    {
        PlayOneShot(defaultEventId);
    }

    public void PlayEvent(string id)
    {
        PlayOneShot(id);
    }

    /// <summary>
    /// Toca um evento a partir de um Animation Event.
    /// </summary>
    public void PlayOneShot(string id)
    {
        if (!CanExecute())
            return;

        string resolvedId = ResolveEventId(id);
        if (string.IsNullOrWhiteSpace(resolvedId))
            return;

        lastHandle = FmodB8.Play(resolvedId);
    }

    public void PlayOneShot()
    {
        PlayOneShot(defaultEventId);
    }

    /// <summary>
    /// Toca um evento de loop a partir de um Animation Event.
    /// </summary>
    public void PlayLoop(string id)
    {
        if (!CanExecute())
            return;

        string resolvedId = ResolveEventId(id);
        if (string.IsNullOrWhiteSpace(resolvedId))
            return;

        lastHandle = FmodB8.PlayLoop(resolvedId);
    }

    public void PlayLoop()
    {
        PlayLoop(defaultEventId);
    }

    public void Play3D(string id)
    {
        if (!CanExecute())
            return;

        string resolvedId = ResolveEventId(id);
        if (string.IsNullOrWhiteSpace(resolvedId))
            return;

        lastHandle = FmodB8.Event(resolvedId)
            .As3D()
            .Position(transform.position)
            .Play();
    }

    public void Play3D()
    {
        Play3D(defaultEventId);
    }

    public void PlayEvent3D(string id)
    {
        Play3D(id);
    }

    public void PlayAttached(string id)
    {
        if (!CanExecute())
            return;

        string resolvedId = ResolveEventId(id);
        if (string.IsNullOrWhiteSpace(resolvedId))
            return;

        lastHandle = FmodB8.Event(resolvedId)
            .As3D()
            .FollowTransform(ResolveTarget())
            .Play();
    }

    public void PlayAttached()
    {
        PlayAttached(defaultEventId);
    }

    public void PlayEventAttached(string id)
    {
        PlayAttached(id);
    }

    public void PlayAttachedRadius(string id)
    {
        if (!CanExecute())
            return;

        string resolvedId = ResolveEventId(id);
        if (string.IsNullOrWhiteSpace(resolvedId))
            return;

        lastHandle = FmodB8.Event(resolvedId)
            .As3D()
            .FollowTransform(ResolveTarget())
            .Radius(Mathf.Max(0.01f, defaultRadius))
            .Play();
    }

    public void PlayAttachedRadius()
    {
        PlayAttachedRadius(defaultEventId);
    }

    public void PlayFadeIn(string id)
    {
        if (!CanExecute())
            return;

        string resolvedId = ResolveEventId(id);
        if (string.IsNullOrWhiteSpace(resolvedId))
            return;

        lastHandle = FmodB8.Event(resolvedId)
            .FadeIn(Mathf.Max(0f, defaultFadeTime))
            .Play();
    }

    public void PlayFadeIn()
    {
        PlayFadeIn(defaultEventId);
    }

    /// <summary>
    /// Pausa um evento a partir de um Animation Event.
    /// </summary>
    public void Pause(string id)
    {
        if (!CanExecute())
            return;

        string resolvedId = ResolveEventId(id);
        if (!string.IsNullOrWhiteSpace(resolvedId))
            FmodB8.Pause(resolvedId);
    }

    public void Pause()
    {
        if (!CanExecute())
            return;

        if (lastHandle != null && lastHandle.IsValid)
            lastHandle.Pause();
        else
            Pause(defaultEventId);
    }

    /// <summary>
    /// Retoma um evento a partir de um Animation Event.
    /// </summary>
    public void Resume(string id)
    {
        if (!CanExecute())
            return;

        string resolvedId = ResolveEventId(id);
        if (!string.IsNullOrWhiteSpace(resolvedId))
            FmodB8.Resume(resolvedId);
    }

    public void Resume()
    {
        if (!CanExecute())
            return;

        if (lastHandle != null && lastHandle.IsValid)
            lastHandle.Resume();
        else
            Resume(defaultEventId);
    }

    public void TogglePause(string id)
    {
        if (!CanExecute())
            return;

        string resolvedId = ResolveEventId(id);
        if (!string.IsNullOrWhiteSpace(resolvedId))
            FmodB8.TogglePause(resolvedId);
    }

    public void TogglePause()
    {
        if (!CanExecute())
            return;

        if (lastHandle != null && lastHandle.IsValid)
            lastHandle.TogglePause();
        else
            TogglePause(defaultEventId);
    }

    public void Stop()
    {
        if (!CanExecute())
            return;

        if (lastHandle != null && lastHandle.IsValid)
            lastHandle.Stop(defaultFadeTime > 0f, Mathf.Max(0f, defaultFadeTime));
        else
            StopFadeOn(defaultEventId);
    }

    public void Stop(string id)
    {
        StopFadeOn(id);
    }

    public void StopEvent(string id)
    {
        StopFadeOn(id);
    }

    /// <summary>
    /// Para um evento imediatamente a partir de um Animation Event.
    /// </summary>
    public void StopFadeOff(string id)
    {
        if (!CanExecute())
            return;

        string resolvedId = ResolveEventId(id);
        if (!string.IsNullOrWhiteSpace(resolvedId))
            FmodB8.Stop(resolvedId, false);
    }

    public void StopFadeOff()
    {
        StopFadeOff(defaultEventId);
    }

    /// <summary>
    /// Para um evento com fade a partir de um Animation Event.
    /// </summary>
    public void StopFadeOn(string id)
    {
        if (!CanExecute())
            return;

        string resolvedId = ResolveEventId(id);
        if (!string.IsNullOrWhiteSpace(resolvedId))
            FmodB8.Stop(resolvedId, true, Mathf.Max(0f, defaultFadeTime));
    }

    public void StopFadeOn()
    {
        StopFadeOn(defaultEventId);
    }

    public void FadeOut()
    {
        if (!CanExecute())
            return;

        if (lastHandle != null && lastHandle.IsValid)
            lastHandle.FadeOut(Mathf.Max(0f, defaultFadeTime));
        else
            StopFadeOn(defaultEventId);
    }

    public void SetVolume(float value)
    {
        if (!CanExecute())
            return;

        if (lastHandle != null && lastHandle.IsValid)
            lastHandle.Volume(value);
        else if (!string.IsNullOrWhiteSpace(defaultEventId))
            FmodB8.SetVolume(defaultEventId, value);
    }

    public void SetPitch(float value)
    {
        if (!CanExecute())
            return;

        if (lastHandle != null && lastHandle.IsValid)
            lastHandle.Pitch(value);
        else if (!string.IsNullOrWhiteSpace(defaultEventId))
            FmodB8.SetPitch(defaultEventId, value);
    }

    public void SetParameter(float value)
    {
        if (!CanExecute())
            return;

        if (string.IsNullOrWhiteSpace(defaultParameter))
            return;

        if (lastHandle != null && lastHandle.IsValid)
            lastHandle.Parameter(defaultParameter, value);
        else if (!string.IsNullOrWhiteSpace(defaultEventId))
            FmodB8.SetParameter(defaultEventId, defaultParameter, value);
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

    public void SetTimelinePosition(int milliseconds)
    {
        if (!CanExecute())
            return;

        int position = Mathf.Max(0, milliseconds);

        if (lastHandle != null && lastHandle.IsValid)
            lastHandle.SetTimelinePosition(position);
        else if (!string.IsNullOrWhiteSpace(defaultEventId))
            FmodB8.SetTimelinePosition(defaultEventId, position);
    }

    public void Keep(string key)
    {
        if (!CanExecute())
            return;

        if (lastHandle != null && lastHandle.IsValid)
            lastHandle.Keep(key);
    }

    public void Detach()
    {
        if (!CanExecute())
            return;

        if (lastHandle != null && lastHandle.IsValid)
            lastHandle.Detach();
        else if (!string.IsNullOrWhiteSpace(defaultEventId))
            FmodB8.Detach(defaultEventId);
    }

    /// <summary>
    /// Le o estado de um evento a partir de um Animation Event.
    /// </summary>
    public void GetState(string id)
    {
        if (!CanExecute())
            return;

        string resolvedId = ResolveEventId(id);
        if (!string.IsNullOrWhiteSpace(resolvedId))
            FmodB8.GetState(resolvedId);
    }

    public void GetState()
    {
        GetState(defaultEventId);
    }

    public void PlayEmitter(FmodEmitterCustom emitterObj)
    {
        if (!CanExecute())
            return;

        FmodEmitterCustom emitter = ResolveEmitter(emitterObj);
        if (emitter != null)
            emitter.Play();
    }

    public void PlayEmitter()
    {
        PlayEmitter(defaultEmitter);
    }

    public void PlayEmitter(string emitterKey)
    {
        if (!CanExecute())
            return;

        FmodEmitterCustom emitter = ResolveEmitter(emitterKey);
        if (emitter != null)
            emitter.Play();
    }

    public void PlayEmitterByKey(string emitterKey)
    {
        PlayEmitter(emitterKey);
    }

    public void PlayEmitterByReference(UnityEngine.Object emitterReference)
    {
        if (!CanExecute())
            return;

        FmodEmitterCustom emitter = ResolveEmitter(emitterReference);
        if (emitter != null)
            emitter.Play();
    }

    public void StopEmitter(FmodEmitterCustom emitterObj)
    {
        if (!CanExecute())
            return;

        FmodEmitterCustom emitter = ResolveEmitter(emitterObj);
        if (emitter != null)
            emitter.Stop(defaultFadeTime > 0f);
    }

    public void StopEmitter()
    {
        StopEmitter(defaultEmitter);
    }

    public void StopEmitter(string emitterKey)
    {
        if (!CanExecute())
            return;

        FmodEmitterCustom emitter = ResolveEmitter(emitterKey);
        if (emitter != null)
            emitter.Stop(defaultFadeTime > 0f);
    }

    public void StopEmitterByKey(string emitterKey)
    {
        StopEmitter(emitterKey);
    }

    public void StopEmitterByReference(UnityEngine.Object emitterReference)
    {
        if (!CanExecute())
            return;

        FmodEmitterCustom emitter = ResolveEmitter(emitterReference);
        if (emitter != null)
            emitter.Stop(defaultFadeTime > 0f);
    }

    public void PauseEmitter(FmodEmitterCustom emitterObj)
    {
        if (!CanExecute())
            return;

        FmodEmitterCustom emitter = ResolveEmitter(emitterObj);
        if (emitter != null)
            emitter.Pause(true);
    }

    public void PauseEmitter()
    {
        PauseEmitter(defaultEmitter);
    }

    public void PauseEmitter(string emitterKey)
    {
        if (!CanExecute())
            return;

        FmodEmitterCustom emitter = ResolveEmitter(emitterKey);
        if (emitter != null)
            emitter.Pause(true);
    }

    public void PauseEmitterByKey(string emitterKey)
    {
        PauseEmitter(emitterKey);
    }

    public void PauseEmitterByReference(UnityEngine.Object emitterReference)
    {
        if (!CanExecute())
            return;

        FmodEmitterCustom emitter = ResolveEmitter(emitterReference);
        if (emitter != null)
            emitter.Pause(true);
    }

    public void ResumeEmitter(FmodEmitterCustom emitterObj)
    {
        if (!CanExecute())
            return;

        FmodEmitterCustom emitter = ResolveEmitter(emitterObj);
        if (emitter != null)
            emitter.Pause(false);
    }

    public void ResumeEmitter()
    {
        ResumeEmitter(defaultEmitter);
    }

    public void ResumeEmitter(string emitterKey)
    {
        if (!CanExecute())
            return;

        FmodEmitterCustom emitter = ResolveEmitter(emitterKey);
        if (emitter != null)
            emitter.Pause(false);
    }

    public void ResumeEmitterByKey(string emitterKey)
    {
        ResumeEmitter(emitterKey);
    }

    public void ResumeEmitterByReference(UnityEngine.Object emitterReference)
    {
        if (!CanExecute())
            return;

        FmodEmitterCustom emitter = ResolveEmitter(emitterReference);
        if (emitter != null)
            emitter.Pause(false);
    }

    /// <summary>
    /// Ativa um componente de emissor legado.
    /// </summary>
    public void AddEmitter(FmodEmitterCustom emitterObj)
    {
        if (!CanExecute())
            return;

        SetEmitterEnabled(emitterObj, true);
    }

    public void AddEmitter()
    {
        if (!CanExecute())
            return;

        SetEmitterEnabled(defaultEmitter, true);
    }

    public void AddEmitter(string emitterKey)
    {
        if (!CanExecute())
            return;

        SetEmitterEnabled(ResolveEmitter(emitterKey), true);
    }

    public void AddEmitterByKey(string emitterKey)
    {
        AddEmitter(emitterKey);
    }

    public void AddEmitterByReference(UnityEngine.Object emitterReference)
    {
        if (!CanExecute())
            return;

        SetEmitterEnabled(ResolveEmitter(emitterReference), true);
    }

    /// <summary>
    /// Desativa um componente de emissor legado.
    /// </summary>
    public void RemoveEmitter(FmodEmitterCustom emitterObj)
    {
        if (!CanExecute())
            return;

        SetEmitterEnabled(emitterObj, false);
    }

    public void RemoveEmitter()
    {
        if (!CanExecute())
            return;

        SetEmitterEnabled(defaultEmitter, false);
    }

    public void RemoveEmitter(string emitterKey)
    {
        if (!CanExecute())
            return;

        SetEmitterEnabled(ResolveEmitter(emitterKey), false);
    }

    public void RemoveEmitterByKey(string emitterKey)
    {
        RemoveEmitter(emitterKey);
    }

    public void RemoveEmitterByReference(UnityEngine.Object emitterReference)
    {
        if (!CanExecute())
            return;

        SetEmitterEnabled(ResolveEmitter(emitterReference), false);
    }

    private void SetEmitterEnabled(FmodEmitterCustom emitterObj, bool enabled)
    {
        FmodEmitterCustom emitter = ResolveEmitter(emitterObj);
        if (emitter != null)
            emitter.enabled = enabled;
    }

    private FmodEmitterCustom ResolveEmitter(FmodEmitterCustom emitterObj)
    {
        if (emitterObj != null)
            return emitterObj;

        if (defaultEmitter != null)
            return defaultEmitter;

        if (warnWhenMissing)
            Debug.LogWarning("[FMODB8] Emitter not assigned for animation event.", this);

        return null;
    }

    private FmodEmitterCustom ResolveEmitter(string emitterKey)
    {
        if (string.IsNullOrWhiteSpace(emitterKey))
            return ResolveEmitter(defaultEmitter);

        if (FmodB8EmitterKey.TryResolve(emitterKey, out FmodEmitterCustom registeredEmitter))
            return registeredEmitter;

        if (warnWhenMissing)
            Debug.LogWarning("[FMODB8] Emitter key not found for animation event: " + emitterKey, this);

        return null;
    }

    private FmodEmitterCustom ResolveEmitter(UnityEngine.Object emitterReference)
    {
        if (emitterReference == null)
            return ResolveEmitter(defaultEmitter);

        if (emitterReference is FmodEmitterCustom directEmitter)
            return directEmitter;

        if (emitterReference is FmodB8EmitterKey emitterKey)
            return emitterKey.Emitter;

        if (emitterReference is GameObject emitterObject)
        {
            FmodB8EmitterKey keyComponent = emitterObject.GetComponent<FmodB8EmitterKey>();
            if (keyComponent != null)
                return keyComponent.Emitter;

            FmodEmitterCustom emitterComponent = emitterObject.GetComponent<FmodEmitterCustom>();
            if (emitterComponent != null)
                return emitterComponent;
        }

        if (warnWhenMissing)
            Debug.LogWarning("[FMODB8] Object reference is not a FMODB8 emitter key or emitter.", this);

        return null;
    }

    private string ResolveEventId(string id)
    {
        string resolvedId = string.IsNullOrWhiteSpace(id) ? defaultEventId : id;

        if (string.IsNullOrWhiteSpace(resolvedId) && warnWhenMissing)
            Debug.LogWarning("[FMODB8] Animation event has no FMOD event id.", this);

        return resolvedId;
    }

    private Transform ResolveTarget()
    {
        return defaultTarget != null ? defaultTarget : transform;
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
        bool result;

        switch (entry.type)
        {
            case ConditionType.InSceneBuildIndex:
                result = SceneManager.GetActiveScene().buildIndex == entry.intValue;
                break;
            case ConditionType.InSceneName:
                result = SceneManager.GetActiveScene().name == entry.stringValue;
                break;
            case ConditionType.GameObjectActive:
                result = entry.gameObject != null && entry.gameObject.activeInHierarchy;
                break;
            case ConditionType.BehaviourEnabled:
                result = entry.behaviour != null && entry.behaviour.isActiveAndEnabled;
                break;
            default:
                result = true;
                break;
        }

        return entry.invert ? !result : result;
    }

    private void OnValidate()
    {
        defaultRadius = Mathf.Max(0.01f, defaultRadius);
        defaultFadeTime = Mathf.Max(0f, defaultFadeTime);
    }
}
