namespace SmartAPX.Providers.Tuya.Entities;

public record TuyaDeviceLog(
    string code, 
    string event_from, 
    long event_id, 
    long event_time, 
    string status, 
    string value
);