#if FMOD_PRESENT
using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FmodCommands : MonoBehaviour
{
    public static FmodCommands Instance;

    [SerializeField] private List<CreateFmodList> eventLists = new List<CreateFmodList>();

    private Dictionary<string, EventReference> eventDict =
        new Dictionary<string, EventReference>();

    private Dictionary<string, EventInstance> instances =
        new Dictionary<string, EventInstance>();

    private HashSet<string> missingEventsLogged =
        new HashSet<string>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildEventDictionary();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void BuildEventDictionary()
    {
        eventDict.Clear();

        if (eventLists == null)
            return;

        foreach (var list in eventLists)
        {
            if (list == null)
                continue;

            if (list.type == ListType.None)
                continue;

            if (list.events == null)
                continue;

            foreach (var entry in list.events)
            {
                if (entry == null || string.IsNullOrEmpty(entry.id))
                    continue;

                if (!eventDict.ContainsKey(entry.id))
                    eventDict.Add(entry.id, entry.reference);
            }
        }
    }


    public EventReference GetEvent(string id)
    {
        if (eventDict.TryGetValue(id, out var e))
            return e;

        if (!FmodMultiplayerSettings.MultiplayerModeEnabled)
            Debug.LogError("Event not found: " + id);

        return default;
    }


    public void PlayOneShot(string id)
    {
        var reference = GetEvent(id);
        if (reference.IsNull) return;

        if (!TryCreateInstance(id, reference, out EventInstance instance))
            return;

        instance.start();
        instance.release();
    }

    public void PlayOneShot3D(string id, Transform target, float radius)
    {
        var reference = GetEvent(id);
        if (reference.IsNull) return;

        if (!TryCreateInstance(id, reference, out EventInstance instance))
            return;

        RuntimeManager.AttachInstanceToGameObject(instance, target.gameObject);
        Apply3DRange(instance, radius);

        instance.start();
        instance.release();
    }


    public void PlayLoop(string id, bool fade = false, float fadeTime = 1f)
    {
        if (instances.ContainsKey(id)) return;

        var reference = GetEvent(id);
        if (reference.IsNull) return;

        if (!TryCreateInstance(id, reference, out EventInstance instance))
            return;

        if (fade)
            instance.setVolume(0);

        instance.start();

        instances[id] = instance;

        if (fade)
            StartCoroutine(FadeIn(instance, fadeTime));
    }


    public void PlayLoop3D(string id, Transform target, float radius, bool fade = false, float fadeTime = 1f)
    {
        if (instances.ContainsKey(id)) return;

        var reference = GetEvent(id);
        if (reference.IsNull) return;

        if (!TryCreateInstance(id, reference, out EventInstance instance))
            return;

        RuntimeManager.AttachInstanceToGameObject(instance, target.gameObject);
        Apply3DRange(instance, radius);

        if (fade)
            instance.setVolume(0);

        instance.start();

        instances[id] = instance;

        if (fade)
            StartCoroutine(FadeIn(instance, fadeTime));
    }

    public void Set3DRange(string id, float radius)
    {
        if (!instances.TryGetValue(id, out var instance))
            return;

        Apply3DRange(instance, radius);
    }

    private void Apply3DRange(EventInstance instance, float radius)
    {
        float maxDistance = Mathf.Max(0.01f, radius);

        instance.setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, 0f);
        instance.setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, maxDistance);
    }

    private bool TryCreateInstance(string id, EventReference reference, out EventInstance instance)
    {
        instance = default;

        try
        {
            instance = RuntimeManager.CreateInstance(reference);
            return true;
        }
        catch (EventNotFoundException)
        {
            LogMissingEvent(id);
            return false;
        }
    }

    private void LogMissingEvent(string id)
    {
        if (FmodMultiplayerSettings.MultiplayerModeEnabled)
            return;

        if (missingEventsLogged.Add(id))
            Debug.LogWarning("[FMOD] Event not found: " + id);
    }


    public void Stop(string id, bool fade = false, float fadeTime = 1f)
    {
        if (!instances.TryGetValue(id, out var instance))
            return;

        if (fade)
        {
            StartCoroutine(FadeOutAndStop(id, instance, fadeTime));
            return;
        }

        instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        instance.release();
        instances.Remove(id);
    }

    IEnumerator FadeOutAndStop(string id, EventInstance instance, float fadeTime)
    {
        instance.getVolume(out float start);

        float t = 0;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            instance.setVolume(Mathf.Lerp(start, 0, t / fadeTime));
            yield return null;
        }

        instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        instance.release();
        instances.Remove(id);
    }

    IEnumerator FadeIn(EventInstance instance, float fadeTime)
    {
        float t = 0;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            instance.setVolume(Mathf.Lerp(0, 1, t / fadeTime));
            yield return null;
        }

        instance.setVolume(1);
    }


    public PLAYBACK_STATE GetState(string id)
    {
        if (instances.TryGetValue(id, out var instance))
        {
            instance.getPlaybackState(out var state);
            return state;
        }

        return PLAYBACK_STATE.STOPPED;
    }


    public void Pause(string id, bool pause)
    {
        if (instances.TryGetValue(id, out var instance))
            instance.setPaused(pause);
    }

    public void TogglePause(string id)
    {
        if (instances.TryGetValue(id, out var instance))
        {
            instance.getPaused(out bool paused);
            instance.setPaused(!paused);
        }
    }
}
#endif
