using System;
using System.Collections.Generic;
using UnityEngine;

namespace FishMuseum.Data
{
    /// <summary>
    /// Supabase'deki "classes" tablosunun tek bir satırını temsil eder.
    /// JsonUtility ile doğrudan deserialize edilebilir.
    /// Alan adları tablodaki sütun adlarıyla birebir eşleşmelidir.
    /// </summary>
    [Serializable]
    public class ClassData
    {
        /// <summary>Tablodaki birincil anahtar (UUID veya int).</summary>
        public string id;

        /// <summary>Sınıf adı. Örn: "7/B", "5-A"</summary>
        public string class_name;

        /// <summary>Sınıfın arşivlenip arşivlenmediği. true ise sınıf artık aktif değildir.</summary>
        public bool is_archived;

        /// <summary>Öğrenci giriş PIN kodu.</summary>
        public string student_pin;

        /// <summary>Oluşturulma zamanı (ISO 8601 string olarak gelir).</summary>
        public string created_at;
    }

    /// <summary>
    /// JsonUtility, kök düzeyde dizileri doğrudan parse edemez.
    /// Supabase GET yanıtı bir JSON dizisi olarak geldiğinden,
    /// bu wrapper sınıf aracılığıyla deserialize edilir.
    ///
    /// Kullanım:
    ///   string json = await SupabaseClient.Instance.GetAsync(...);
    ///   var wrapper = ClassDataList.FromJson(json);
    ///   List<ClassData> list = wrapper.items;
    /// </summary>
    [Serializable]
    public class ClassDataList
    {
        public List<ClassData> items;

        public static ClassDataList FromJson(string json)
        {
            // JsonUtility dizileri okuyamadığı için wrapper trick kullanıyoruz
            string wrapped = $"{{\"items\":{json}}}";
            return JsonUtility.FromJson<ClassDataList>(wrapped);
        }
    }
}
