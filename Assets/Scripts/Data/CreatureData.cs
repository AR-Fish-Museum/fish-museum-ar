using System;
using System.Collections.Generic;
using UnityEngine;

namespace FishMuseum.Data
{
    /// <summary>
    /// Supabase'deki "creatures" tablosunun tek bir satırını temsil eder.
    /// Alan adları tablodaki sütun adlarıyla birebir eşleşmelidir.
    /// </summary>
    [Serializable]
    public class CreatureData
    {
        public string id;
        public string name;
        public string description;
        public string image_url;
        public float  base_weight;
    }

    /// <summary>
    /// JsonUtility kök dizileri parse edemediği için wrapper kullanılır.
    /// </summary>
    [Serializable]
    public class CreatureDataList
    {
        public List<CreatureData> items;

        public static CreatureDataList FromJson(string json)
        {
            string wrapped = $"{{\"items\":{json}}}";
            return JsonUtility.FromJson<CreatureDataList>(wrapped);
        }
    }
}
