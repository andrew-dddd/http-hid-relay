using System;

namespace HttpHidRelay.Protocols;

public interface IRelayProtocol : IDisposable
{
    void SendCommand(byte relay, bool turnOn);
}
