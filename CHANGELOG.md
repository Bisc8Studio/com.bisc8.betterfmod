# Changelog

Todas as alterações relevantes deste package serão documentadas neste arquivo. O formato segue [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/) e o versionamento segue [Semantic Versioning](https://semver.org/lang/pt-BR/).

## [1.3.1] - 2026-07-13

### Corrigido

- FMOD passou a ser armazenado em uma pasta oculta do package e instalado automaticamente em `Assets/BISC8/FMODB8/FMOD`.
- Evitado o carregamento de DLLs nativas diretamente de `Library/PackageCache`, que fazia o Unity acumular diretórios `.del--*` ao atualizar o package Git no Windows.
- A geração de `FmodEvents` foi movida do cache do package para `Assets/BISC8/FMODB8/Generated`, impedindo alterações locais dentro de packages Git.
- A remoção de cópias antigas agora protege a instalação ativa quando o package usa a fonte FMOD oculta.

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
