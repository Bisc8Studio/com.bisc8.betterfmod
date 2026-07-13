# Referência da API

## FmodB8

API estática principal. Métodos que recebem um ID operam sobre todas as instâncias ativas com aquele ID, exceto getters, que consultam a instância mais recente.

### Criação e transporte

| Método | Retorno | Descrição |
|---|---|---|
| `Event(string id)` | `FmodEventBuilder` | Inicia uma configuração fluente. |
| `Play(string id)` | `FmodHandle` | Cria e toca uma instância. |
| `PlayLoop(string id)` | `FmodHandle` | Cria uma instância controlável; o loop real depende da configuração do evento no FMOD Studio. |
| `Stop(id[, fade, fadeTime])` | `void` | Para todas as instâncias do ID. |
| `StopAll([fade, fadeTime])` | `void` | Para todas as instâncias gerenciadas. |
| `Pause`, `Resume`, `TogglePause` | `void` | Altera o estado das instâncias do ID. |

### Instância

| Grupo | Métodos |
|---|---|
| Parâmetros | `SetParameter`, `GetParameter`, `SetParameterLabel`, `SetGlobalParameter`, `GetGlobalParameter` |
| Ganho | `SetVolume`, `GetVolume`, `FadeIn`, `FadeOut`, `FadeTo` |
| Pitch | `SetPitch`, `GetPitch` |
| Estado | `IsPlaying`, `IsPaused`, `Exists`, `GetState` |
| Timeline | `GetTimelinePosition`, `SetTimelinePosition` |
| 3D | `Follow`, `Detach`, `SetPosition`, `SetVelocity`, `Radius` |

### Mixagem e snapshots

| Método | Descrição |
|---|---|
| `SetBusVolume`, `GetBusVolume` | Define ou lê volume de bus. |
| `SetBusPaused` | Pausa/retoma um bus. |
| `StopBus` | Para eventos roteados pelo bus. |
| `SetVcaVolume`, `GetVcaVolume` | Define ou lê volume de VCA. As variantes `VCA` em maiúsculas são aliases. |
| `StartSnapshot`, `StopSnapshot` | Inicia ou para snapshot por path. |
| `Kept`, `GetKept` | Recupera handle armazenado por `Keep`. |

## FmodEventBuilder

| Método | Descrição |
|---|---|
| `Loop()` | Solicita reprodução controlável; não altera a propriedade de loop criada no FMOD Studio. |
| `As3D()` | Marca a configuração como espacial. |
| `FollowTransform(target)` / `Transform(target)` | Atualiza atributos 3D seguindo o alvo. |
| `Position(value)` | Define posição 3D fixa. |
| `Velocity(value)` | Define velocidade 3D. |
| `Radius(value)` | Define distância máxima 3D. |
| `Volume(value)` / `Pitch(value)` | Define valores iniciais. |
| `FadeIn(seconds)` | Aplica fade após iniciar. |
| `Parameter(name, value)` | Define parâmetro numérico inicial. |
| `ParameterLabel(name, label)` | Define parâmetro por label. |
| `TimelinePosition(milliseconds)` | Define posição inicial da timeline. |
| `Play()` / `Start()` | Cria a instância, aplica a configuração e retorna o handle. |

## FmodHandle

Propriedades:

- `Id`: identificador interno único da instância.
- `EventId`: ID ou path usado na criação.
- `IsValid`: indica se a instância ainda existe.

Todos os modificadores retornam o próprio handle: `Stop`, `Pause`, `Resume`, `TogglePause`, `Parameter`, `SetParameter`, `SetParameterLabel`, `Volume`, `SetVolume`, `FadeIn`, `FadeOut`, `FadeTo`, `Pitch`, `SetPitch`, `SetTimelinePosition`, `Follow`, `As3D`, `FollowTransform`, `AttachTo`, `Transform`, `Detach`, `SetPosition`, `Position`, `SetVelocity`, `Velocity`, `Radius` e `Keep`.

Getters: `GetParameter`, `GetVolume`, `GetPitch`, `IsPlaying`, `IsPaused`, `GetState` e `GetTimelinePosition`.

`FmodHandle.Invalid(id)` cria um objeto seguro para representar falha. Métodos em um handle inválido não lançam exceção e getters retornam os valores padrão definidos pelo serviço.

## FmodPlaybackState

Estados possíveis: `Stopped`, `Starting`, `Playing`, `Sustaining` e `Stopping`. Falhas e handles inválidos são representados como `Stopped` nas consultas de estado.

## FmodCommands

Normalmente este serviço é usado pelo prefab, não diretamente. APIs públicas úteis para integração:

- `EnsureInstance()` obtém ou cria o singleton persistente.
- `RebuildEventLookup()` relê as listas atribuídas.
- `GetEvent(id)` lê um `EventReference` cadastrado.
- `MultiplayerTransport` aceita uma implementação de `IFmodMultiplayerTransport`.
- `MultiplayerButtonActionRequested` permite conectar um transporte customizado.
- `ReceiveMultiplayerButtonAction(payload)` reproduz localmente um payload recebido.
