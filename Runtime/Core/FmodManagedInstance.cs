#if FMOD_PRESENT
using FMOD.Studio;
using UnityEngine;

internal sealed class FmodManagedInstance
{
    internal FmodManagedInstance(int handleId, string eventId, EventInstance instance)
    {
        HandleId = handleId;
        EventId = eventId;
        Instance = instance;
    }

    internal int HandleId { get; }
    internal string EventId { get; }
    internal EventInstance Instance { get; }
    internal Transform FollowTarget { get; set; }
    internal Vector3 Position { get; set; }
    internal Vector3 Velocity { get; set; }
    internal Coroutine FadeCoroutine { get; set; }
    internal bool IsReleased { get; set; }
    internal bool IsValid => !IsReleased && Instance.isValid();
}
#endif
