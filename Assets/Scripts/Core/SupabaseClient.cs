using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace FishMuseum.Core
{
    /// <summary>
    /// Supabase REST API ile iletişimi yöneten singleton ağ yöneticisi.
    /// Sahneye bir kez yerleştirilen bu bileşen, tüm sistemler tarafından
    /// SupabaseClient.Instance üzerinden erişilebilir.
    /// </summary>
    public class SupabaseClient : MonoBehaviour
    {
        // ──────────────────────────────────────────────────
        //  Bağlantı bilgileri — .env dosyasından okunur
        // ──────────────────────────────────────────────────
        private string _supabaseUrl;
        private string _supabaseAnonKey;

        // ──────────────────────────────────────────────────
        //  Singleton
        // ──────────────────────────────────────────────────
        public static SupabaseClient Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            EnvLoader.Load();

            _supabaseUrl     = EnvLoader.Get("SUPABASE_URL");
            _supabaseAnonKey = EnvLoader.Get("SUPABASE_ANON_KEY");

            if (string.IsNullOrEmpty(_supabaseUrl) || string.IsNullOrEmpty(_supabaseAnonKey))
            {
                Debug.LogError("[SupabaseClient] SUPABASE_URL veya SUPABASE_ANON_KEY " +
                               ".env dosyasında bulunamadı ya da boş. " +
                               "Proje kök dizinindeki .env dosyasını kontrol edin.");
            }
        }

        // ──────────────────────────────────────────────────
        //  Public API
        // ──────────────────────────────────────────────────

        /// <summary>
        /// Verilen endpoint'e GET isteği gönderir ve ham JSON stringi döndürür.
        /// Hata durumunda null döner; hata konsola yazılır.
        /// </summary>
        /// <param name="endpoint">
        /// Sadece tablo adı ve sorgu parametreleri. Örn: "classes?is_archived=eq.false"
        /// rest/v1/ ön eki SupabaseClient tarafından eklenir.
        /// </param>
        public async Task<string> GetAsync(string endpoint)
        {
            string finalUrl = BuildUrl(endpoint);

            using UnityWebRequest request = UnityWebRequest.Get(finalUrl);
            AddAuthHeaders(request);

            try
            {
                await SendRequestAsync(request);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SupabaseClient] GET isteği başarısız: {ex.Message}");
                return null;
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(
                    $"[SupabaseClient] Hata {request.responseCode}: {request.error}\n" +
                    $"URL: {finalUrl}"
                );
                return null;
            }

            return request.downloadHandler.text;
        }

        /// <summary>
        /// Verilen endpoint'e JSON body ile POST isteği gönderir.
        /// Yanıt JSON stringini döndürür; hata durumunda null döner.
        /// </summary>
        public async Task<string> PostAsync(string endpoint, string jsonBody)
        {
            string finalUrl = BuildUrl(endpoint);
            byte[] bodyBytes = Encoding.UTF8.GetBytes(jsonBody);

            using UnityWebRequest request = new UnityWebRequest(finalUrl, "POST")
            {
                uploadHandler   = new UploadHandlerRaw(bodyBytes),
                downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("Content-Type", "application/json");
            AddAuthHeaders(request);
            request.SetRequestHeader("Prefer", "return=representation");

            try
            {
                await SendRequestAsync(request);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SupabaseClient] POST isteği başarısız: {ex.Message}");
                return null;
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(
                    $"[SupabaseClient] Hata {request.responseCode}: {request.error}\n" +
                    $"URL: {finalUrl}"
                );
                return null;
            }

            return request.downloadHandler.text;
        }

        // ──────────────────────────────────────────────────
        //  Yardımcı metodlar
        // ──────────────────────────────────────────────────

        /// <summary>
        /// Endpoint'i her koşulda tek bir "baseUrl/rest/v1/endpoint" formatına normalize eder.
        /// SUPABASE_URL içinde rest/v1 olsa da olmasa da doğru URL üretilir.
        /// </summary>
        private string BuildUrl(string endpoint)
        {
            // 1) Base URL'den sondaki slash ve varsa rest/v1 ön ekini temizle
            string baseUrl = _supabaseUrl.TrimEnd('/');
            if (baseUrl.EndsWith("/rest/v1")) baseUrl = baseUrl[..^"/rest/v1".Length];
            if (baseUrl.EndsWith("/rest"))    baseUrl = baseUrl[..^"/rest".Length];

            // 2) Endpoint'ten baştaki slash ve varsa rest/v1 ön ekini temizle
            string cleanEndpoint = endpoint.TrimStart('/');
            if (cleanEndpoint.StartsWith("rest/v1/")) cleanEndpoint = cleanEndpoint["rest/v1/".Length..];
            if (cleanEndpoint.StartsWith("rest/"))    cleanEndpoint = cleanEndpoint["rest/".Length..];

            string finalUrl = $"{baseUrl}/rest/v1/{cleanEndpoint}";
            Debug.Log($"[SupabaseClient] İşlenmiş Final URL: {finalUrl}");
            return finalUrl;
        }

        private void AddAuthHeaders(UnityWebRequest request)
        {
            request.SetRequestHeader("apikey",        _supabaseAnonKey);
            request.SetRequestHeader("Authorization", $"Bearer {_supabaseAnonKey}");
            request.SetRequestHeader("Accept",        "application/json");
        }

        /// <summary>
        /// UnityWebRequestAsyncOperation'ı gerçek bir awaitable Task'e dönüştürür.
        /// </summary>
        private static Task SendRequestAsync(UnityWebRequest request)
        {
            var tcs = new TaskCompletionSource<bool>();
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            operation.completed += _ =>
            {
                tcs.SetResult(true);
            };

            return tcs.Task;
        }
    }
}
