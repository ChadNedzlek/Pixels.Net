namespace VaettirNet.PixelsDice.Net.Messages;

internal enum MessageType : byte
{
    None = 0,
    WhoAreYou,
    // ReSharper disable once InconsistentNaming
    IAmADie,
    RollState,
    Telemetry,
    BulkSetup,
    BulkSetupAck,
    BulkData,
    BulkDataAck,
    TransferAnimSet,
    TransferAnimSetAck,
    TransferAnimSetFinished,
    TransferSettings,
    TransferSettingsAck,
    TransferSettingsFinished,
    TransferTestAnimSet,
    TransferTestAnimSetAck,
    TransferTestAnimSetFinished,
    DebugLog,
    PlayAnim,
    PlayAnimEvent,
    StopAnim,
    RemoteAction,
    RequestRollState,
    RequestAnimSet,
    RequestSettings,
    RequestTelemetry,
    ProgramDefaultAnimSet,
    ProgramDefaultAnimSetFinished,
    Blink,
    BlinkAck,
    RequestDefaultAnimSetColor,
    DefaultAnimSetColor,
    RequestBatteryLevel,
    BatteryLevel,
    RequestRssi,
    Rssi,
    Calibrate,
    CalibrateFace,
    NotifyUser,
    NotifyUserAck,
    TestHardware,
    StoreValue,
    StoreValueAck,
    SetTopLevelState,
    ProgramDefaultParameters,
    ProgramDefaultParametersFinished,
    SetDesignAndColor,
    SetDesignAndColorAck,
    SetCurrentBehavior,
    SetCurrentBehaviorAck,
    SetName,
    SetNameAck,
    PowerOperation,
    ExitValidation,
    TransferInstantAnimSet,
    TransferInstantAnimSetAck,
    TransferInstantAnimSetFinished,
    PlayInstantAnim,
    StopAllAnims,
    RequestTemperature,
    Temperature,
    SetBatteryControllerMode,
    Unused,
    Discharge,
    BlinkId,
    BlinkIdAck,
    TransferTest,
    TransferTestAck,
    TransferTestFinished,
    ClearSettings,
    ClearSettingsAck,

		
    TestBulkSend,
    TestBulkReceive,
    SetAllLeDsToColor,
    AttractMode,
    PrintNormals,
    PrintA2DReadings,
    LightUpFace,
    SetLedToColor,
    PrintAnimControllerState,

    Count,
};