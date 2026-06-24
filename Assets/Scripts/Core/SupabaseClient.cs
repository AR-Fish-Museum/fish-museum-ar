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
        //  Bağlantı bilgileri — önce .env, yoksa Inspector fallback
        // ──────────────────────────────────────────────────
        [Header("Supabase Fallback (Android build için)")]
        [Tooltip("Sadece Supabase ANON/PUBLIC key kullanın. Service role key ASLA girmeyin.")]
        [SerializeField] private string supabaseUrlFallback;
        [SerializeField] private string supabaseAnonKeyFallback;

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

            string envUrl = EnvLoader.Get("SUPABASE_URL");
            string envKey = EnvLoader.Get("SUPABASE_ANON_KEY");

            bool hasEnvUrl      = !string.IsNullOrEmpty(envUrl);
            bool hasEnvKey      = !string.IsNullOrEmpty(envKey);
            bool hasFallbackUrl = !string.IsNullOrEmpty(supabaseUrlFallback);

            // env doluysa env, değilse Inspector fallback
            _supabaseUrl     = hasEnvUrl ? envUrl : supabaseUrlFallback;
            _supabaseAnonKey = hasEnvKey ? envKey : supabaseAnonKeyFallback;

            // Base URL temizliği
            if (!string.IsNullOrEmpty(_supabaseUrl))
            {
                _supabaseUrl = _supabaseUrl.Trim().TrimEnd('/');

                if (!_supabaseUrl.StartsWith("https://"))
                {
                    Debug.LogError("[SupabaseClient] Base URL 'https://' ile başlamıyor: " +
                                   _supabaseUrl + " — geçerli bir Supabase URL girin.");
                }
            }

            // Durum logları (anon key DEĞERİ asla loglanmaz)
            Debug.Log($"[SupabaseClient] Env URL var mı: {hasEnvUrl}");
            Debug.Log($"[SupabaseClient] Fallback URL var mı: {hasFallbackUrl}");
            Debug.Log($"[SupabaseClient] Final BaseUrl set: {!string.IsNullOrEmpty(_supabaseUrl)}");
            Debug.Log($"[SupabaseClient] AnonKey set: {!string.IsNullOrEmpty(_supabaseAnonKey)}");

            if (string.IsNullOrEmpty(_supabaseUrl) || string.IsNullOrEmpty(_supabaseAnonKey))
            {
                Debug.LogError("[SupabaseClient] Supabase URL veya ANON KEY boş. " +
                               "Editor'da .env, Android build'de Inspector fallback alanlarını kontrol edin.");
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

                string errorBody = request.downloadHandler != null ? request.downloadHandler.text : null;
                if (!string.IsNullOrEmpty(errorBody))
                    Debug.LogError($"[SupabaseClient] Hata Body: {errorBody}");

                return null;
            }

            Debug.Log($"[SupabaseClient] POST başarılı. Response boş mu: " +
                      $"{string.IsNullOrEmpty(request.downloadHandler.text)}");

            return request.downloadHandler.text;
        }

        /// <summary>
        /// Verilen endpoint'e JSON body ile PATCH (güncelleme) isteği gönderir.
        /// PostAsync ile aynı header yapısını kullanır; eklenen/güncellenen satırı döndürür.
        /// </summary>
        public async Task<string> PatchAsync(string endpoint, string jsonBody)
        {
            string finalUrl = BuildUrl(endpoint);
            byte[] bodyBytes = Encoding.UTF8.GetBytes(jsonBody);

            // PATCH'i Put üzerinden kur + method override:
            // new UnityWebRequest(url, "PATCH") bazı platformlarda (Android) header/method
            // düşürüp 401'e yol açabiliyor. Put yerleşik verb'i bunu güvenilir yapar.
            using UnityWebRequest request = UnityWebRequest.Put(finalUrl, bodyBytes);
            request.method          = "PATCH";
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            AddAuthHeaders(request);
            request.SetRequestHeader("Prefer", "return=representation");

            try
            {
                await SendRequestAsync(request);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SupabaseClient] PATCH isteği başarısız: {ex.Message}");
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

            Debug.Log($"[SupabaseClient] PATCH başarılı. Response boş mu: " +
                      $"{string.IsNullOrEmpty(request.downloadHandler.text)}");

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
            if (string.IsNullOrEmpty(_supabaseUrl))
            {
                Debug.LogError("[SupabaseClient] BaseUrl boş. Supabase bağlantısı kurulamaz.");
                return endpoint;
            }
            
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
