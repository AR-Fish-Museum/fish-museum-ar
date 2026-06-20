using System;
using System.Collections.Generic;
using UnityEngine;

namespace FishMuseum.Data
{
    /// <summary>
    /// Supabase'deki "users" tablosunun tek bir satırını temsil eder.
    /// Alan adları tablodaki sütun adlarıyla birebir eşleşmelidir.
    /// </summary>
    [Serializable]
    public class UserData
    {
        public string id;
        public string class_id;
        public string first_name;
        public string last_name;
        public string role;
        public int    total_score;
        public bool   is_banned;
        public string created_at;
    }

    /// <summary>
    /// JsonUtility kök dizileri parse edemediği için wrapper kullanılır.
    /// (QuestionDataList ile aynı mantık.)
    /// </summary>
    [Serializable]
    public class UserDataList
    {
        public List<UserData> items;

        public static UserDataList FromJson(string json)
        {
            string wrapped = $"{{\"items\":{json}}}";
            return JsonUtility.FromJson<UserDataList>(wrapped);
        }
    }
}