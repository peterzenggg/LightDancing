namespace LightDancing.Enums
{
    public enum Handles
    {
        Lighting = 0x00,
        Background = 0x01,
        Auxiliary = 0x02,
    }
    public enum CommandIds
    {
        SetProperty = 0x01,
        GetProperty = 0x02,
        CloseHandle = 0x05,
        WriteEndpoint = 0x06,
        StreamEndpoint = 0x07,
        ReadEndpoint = 0x08,
        CheckHandle = 0x09,
        OpenEndpoint = 0x0D,
        PingDevice = 0x12,
        ConfirmChange = 0x15
    }
    public enum PropertyNames : byte
    {
        PollingRate = 0x01,
        HWBrightness = 0x02,
        Mode = 0x03,
        AngleSnapping = 0x07,
        IdleMode = 0x0d,
        BatteryLevel = 0x0F,
        BatteryStatus = 0x10,
        VendorId = 0x11,
        ProductId = 0x12,
        FirmwareVersion = 0x13,
        DPIProfile = 0x1E,
        DPIMask = 0x1F,
        DPIX = 0x21,
        DPIY = 0x22,
        IdleModeTimeout = 0x37,
        HWLayout = 0x41,
        MaxPollingRate = 0x96
    }
    public enum Endpoints
    {
        Lighting = 0x01,
        Buttons = 0x02,
        PairingID = 0x05,
        FanRPM = 0x17,
        FanSpeeds = 0x18,
        FanStates = 0x1A,
        LedCount_3Pin = 0x1D,
        LedCount_4Pin = 0x1E,
        TemperatureData = 0x21,
        LightingController = 0x22,
        ErrorLog = 0x27
    }
}
