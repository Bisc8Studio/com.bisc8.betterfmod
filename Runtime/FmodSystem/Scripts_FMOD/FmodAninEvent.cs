using UnityEngine;

public class FmodAninEvent : MonoBehaviour
{
    public void PlayOneShot(string id)
    {
        FmodCommands.Instance.PlayOneShot(id);
    }

    public void PlayLoop(string id)
    {
        FmodCommands.Instance.PlayLoop(id);
    }

    public void Pause(string id)
    {
        FmodCommands.Instance.Pause(id, false);
    }

    public void StopFadeOff(string id)
    {
        FmodCommands.Instance.Stop(id, false);
    }

    public void StopFadeOn(string id)
    {
        FmodCommands.Instance.Stop(id, true);
    }

    public void GetState(string id)
    {
        FmodCommands.Instance.GetState(id);
    }

    public void AddEmitter(FmodEmitterCustom emitterObj)
    {
        FmodCommands.Instance.AddEmitter(emitterObj);
    }

    public void RemoveEmitter(FmodEmitterCustom emitterObj)
    {
        FmodCommands.Instance.RemoveEmitter(emitterObj);
    }
}
