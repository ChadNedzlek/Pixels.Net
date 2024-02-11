using System;
using VaettirNet.PixelsDice.Net.Animations.Protocol.ConditionData;

namespace VaettirNet.PixelsDice.Net.Animations.Conditions;

public class ConnectionStateCondition : Condition
{
    public bool OnConnect { get; }
    public bool OnDisconnect { get; }

    public ConnectionStateCondition(bool onConnect, bool onDisconnect)
    {
        if (!onConnect && !onDisconnect)
        {
            throw new ArgumentException("At least one of connect/disconnect must be set");
        }

        OnConnect = onConnect;
        OnDisconnect = onDisconnect;
    }

    private protected override TypedCondition ToProtocol()
    {
        return new TypedCondition<ConnectionStateConditionData>(new ConnectionStateConditionData
        {
            Condition = (OnConnect ? ConnectionStateType.Connected : 0) | (OnDisconnect ? ConnectionStateType.Disconnected : 0),
        });
    }
}