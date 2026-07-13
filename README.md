# BISC8 Simple FMOD

**Versão atual:** 1.3.2

**Unity mínima:** 6000.3

**FMOD incluído:** 2.03.19

O BISC8 Simple FMOD é um toolkit de implementação de áudio para Unity. Seu foco são os componentes, prefabs, listas e comandos do FMODB8 que permitem configurar o comportamento do áudio no Inspector ou por uma API simplificada. A integração FMOD incluída é a base de reprodução, não o assunto principal do package.

## Recursos do FMODB8

| Recurso | Quando usar |
|---|---|
| `FmodEmitter` | Emissor simples configurado no Inspector, com follow, raio, loop e fades. |
| `FmodEmitterCustom` | Emissor avançado com cascata ordenada de modificadores. No menu aparece como **FMODB8 Emmiter**. |
| `FmodButton` | Executar uma ou várias ações de áudio em cliques e hover de UI ou objetos 3D. |
| `FmodTrigger` | Tocar ou parar eventos por trigger, colisão, clique e ciclo de vida. |
| `FmodArea` | Aplicar música, snapshot ou parâmetro enquanto o Player está em uma área. |
| `FmodParameter` | Vincular automaticamente um parâmetro a Slider, velocidade, Animator, distância ou outro componente. |
| `FmodSlider` | Controlar e salvar volumes de Master, Music, SFX e outros buses. |
| `FmodAninEvent` | Chamar comandos FMODB8 por Animation Events. |
| `CreateFmodList` | Organizar IDs, referências e estágio de implementação dos eventos. |
| `FmodEvents` | Usar IDs gerados como constantes C# em vez de strings espalhadas pelo projeto. |
| `FmodB8` / `FmodHandle` | Controlar áudio por código, coletivamente por ID ou por instância. |
| Multiplayer | Replicar ações configuradas em `FmodButton` pelo Netcode for GameObjects. |

Veja [Componentes e prefabs](Documentation~/components.md) para a descrição de cada campo do Inspector e exemplos completos.

## Requisitos

| Item | Versão/observação |
|---|---|
| Unity | 6000.3 ou posterior |
| FMOD | Integração 2.03.19 incluída |
| Plataformas nativas incluídas | Windows (x86, x86_64 e ARM64) e Linux (x86_64) |
| Multiplayer | Opcional: Netcode for GameObjects 1.0.0 ou posterior |

As dependências Unity UI, IMGUI, Timeline, Animation, Physics e Physics 2D são resolvidas automaticamente pelo Package Manager.

> Para publicar em outra plataforma, adicione os binários nativos compatíveis fornecidos pelo FMOD e valide a configuração de importação dos plugins antes do build.

## Instalação

### Unity Package Manager

1. Abra **Window > Package Management > Package Manager**.
2. Clique em **+ > Install package from git URL...** ou **Install package from disk...**.
3. Para instalação local, selecione este `package.json`.
4. Aguarde a importação e a compilação.

Os scripts e assemblies FMOD permanecem visíveis no package para permitir a primeira compilação. Somente as bibliotecas nativas ficam ocultas do `Library/PackageCache` e são instaladas automaticamente em `Assets/BISC8/FMODB8/FMODNative`. Isso impede que DLLs carregadas bloqueiem atualizações do package no Windows. O comando **FMOD > FMODB8 > Setup** permite repetir ou reparar a instalação manualmente.

## Início rápido

1. Crie o sistema por **GameObject > FMODB8 > FMODB8 System**.
2. Crie uma lista por **Assets > FMODB8 > Create Event List**.
3. Na lista, escolha o tipo, adicione um ID e associe o respectivo `EventReference`.
4. Arraste a lista para **Event Lists** no componente `FmodCommands` do sistema.
5. Adicione um `StudioListener` do FMOD à câmera usada como listener.
6. Toque o evento:

```csharp
FmodB8.Play("PlayerJump");
```

Também é possível usar diretamente um path FMOD, sem lista:

```csharp
FmodB8.Play("event:/SFX/Player/Jump");
```

Controle uma instância específica com um handle:

```csharp
FmodHandle music = FmodB8.Event("MainTheme")
    .Loop()
    .Volume(0.8f)
    .FadeIn(1.5f)
    .Play();

music.Parameter("Intensity", 2f);
music.FadeOut(1f);
```

Para áudio 3D:

```csharp
FmodHandle engine = FmodB8.Event("Engine")
    .Loop()
    .FollowTransform(transform)
    .Radius(30f)
    .Parameter("RPM", 1200f)
    .Play();
```

## Documentação

- [Manual completo](Documentation~/index.md)
- [Componentes e prefabs](Documentation~/components.md)
- [Referência da API](Documentation~/api-reference.md)
- [Multiplayer](Documentation~/multiplayer.md)
- [Solução de problemas](Documentation~/troubleshooting.md)
- [Histórico de alterações](CHANGELOG.md)
- [Avisos de terceiros](Third%20Party%20Notices.md)

## Licenças

O código e os binários do FMOD incluídos em `Runtime/FmodSystem/Plugins_FMOD/CustomFMOD/FMOD` permanecem sujeitos à licença da Firelight Technologies disponível em `Runtime/FmodSystem/Plugins_FMOD/CustomFMOD/FMOD/LICENSE.txt`. Confirme que o projeto possui uma licença FMOD adequada ao seu uso e distribuição.
