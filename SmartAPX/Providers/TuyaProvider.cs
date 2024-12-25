using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using SmartAPX.Providers.Tuya.Entities;

namespace SmartAPX.Providers;

public class TuyaProvider(string baseURL, string clientId, string secret) {

    private string? savedAccessToken = null;
    
    public async Task<TuyaResponse<TuyaPaginationResponse<TuyaDeviceLog>>?> GetDeviceLogs(string deviceId) {
        var url = $"/v1.0/devices/{deviceId}/logs?end_time=100000000000000&start_time=0&type=7";
        return await this.SendRequestAsync<TuyaPaginationResponse<TuyaDeviceLog>>(HttpMethod.Get, url);
    }
    
    public async Task<TuyaResponse<T>> SendRequestAsync<T>(HttpMethod httpMethod, string url, string body = "", bool runWithoutToken = false) {
        string fullUrl = $"https://openapi.tuyaeu.com{url}";
        var timestamp = GetTime().ToString();
        
        string accessToken = runWithoutToken ? "" : savedAccessToken ?? "";
        
        // @RubenPX: PTM casi una tarde entera para descifrar esto y hacer que funcione...
        // Aplicamos la especificación requerida por tuya : https://developer.tuya.com/en/docs/iot/api-request?id=Ka4a8uuo1j4t4
        var stringToSign = StringToSign(httpMethod.ToString().ToUpperInvariant(), url, body);
        var str = clientId + accessToken + timestamp + stringToSign;
        var sign = CalcSign(str);
        
        // Prepare request
        HttpRequestMessage request = new HttpRequestMessage(httpMethod, fullUrl);
        request.Headers.Add("client_id", clientId);
        request.Headers.Add("sign", sign);
        request.Headers.Add("t", timestamp);
        request.Headers.Add("access_token", accessToken);
        request.Headers.Add("sign_method", "HMAC-SHA256");
        
        // Send request
        using(HttpClient client = new HttpClient()) {
            HttpResponseMessage response = await client.SendAsync(request);
            string responseJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TuyaResponse<T>>(responseJson)!;    
        }
    }
    
    private long GetTime() {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    private string CalcSign(string str) {
        using (var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(secret))) {
            var hash = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(str));
            return BitConverter.ToString(hash).Replace("-", "").ToUpper();
        }
    }

    private string StringToSign(string method, string url, string body) {
        var map = new Dictionary<string, string>();
        var sha256 = SHA256Hash(body);
        return $"{method}\n{sha256}\n\n{url}";;
    }

    private string SHA256Hash(string input) {
        using (var sha256 = SHA256.Create()) {
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}