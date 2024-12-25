namespace SmartAPX.Providers.Tuya.Entities;

public record TuyaTokenResult(
    string access_token, 
    int expire_time, 
    string refresh_token, 
    string uid
);