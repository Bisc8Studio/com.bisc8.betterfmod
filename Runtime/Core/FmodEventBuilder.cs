using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Configura um evento BetterFMOD antes de criar a instância de áudio.
/// </summary>
public sealed class FmodEventBuilder
{
    private readonly string eventId;
    private readonly List<ParameterValue> parameters = new();
    private readonly List<ParameterLabelValue> parameterLabels = new();
    private bool loop;
    private bool use3D;
    private Transform followTransform;
    private Vector3? position;
    private Vector3? velocity;
    private float? radius;
    private float? volume;
    private float? pitch;
    private float? fadeIn;
    private int? timelinePosition;

    internal FmodEventBuilder(string eventId)
    {
        this.eventId = eventId;
    }

    /// <summary>
    /// Marca o evento para ser iniciado como loop controlável.
    /// </summary>
    public FmodEventBuilder Loop()
    {
        loop = true;
        return this;
    }

    /// <summary>
    /// Marca o evento como áudio 3D para deixar clara a intenção da configuração espacial.
    /// </summary>
    public FmodEventBuilder As3D()
    {
        use3D = true;
        return this;
    }

    /// <summary>
    /// Faz a instância seguir o Transform informado após tocar.
    /// </summary>
    public FmodEventBuilder FollowTransform(Transform target)
    {
        followTransform = target;
        use3D = true;
        return this;
    }

    /// <summary>
    /// Faz a instância seguir o Transform informado após tocar.
    /// </summary>
    public FmodEventBuilder Transform(Transform target)
    {
        return FollowTransform(target);
    }

    /// <summary>
    /// Define a posição 3D inicial da instância.
    /// </summary>
    public FmodEventBuilder Position(Vector3 value)
    {
        position = value;
        use3D = true;
        return this;
    }

    /// <summary>
    /// Define a velocidade 3D inicial da instância.
    /// </summary>
    public FmodEventBuilder Velocity(Vector3 value)
    {
        velocity = value;
        use3D = true;
        return this;
    }

    /// <summary>
    /// Define o raio máximo de audição 3D da instância.
    /// </summary>
    public FmodEventBuilder Radius(float value)
    {
        radius = value;
        use3D = true;
        return this;
    }

    /// <summary>
    /// Define o volume inicial da instância.
    /// </summary>
    public FmodEventBuilder Volume(float value)
    {
        volume = value;
        return this;
    }

    /// <summary>
    /// Define o pitch inicial da instância.
    /// </summary>
    public FmodEventBuilder Pitch(float value)
    {
        pitch = value;
        return this;
    }

    /// <summary>
    /// Define um fade in para ser aplicado quando a instância tocar.
    /// </summary>
    public FmodEventBuilder FadeIn(float seconds)
    {
        fadeIn = seconds;
        return this;
    }

    /// <summary>
    /// Define um parâmetro inicial da instância.
    /// </summary>
    public FmodEventBuilder Parameter(string parameter, float value)
    {
        parameters.Add(new ParameterValue(parameter, value));
        return this;
    }

    /// <summary>
    /// Define um parâmetro por label inicial da instância.
    /// </summary>
    public FmodEventBuilder ParameterLabel(string parameter, string label)
    {
        parameterLabels.Add(new ParameterLabelValue(parameter, label));
        return this;
    }

    /// <summary>
    /// Define a posição inicial da timeline em milissegundos.
    /// </summary>
    public FmodEventBuilder TimelinePosition(int milliseconds)
    {
        timelinePosition = milliseconds;
        return this;
    }

    /// <summary>
    /// Cria, configura e toca a instância do evento.
    /// </summary>
    public FmodHandle Play()
    {
        FmodHandle handle = loop
            ? FmodCommands.EnsureInstance().PlayLoop(eventId)
            : FmodCommands.EnsureInstance().Play(eventId);

        ApplyTo(handle);
        return handle;
    }

    /// <summary>
    /// Cria, configura e toca a instância do evento.
    /// </summary>
    public FmodHandle Start()
    {
        return Play();
    }

    private void ApplyTo(FmodHandle handle)
    {
        if (handle == null || !handle.IsValid)
            return;

        if (followTransform != null)
            handle.FollowTransform(followTransform);
        else if (position.HasValue)
            handle.Position(position.Value);
        else if (use3D)
            handle.As3D();

        if (velocity.HasValue)
            handle.Velocity(velocity.Value);

        if (radius.HasValue)
            handle.Radius(radius.Value);

        if (volume.HasValue)
            handle.Volume(volume.Value);

        if (pitch.HasValue)
            handle.Pitch(pitch.Value);

        if (timelinePosition.HasValue)
            handle.SetTimelinePosition(timelinePosition.Value);

        foreach (ParameterValue parameter in parameters)
            handle.Parameter(parameter.Name, parameter.Value);

        foreach (ParameterLabelValue parameter in parameterLabels)
            handle.SetParameterLabel(parameter.Name, parameter.Label);

        if (fadeIn.HasValue)
            handle.FadeIn(fadeIn.Value);
    }

    private readonly struct ParameterValue
    {
        internal ParameterValue(string name, float value)
        {
            Name = name;
            Value = value;
        }

        internal string Name { get; }
        internal float Value { get; }
    }

    private readonly struct ParameterLabelValue
    {
        internal ParameterLabelValue(string name, string label)
        {
            Name = name;
            Label = label;
        }

        internal string Name { get; }
        internal string Label { get; }
    }
}
