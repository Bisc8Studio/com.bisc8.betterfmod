#if FMOD_PRESENT

using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FmodCommands : MonoBehaviour
{
    public static FmodCommands Instance;

    [SerializeField] private List<CreateFmodList> eventLists;

    private Dictionary<string, EventReference> eventDict = new Dictionary<string, EventReference>();

    private Dictionary<string, EventInstance> instances = new Dictionary<string, EventInstance>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        foreach (var list in eventLists)
        {
            if (list.type == ListType.None)
                continue;

            foreach (var entry in list.events)
            {
                if (eventDict.ContainsKey(entry.id))
                {
                    Debug.LogWarning("ID duplicado: " + entry.id);
                    continue;
                }

                eventDict.Add(entry.id, entry.reference);
            }
        }
    }

    public bool HasEvent(string id)
    {
        return eventDict.ContainsKey(id);
    }

    public EventReference GetEvent(string id)
    {
        if (HasEvent(id))
            return eventDict[id];

        Debug.LogError("Evento n�o encontrado: " + id);

        return default;
    }

    public void PlayOneShot(string id)
    {
        var reference = GetEvent(id);

        if (reference.IsNull)
            return;

        RuntimeManager.PlayOneShot(reference);
    }

    public void PlayLoop(string id, bool fade = false, float fadeTime = 1f)
    {
        if (instances.ContainsKey(id))
            return;

        var reference = GetEvent(id);

        if (reference.IsNull)
            return;

        EventInstance instance =
            RuntimeManager.CreateInstance(reference);

        if (fade)
        {
            instance.setVolume(0);
        }

        instance.start();

        instances[id] = instance;

        if (fade)
        {
            StartCoroutine(FadeIn(instance, fadeTime));
        }
    }

    IEnumerator FadeIn(EventInstance instance, float fadeTime)
    {
        float timer = 0;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;

            float volume =
                Mathf.Lerp(0, 1, timer / fadeTime);

            instance.setVolume(volume);

            yield return null;
        }

        instance.setVolume(1);
    }

    public void Pause(string id, bool pause)
    {
        if (instances.TryGetValue(id, out var instance))
        {
            instance.setPaused(pause);
        }
    }

    public void Stop(string id, bool fade = false, float fadeTime = 1f)
    {
        if (instances.TryGetValue(id, out var instance))
        {
            if (fade)
            {
                StartCoroutine(
                    FadeOutAndStop(id, instance, fadeTime)
                );
            }
            else
            {
                instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);

                instance.release();

                instances.Remove(id);
            }
        }
    }

    IEnumerator FadeOutAndStop(
        string id,
        EventInstance instance,
        float fadeTime)
    {
        instance.getVolume(out float startVolume);

        float timer = 0;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;

            float volume =
                Mathf.Lerp(startVolume, 0, timer / fadeTime);

            instance.setVolume(volume);

            yield return null;
        }

        instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);

        instance.release();

        instances.Remove(id);
    }

    public PLAYBACK_STATE GetState(string id)
    {
        if (instances.TryGetValue(id, out var instance))
        {
            instance.getPlaybackState(out PLAYBACK_STATE state);

            return state;
        }

        return PLAYBACK_STATE.STOPPED;
    }

    public void AddEmitter(FmodEmitterCustom emitterObj)
    {
        emitterObj.enabled = true;
    }

    public void RemoveEmitter(FmodEmitterCustom emitterObj)
    {
        emitterObj.enabled = false;
    }
}

#endif