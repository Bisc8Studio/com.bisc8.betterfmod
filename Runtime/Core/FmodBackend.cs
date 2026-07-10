using FMOD.Studio;
using FMODUnity;

internal interface IFmodBackend
{
    bool LogWarnings { get; }
    EventInstance CreateInstance(string eventPath);
    EventInstance CreateInstance(string eventId, EventReference eventReference);
}

internal sealed class SinglePlayerFmodBackend : IFmodBackend
{
    internal SinglePlayerFmodBackend(bool logWarnings)
    {
        LogWarnings = logWarnings;
    }

    public bool LogWarnings { get; }

    EventInstance IFmodBackend.CreateInstance(string eventPath)
    {
        return RuntimeManager.CreateInstance(eventPath);
    }

    EventInstance IFmodBackend.CreateInstance(string eventId, EventReference eventReference)
    {
        return RuntimeManager.CreateInstance(eventReference);
    }
}

internal sealed class MultiplayerFmodBackend : IFmodBackend
{
    private readonly SinglePlayerFmodBackend localBackend = new(false);

    public bool LogWarnings => false;

    EventInstance IFmodBackend.CreateInstance(string eventPath)
    {
        return ((IFmodBackend)localBackend).CreateInstance(eventPath);
    }

    EventInstance IFmodBackend.CreateInstance(string eventId, EventReference eventReference)
    {
        return ((IFmodBackend)localBackend).CreateInstance(eventId, eventReference);
    }
}

internal static class FmodBackendProvider
{
    private static readonly IFmodBackend SinglePlayer = new SinglePlayerFmodBackend(true);
    private static readonly IFmodBackend Multiplayer = new MultiplayerFmodBackend();

    internal static IFmodBackend GetBackend()
    {
        return FmodMultiplayerSettings.MultiplayerModeEnabled ? Multiplayer : SinglePlayer;
    }
}
