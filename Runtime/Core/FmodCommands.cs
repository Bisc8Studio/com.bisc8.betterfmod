#if FMOD_PRESENT
using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central BetterFMOD runtime service that owns event lookup, instance lifetime, and FMOD communication.
/// </summary>
public class FmodCommands : MonoBehaviour
{
    /// <summary>
    /// Gets the active BetterFMOD command service.
    /// </summary>
    public static FmodCommands Instance;

    [SerializeField] private List<CreateFmodList> eventLists = new();

    private readonly Dictionary<string, EventReference> eventReferences = new();
    private readonly Dictionary<int, FmodManagedInstance> instancesByHandle = new();
    private readonly Dictionary<string, HashSet<int>> handlesByEvent = new();
    private readonly HashSet<string> missingEventsLogged = new();
    private readonly FmodParameterManager parameterManager = new();
    private FmodFadeManager fadeManager;
    private FmodBusManager busManager;
    private FmodSnapshotManager snapshotManager;
    private IFmodBackend backend;
    private int nextHandleId = 1;
    private bool initialized;

    internal FmodBusManager BusManager => busManager;

    internal FmodSnapshotManager SnapshotManager => snapshotManager;

    /// <summary>
    /// Returns the active command service, creating one when the scene does not contain a BetterFMOD system object.
    /// </summary>
    public static FmodCommands EnsureInstance()
    {
        if (Instance != null)
        {
            Instance.Initialize();
            return Instance;
        }

        Instance = FindFirstObjectByType<FmodCommands>();

        if (Instance != null)
        {
            Instance.Initialize();
            return Instance;
        }

        GameObject system = new GameObject("BetterFMOD");
        Instance = system.AddComponent<FmodCommands>();
        Instance.Initialize();
        DontDestroyOnLoad(system);
        return Instance;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Initialize();
    }

    private void Initialize()
    {
        if (initialized)
            return;

        backend = FmodBackendProvider.GetBackend();
        fadeManager = new FmodFadeManager(this);
        busManager = new FmodBusManager(backend);
        snapshotManager = new FmodSnapshotManager(this);
        RebuildEventLookup();
        initialized = true;
    }

    private void Update()
    {
        backend = FmodBackendProvider.GetBackend();
        busManager?.SetBackend(backend);
        UpdateFollowTargets();
        CleanupStoppedInstances();
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        StopAll();
        Instance = null;
    }

    /// <summary>
    /// Rebuilds the event lookup table from the configured BetterFMOD event lists.
    /// </summary>
    public void RebuildEventLookup()
    {
        eventReferences.Clear();

        foreach (CreateFmodList list in eventLists)
        {
            if (list == null || list.type == ListType.None || list.events == null)
                continue;

            foreach (FMODListEntry entry in list.events)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.id) || entry.reference.IsNull)
                    continue;

                eventReferences[entry.id] = entry.reference;
            }
        }
    }

    /// <summary>
    /// Gets the FMOD event reference registered for the provided BetterFMOD event id.
    /// </summary>
    public EventReference GetEvent(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return default;

        if (eventReferences.TryGetValue(id, out EventReference reference))
            return reference;

        if (!LooksLikeFmodPath(id))
            LogMissingEvent(id);

        return default;
    }

    /// <summary>
    /// Plays an event and returns a handle that can control the created instance.
    /// </summary>
    public FmodHandle Play(string id)
    {
        return CreateAndStart(id);
    }

    /// <summary>
    /// Plays an event attached to a transform and returns a handle that can control the created instance.
    /// </summary>
    public FmodHandle Play(string id, Transform target)
    {
        return CreateAndStart(id).Follow(target);
    }

    /// <summary>
    /// Plays an event intended to be controlled as a loop and returns a handle for that specific instance.
    /// </summary>
    public FmodHandle PlayLoop(string id)
    {
        return CreateAndStart(id);
    }

    /// <summary>
    /// Plays an event intended to be controlled as a loop attached to a transform and returns a handle for that specific instance.
    /// </summary>
    public FmodHandle PlayLoop(string id, Transform target)
    {
        return CreateAndStart(id).Follow(target);
    }

    /// <summary>
    /// Plays an FMOD one shot event without keeping a public handle.
    /// </summary>
    public void PlayOneShot(string id)
    {
        Play(id);
    }

    /// <summary>
    /// Plays an FMOD one shot event attached to a transform without keeping a public handle.
    /// </summary>
    public void PlayOneShot3D(string id, Transform target, float radius)
    {
        Play(id, target).Radius(radius);
    }

    /// <summary>
    /// Plays a managed looping event.
    /// </summary>
    public void PlayLoop(string id, bool fade = false, float fadeTime = 1f)
    {
        FmodHandle handle = PlayLoop(id);

        if (fade)
            handle.FadeIn(fadeTime);
    }

    /// <summary>
    /// Plays a managed looping event attached to a transform.
    /// </summary>
    public void PlayLoop3D(string id, Transform target, float radius, bool fade = false, float fadeTime = 1f)
    {
        FmodHandle handle = PlayLoop(id, target).Radius(radius);

        if (fade)
            handle.FadeIn(fadeTime);
    }

    /// <summary>
    /// Stops every active instance of an event.
    /// </summary>
    public void Stop(string id, bool fade = false, float fadeTime = 1f)
    {
        if (string.IsNullOrWhiteSpace(id) || !handlesByEvent.TryGetValue(id, out HashSet<int> handles))
            return;

        int[] copy = new int[handles.Count];
        handles.CopyTo(copy);

        foreach (int handleId in copy)
        {
            if (fade)
                Stop(handleId, true, fadeTime);
            else
                Stop(handleId, false, 0f);
        }
    }

    /// <summary>
    /// Stops every active managed FMOD instance.
    /// </summary>
    public void StopAll(bool fade = false, float fadeTime = 1f)
    {
        int[] handles = new int[instancesByHandle.Count];
        instancesByHandle.Keys.CopyTo(handles, 0);

        foreach (int handleId in handles)
            Stop(handleId, fade, fadeTime);
    }

    /// <summary>
    /// Pauses every active instance of an event.
    /// </summary>
    public void Pause(string id, bool pause)
    {
        ForEachInstance(id, managed => managed.Instance.setPaused(pause));
    }

    /// <summary>
    /// Resumes every active instance of an event.
    /// </summary>
    public void Resume(string id)
    {
        Pause(id, false);
    }

    /// <summary>
    /// Toggles pause on every active instance of an event.
    /// </summary>
    public void TogglePause(string id)
    {
        ForEachInstance(id, managed =>
        {
            managed.Instance.getPaused(out bool paused);
            managed.Instance.setPaused(!paused);
        });
    }

    /// <summary>
    /// Sets a parameter on every active instance of an event.
    /// </summary>
    public void SetParameter(string id, string parameter, float value)
    {
        ForEachInstance(id, managed => parameterManager.SetParameter(managed.Instance, parameter, value));
    }

    /// <summary>
    /// Gets a parameter from the newest active instance of an event.
    /// </summary>
    public float GetParameter(string id, string parameter)
    {
        FmodManagedInstance managed = GetNewestInstance(id);
        return managed == null ? 0f : parameterManager.GetParameter(managed.Instance, parameter);
    }

    /// <summary>
    /// Sets a labeled parameter on every active instance of an event.
    /// </summary>
    public void SetParameterLabel(string id, string parameter, string label)
    {
        ForEachInstance(id, managed => parameterManager.SetParameterLabel(managed.Instance, parameter, label));
    }

    /// <summary>
    /// Sets an FMOD global parameter.
    /// </summary>
    public void SetGlobalParameter(string parameter, float value)
    {
        parameterManager.SetGlobalParameter(parameter, value);
    }

    /// <summary>
    /// Gets an FMOD global parameter.
    /// </summary>
    public float GetGlobalParameter(string parameter)
    {
        return parameterManager.GetGlobalParameter(parameter);
    }

    /// <summary>
    /// Sets the volume on every active instance of an event.
    /// </summary>
    public void SetVolume(string id, float volume)
    {
        ForEachInstance(id, managed => managed.Instance.setVolume(volume));
    }

    /// <summary>
    /// Gets the volume from the newest active instance of an event.
    /// </summary>
    public float GetVolume(string id)
    {
        FmodManagedInstance managed = GetNewestInstance(id);
        return managed == null ? 0f : GetVolume(managed.HandleId);
    }

    /// <summary>
    /// Fades every active instance of an event to a volume over time.
    /// </summary>
    public void FadeTo(string id, float volume, float duration)
    {
        ForEachInstance(id, managed => FadeTo(managed.HandleId, volume, duration));
    }

    /// <summary>
    /// Fades in every active instance of an event.
    /// </summary>
    public void FadeIn(string id, float duration)
    {
        ForEachInstance(id, managed => FadeIn(managed.HandleId, duration));
    }

    /// <summary>
    /// Fades out and stops every active instance of an event.
    /// </summary>
    public void FadeOut(string id, float duration)
    {
        Stop(id, true, duration);
    }

    /// <summary>
    /// Sets pitch on every active instance of an event.
    /// </summary>
    public void SetPitch(string id, float pitch)
    {
        ForEachInstance(id, managed => managed.Instance.setPitch(pitch));
    }

    /// <summary>
    /// Gets pitch from the newest active instance of an event.
    /// </summary>
    public float GetPitch(string id)
    {
        FmodManagedInstance managed = GetNewestInstance(id);
        return managed == null ? 0f : GetPitch(managed.HandleId);
    }

    /// <summary>
    /// Returns true when any active instance of an event is playing or sustaining.
    /// </summary>
    public bool IsPlaying(string id)
    {
        bool playing = false;
        ForEachInstance(id, managed => playing |= IsPlaying(managed.HandleId));
        return playing;
    }

    /// <summary>
    /// Returns true when any active instance of an event is paused.
    /// </summary>
    public bool IsPaused(string id)
    {
        bool paused = false;
        ForEachInstance(id, managed => paused |= IsPaused(managed.HandleId));
        return paused;
    }

    /// <summary>
    /// Returns true when at least one active instance exists for an event.
    /// </summary>
    public bool Exists(string id)
    {
        return !string.IsNullOrWhiteSpace(id) && handlesByEvent.TryGetValue(id, out HashSet<int> handles) && handles.Count > 0;
    }

    /// <summary>
    /// Gets the playback state for the newest active instance of an event.
    /// </summary>
    public PLAYBACK_STATE GetState(string id)
    {
        FmodManagedInstance managed = GetNewestInstance(id);
        return managed == null ? PLAYBACK_STATE.STOPPED : GetStudioState(managed.HandleId);
    }

    /// <summary>
    /// Gets the BetterFMOD playback state for the newest active instance of an event.
    /// </summary>
    public FmodPlaybackState GetBetterState(string id)
    {
        return FmodStateUtility.ToBetterState(GetState(id));
    }

    /// <summary>
    /// Gets the timeline position from the newest active instance of an event.
    /// </summary>
    public int GetTimelinePosition(string id)
    {
        FmodManagedInstance managed = GetNewestInstance(id);
        return managed == null ? 0 : GetTimelinePosition(managed.HandleId);
    }

    /// <summary>
    /// Sets the timeline position on every active instance of an event.
    /// </summary>
    public void SetTimelinePosition(string id, int milliseconds)
    {
        ForEachInstance(id, managed => SetTimelinePosition(managed.HandleId, milliseconds));
    }

    /// <summary>
    /// Makes every active instance of an event follow a transform.
    /// </summary>
    public void Follow(string id, Transform target)
    {
        ForEachInstance(id, managed => Follow(managed.HandleId, target));
    }

    /// <summary>
    /// Detaches every active instance of an event from a transform.
    /// </summary>
    public void Detach(string id)
    {
        ForEachInstance(id, managed => Detach(managed.HandleId));
    }

    /// <summary>
    /// Sets the 3D position on every active instance of an event.
    /// </summary>
    public void SetPosition(string id, Vector3 position)
    {
        ForEachInstance(id, managed => SetPosition(managed.HandleId, position));
    }

    /// <summary>
    /// Sets the 3D velocity on every active instance of an event.
    /// </summary>
    public void SetVelocity(string id, Vector3 velocity)
    {
        ForEachInstance(id, managed => SetVelocity(managed.HandleId, velocity));
    }

    /// <summary>
    /// Sets the 3D maximum distance on every active instance of an event.
    /// </summary>
    public void Set3DRange(string id, float radius)
    {
        ForEachInstance(id, managed => Set3DRange(managed.HandleId, radius));
    }

    internal bool TryGetInstance(int handleId, out FmodManagedInstance managed)
    {
        return instancesByHandle.TryGetValue(handleId, out managed) && managed != null && managed.IsValid;
    }

    internal void Stop(int handleId, bool fade, float fadeTime)
    {
        if (!TryGetInstance(handleId, out FmodManagedInstance managed))
            return;

        if (fade && fadeTime > 0f)
        {
            fadeManager.FadeOutAndStop(managed, fadeTime);
            return;
        }

        ReleaseInstance(managed, STOP_MODE.IMMEDIATE);
    }

    internal void Pause(int handleId)
    {
        if (TryGetInstance(handleId, out FmodManagedInstance managed))
            managed.Instance.setPaused(true);
    }

    internal void Resume(int handleId)
    {
        if (TryGetInstance(handleId, out FmodManagedInstance managed))
            managed.Instance.setPaused(false);
    }

    internal void TogglePause(int handleId)
    {
        if (!TryGetInstance(handleId, out FmodManagedInstance managed))
            return;

        managed.Instance.getPaused(out bool paused);
        managed.Instance.setPaused(!paused);
    }

    internal void SetParameter(int handleId, string parameter, float value)
    {
        if (TryGetInstance(handleId, out FmodManagedInstance managed))
            parameterManager.SetParameter(managed.Instance, parameter, value);
    }

    internal float GetParameter(int handleId, string parameter)
    {
        return TryGetInstance(handleId, out FmodManagedInstance managed)
            ? parameterManager.GetParameter(managed.Instance, parameter)
            : 0f;
    }

    internal void SetParameterLabel(int handleId, string parameter, string label)
    {
        if (TryGetInstance(handleId, out FmodManagedInstance managed))
            parameterManager.SetParameterLabel(managed.Instance, parameter, label);
    }

    internal void SetVolume(int handleId, float volume)
    {
        if (TryGetInstance(handleId, out FmodManagedInstance managed))
            managed.Instance.setVolume(volume);
    }

    internal float GetVolume(int handleId)
    {
        if (!TryGetInstance(handleId, out FmodManagedInstance managed))
            return 0f;

        managed.Instance.getVolume(out float volume);
        return volume;
    }

    internal void FadeIn(int handleId, float duration)
    {
        if (!TryGetInstance(handleId, out FmodManagedInstance managed))
            return;

        managed.Instance.setVolume(0f);
        fadeManager.FadeTo(managed, 1f, duration);
    }

    internal void FadeTo(int handleId, float volume, float duration)
    {
        if (TryGetInstance(handleId, out FmodManagedInstance managed))
            fadeManager.FadeTo(managed, volume, duration);
    }

    internal void SetPitch(int handleId, float pitch)
    {
        if (TryGetInstance(handleId, out FmodManagedInstance managed))
            managed.Instance.setPitch(pitch);
    }

    internal float GetPitch(int handleId)
    {
        if (!TryGetInstance(handleId, out FmodManagedInstance managed))
            return 0f;

        managed.Instance.getPitch(out float pitch);
        return pitch;
    }

    internal bool IsPlaying(int handleId)
    {
        PLAYBACK_STATE state = GetStudioState(handleId);
        return state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.SUSTAINING || state == PLAYBACK_STATE.STARTING;
    }

    internal bool IsPaused(int handleId)
    {
        if (!TryGetInstance(handleId, out FmodManagedInstance managed))
            return false;

        managed.Instance.getPaused(out bool paused);
        return paused;
    }

    internal PLAYBACK_STATE GetStudioState(int handleId)
    {
        if (!TryGetInstance(handleId, out FmodManagedInstance managed))
            return PLAYBACK_STATE.STOPPED;

        managed.Instance.getPlaybackState(out PLAYBACK_STATE state);
        return state;
    }

    internal FmodPlaybackState GetBetterState(int handleId)
    {
        return FmodStateUtility.ToBetterState(GetStudioState(handleId));
    }

    internal int GetTimelinePosition(int handleId)
    {
        if (!TryGetInstance(handleId, out FmodManagedInstance managed))
            return 0;

        managed.Instance.getTimelinePosition(out int position);
        return position;
    }

    internal void SetTimelinePosition(int handleId, int milliseconds)
    {
        if (TryGetInstance(handleId, out FmodManagedInstance managed))
            managed.Instance.setTimelinePosition(milliseconds);
    }

    internal void Follow(int handleId, Transform target)
    {
        if (!TryGetInstance(handleId, out FmodManagedInstance managed) || target == null)
            return;

        managed.FollowTarget = target;
        RuntimeManager.AttachInstanceToGameObject(managed.Instance, target);
    }

    internal void Detach(int handleId)
    {
        if (!TryGetInstance(handleId, out FmodManagedInstance managed))
            return;

        managed.FollowTarget = null;
        RuntimeManager.DetachInstanceFromGameObject(managed.Instance);
    }

    internal void SetPosition(int handleId, Vector3 position)
    {
        if (!TryGetInstance(handleId, out FmodManagedInstance managed))
            return;

        managed.FollowTarget = null;
        managed.Position = position;
        Apply3DAttributes(managed);
    }

    internal void SetVelocity(int handleId, Vector3 velocity)
    {
        if (!TryGetInstance(handleId, out FmodManagedInstance managed))
            return;

        managed.Velocity = velocity;
        Apply3DAttributes(managed);
    }

    internal void Set3DRange(int handleId, float radius)
    {
        if (!TryGetInstance(handleId, out FmodManagedInstance managed))
            return;

        float maxDistance = Mathf.Max(0.01f, radius);
        managed.Instance.setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, 0f);
        managed.Instance.setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, maxDistance);
    }

    internal Coroutine StartManagedCoroutine(IEnumerator routine)
    {
        return StartCoroutine(routine);
    }

    internal void StopManagedCoroutine(Coroutine coroutine)
    {
        if (coroutine != null)
            StopCoroutine(coroutine);
    }

    internal void ReleaseInstance(FmodManagedInstance managed, STOP_MODE stopMode)
    {
        if (managed == null || managed.IsReleased)
            return;

        if (managed.FadeCoroutine != null)
            StopCoroutine(managed.FadeCoroutine);

        if (managed.Instance.isValid())
        {
            managed.Instance.stop(stopMode);
            managed.Instance.release();
        }

        managed.IsReleased = true;
        instancesByHandle.Remove(managed.HandleId);

        if (handlesByEvent.TryGetValue(managed.EventId, out HashSet<int> handles))
        {
            handles.Remove(managed.HandleId);

            if (handles.Count == 0)
                handlesByEvent.Remove(managed.EventId);
        }
    }

    private FmodHandle CreateAndStart(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return FmodHandle.Invalid(id);

        if (!TryCreateInstance(id, out EventInstance instance))
            return FmodHandle.Invalid(id);

        int handleId = nextHandleId++;
        FmodManagedInstance managed = new FmodManagedInstance(handleId, id, instance);
        instancesByHandle.Add(handleId, managed);

        if (!handlesByEvent.TryGetValue(id, out HashSet<int> handles))
        {
            handles = new HashSet<int>();
            handlesByEvent.Add(id, handles);
        }

        handles.Add(handleId);
        instance.start();
        return new FmodHandle(this, handleId, id);
    }

    private bool TryCreateInstance(string id, out EventInstance instance)
    {
        instance = default;
        backend = FmodBackendProvider.GetBackend();

        try
        {
            if (eventReferences.TryGetValue(id, out EventReference reference))
            {
                instance = backend.CreateInstance(id, reference);
                return instance.isValid();
            }

            if (LooksLikeFmodPath(id))
            {
                instance = backend.CreateInstance(id);
                return instance.isValid();
            }
        }
        catch (EventNotFoundException)
        {
            LogMissingEvent(id);
            return false;
        }
        catch (System.Exception exception)
        {
            LogFmodWarning("[FMOD] Could not create event '" + id + "': " + exception.Message);
            return false;
        }

        LogMissingEvent(id);
        return false;
    }

    private void ForEachInstance(string id, System.Action<FmodManagedInstance> action)
    {
        if (string.IsNullOrWhiteSpace(id) || action == null || !handlesByEvent.TryGetValue(id, out HashSet<int> handles))
            return;

        int[] copy = new int[handles.Count];
        handles.CopyTo(copy);

        foreach (int handleId in copy)
        {
            if (TryGetInstance(handleId, out FmodManagedInstance managed))
                action(managed);
        }
    }

    private FmodManagedInstance GetNewestInstance(string id)
    {
        if (string.IsNullOrWhiteSpace(id) || !handlesByEvent.TryGetValue(id, out HashSet<int> handles))
            return null;

        FmodManagedInstance newest = null;

        foreach (int handleId in handles)
        {
            if (!TryGetInstance(handleId, out FmodManagedInstance managed))
                continue;

            if (newest == null || managed.HandleId > newest.HandleId)
                newest = managed;
        }

        return newest;
    }

    private void UpdateFollowTargets()
    {
        foreach (FmodManagedInstance managed in instancesByHandle.Values)
        {
            if (managed.FollowTarget != null)
                managed.Instance.set3DAttributes(managed.FollowTarget.To3DAttributes(managed.Velocity));
        }
    }

    private void CleanupStoppedInstances()
    {
        List<FmodManagedInstance> stopped = null;

        foreach (FmodManagedInstance managed in instancesByHandle.Values)
        {
            if (!managed.IsValid)
            {
                stopped ??= new List<FmodManagedInstance>();
                stopped.Add(managed);
                continue;
            }

            managed.Instance.getPlaybackState(out PLAYBACK_STATE state);

            if (state == PLAYBACK_STATE.STOPPED)
            {
                stopped ??= new List<FmodManagedInstance>();
                stopped.Add(managed);
            }
        }

        if (stopped == null)
            return;

        foreach (FmodManagedInstance managed in stopped)
            ReleaseInstance(managed, STOP_MODE.IMMEDIATE);
    }

    private void Apply3DAttributes(FmodManagedInstance managed)
    {
        FMOD.ATTRIBUTES_3D attributes = managed.Position.To3DAttributes();
        attributes.velocity = RuntimeUtils.ToFMODVector(managed.Velocity);
        managed.Instance.set3DAttributes(attributes);
    }

    private bool LooksLikeFmodPath(string id)
    {
        return id.StartsWith("event:/", System.StringComparison.OrdinalIgnoreCase)
            || id.StartsWith("snapshot:/", System.StringComparison.OrdinalIgnoreCase);
    }

    private void LogMissingEvent(string id)
    {
        backend ??= FmodBackendProvider.GetBackend();

        if (!backend.LogWarnings)
            return;

        if (missingEventsLogged.Add(id))
            Debug.LogWarning("[FMOD] Event not found: " + id);
    }

    private void LogFmodWarning(string message)
    {
        backend ??= FmodBackendProvider.GetBackend();

        if (backend.LogWarnings)
            Debug.LogWarning(message);
    }
}
#endif
