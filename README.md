# BISC8 Simple FMOD

O BISC8 Simple FMOD oferece uma camada de alto nível sobre o FMOD for Unity para tocar e controlar áudio com menos código. O package inclui uma API estática, configuração fluente por instância, componentes para uso pelo Inspector, listas de eventos, prefabs e transporte opcional para Netcode for GameObjects.

## Recursos

- Reprodução por ID cadastrado ou path FMOD (`event:/...` e `snapshot:/...`).
- Controle individual por `FmodHandle` ou coletivo por ID.
- Áudio 3D, follow de `Transform`, parâmetros, timeline, volume, pitch e fades.
- Controle de buses, VCAs e snapshots.
- Componentes para emissores, triggers, áreas, botões, sliders e Animation Events.
- Geração de constantes C# a partir das listas de eventos.
- Ações multiplayer opcionais com Netcode for GameObjects.
- FMOD for Unity 2.03.19 incluído no package.

## Requisitos

| Item | Versão/observação |
|---|---|
| Unity | 6000.3 ou posterior |
| FMOD | Integração 2.03.19 incluída |
| Plataformas nativas incluídas | Windows (x86, x86_64 e ARM64) e Linux (x86_64) |
| Multiplayer | Opcional: Netcode for GameObjects 1.0.0 ou posterior |

> Para publicar em outra plataforma, adicione os binários nativos compatíveis fornecidos pelo FMOD e valide a configuração de importação dos plugins antes do build.

## Instalação

### Unity Package Manager

1. Abra **Window > Package Management > Package Manager**.
2. Clique em **+ > Install package from git URL...** ou **Install package from disk...**.
3. Para instalação local, selecione este `package.json`.
4. Aguarde a importação e a compilação.

O FMOD já está ativo dentro do package. O comando **FMOD > FMODB8 > Setup** existe para compatibilidade com versões antigas que usavam uma pasta FMOD oculta. Se houver uma cópia antiga em `Assets/BISC8/FMODB8/FMOD`, use **FMOD > FMODB8 > Remove Outdated** para evitar plugins duplicados.

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
