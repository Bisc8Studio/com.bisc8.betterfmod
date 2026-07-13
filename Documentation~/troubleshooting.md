# Solução de problemas

## `Event not found`

- Confirme se o ID está na lista atribuída ao `FmodCommands`.
- Confirme se a entrada possui um `EventReference` válido.
- Depois de alterar listas durante o Play Mode, chame `RebuildEventLookup()`.
- Ao usar path direto, use `event:/...` ou `snapshot:/...` completo.
- Verifique se os banks foram exportados e encontrados pela configuração do FMOD.

## Não há áudio

- Verifique erros anteriores no Console do Unity.
- Confirme que a câmera possui `StudioListener`.
- Confira banks, plataforma selecionada e paths em **FMOD > Edit Settings**.
- Confirme volume de evento, bus, VCA e sistema operacional.
- Verifique `handle.IsValid` logo após `Play()` para identificar falha de criação.

## Áudio 3D não acompanha o objeto

- Use `FollowTransform(transform)` em vez de somente `Position`.
- Configure o evento como 3D no FMOD Studio.
- Adicione um `StudioListener` ao listener correto.
- Confira se o raio e as distâncias min/max do evento são adequados.

## Plugins ou assemblies FMOD duplicados

O package atual já inclui os assemblies FMOD e instala as bibliotecas nativas em `Assets/BISC8/FMODB8/FMODNative`. Remova outras integrações FMOD, mas preserve `FMODNative`. Se o Windows estiver bloqueando uma DLL durante um reparo, feche o Unity antes de executar a instalação novamente.

## O menu `Setup` informa que a fonte não foi encontrada

Confirme que o package está instalado com o nome `com.bisc8.simplefmod` e que contém `Runtime/FmodSystem/Plugins_FMOD/CustomFMOD/FMOD/FMODUnity.asmdef`. Reinstale o package se a estrutura estiver incompleta.

## Pastas `.del--*` no PackageCache

Versões até 1.3.0 carregavam as DLLs FMOD diretamente do cache. No Windows, o Unity não conseguia apagar a versão anterior durante uma atualização e deixava diretórios `.del--*`. A versão 1.3.2 mantém apenas os assemblies no package e instala os binários em `Assets`, evitando novos resíduos. Para remover resíduos antigos, feche o projeto Unity e apague somente as pastas `.del--*` na raiz de `Library/PackageCache`.

## Erros `FMODUnity could not be found` na versão 1.3.1

A versão 1.3.1 ocultava toda a integração antes da primeira compilação. Atualize para 1.3.2 ou posterior. A versão atual mantém `FMODUnity.asmdef` visível e oculta somente as bibliotecas nativas.

## Erros de referência de assembly

Use **FMOD > FMODB8 > Fix Commands** após mover scripts ou migrar uma versão antiga. Esse comando adiciona a referência `BISC8.FMODB8.Runtime` aos assembly definitions do projeto que ainda não a possuem. Aguarde o fim da compilação antes de executar o setup.

## `FmodEvents` não contém meus IDs

1. Salve os assets de lista.
2. Execute **FMOD > FMODB8 > Generate Events**.
3. Confira o Console e aguarde a recompilação.
4. Confirme a criação de `Assets/BISC8/FMODB8/Generated/FmodEvents.Generated.cs` e do respectivo `.asmref`.

## O botão multiplayer não toca

- Confirme que Netcode for GameObjects está instalado e sem erros de compilação.
- Verifique se `NetworkManager.IsListening` está ativo.
- Confirme a presença e ativação de `FmodUnityNetcodeTransport`.
- Use um comando replicável: `Play`, `PlayLoop` ou `StartSnapshot`.
- Confirme `Playback Scope = Multiplayer`.
- Verifique se o evento e seus banks existem em todos os clientes.

## Sliders não preservam valor no Editor

`FmodSlider` apaga as chaves configuradas no `Start` quando executado no Editor. Essa limpeza não ocorre no player compilado. Para testar persistência dentro do Editor, use outro componente ou ajuste esse comportamento no projeto.

## Build para macOS, Android, iOS ou consoles

Esta distribuição contém binários nativos para Windows e Linux. Obtenha do FMOD os binários correspondentes à plataforma, respeite a licença aplicável, configure os import settings e faça testes no dispositivo. A presença do código de plataforma, por si só, não substitui as bibliotecas nativas.
