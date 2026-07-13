# Componentes e prefabs

Todos os componentes principais aparecem em **Add Component > FMODB8**. Os prefabs podem ser criados por **GameObject > FMODB8**.

## Prefabs

| Prefab/menu | Uso |
|---|---|
| `FMODB8 System` | Cria o serviço `FmodCommands`; atribua as listas de eventos em `Event Lists`. |
| `FMODB8 Multiplayer` | Cria as configurações multiplayer e, quando NGO está instalado, adiciona `FmodUnityNetcodeTransport`. |
| `FMODB8 Empty Emmiter` | Emissor configurável pronto para posicionar na cena. |
| `FMODB8 Slider Config` | Estrutura inicial para sliders de volume. |

## FmodEmitter

Emissor direto para um evento. Pode tocar no `Awake`, executar como one-shot ou loop, seguir um `Transform`, definir raio e aplicar fade de entrada/saída. `Play`, `Stop`, `Pause`, `Resume` e `SetParameter` também podem ser chamados por Unity Events.

Use `FmodEmitter` para a maioria das fontes simples controladas pelo Inspector.

## FmodEmitterCustom

Emissor baseado em uma cascata de operações. Os modos `Basic` e `Advanced` alteram o Inspector; no modo avançado é possível encadear posição 3D, attach, velocidade, raio, volume, pitch, fades, parâmetros, timeline, pausa e `Keep`.

Quando uma operação 3D exige dependências ausentes, a validação adiciona `As3D` e um `Attach` ao próprio objeto. O gizmo de raio aparece no modo avançado.

## FmodTrigger

Dispara um evento em um ou mais momentos:

- entrada ou saída de trigger 3D/2D;
- colisão 3D/2D;
- enable, disable ou destroy;
- clique de mouse ou pointer do EventSystem.

`Play On` e `Stop On` são flags combináveis. Para triggers, configure collider, `Is Trigger` e Rigidbody segundo as regras da física da Unity. Para pointer em UI, a cena precisa de um EventSystem.

## FmodArea

Aplica um estado de áudio ao entrar em um collider: inicia snapshot, inicia música em loop e/ou altera um parâmetro. Na saída, faz fade dos handles e aplica o valor de saída do parâmetro. O alvo é filtrado por tag; uma tag vazia aceita qualquer collider.

Atualmente o componente recebe callbacks de trigger 3D.

## FmodParameter

Atualiza um parâmetro FMOD a cada frame usando uma fonte configurável:

| Fonte | Valor lido |
|---|---|
| `Float` / `Int` | Valor manual do Inspector ou de `SetFloat`/`SetInt`. |
| `Slider` | `Slider.value`. |
| `RigidbodySpeed` | Magnitude da velocidade linear. |
| `Animator` | Parâmetro float do Animator. |
| `Distance` | Distância entre dois Transforms. |
| `ComponentMember` | Campo ou propriedade `float`, `int`, `double` ou `bool`, inclusive não pública. |

O valor é multiplicado por `Multiplier`. Ative `Global Parameter` para ignorar o Event ID e escrever em um parâmetro global.

## FmodButton

Executa ações em eventos de Canvas (`enter`, `exit`, `click`) ou objetos de mundo (`OnMouseEnter`, `OnMouseExit`, `OnMouseDown`). Cada ação possui:

- momento de disparo;
- comando raiz, como play, stop, parâmetro, bus, VCA ou snapshot;
- escopo local ou multiplayer;
- cascata opcional aplicada ao handle retornado.

Somente `Play`, `PlayLoop`, `StartSnapshot` e `Kept` retornam handle e aceitam cascata. Somente `Play`, `PlayLoop` e `StartSnapshot` são enviados pelo transporte multiplayer pronto.

## FmodSlider

Conecta sliders aos buses padrão `bus:/Master`, `bus:/Master/Music` e `bus:/Master/SFX`, além de buses adicionais. Os valores são salvos em `PlayerPrefs` pelo nome configurado.

Em execução no Editor, o componente limpa suas chaves de `PlayerPrefs` no `Start`; em build, os valores persistem normalmente.

## FmodAninEvent

Ponte legada para Animation Events. Expõe métodos com parâmetro `string` para play, loop, stop e consulta de estado, além de ativar/desativar `FmodEmitterCustom`.

O nome da classe e o menu preservam a grafia `Anin` por compatibilidade com cenas e animações existentes.

