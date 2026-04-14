using HidApi;
using System;

namespace HttpHidRelay.Protocols;

public sealed class QinHengProtocol(Device device) : IRelayProtocol
{
    private bool disposedValue;

    public void SendCommand(byte relay, bool turnOn)
    {
        byte[] buffer = new byte[9];
        buffer[0] = 0x00;
        buffer[1] = turnOn ? (byte)0xA1 : (byte)0xA0;
        buffer[2] = relay;
        device.Write(buffer); // Te urządzenia używają standardowego zapisu
    }

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                device.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}