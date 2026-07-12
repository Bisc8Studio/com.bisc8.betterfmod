using UnityEngine;
using FMODUnity;
using System.Collections.Generic;
using System;

/// <summary>
/// Armazena ids de eventos BetterFMOD e suas referencias de evento FmodB8.
/// </summary>
public class CreateFmodList : ScriptableObject
{
    public ListType type;
    public string typeName;
    public List<FMODListEntry> events;
}

/// <summary>
/// Representa um id BetterFMOD mapeado para uma referencia de evento FmodB8.
/// </summary>
[Serializable]
public class FMODListEntry
{
    public string id;
    public EventReference reference;
    public StageInProject stageInProject = StageInProject.Undone;
}

/// <summary>
/// Define o estado de implementacao de um evento entre Unity e FMOD Studio.
/// </summary>
public enum StageInProject
{
    Undone,
    InProcess,
    Done,
    Implemented
}

/// <summary>
/// Define uma categoria de lista de eventos BetterFMOD.
/// </summary>
public enum ListType
{
    None,
    Sfx,
    Music,
    Other
}
