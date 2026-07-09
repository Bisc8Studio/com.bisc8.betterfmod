#if BISC8_BETTERFMOD_PRESENT
using UnityEngine;

/// <summary>
/// Controla uma unica instancia de evento criada pelo BetterFMOD.
/// </summary>
public sealed class FmodHandle
{
    private readonly FmodCommands commands;

    internal FmodHandle(FmodCommands commands, int id, string eventId)
    {
        this.commands = commands;
        Id = id;
        EventId = eventId;
    }

    /// <summary>
    /// Cria um handle invalido para um evento que falhou ou nao foi encontrado.
    /// </summary>
    public static FmodHandle Invalid(string eventId = "")
    {
        return new FmodHandle(null, 0, eventId);
    }

    /// <summary>
    /// Le o id unico desta instancia em runtime.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Le o id BetterFMOD ou o path FMOD usado para criar esta instancia.
    /// </summary>
    public string EventId { get; }

    /// <summary>
    /// Retorna verdadeiro se este handle ainda aponta para uma instancia valida.
    /// </summary>
    public bool IsValid => commands != null && commands.TryGetInstance(Id, out _);

    /// <summary>
    /// Para esta instancia imediatamente.
    /// </summary>
    public FmodHandle Stop()
    {
        commands?.Stop(Id, false, 0f);
        return this;
    }

    /// <summary>
    /// Para esta instancia, com fade opcional.
    /// </summary>
    public FmodHandle Stop(bool fade, float fadeTime = 1f)
    {
        commands?.Stop(Id, fade, fadeTime);
        return this;
    }

    /// <summary>
    /// Pausa esta instancia.
    /// </summary>
    public FmodHandle Pause()
    {
        commands?.Pause(Id);
        return this;
    }

    /// <summary>
    /// Retoma esta instancia pausada.
    /// </summary>
    public FmodHandle Resume()
    {
        commands?.Resume(Id);
        return this;
    }

    /// <summary>
    /// Alterna esta instancia entre pausada e tocando.
    /// </summary>
    public FmodHandle TogglePause()
    {
        commands?.TogglePause(Id);
        return this;
    }

    /// <summary>
    /// Define um parametro nesta instancia.
    /// </summary>
    public FmodHandle Parameter(string parameter, float value)
    {
        commands?.SetParameter(Id, parameter, value);
        return this;
    }

    /// <summary>
    /// Define um parametro nesta instancia.
    /// </summary>
    public FmodHandle SetParameter(string parameter, float value)
    {
        return Parameter(parameter, value);
    }

    /// <summary>
    /// Le um parametro desta instancia.
    /// </summary>
    public float GetParameter(string parameter)
    {
        return commands == null ? 0f : commands.GetParameter(Id, parameter);
    }

    /// <summary>
    /// Define um parametro por label nesta instancia.
    /// </summary>
    public FmodHandle SetParameterLabel(string parameter, string label)
    {
        commands?.SetParameterLabel(Id, parameter, label);
        return this;
    }

    /// <summary>
    /// Define o volume desta instancia.
    /// </summary>
    public FmodHandle Volume(float volume)
    {
        commands?.SetVolume(Id, volume);
        return this;
    }

    /// <summary>
    /// Define o volume desta instancia.
    /// </summary>
    public FmodHandle SetVolume(float volume)
    {
        return Volume(volume);
    }

    /// <summary>
    /// Le o volume desta instancia.
    /// </summary>
    public float GetVolume()
    {
        return commands == null ? 0f : commands.GetVolume(Id);
    }

    /// <summary>
    /// Aplica fade in nesta instancia a partir do silencio.
    /// </summary>
    public FmodHandle FadeIn(float duration)
    {
        commands?.FadeIn(Id, duration);
        return this;
    }

    /// <summary>
    /// Aplica fade out e para esta instancia.
    /// </summary>
    public FmodHandle FadeOut(float duration)
    {
        commands?.Stop(Id, true, duration);
        return this;
    }

    /// <summary>
    /// Altera gradualmente esta instancia ate o volume informado.
    /// </summary>
    public FmodHandle FadeTo(float volume, float duration)
    {
        commands?.FadeTo(Id, volume, duration);
        return this;
    }

    /// <summary>
    /// Define o pitch desta instancia.
    /// </summary>
    public FmodHandle Pitch(float pitch)
    {
        commands?.SetPitch(Id, pitch);
        return this;
    }

    /// <summary>
    /// Define o pitch desta instancia.
    /// </summary>
    public FmodHandle SetPitch(float pitch)
    {
        return Pitch(pitch);
    }

    /// <summary>
    /// Le o pitch desta instancia.
    /// </summary>
    public float GetPitch()
    {
        return commands == null ? 0f : commands.GetPitch(Id);
    }

    /// <summary>
    /// Retorna verdadeiro se esta instancia estiver tocando.
    /// </summary>
    public bool IsPlaying()
    {
        return commands != null && commands.IsPlaying(Id);
    }

    /// <summary>
    /// Retorna verdadeiro se esta instancia estiver pausada.
    /// </summary>
    public bool IsPaused()
    {
        return commands != null && commands.IsPaused(Id);
    }

    /// <summary>
    /// Le o estado de playback desta instancia.
    /// </summary>
    public FmodPlaybackState GetState()
    {
        return commands == null ? FmodPlaybackState.Stopped : commands.GetBetterState(Id);
    }

    /// <summary>
    /// Le a posicao da timeline desta instancia em milissegundos.
    /// </summary>
    public int GetTimelinePosition()
    {
        return commands == null ? 0 : commands.GetTimelinePosition(Id);
    }

    /// <summary>
    /// Define a posicao da timeline desta instancia em milissegundos.
    /// </summary>
    public FmodHandle SetTimelinePosition(int milliseconds)
    {
        commands?.SetTimelinePosition(Id, milliseconds);
        return this;
    }

    /// <summary>
    /// Faz esta instancia seguir um Transform.
    /// </summary>
    public FmodHandle Follow(Transform target)
    {
        commands?.Follow(Id, target);
        return this;
    }

    /// <summary>
    /// Marca esta instância como áudio 3D para deixar clara a intenção no encadeamento.
    /// </summary>
    public FmodHandle As3D()
    {
        return this;
    }

    /// <summary>
    /// Faz esta instância seguir o Transform informado.
    /// </summary>
    public FmodHandle FollowTransform(Transform target)
    {
        return Follow(target);
    }

    /// <summary>
    /// Anexa esta instância ao Transform informado.
    /// </summary>
    public FmodHandle AttachTo(Transform target)
    {
        return Follow(target);
    }

    /// <summary>
    /// Faz esta instância seguir o Transform informado.
    /// </summary>
    public FmodHandle Transform(Transform target)
    {
        return Follow(target);
    }

    /// <summary>
    /// Desanexa esta instancia do Transform que ela estava seguindo.
    /// </summary>
    public FmodHandle Detach()
    {
        commands?.Detach(Id);
        return this;
    }

    /// <summary>
    /// Define a posicao 3D desta instancia no mundo.
    /// </summary>
    public FmodHandle SetPosition(Vector3 position)
    {
        commands?.SetPosition(Id, position);
        return this;
    }

    /// <summary>
    /// Define a posicao 3D desta instancia no mundo.
    /// </summary>
    public FmodHandle Position(Vector3 position)
    {
        return SetPosition(position);
    }

    /// <summary>
    /// Define a velocidade 3D desta instancia.
    /// </summary>
    public FmodHandle SetVelocity(Vector3 velocity)
    {
        commands?.SetVelocity(Id, velocity);
        return this;
    }

    /// <summary>
    /// Define a velocidade 3D desta instancia.
    /// </summary>
    public FmodHandle Velocity(Vector3 velocity)
    {
        return SetVelocity(velocity);
    }

    /// <summary>
    /// Define o raio maximo 3D desta instancia.
    /// </summary>
    public FmodHandle Radius(float radius)
    {
        commands?.Set3DRange(Id, radius);
        return this;
    }

    /// <summary>
    /// Guarda esta instancia no registro global do BetterFMOD.
    /// Se nenhuma chave for informada, usa o EventId como chave.
    /// Use FmodB8.GetKept(key) para recuperar a instancia depois.
    /// </summary>
    public FmodHandle Keep(string key = null)
    {
        string resolvedKey = string.IsNullOrWhiteSpace(key) ? EventId : key;
        commands?.KeepHandle(this, resolvedKey);
        return this;
    }
}
#endif
