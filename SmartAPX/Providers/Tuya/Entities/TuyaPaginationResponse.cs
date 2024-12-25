namespace SmartAPX.Providers.Tuya.Entities;

public record TuyaPaginationResponse<T>(
    string current_row_key, 
    string device_id, 
    bool has_next, 
    List<T> logs
);