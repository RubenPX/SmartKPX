using DotNetEnv;
using SmartAPX.Providers;

namespace SmartAPX.Routes;

public static class Tuya {
    public static void AddTuyaEndpoints(this IEndpointRouteBuilder endpoints, string prefix = "/tuya") {

        TuyaProvider provider = new TuyaProvider(Env.GetString("tuya_client"), Env.GetString("tuya_secret"));
        
        endpoints.MapGet(prefix, () => {
            return new { Message = "Tuya endpoints." };
        });
        
        endpoints.MapGet(prefix + "/device/{device}/logs", (HttpRequest request, string device) => {
            long start_time = long.TryParse(request.Query["start_time"], out var stime) ? stime : 0L;
            long? end_time = long.TryParse(request.Query["end_time"], out var etime) ? etime : (long?)null;
            var type = request.Query["type"];
            
            return provider.GetDeviceLogs(device,type, start_time, end_time);
        });
    }
}