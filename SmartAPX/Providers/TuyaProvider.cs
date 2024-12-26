using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using SmartAPX.Providers.Tuya.Entities;

namespace SmartAPX.Providers;

public class TuyaProvider(string clientId, string secret, string baseURL = "https://openapi.tuyaeu.com") {

    private TuyaTokenResult? tokenData = null;

    public async Task<string> getAccessToken() {
        if (tokenData == null) tokenData = await GetTuyaToken("token null");
        var expirationTime = DateTime.UtcNow.AddSeconds(tokenData.expire_time);
        if (DateTime.UtcNow >= expirationTime) tokenData = await GetTuyaToken("token expired");
        return tokenData.access_token;
    }
    
    // https://developer.tuya.com/en/docs/cloud/device-management?id=K9g6rfntdz78a#title-35-Query%20device%20logs
    public Task<TuyaResponse<TuyaPaginationResponse<TuyaDeviceLog>>?> GetDeviceLogs(string deviceId, string type = "1,2,3,4,5,6,7,8,9,10", long startTime = 0L, long? endTime = null) {
        if (endTime == null) endTime = GetTime();
        var url = $"/v1.0/devices/{deviceId}/logs?end_time={endTime}&start_time={startTime}&type={type}";
        return SendRequestAsync<TuyaPaginationResponse<TuyaDeviceLog>>(HttpMethod.Get, url);
    }

    public async Task<TuyaTokenResult> GetTuyaToken(string reason) {
        Console.WriteLine($"Tuya Auth: {reason}");
        var response = await SendRequestAsync<TuyaTokenResult>(HttpMethod.Get, "/v1.0/token?grant_type=1", runWithoutToken: true);
        return response.result!;
    }
    
    public async Task<TuyaResponse<T>> SendRequestAsync<T>(HttpMethod httpMethod, string url, string body = "", bool runWithoutToken = false) {
        string fullUrl = $"{baseURL}{url}";
        var timestamp = GetTime().ToString();
        
        string token = runWithoutToken ? "" : await getAccessToken();
        
        // @RubenPX: PTM casi una tarde entera para descifrar esto y hacer que funcione...
        // Aplicamos la especificación requerida por tuya : https://developer.tuya.com/en/docs/iot/api-request?id=Ka4a8uuo1j4t4
        var stringToSign = StringToSign(httpMethod.ToString().ToUpperInvariant(), url, body);
        var str = clientId + token + timestamp + stringToSign;
        var sign = CalcSign(str);
        
        // Prepare request
        HttpRequestMessage request = new HttpRequestMessage(httpMethod, fullUrl);
        request.Headers.Add("client_id", clientId);
        request.Headers.Add("sign", sign);
        request.Headers.Add("t", timestamp);
        if (!string.IsNullOrWhiteSpace(token)) request.Headers.Add("access_token", token);
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
        return $"{method}\n{sha256}\n\n{url}";
    }

    private string SHA256Hash(string input) {
        using (var sha256 = SHA256.Create()) {
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}