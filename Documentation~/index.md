# Manual do BISC8 Simple FMOD

**Documentação da versão 1.3.1** — Unity 6000.3 ou posterior — FMOD 2.03.19

## Visão geral

O BISC8 Simple FMOD centraliza a criação e o gerenciamento de instâncias FMOD. A classe `FmodCommands` mantém as referências de eventos e acompanha cada instância criada. A API `FmodB8` expõe os controles mais usados, enquanto `FmodHandle` permite alterar somente uma reprodução específica.

Use uma das três formas de trabalho:

- **API estática:** comandos rápidos ou aplicados a todas as instâncias ativas de um ID.
- **Builder e handle:** configuração encadeada e controle de uma instância específica.
- **Componentes:** configuração pelo Inspector, Unity Events, colliders, UI e Animation Events.

## Requisitos e compatibilidade

- Unity 6000.3 ou posterior.
- FMOD for Unity 2.03.19, incluído no package.
- Binários nativos incluídos para Windows (x86, x86_64 e ARM64) e Linux (x86_64).
- Netcode for GameObjects 1.0.0 ou posterior somente para a integração multiplayer pronta.

O package instala `FMODUnity` em `Assets/BISC8/FMODB8/FMOD` e o referencia em runtime e no Editor. Não instale uma segunda integração FMOD no mesmo projeto.

## Configuração inicial

### 1. Adicione o sistema

Use **GameObject > FMODB8 > FMODB8 System**. O prefab contém `FmodCommands`, que é preservado entre cenas. Se a API for chamada sem um sistema na cena, um GameObject `FMODB8` será criado automaticamente, mas ele não terá listas atribuídas.

### 2. Configure o FMOD

Configure banks e paths em **FMOD > Edit Settings**, conforme o fluxo normal do FMOD for Unity. Adicione `StudioListener` à câmera ou ao objeto que representa o ouvinte.

A fonte FMOD fica em uma pasta oculta do package para que DLLs carregadas não bloqueiem atualizações do cache. Na primeira importação, o instalador copia automaticamente a integração para `Assets/BISC8/FMODB8/FMOD`. Use **FMOD > FMODB8 > Setup** para reparar a instalação manualmente. Não remova essa pasta enquanto o package usar a fonte oculta.

### 3. Cadastre eventos por ID

1. Use **Assets > FMODB8 > Create Event List**.
2. Defina `Type` como `Sfx`, `Music` ou `Other`.
3. Adicione entradas com um ID legível e um `EventReference`.
4. Arraste uma ou mais listas para `Event Lists` no `FmodCommands`.

O ID é a chave usada no código e nos componentes. IDs repetidos em listas posteriores substituem os anteriores no lookup do sistema. Depois de alterar as listas em runtime, chame `RebuildEventLookup()`.

Paths `event:/...` e `snapshot:/...` podem ser usados diretamente sem cadastro.

Cada entrada também possui `Stage In Project`, usado para acompanhar a implementação:

| Estágio | Cor associada no FMOD Studio |
|---|---|
| `Undone` | Vermelho |
| `InProcess` | Amarelo |
| `Done` | Verde |
| `Implemented` | Azul |

Com o FMOD Studio aberto, conectado e com o mesmo projeto carregado, **Get To FMOD** lê as cores dos eventos para a lista e **Send To FMOD** envia os estágios da lista como cores. Na primeira sincronização, leia ao menos um evento de cada cor usada para que o Editor reconheça o formato de cor exposto pela versão do FMOD Studio.

### 4. Gere IDs fortemente tipados

Use **FMOD > FMODB8 > Generate Events**. O gerador lê todos os assets `CreateFmodList` e atualiza `Assets/BISC8/FMODB8/Generated/FmodEvents.Generated.cs`. Um `BISC8.FMODB8.Generated.asmref` no mesmo diretório mantém a classe gerada dentro do assembly de runtime do FMODB8.

```csharp
FmodB8.Play(FmodEvents.PlayerJump);
```

Nomes inválidos para C# são normalizados. Em caso de colisão, o gerador cria um sufixo numérico. O arquivo pertence ao projeto e pode ser versionado normalmente; o gerador nunca escreve em `Library/PackageCache`.

## Formas de reprodução

### Comando simples

```csharp
FmodB8.Play("Explosion");
FmodB8.PlayLoop("Ambience");
FmodB8.Stop("Ambience", fade: true, fadeTime: 1f);
```

Comandos por ID como `Stop`, `Pause`, `SetVolume` e `SetParameter` afetam todas as instâncias ativas criadas com aquele mesmo ID. Os getters por ID consultam a instância ativa mais recente.

### Builder

O builder reúne a configuração antes de devolver o handle:

```csharp
FmodHandle handle = FmodB8.Event("VehicleEngine")
    .Loop()
    .FollowTransform(transform)
    .Velocity(velocity)
    .Radius(40f)
    .Volume(0.75f)
    .Pitch(1f)
    .Parameter("RPM", rpm)
    .FadeIn(0.25f)
    .Play();
```

`Start()` é alias de `Play()`, `Transform()` é alias de `FollowTransform()` e `As3D()` declara a intenção espacial sem definir um alvo.

### Handle

Guarde o handle quando precisar controlar uma reprodução exata:

```csharp
private FmodHandle ambience;

void Begin()
{
    ambience = FmodB8.PlayLoop("ForestAmbience");
}

void End()
{
    if (ambience != null && ambience.IsValid)
        ambience.Stop(true, 1.5f);
}
```

Os métodos modificadores do handle retornam o próprio handle e aceitam encadeamento. Um handle deixa de ser válido quando a instância para e é liberada.

### Handles nomeados

`Keep` armazena um handle no sistema para recuperá-lo em outro ponto:

```csharp
FmodB8.PlayLoop("MainTheme").Keep("music");
FmodB8.Kept("music").FadeTo(0.3f, 0.5f);
FmodB8.GetKept("music").Stop(true);
```

Uma chave inexistente devolve um handle inválido; valide `IsValid` quando necessário.

## Parâmetros e mixagem

```csharp
handle.Parameter("Speed", speed);
handle.SetParameterLabel("Surface", "Metal");

FmodB8.SetGlobalParameter("GameState", 2f);
float value = FmodB8.GetGlobalParameter("GameState");

FmodB8.SetBusVolume("bus:/Master/Music", 0.7f);
FmodB8.SetBusPaused("bus:/Master/SFX", true);
FmodB8.StopBus("bus:/Master/Ambience");

FmodB8.SetVcaVolume("vca:/SFX", 0.8f);
```

Volumes normalmente usam o intervalo de `0` a `1`, mas o package encaminha o valor informado ao FMOD. Use paths completos para buses e VCAs.

## Snapshots

```csharp
FmodHandle snapshot = FmodB8.StartSnapshot("snapshot:/Underwater");
snapshot.FadeOut(0.5f);

// Alternativa coletiva por path:
FmodB8.StopSnapshot("snapshot:/Underwater");
```

## Áudio 3D

Para atualização contínua, use `FollowTransform`. Para uma fonte fixa, use `Position`. `Velocity` informa a velocidade ao FMOD e `Radius` altera a distância máxima 3D da instância.

```csharp
FmodHandle shot = FmodB8.Event("event:/SFX/Shot")
    .Position(muzzle.position)
    .Radius(60f)
    .Play();
```

O evento também deve estar configurado como 3D no FMOD Studio. Um `StudioListener` é necessário para posicionamento correto.

## Próximos tópicos

- [Componentes e prefabs](components.md)
- [Referência da API](api-reference.md)
- [Multiplayer](multiplayer.md)
- [Solução de problemas](troubleshooting.md)
