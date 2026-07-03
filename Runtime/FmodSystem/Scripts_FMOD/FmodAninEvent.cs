#if FMOD_PRESENT
using UnityEngine;

/// <summary>
/// Legacy animation event bridge for BetterFMOD commands.
/// </summary>
public class FmodAninEvent : MonoBehaviour
{
    /// <summary>
    /// Plays a one shot event from an animation event.
    /// </summary>
    public void PlayOneShot(string id)
    {
        Fmod.Play(id);
    }

    /// <summary>
    /// Plays a loop event from an animation event.
    /// </summary>
    public void PlayLoop(string id)
    {
        Fmod.PlayLoop(id);
    }

    /// <summary>
    /// Resumes an event from an animation event.
    /// </summary>
    public void Pause(string id)
    {
        Fmod.Resume(id);
    }

    /// <summary>
    /// Stops an event immediately from an animation event.
    /// </summary>
    public void StopFadeOff(string id)
    {
        Fmod.Stop(id, false);
    }

    /// <summary>
    /// Stops an event with fade from an animation event.
    /// </summary>
    public void StopFadeOn(string id)
    {
        Fmod.Stop(id, true);
    }

    /// <summary>
    /// Gets an event state from an animation event.
    /// </summary>
    public void GetState(string id)
    {
        Fmod.GetState(id);
    }

    /// <summary>
    /// Enables a legacy emitter component.
    /// </summary>
    public void AddEmitter(FmodEmitterCustom emitterObj)
    {
        SetEmitterEnabled(emitterObj, true);
    }

    /// <summary>
    /// Disables a legacy emitter component.
    /// </summary>
    public void RemoveEmitter(FmodEmitterCustom emitterObj)
    {
        SetEmitterEnabled(emitterObj, false);
    }

    private void SetEmitterEnabled(FmodEmitterCustom emitterObj, bool enabled)
    {
        if (emitterObj != null)
        {
            emitterObj.enabled = enabled;
            return;
        }

        GameObject emitter = GameObject.FindGameObjectWithTag("FmodEmitter");

        if (emitter == null)
        {
            Debug.Log("Emitter not found");
            return;
        }

        FmodEmitterCustom component = emitter.GetComponent<FmodEmitterCustom>();

        if (component != null)
            component.enabled = enabled;
    }
}
#endif
