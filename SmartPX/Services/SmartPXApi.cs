namespace SmartPX;

public class SmartPXApi {
    private readonly HttpClient client = new HttpClient() { BaseAddress = new Uri("http://localhost:5062") };

    public async Task helloRoute() {
        client.GetAsync("/tuya");
    }
    
    public async Task GetDeviceLogs(string deviceId, string type = "1,2,3,4,5,6,7,8,9,10", long startTime = 0L, long? endTime = null) {
        client.GetAsync($"/tuya/device/{deviceId}/logs?type={type}&start_time={startTime}&end_time={endTime}");
    }
}