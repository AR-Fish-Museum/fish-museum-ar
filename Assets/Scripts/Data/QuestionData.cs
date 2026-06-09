using System;
using System.Collections.Generic;
using UnityEngine;

namespace FishMuseum.Data
{
    /// <summary>
    /// Supabase'deki "questions" tablosunun tek bir satırını temsil eder.
    /// Alan adları tablodaki sütun adlarıyla birebir eşleşmelidir.
    /// </summary>
    [Serializable]
    public class QuestionData
    {
        public string id;
        public string creature_id;
        public string class_id;       // null → misafir sorusu
        public string question_text;
        public string option_a;
        public string option_b;
        public string option_c;
        public string option_d;
        public string correct_option; // "a" | "b" | "c" | "d"
        public bool   is_active;
    }

    [Serializable]
    public class QuestionDataList
    {
        public List<QuestionData> items;

        public static QuestionDataList FromJson(string json)
        {
            string wrapped = $"{{\"items\":{json}}}";
            return JsonUtility.FromJson<QuestionDataList>(wrapped);
        }
    }
}
