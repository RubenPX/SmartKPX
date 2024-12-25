namespace SmartAPX.Providers.Tuya.Entities;

public record TuyaResponse<T>(
    bool success, 
    string t, 
    string tid, 
    string? msg, 
    T? result
);