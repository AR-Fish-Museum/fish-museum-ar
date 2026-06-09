using System;
using System.IO;
using UnityEngine;

namespace FishMuseum.Core
{
    /// <summary>
    /// Proje kök dizinindeki .env dosyasını okuyarak
    /// anahtar=değer çiftlerini sistem ortam değişkenlerine yükler.
    ///
    /// ÖNEMLI — Platform notu:
    ///   Bu sınıf System.IO.File kullandığından yalnızca Unity Editor ve
    ///   Standalone (PC/Mac) build'lerde çalışır.
    ///   Android / iOS build'lerinde dosya sistemi erişimi farklıdır;
    ///   bu platformlar için değerleri BuildConfig veya
    ///   StreamingAssets yoluyla yönetmeniz gerekir.
    ///   Şimdilik geliştirme ortamı (Editor) için tasarlanmıştır.
    /// </summary>
    public static class EnvLoader
    {
        private const string ENV_FILE_NAME = ".env";

        /// <summary>
        /// .env dosyasını bulup yükler. Bir kez çağrılması yeterlidir.
        /// </summary>
        public static void Load()
        {
            string envPath = GetEnvFilePath();

            if (string.IsNullOrEmpty(envPath))
            {
                Debug.LogWarning("[EnvLoader] .env dosyası bulunamadı. " +
                                 "Proje kök dizininde .env dosyası oluşturun.");
                return;
            }

            try
            {
                string[] lines = File.ReadAllLines(envPath);
                int loadedCount = 0;

                foreach (string line in lines)
                {
                    string trimmed = line.Trim();

                    // Boş satır veya yorum satırını atla
                    if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                        continue;

                    int separatorIndex = trimmed.IndexOf('=');
                    if (separatorIndex < 1)
                        continue;

                    string key   = trimmed.Substring(0, separatorIndex).Trim();
                    string value = trimmed.Substring(separatorIndex + 1).Trim();

                    // Değeri tırnak içindeyse tırnakları soy: "değer" → değer
                    value = StripQuotes(value);

                    Environment.SetEnvironmentVariable(key, value);
                    loadedCount++;
                }

                Debug.Log($"[EnvLoader] {loadedCount} değişken yüklendi. ({envPath})");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[EnvLoader] .env dosyası okunamadı: {ex.Message}");
            }
        }

        /// <summary>
        /// Belirli bir anahtarı döndürür; bulunamazsa varsayılan değeri döner.
        /// EnvLoader.Load() çağrıldıktan sonra kullanılmalıdır.
        /// </summary>
        public static string Get(string key, string defaultValue = "")
        {
            string value = Environment.GetEnvironmentVariable(key);
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        // ──────────────────────────────────────────────────
        //  Yardımcı metodlar
        // ──────────────────────────────────────────────────

        private static string GetEnvFilePath()
        {
            // Application.dataPath → .../Assets
            // Bir üst dizin → proje kökü
            string projectRoot = Path.GetFullPath(
                Path.Combine(Application.dataPath, "..")
            );

            string fullPath = Path.Combine(projectRoot, ENV_FILE_NAME);
            return File.Exists(fullPath) ? fullPath : null;
        }

        private static string StripQuotes(string value)
        {
            if (value.Length >= 2 &&
                ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                 (value.StartsWith("'")  && value.EndsWith("'"))))
            {
                return value.Substring(1, value.Length - 2);
            }
            return value;
        }
    }
}
