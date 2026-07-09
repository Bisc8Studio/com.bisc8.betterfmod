#if FMOD_PRESENT
using UnityEngine;

/// <summary>
/// API publica principal do BetterFMOD para tocar eventos, controlar instancias, parametros, buses, VCAs e snapshots.
/// </summary>
public static class FmodB8
{
    /// <summary>
    /// Cria uma configuracao fluente para um evento antes de tocar.
    /// </summary>
    public static FmodEventBuilder Event(string id)
    {
        return new FmodEventBuilder(id);
    }

    /// <summary>
    /// Toca um evento imediatamente e retorna o controle da instancia criada.
    /// </summary>
    public static FmodHandle Play(string id)
    {
        return FmodCommands.EnsureInstance().Play(id);
    }

    /// <summary>
    /// Toca um evento como loop controlavel e retorna o controle da instancia criada.
    /// </summary>
    public static FmodHandle PlayLoop(string id)
    {
        return FmodCommands.EnsureInstance().PlayLoop(id);
    }


    /// <summary>
    /// Para todas as instancias ativas de um evento.
    /// </summary>
    public static void Stop(string id)
    {
        FmodCommands.EnsureInstance().Stop(id);
    }

    /// <summary>
    /// Para todas as instancias ativas de um evento, com fade opcional.
    /// </summary>
    public static void Stop(string id, bool fade, float fadeTime = 1f)
    {
        FmodCommands.EnsureInstance().Stop(id, fade, fadeTime);
    }

    /// <summary>
    /// Para todas as instancias ativas controladas pelo BetterFMOD.
    /// </summary>
    public static void StopAll()
    {
        FmodCommands.EnsureInstance().StopAll();
    }

    /// <summary>
    /// Para todas as instancias ativas controladas pelo BetterFMOD, com fade opcional.
    /// </summary>
    public static void StopAll(bool fade, float fadeTime = 1f)
    {
        FmodCommands.EnsureInstance().StopAll(fade, fadeTime);
    }

    /// <summary>
    /// Pausa todas as instancias ativas de um evento.
    /// </summary>
    public static void Pause(string id)
    {
        FmodCommands.EnsureInstance().Pause(id, true);
    }

    /// <summary>
    /// Retoma todas as instancias pausadas de um evento.
    /// </summary>
    public static void Resume(string id)
    {
        FmodCommands.EnsureInstance().Resume(id);
    }

    /// <summary>
    /// Alterna entre pausado e tocando em todas as instancias ativas de um evento.
    /// </summary>
    public static void TogglePause(string id)
    {
        FmodCommands.EnsureInstance().TogglePause(id);
    }

    /// <summary>
    /// Define um parametro em todas as instancias ativas de um evento.
    /// </summary>
    public static void SetParameter(string id, string parameter, float value)
    {
        FmodCommands.EnsureInstance().SetParameter(id, parameter, value);
    }

    /// <summary>
    /// Le um parametro da instancia ativa mais recente de um evento.
    /// </summary>
    public static float GetParameter(string id, string parameter)
    {
        return FmodCommands.EnsureInstance().GetParameter(id, parameter);
    }

    /// <summary>
    /// Define um parametro global do FmodB8.
    /// </summary>
    public static void SetGlobalParameter(string parameter, float value)
    {
        FmodCommands.EnsureInstance().SetGlobalParameter(parameter, value);
    }

    /// <summary>
    /// Le um parametro global do FmodB8.
    /// </summary>
    public static float GetGlobalParameter(string parameter)
    {
        return FmodCommands.EnsureInstance().GetGlobalParameter(parameter);
    }

    /// <summary>
    /// Define um parametro por label em todas as instancias ativas de um evento.
    /// </summary>
    public static void SetParameterLabel(string id, string parameter, string label)
    {
        FmodCommands.EnsureInstance().SetParameterLabel(id, parameter, label);
    }

    /// <summary>
    /// Define o volume em todas as instancias ativas de um evento.
    /// </summary>
    public static void SetVolume(string id, float volume)
    {
        FmodCommands.EnsureInstance().SetVolume(id, volume);
    }

    /// <summary>
    /// Le o volume da instancia ativa mais recente de um evento.
    /// </summary>
    public static float GetVolume(string id)
    {
        return FmodCommands.EnsureInstance().GetVolume(id);
    }

    /// <summary>
    /// Aplica fade in em todas as instancias ativas de um evento.
    /// </summary>
    public static void FadeIn(string id, float duration)
    {
        FmodCommands.EnsureInstance().FadeIn(id, duration);
    }

    /// <summary>
    /// Aplica fade out e para todas as instancias ativas de um evento.
    /// </summary>
    public static void FadeOut(string id, float duration)
    {
        FmodCommands.EnsureInstance().FadeOut(id, duration);
    }

    /// <summary>
    /// Altera gradualmente o volume de todas as instancias ativas de um evento.
    /// </summary>
    public static void FadeTo(string id, float volume, float duration)
    {
        FmodCommands.EnsureInstance().FadeTo(id, volume, duration);
    }

    /// <summary>
    /// Define o pitch em todas as instancias ativas de um evento.
    /// </summary>
    public static void SetPitch(string id, float pitch)
    {
        FmodCommands.EnsureInstance().SetPitch(id, pitch);
    }

    /// <summary>
    /// Le o pitch da instancia ativa mais recente de um evento.
    /// </summary>
    public static float GetPitch(string id)
    {
        return FmodCommands.EnsureInstance().GetPitch(id);
    }

    /// <summary>
    /// Retorna verdadeiro se alguma instancia do evento estiver tocando.
    /// </summary>
    public static bool IsPlaying(string id)
    {
        return FmodCommands.EnsureInstance().IsPlaying(id);
    }

    /// <summary>
    /// Retorna verdadeiro se alguma instancia do evento estiver pausada.
    /// </summary>
    public static bool IsPaused(string id)
    {
        return FmodCommands.EnsureInstance().IsPaused(id);
    }

    /// <summary>
    /// Retorna verdadeiro se existir ao menos uma instancia ativa do evento.
    /// </summary>
    public static bool Exists(string id)
    {
        return FmodCommands.EnsureInstance().Exists(id);
    }

    /// <summary>
    /// Le o estado da instancia ativa mais recente de um evento.
    /// </summary>
    public static FmodPlaybackState GetState(string id)
    {
        return FmodCommands.EnsureInstance().GetBetterState(id);
    }

    /// <summary>
    /// Le a posicao da timeline, em milissegundos, da instancia ativa mais recente de um evento.
    /// </summary>
    public static int GetTimelinePosition(string id)
    {
        return FmodCommands.EnsureInstance().GetTimelinePosition(id);
    }

    /// <summary>
    /// Define a posicao da timeline, em milissegundos, em todas as instancias ativas de um evento.
    /// </summary>
    public static void SetTimelinePosition(string id, int milliseconds)
    {
        FmodCommands.EnsureInstance().SetTimelinePosition(id, milliseconds);
    }

    /// <summary>
    /// Faz todas as instancias ativas de um evento seguirem um Transform.
    /// </summary>
    public static void Follow(string id, Transform target)
    {
        FmodCommands.EnsureInstance().Follow(id, target);
    }

    /// <summary>
    /// Desanexa todas as instancias ativas de um evento de seus Transforms.
    /// </summary>
    public static void Detach(string id)
    {
        FmodCommands.EnsureInstance().Detach(id);
    }

    /// <summary>
    /// Define a posicao 3D de todas as instancias ativas de um evento.
    /// </summary>
    public static void SetPosition(string id, Vector3 position)
    {
        FmodCommands.EnsureInstance().SetPosition(id, position);
    }

    /// <summary>
    /// Define a velocidade 3D de todas as instancias ativas de um evento.
    /// </summary>
    public static void SetVelocity(string id, Vector3 velocity)
    {
        FmodCommands.EnsureInstance().SetVelocity(id, velocity);
    }

    /// <summary>
    /// Define o raio maximo 3D de todas as instancias ativas de um evento.
    /// </summary>
    public static void Radius(string id, float radius)
    {
        FmodCommands.EnsureInstance().Set3DRange(id, radius);
    }

    /// <summary>
    /// Define o volume de um bus do FmodB8.
    /// </summary>
    public static void SetBusVolume(string path, float volume)
    {
        FmodCommands.EnsureInstance().BusManager.SetBusVolume(path, volume);
    }

    /// <summary>
    /// Le o volume de um bus do FmodB8.
    /// </summary>
    public static float GetBusVolume(string path)
    {
        return FmodCommands.EnsureInstance().BusManager.GetBusVolume(path);
    }

    /// <summary>
    /// Pausa ou retoma um bus do FmodB8.
    /// </summary>
    public static void SetBusPaused(string path, bool paused)
    {
        FmodCommands.EnsureInstance().BusManager.SetBusPaused(path, paused);
    }

    /// <summary>
    /// Para todos os eventos roteados por um bus do FmodB8.
    /// </summary>
    public static void StopBus(string path)
    {
        FmodCommands.EnsureInstance().BusManager.StopBus(path);
    }

    /// <summary>
    /// Define o volume de um VCA do FmodB8.
    /// </summary>
    public static void SetVcaVolume(string path, float volume)
    {
        FmodCommands.EnsureInstance().BusManager.SetVcaVolume(path, volume);
    }

    /// <summary>
    /// Define o volume de um VCA do FmodB8.
    /// </summary>
    public static void SetVCAVolume(string path, float volume)
    {
        SetVcaVolume(path, volume);
    }

    /// <summary>
    /// Le o volume de um VCA do FmodB8.
    /// </summary>
    public static float GetVcaVolume(string path)
    {
        return FmodCommands.EnsureInstance().BusManager.GetVcaVolume(path);
    }

    /// <summary>
    /// Le o volume de um VCA do FmodB8.
    /// </summary>
    public static float GetVCAVolume(string path)
    {
        return GetVcaVolume(path);
    }

    /// <summary>
    /// Inicia um snapshot do FMOD e retorna o controle da instancia criada.
    /// </summary>
    public static FmodHandle StartSnapshot(string path)
    {
        return FmodCommands.EnsureInstance().SnapshotManager.StartSnapshot(path);
    }

    /// <summary>
    /// Para todas as instancias ativas de um snapshot do FmodB8.
    /// </summary>
    public static void StopSnapshot(string path)
    {
        FmodCommands.EnsureInstance().SnapshotManager.StopSnapshot(path);
    }

    /// <summary>
    /// Recupera um handle de instancia guardado anteriormente com .Keep(key).
    /// Permite encadear operacoes sem precisar armazenar o handle em variavel.
    /// Exemplo: FmodB8.Play("som").Keep("bgm") -> FmodB8.Kept("bgm").Stop(true)
    /// Retorna um handle invalido quando a chave nao existe.
    /// </summary>
    public static FmodHandle Kept(string key)
    {
        return FmodCommands.EnsureInstance().GetKeptHandle(key);
    }

    /// <summary>
    /// Alias de FmodB8.Kept(key).
    /// </summary>
    public static FmodHandle GetKept(string key) => Kept(key);
}
#endif
