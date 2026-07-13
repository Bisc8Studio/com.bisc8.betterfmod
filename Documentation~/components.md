# Componentes e prefabs do FMODB8

Esta página documenta os componentes criados pelo BISC8 Simple FMOD. Eles aparecem em **Add Component > FMODB8** e são o fluxo principal do package para implementar áudio sem escrever um script específico para cada objeto.

## Escolha rápida

| Quero... | Use |
|---|---|
| Tocar um evento simples em um objeto | `FmodEmitter` |
| Montar um emissor com volume, pitch, parâmetros, 3D e fades ordenados | `FmodEmitterCustom` |
| Tocar por clique ou hover | `FmodButton` |
| Tocar por trigger, colisão ou ciclo de vida | `FmodTrigger` |
| Alterar o ambiente sonoro dentro de uma região | `FmodArea` |
| Alimentar um parâmetro com um valor de gameplay | `FmodParameter` |
| Criar controles de volume | `FmodSlider` |
| Disparar som por Animation Event | `FmodAninEvent` |

## FmodEmitter

`FmodEmitter` é o emissor simples recomendado para a maioria dos GameObjects. Adicione por **Add Component > FMODB8 > FMODB8 Emitter**.

### Campos

| Campo | Função |
|---|---|
| `Event Id` | ID cadastrado em uma `CreateFmodList` ou path `event:/...`. |
| `Loop` | Solicita uma reprodução controlável. O evento precisa ter comportamento de loop configurado no FMOD Studio. |
| `One Shot` | Quando desativado, o componente também trata o evento como reprodução controlável. |
| `Play On Awake` | Chama `Play()` durante `Awake`. |
| `Stop On Disable` | Para o handle quando o componente ou GameObject é desativado. |
| `Stop On Destroy` | Para o handle quando o GameObject é destruído. |
| `Follow Target` | Transform seguido pela instância 3D. Vazio usa o próprio Transform. |
| `Radius` | Distância máxima 3D aplicada à instância. Valores menores ou iguais a zero não alteram o raio. |
| `Fade In` | Duração da entrada gradual. Zero desativa. |
| `Fade Out` | Duração usada por `Stop()`. Zero para imediatamente. |

### Comportamento

`Play()` cria uma instância, armazena seu `FmodHandle`, faz o evento seguir `Follow Target` ou o próprio objeto e aplica raio/fade. Chamadas posteriores substituem o handle guardado pelo componente; para múltiplas instâncias simultâneas, guarde os handles pela API.

Métodos públicos para Unity Events ou scripts:

```csharp
FmodHandle Play();
void Stop();
void Pause();
void Resume();
void SetParameter(string parameter, float value);
```

### Exemplo: motor de veículo

1. Adicione `FmodEmitter` ao veículo.
2. Configure `Event Id = VehicleEngine`.
3. Desative `One Shot` e ative `Stop On Destroy`.
4. Deixe `Follow Target` vazio para seguir o veículo.
5. Defina o raio desejado.
6. Atualize o parâmetro do motor por outro script:

```csharp
[SerializeField] private FmodEmitter engineEmitter;

void UpdateEngine(float rpm)
{
    engineEmitter.SetParameter("RPM", rpm);
}
```

## FmodEmitterCustom

`FmodEmitterCustom` é o emissor avançado baseado em cascata. No menu atual aparece como **Add Component > FMODB8 > FMODB8 Emmiter**; a grafia `Emmiter` é preservada por compatibilidade. O nome da classe é `FmodEmitterCustom`.

### Modos do Inspector

| Modo | Conteúdo |
|---|---|
| `None` | Oculta a configuração e não oferece campos de reprodução no Inspector. |
| `Basic` | Mostra evento, one-shot e gatilhos de play/stop. |
| `Advanced` | Inclui todos os campos básicos e uma cascata ordenada de modificadores. |

### Configuração básica

| Campo | Função |
|---|---|
| `Event Id` | ID FMODB8 ou path de evento. |
| `One Shot` | Ativado toca como one-shot; desativado usa reprodução controlável. |
| `Play Event` | `None`, `OnEnable`, `OnStart` ou `OnMouseEnter`. |
| `Stop Event` | `None`, `OnDisable` ou `OnDestroy`. |

Quando `Play Event = OnEnable`, o componente espera um frame antes de tocar. Isso permite que os outros objetos terminem sua inicialização.

`OnMouseEnter` exige um Collider no objeto e uma câmera capaz de detectá-lo.

### Cascata avançada

Depois de criar o evento, o componente executa cada passo da cascata na ordem mostrada. Use **Up**, **Down** e **-** para ordenar ou remover passos.

| Passo | Valor usado | Resultado |
|---|---|---|
| `As3D` | — | Marca a instância para configuração espacial. |
| `Attach` | Transform | Faz o áudio seguir o Transform; vazio usa o próprio objeto. |
| `Position` | Vector3 | Define uma posição 3D fixa. |
| `Velocity` | Vector3 | Define a velocidade espacial usada pelo FMOD. |
| `Radius` | Float | Define o raio máximo; o mínimo aceito é `0.01`. |
| `Volume` | Float | Define o volume da instância. |
| `Pitch` | Float | Define o pitch da instância. |
| `FadeIn` | Duração | Faz entrada gradual. |
| `Parameter` | Nome + valor | Define um parâmetro numérico. |
| `ParameterLabel` | Nome + label | Define um parâmetro por label. |
| `TimelinePosition` | Milissegundos | Move a timeline antes/depois dos demais passos conforme a ordem. |
| `FadeOut` | Duração | Faz fade e encerra a instância. |
| `FadeTo` | Volume + duração | Move gradualmente para outro volume. |
| `Stop` | Fade + tempo | Para a instância. |
| `Pause` | — | Pausa. |
| `Resume` | — | Retoma. |
| `TogglePause` | — | Alterna pausa. |
| `Detach` | — | Para de seguir o Transform. |
| `Keep` | Chave opcional | Guarda o handle para recuperar com `FmodB8.Kept(key)`. |

Ao detectar `Radius`, `Attach`, `Position` ou `Velocity`, a validação acrescenta automaticamente `As3D` se necessário. Se não existir posição ou attach, também acrescenta `Attach` usando o próprio objeto. No modo avançado, passos `Radius` desenham um gizmo com a cor escolhida.

### Exemplo de cascata 3D

Para uma máquina que liga ao habilitar:

1. `Mode = Advanced`.
2. `Event Id = FactoryMachine`.
3. `One Shot = false`.
4. `Play Event = OnEnable` e `Stop Event = OnDisable`.
5. Adicione `Attach` apontando para a máquina.
6. Adicione `Radius = 25`.
7. Adicione `Volume = 0.8`.
8. Adicione `FadeIn = 0.5`.
9. Adicione `Parameter`, nome `MachineType`, valor desejado.
10. Adicione `Keep`, chave `factory-machine` se outro sistema precisar controlar o mesmo handle.

## FmodButton

`FmodButton` executa uma lista de ações em interações de Canvas ou objetos do mundo. Adicione por **Add Component > FMODB8 > FMODB8 Button**.

### Momentos

| Momento | Origem |
|---|---|
| `OnEnterCanvas` | `IPointerEnterHandler` do EventSystem. |
| `OnExitCanvas` | `IPointerExitHandler` do EventSystem. |
| `OnClickCanvas` | `IPointerClickHandler` do EventSystem. |
| `OnEnterWorld` | `OnMouseEnter`. |
| `OnExitWorld` | `OnMouseExit`. |
| `OnClickWorld` | `OnMouseDown`. |

UI exige um EventSystem e um Graphic raycastável. Interações de mundo normalmente exigem Collider.

### Comandos raiz

Cada ação começa com um comando raiz. Os principais grupos são:

| Grupo | Comandos |
|---|---|
| Reprodução | `Play`, `PlayLoop`, `Stop`, `Pause`, `Resume`, `TogglePause`, `StopAll` |
| Fades/ganho | `FadeIn`, `FadeOut`, `FadeTo`, `SetVolume`, `SetPitch` |
| Parâmetros | `SetParameter`, `SetParameterLabel`, `SetGlobalParameter` |
| Mixagem | `StartSnapshot`, `StopSnapshot`, `SetBusVolume`, `SetVcaVolume` |
| Handle guardado | `Kept` |

Os campos exibidos pelo Inspector mudam conforme o comando: ID/path, fade, duração, volume, parâmetro ou label.

### Cascata do botão

`Play`, `PlayLoop`, `StartSnapshot` e `Kept` devolvem um handle e aceitam uma cascata. Os modificadores disponíveis são equivalentes aos do emissor avançado: 3D, volume, pitch, raio, fades, parâmetros, timeline, follow, posição, velocidade, stop e keep.

Quando um modificador 3D precisa de âncora, a validação adiciona `As3D` e `Follow` apontando para o Transform do botão.

### Local ou multiplayer

`Playback Scope = Local` executa no cliente atual. `Multiplayer` envia pelo transporte configurado. O transporte pronto replica somente `Play`, `PlayLoop` e `StartSnapshot`; veja [Multiplayer](multiplayer.md).

### Exemplo: botão de menu

Crie duas ações:

1. `OnEnterCanvas > Play > UiHover` para o hover.
2. `OnClickCanvas > Play > UiConfirm` para o clique.

Ambas devem usar escopo `Local`, pois sons de interface normalmente não são replicados.

## FmodTrigger

`FmodTrigger` toca e para um evento a partir de callbacks Unity 2D/3D, clique ou ciclo de vida.

| Campo | Função |
|---|---|
| `Event Id` | ID ou path do evento. |
| `Loop` | Mantém uma reprodução controlável. |
| `Follow Self` | Faz a instância acompanhar o próprio Transform. |
| `Play On` | Flags combináveis para entrada/saída, colisão, enable, disable, destroy e click. |
| `Stop On` | Flags combináveis que encerram a reprodução. |
| `Fade Out` | Duração usada ao parar. |

Momentos disponíveis: `TriggerEnter`, `TriggerExit`, `Collision`, `Enable`, `Disable`, `Destroy` e `Click`.

O componente responde a trigger e colisão em 2D e 3D. Configure Colliders, `Is Trigger` e Rigidbodies conforme as regras da física Unity. O componente não filtra por tag; qualquer objeto que gere o callback dispara a ação.

## FmodArea

`FmodArea` combina até três mudanças enquanto um alvo permanece dentro de uma área 3D:

- iniciar/parar um snapshot;
- iniciar/parar uma música em loop;
- alterar um parâmetro na entrada e na saída.

| Campo | Função |
|---|---|
| `Target Tag` | Tag aceita. Vazio aceita qualquer Collider. |
| `Snapshot` | Path/ID do snapshot iniciado na entrada. |
| `Music` | Evento iniciado em loop com fade-in fixo de `0.25` segundo. |
| `Parameter Event` | Evento cujas instâncias recebem o parâmetro. |
| `Parameter` | Nome do parâmetro. |
| `Enter Value` | Valor aplicado na entrada. |
| `Exit Value` | Valor aplicado na saída. |
| `Fade Out` | Duração para encerrar snapshot e música. |

O GameObject da área deve ter um Collider 3D marcado como trigger. Esta versão não implementa callbacks de área 2D.

`Enter()` e `Exit()` são públicos e também podem ser chamados por Unity Events.

## FmodParameter

`FmodParameter` lê uma fonte de valor a cada `Update`, multiplica o resultado e envia para um parâmetro local ou global.

### Campos comuns

| Campo | Função |
|---|---|
| `Event Id` | ID das instâncias que receberão o parâmetro; ignorado para parâmetro global. |
| `Parameter` | Nome do parâmetro. |
| `Global Parameter` | Envia pelo sistema global do FMOD em vez de procurar instâncias do evento. |
| `Source` | Origem do valor. |
| `Multiplier` | Multiplicador final aplicado antes do envio. |

### Fontes

| Fonte | Campos usados | Valor |
|---|---|---|
| `Float` | `Float Value` | Float manual. |
| `Int` | `Int Value` | Inteiro convertido para float. |
| `Slider` | `Slider` | `Slider.value`. |
| `RigidbodySpeed` | `Rigidbody Source` | Magnitude de `linearVelocity`. |
| `Animator` | `Animator` + `Animator Parameter` | Parâmetro float do Animator. |
| `Distance` | `Distance From` + `Distance To` | Distância entre os Transforms. |
| `ComponentMember` | `Component Source` + `Member Name` | Campo/propriedade `float`, `int`, `double` ou `bool`, inclusive não público. |

Os métodos `SetFloat` e `SetInt` alteram o valor manual e aplicam imediatamente. `Apply()` permite forçar uma atualização por Unity Event.

Como o envio ocorre a cada frame, evite criar muitos componentes que usam reflexão por `ComponentMember` sem necessidade.

## FmodSlider

`FmodSlider` conecta elementos `UnityEngine.UI.Slider` aos volumes dos buses e salva valores em `PlayerPrefs`.

| Campo | Bus/chave padrão |
|---|---|
| `Master Slider` | `bus:/Master` / `Master` |
| `Music Slider` | `bus:/Master/Music` / `Music` |
| `Sfx Slider` | `bus:/Master/SFX` / `SFX` |
| `Other Sliders` | Path e chave definidos em cada entrada. |

Cada item adicional possui `Name`, usado como chave em `PlayerPrefs`, `Bus Path` e `Slider`.

No Editor, as chaves são apagadas no `Start` pelo comportamento atual do componente. Em um player compilado, os valores persistem.

## FmodAninEvent

`FmodAninEvent` é a ponte para Animation Events. O nome `Anin` é mantido por compatibilidade com animações existentes.

Métodos disponíveis:

| Método | Uso no Animation Event |
|---|---|
| `PlayOneShot(string id)` | Toca um evento. |
| `PlayLoop(string id)` | Inicia reprodução controlável. |
| `Pause(string id)` | Apesar do nome legado, atualmente chama `Resume(id)`. |
| `StopFadeOff(string id)` | Para imediatamente. |
| `StopFadeOn(string id)` | Para com fade padrão. |
| `GetState(string id)` | Consulta o estado, sem retornar valor ao Animation Event. |
| `AddEmitter(FmodEmitterCustom)` | Ativa um emissor avançado. |
| `RemoveEmitter(FmodEmitterCustom)` | Desativa um emissor avançado. |

Se `AddEmitter`/`RemoveEmitter` receberem referência nula, o componente procura um GameObject com a tag `FmodEmitter`.

## Prefabs do package

Crie pelos menus em **GameObject > FMODB8**:

| Menu/prefab | Conteúdo e uso |
|---|---|
| `FMODB8 System` | `FmodCommands`; recebe as listas de eventos e persiste entre cenas. |
| `FMODB8 Multiplayer` | `FmodMultiplayerSettings` e transporte NGO quando Netcode está instalado. |
| `FMODB8 Empty Emmiter` | GameObject pronto com `FmodEmitterCustom`. |
| `FMODB8 Slider Config` | Estrutura inicial para `FmodSlider` e controles de volume. |

## CreateFmodList

Crie por **Assets > FMODB8 > Create Event List**. A lista associa os IDs usados pelos componentes às referências FMOD.

| Campo | Função |
|---|---|
| `Type` | `Sfx`, `Music`, `Other` ou `None`. |
| `Type Name` | Nome livre exibido quando `Type = Other`. |
| `ID` | Nome usado em `Event Id`, `Sound Id` e na API. |
| `Reference` | `EventReference` selecionado no browser FMOD. |
| `Stage In Project` | `Undone`, `InProcess`, `Done` ou `Implemented`. |

Arraste as listas para `Event Lists` no `FmodCommands`. Os botões **Get To FMOD** e **Send To FMOD** sincronizam os estágios com as cores do projeto aberto no FMOD Studio.

## Fluxo recomendado

1. Crie `FMODB8 System` na primeira cena.
2. Crie listas separadas para SFX, música e outros grupos.
3. Cadastre IDs claros, como `PlayerFootstep`, `UiConfirm` e `MainTheme`.
4. Atribua as listas ao `FmodCommands`.
5. Use `FmodEmitter` para fontes simples.
6. Use `FmodEmitterCustom` somente quando a ordem de vários modificadores for necessária.
7. Use `FmodButton`, `FmodTrigger`, `FmodArea` e `FmodParameter` para comportamentos específicos.
8. Gere `FmodEvents` para o código por **FMOD > FMODB8 > Generate Events**.

