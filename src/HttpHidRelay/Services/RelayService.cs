using HidApi;
using HttpHidRelay.Protocols;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace HttpHidRelay.Services;

internal sealed class RelayService(IConfiguration config, ILogger<RelayService> logger) : IDisposable
{
    private readonly IRelayProtocol _device = CreateConnection(config, logger);

    public void SetRelayState(byte relay, bool turnOn) => _device.SendCommand(relay, turnOn);

    public void Dispose() => _device.Dispose();

    private static IRelayProtocol CreateConnection(IConfiguration config, ILogger logger)
    {        
        string vidStr = config["RelayConfig:VendorId"] ?? "0x16C0";
        string pidStr = config["RelayConfig:ProductId"] ?? "0x05DF";
        string path = config["RelayConfig:DevicePath"] ?? "/dev/usb_relay_1";
        bool usePath = config.GetValue<bool>("RelayConfig:UsePathMapping");

        if (usePath)
        {
            logger.LogInformation("Connecting with device using path: {Path}", path);
            return CreateProtocol(new Device(path));
        }
        else
        {
            var vid = Convert.ToUInt16(vidStr, 16);
            var pid = Convert.ToUInt16(pidStr, 16);
            logger.LogInformation("Connecting with device using VID: {VID}, PID: {PID}", vidStr, pidStr);
            return CreateProtocol(new Device(vid, pid));
        }
    }

    private static IRelayProtocol CreateProtocol(Device device)
    {
        var deviceInfo = device.GetDeviceInfo();
        var vid = deviceInfo.VendorId;

        return vid switch
        {
            0x16C0 => new DctTechProtocol(device),  // DctTech 
            0x1A86 => new QinHengProtocol(device),  // QinHeng 
            _ => throw new NotSupportedException($"Device with VID {vid:X4} is not supported.")
        };
    }
}