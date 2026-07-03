#if FMOD_PRESENT
internal sealed class FmodSnapshotManager
{
    private readonly FmodCommands commands;

    internal FmodSnapshotManager(FmodCommands commands)
    {
        this.commands = commands;
    }

    internal FmodHandle StartSnapshot(string path)
    {
        return commands.PlayLoop(path);
    }

    internal void StopSnapshot(string path)
    {
        commands.Stop(path, true, 0.25f);
    }
}
#endif
