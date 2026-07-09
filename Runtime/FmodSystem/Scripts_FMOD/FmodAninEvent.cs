#if BISC8_BETTERFMOD_PRESENT
using UnityEngine;

/// <summary>
/// Ponte legado de Animation Event para comandos BetterFMOD.
/// </summary>
public class FmodAninEvent : MonoBehaviour
{
    /// <summary>
    /// Toca um evento a partir de um Animation Event.
    /// </summary>
    public void PlayOneShot(string id)
    {
        FmodB8.Play(id);
    }

    /// <summary>
    /// Toca um evento de loop a partir de um Animation Event.
    /// </summary>
    public void PlayLoop(string id)
    {
        FmodB8.PlayLoop(id);
    }

    /// <summary>
    /// Retoma um evento a partir de um Animation Event.
    /// </summary>
    public void Pause(string id)
    {
        FmodB8.Resume(id);
    }

    /// <summary>
    /// Para um evento imediatamente a partir de um Animation Event.
    /// </summary>
    public void StopFadeOff(string id)
    {
        FmodB8.Stop(id, false);
    }

    /// <summary>
    /// Para um evento com fade a partir de um Animation Event.
    /// </summary>
    public void StopFadeOn(string id)
    {
        FmodB8.Stop(id, true);
    }

    /// <summary>
    /// Le o estado de um evento a partir de um Animation Event.
    /// </summary>
    public void GetState(string id)
    {
        FmodB8.GetState(id);
    }

    /// <summary>
    /// Ativa um componente de emissor legado.
    /// </summary>
    public void AddEmitter(FmodEmitterCustom emitterObj)
    {
        SetEmitterEnabled(emitterObj, true);
    }

    /// <summary>
    /// Desativa um componente de emissor legado.
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
