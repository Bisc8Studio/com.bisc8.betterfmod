#if FMOD_PRESENT
using UnityEngine;
using FMODUnity;
using System.Collections.Generic;
using System;

/// <summary>
/// Stores BetterFMOD event ids and their FMOD event references.
/// </summary>
public class CreateFmodList : ScriptableObject
{
    public ListType type;
    public string typeName;
    public List<FMODListEntry> events;
}

/// <summary>
/// Represents a BetterFMOD event id mapped to an FMOD event reference.
/// </summary>
[Serializable]
public class FMODListEntry
{
    public string id;
    public EventReference reference;
}

/// <summary>
/// Defines a BetterFMOD event list category.
/// </summary>
public enum ListType
{
    None,
    Sfx,
    Music,
    Other
}
#endif
