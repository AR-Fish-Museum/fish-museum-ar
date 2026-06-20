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
        public string class_id;       // null → misafir sorusu
        public string creature_id;
        public string subject;
        public string topic;
        public int    grade_level;
        public string question_text;

        // options (jsonb) → iç içe nesne olarak parse edilir.
        // NOT: JsonUtility bir JSON nesnesini string'e yazamadığından
        // bu alan QuestionOptions tipindedir (string değil).
        public QuestionOptions options;

        public string correct_option; // "a" | "b" | "c" | "d"
        public bool   is_active;
        public string created_at;

        // ── Geriye dönük uyumluluk (eski option_a..d kullanımı için) ──
        public string option_a;
        public string option_b;
        public string option_c;
        public string option_d;

        // ── Şık erişim yardımcıları ──────────────────────────────────
        // Önce jsonb 'options' nesnesinden okur; boşsa eski option_x alanına düşer.
        public string GetOptionA() => GetOption("a");
        public string GetOptionB() => GetOption("b");
        public string GetOptionC() => GetOption("c");
        public string GetOptionD() => GetOption("d");

        private string GetOption(string key)
        {
            if (options != null)
            {
                switch (key)
                {
                    case "a": if (!string.IsNullOrEmpty(options.a)) return options.a; break;
                    case "b": if (!string.IsNullOrEmpty(options.b)) return options.b; break;
                    case "c": if (!string.IsNullOrEmpty(options.c)) return options.c; break;
                    case "d": if (!string.IsNullOrEmpty(options.d)) return options.d; break;
                }
            }

            switch (key)
            {
                case "a": return option_a;
                case "b": return option_b;
                case "c": return option_c;
                case "d": return option_d;
                default:  return null;
            }
        }
    }

    /// <summary>
    /// questions.options (jsonb) alanının serializable karşılığı.
    /// Format: {"a":"...","b":"...","c":"...","d":"..."}
    /// </summary>
    [Serializable]
    public class QuestionOptions
    {
        public string a;
        public string b;
        public string c;
        public string d;
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