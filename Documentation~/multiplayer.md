# Multiplayer

O multiplayer do FMODB8 replica ações de `FmodButton`; ele não sincroniza continuamente timeline, parâmetros ou estado de todas as instâncias. A reprodução ocorre localmente em cada cliente que recebe o payload.

## Netcode for GameObjects

Quando `com.unity.netcode.gameobjects` 1.0.0 ou posterior está instalado, o assembly `BISC8.FMODB8.Netcode` é habilitado automaticamente.

1. Configure um `NetworkManager` funcional.
2. Crie **GameObject > FMODB8 > FMODB8 Multiplayer**.
3. Confirme que o objeto possui `FmodMultiplayerSettings` e `FmodUnityNetcodeTransport`.
4. Em cada `FmodButtonAction`, escolha `Playback Scope = Multiplayer`.

O transporte usa uma named message confiável e sequenciada. O cliente envia ao servidor; o servidor redistribui aos clientes conectados e reproduz localmente quando também é cliente. O payload carrega a posição de origem para cascatas 3D, não uma referência de `Transform` remota.

Somente os comandos `Play`, `PlayLoop` e `StartSnapshot` são considerados replicáveis pelo transporte pronto. Outros comandos permanecem locais.

## FmodMultiplayerSettings

- `Is Multiplayer`: ativa o modo multiplayer.
- `Auto Configure Fmod Buttons`: muda automaticamente o escopo de comandos replicáveis encontrados na cena.
- `Dont Destroy On Load`: preserva o objeto entre cenas.
- O fallback de reprodução local quando não há transporte existe na API, mas o campo correspondente está oculto no Inspector na versão atual.

A configuração automática percorre os `FmodButton` existentes quando as settings são aplicadas. Para botões instanciados depois disso, defina o escopo no prefab ou chame `ApplyToSceneButtons()` novamente.

## Transporte customizado

Implemente `IFmodMultiplayerTransport`:

```csharp
public sealed class MyFmodTransport : MonoBehaviour, IFmodMultiplayerTransport
{
    public bool CanSendFmodCommands => IsConnected;

    void OnEnable()
    {
        FmodCommands.MultiplayerTransport = this;
    }

    void OnDisable()
    {
        if (FmodCommands.MultiplayerTransport == this)
            FmodCommands.MultiplayerTransport = null;
    }

    public void SendFmodButtonAction(FmodButtonActionPayload payload)
    {
        // Serialize e envie pelo transporte do projeto.
    }

    private bool IsConnected => true;
}
```

No receptor, desserialize e execute:

```csharp
FmodCommands.ReceiveMultiplayerButtonAction(payload);
```

Como alternativa, assine `FmodCommands.MultiplayerButtonActionRequested`. Evite registrar ao mesmo tempo um transporte e um subscriber que enviem o mesmo payload, ou o som poderá ser duplicado.

## Boas práticas

- Não replique sons puramente locais, como hover de UI.
- Use IDs e banks idênticos em todos os clientes.
- Para sons 3D, inclua uma cascata espacial no botão; a posição da origem será serializada.
- Faça deduplicação no transporte customizado se sua camada de rede puder retransmitir mensagens.
- Teste host dedicado, host-cliente e cliente remoto separadamente.
