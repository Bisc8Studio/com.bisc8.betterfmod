# Changelog

Todas as alterações relevantes deste package serão documentadas neste arquivo. O formato segue [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/) e o versionamento segue [Semantic Versioning](https://semver.org/lang/pt-BR/).

## [1.3.2] - 2026-07-13

### Corrigido

- Corrigida a primeira compilação da 1.3.1, que não encontrava os namespaces `FMOD` e `FMODUnity` enquanto toda a integração estava oculta.
- Scripts e assemblies FMOD voltaram a permanecer visíveis no package; somente as bibliotecas nativas ficam ocultas.
- Bibliotecas nativas são instaladas automaticamente em `Assets/BISC8/FMODB8/FMODNative`, evitando DLLs bloqueadas e diretórios `.del--*` no cache.
- Paths usados pelo Play Mode e pela seleção de binários FMOD agora apontam para a instalação nativa em `Assets`.
- Pastas nativas instaladas recebem GUIDs próprios no projeto, evitando conflitos de metadados com a fonte oculta.
- Dependências de UI, IMGUI, Timeline, Animation e física usadas pelo package agora são declaradas no `package.json`.
- Documentação reorganizada para priorizar os componentes, prefabs e fluxos próprios do FMODB8.

## [1.3.1] - 2026-07-13

### Alterado

- A integração FMOD inteira foi movida temporariamente para uma pasta oculta como tentativa de evitar DLLs bloqueadas no cache.
- A geração de `FmodEvents` foi movida do cache do package para `Assets/BISC8/FMODB8/Generated`, impedindo alterações locais dentro de packages Git.

### Problema conhecido

- A integração inteira oculta impedia que `BISC8.FMODB8.Runtime` encontrasse `FMOD` e `FMODUnity` na primeira compilação. Corrigido na 1.3.2.

## [1.3.0] - 2026-07-13

### Adicionado

- API pública `FmodB8`, configuração fluente com `FmodEventBuilder` e controle individual com `FmodHandle`.
- Controle de parâmetros locais e globais, volume, pitch, timeline, fades, buses, VCAs e snapshots.
- Suporte a áudio 3D, posição, velocidade, raio e acompanhamento de `Transform`.
- Componentes `FmodEmitter`, `FmodTrigger`, `FmodParameter` e `FmodArea`.
- Componentes configuráveis `FmodButton`, `FmodEmitterCustom`, `FmodSlider` e ponte para Animation Events.
- Listas de eventos e geração automática da classe `FmodEvents`.
- Prefabs de sistema, emissor, sliders e configuração multiplayer.
- Integração opcional com Netcode for GameObjects.

### Alterado

- FMOD for Unity 2.03.19 distribuído diretamente no package.
- Setup reconhece layouts atuais e legados do FMOD e remove o símbolo legado `FMOD_PRESENT`.

### Corrigido

- Detecção e remoção de cópias FMOD antigas que causam plugins nativos duplicados.
- Validação automática das dependências de cascatas 3D em botões e emissores.
