using System.Collections.Generic;
using System.Threading.Tasks;
using FishMuseum.Core;
using FishMuseum.Data;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

namespace FishMuseum.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class TeacherPanelController : MonoBehaviour
    {
        [SerializeField] private string teacherPinForNewClass = "1234";

        private TextField     _classNameInput;
        private TextField     _questionInput;
        private TextField     _optAInput;
        private TextField     _optBInput;
        private TextField     _optCInput;
        private TextField     _optDInput;
        private DropdownField _correctDropdown;
        private Button        _btnSave;
        private Button        _btnLoad;
        private Button        _btnCancelEdit;
        private VisualElement _questionList;
        private Label         _feedback;

        private string _currentClassId;
        private string _editingQuestionId;
        private bool   _isEditMode;

        private void Start()
        {
            var doc = GetComponent<UIDocument>();
            if (doc == null) { Debug.LogError("[TeacherPanel] UIDocument bulunamadı."); return; }

            var root = doc.rootVisualElement;
            if (root == null) { Debug.LogError("[TeacherPanel] rootVisualElement null."); return; }

            _classNameInput  = root.Q<TextField>("teacher-class-name-input");
            _questionInput   = root.Q<TextField>("teacher-question-input");
            _optAInput       = root.Q<TextField>("teacher-option-a-input");
            _optBInput       = root.Q<TextField>("teacher-option-b-input");
            _optCInput       = root.Q<TextField>("teacher-option-c-input");
            _optDInput       = root.Q<TextField>("teacher-option-d-input");
            _correctDropdown = root.Q<DropdownField>("teacher-correct-dropdown");
            _btnSave         = root.Q<Button>("btn-teacher-save-question");
            _btnLoad         = root.Q<Button>("btn-teacher-load-questions");
            _btnCancelEdit   = root.Q<Button>("btn-teacher-cancel-edit");
            _questionList    = root.Q<VisualElement>("teacher-question-list");
            _feedback        = root.Q<Label>("teacher-panel-feedback");

            if (_correctDropdown != null)
            {
                _correctDropdown.choices = new List<string> { "a", "b", "c", "d" };
                _correctDropdown.index = 0;
            }

            if (_btnSave       != null) _btnSave.clicked       += () => _ = OnSaveQuestionAsync();
            if (_btnLoad       != null) _btnLoad.clicked       += () => _ = LoadQuestionsForClassAsync();
            if (_btnCancelEdit != null) _btnCancelEdit.clicked += () => { ExitEditMode(); ClearQuestionForm(); };
        }

        // ── Kaydet / Güncelle ─────────────────────────────────────────
        private async Task OnSaveQuestionAsync()
        {
            string className = _classNameInput?.value.Trim() ?? string.Empty;
            string question  = _questionInput?.value.Trim()  ?? string.Empty;
            string a = _optAInput?.value.Trim() ?? string.Empty;
            string b = _optBInput?.value.Trim() ?? string.Empty;
            string c = _optCInput?.value.Trim() ?? string.Empty;
            string d = _optDInput?.value.Trim() ?? string.Empty;
            string correct = _correctDropdown?.value ?? "a";

            if (string.IsNullOrEmpty(className) || !className.Contains("/"))
            { SetFeedback("Geçerli bir sınıf adı girin (örn. 7/A)."); return; }
            if (string.IsNullOrEmpty(question))
            { SetFeedback("Soru metni boş olamaz."); return; }
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b) ||
                string.IsNullOrEmpty(c) || string.IsNullOrEmpty(d))
            { SetFeedback("Tüm şıkları doldurun."); return; }
            if (correct != "a" && correct != "b" && correct != "c" && correct != "d")
            { SetFeedback("Doğru cevap a/b/c/d olmalı."); return; }

            if (SupabaseClient.Instance == null)
            { SetFeedback("Sunucu bağlantısı kurulamadı."); return; }

            var options = new QuestionOptions { a = a, b = b, c = c, d = d };

            if (_isEditMode && !string.IsNullOrEmpty(_editingQuestionId))
            {
                // ── GÜNCELLE (PATCH) ──
                SetFeedback("Güncelleniyor...");

                var payload = new UpdateQuestionPayload
                {
                    question_text  = question,
                    options        = options,
                    correct_option = correct,
                    is_active      = true
                };
                string json = JsonUtility.ToJson(payload);
                string endpoint = $"questions?id=eq.{_editingQuestionId}";

                string resp;
                try { resp = await SupabaseClient.Instance.PatchAsync(endpoint, json); }
                catch (System.Exception e)
                {
                    Debug.LogError("[TeacherPanel] Soru PATCH hatası: " + e.Message);
                    SetFeedback("Soru güncellenemedi.");
                    return;
                }

                if (string.IsNullOrEmpty(resp)) { SetFeedback("Soru güncellenemedi."); return; }

                Debug.Log("[TeacherPanel] Soru güncellendi.");
                SetFeedback("Soru başarıyla güncellendi.");

                ExitEditMode();
                ClearQuestionForm();
                await LoadQuestionsForClassAsync();
                return;
            }

            // ── YENİ SORU (POST) ──
            string classId = await ResolveClassIdAsync(className);
            if (string.IsNullOrEmpty(classId))
            { SetFeedback("Sınıf oluşturulamadı. Lütfen tekrar deneyin."); return; }

            var qPayload = new NewQuestionPayload
            {
                class_id       = classId,
                question_text  = question,
                options        = options,
                correct_option = correct,
                is_active      = true
            };
            string qJson = JsonUtility.ToJson(qPayload);

            string qResp;
            try { qResp = await SupabaseClient.Instance.PostAsync("questions", qJson); }
            catch (System.Exception e)
            {
                Debug.LogError("[TeacherPanel] Soru POST hatası: " + e.Message);
                SetFeedback("Soru kaydedilemedi.");
                return;
            }

            if (string.IsNullOrEmpty(qResp)) { SetFeedback("Soru kaydedilemedi."); return; }

            Debug.Log("[TeacherPanel] Soru kaydedildi.");
            SetFeedback("Soru başarıyla kaydedildi.");

            ClearQuestionForm();
            await LoadQuestionsForClassAsync();
        }

        // ── Soruları listele ──────────────────────────────────────────
        private async Task LoadQuestionsForClassAsync()
        {
            string className = _classNameInput?.value.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(className) || !className.Contains("/"))
            { SetFeedback("Geçerli bir sınıf adı girin (örn. 7/A)."); return; }

            if (SupabaseClient.Instance == null)
            { SetFeedback("Sunucu bağlantısı kurulamadı."); return; }

            string classId = await FindClassIdAsync(className);
            if (string.IsNullOrEmpty(classId))
            {
                _questionList?.Clear();
                SetFeedback("Bu sınıf bulunamadı veya hiç sorusu yok.");
                return;
            }

            _currentClassId = classId;

            string endpoint = $"questions?class_id=eq.{classId}&is_active=eq.true&order=created_at.desc";
            string json;
            try { json = await SupabaseClient.Instance.GetAsync(endpoint); }
            catch (System.Exception e)
            {
                Debug.LogError("[TeacherPanel] Soru listesi GET hatası: " + e.Message);
                SetFeedback("Sorular yüklenemedi.");
                return;
            }

            if (json == null) { SetFeedback("Sorular yüklenemedi."); return; }

            QuestionDataList result;
            try { result = QuestionDataList.FromJson(json); }
            catch (System.Exception e)
            {
                Debug.LogError("[TeacherPanel] Soru listesi parse hatası: " + e.Message);
                SetFeedback("Sorular işlenemedi.");
                return;
            }

            RenderQuestionList(result?.items);
        }

        private void RenderQuestionList(List<QuestionData> questions)
        {
            if (_questionList == null) return;
            _questionList.Clear();

            if (questions == null || questions.Count == 0)
            {
                SetFeedback("Bu sınıf için aktif soru yok.");
                return;
            }

            SetFeedback($"{questions.Count} aktif soru listelendi.");

            foreach (var q in questions)
            {
                if (q == null) continue;
                var captured = q;

                var item = new VisualElement();
                item.AddToClassList("question-list-item");

                string preview = captured.question_text ?? "(boş soru)";
                if (preview.Length > 60) preview = preview.Substring(0, 60) + "…";
                var text = new Label { text = preview };
                text.AddToClassList("question-list-text");

                var actions = new VisualElement();
                actions.AddToClassList("question-list-actions");

                var editBtn = new Button { text = "Düzenle" };
                editBtn.AddToClassList("question-edit-btn");
                editBtn.clicked += () => OnEditQuestion(captured);

                var delBtn = new Button { text = "Sil" };
                delBtn.AddToClassList("question-delete-btn");
                delBtn.clicked += () => _ = OnDeleteQuestionAsync(captured);

                actions.Add(editBtn);
                actions.Add(delBtn);
                item.Add(text);
                item.Add(actions);
                _questionList.Add(item);
            }
        }

        // ── Düzenle ───────────────────────────────────────────────────
        private void OnEditQuestion(QuestionData q)
        {
            if (q == null) return;

            FillFormForEdit(q);

            _isEditMode = true;
            _editingQuestionId = q.id;

            if (_btnSave != null) _btnSave.text = "Soruyu Güncelle";
            _btnCancelEdit?.RemoveFromClassList("hidden");

            SetFeedback("Düzenleme modundasınız.");
        }

        private void FillFormForEdit(QuestionData q)
        {
            if (_questionInput != null) _questionInput.value = q.question_text ?? string.Empty;
            if (_optAInput != null) _optAInput.value = q.GetOptionA() ?? string.Empty;
            if (_optBInput != null) _optBInput.value = q.GetOptionB() ?? string.Empty;
            if (_optCInput != null) _optCInput.value = q.GetOptionC() ?? string.Empty;
            if (_optDInput != null) _optDInput.value = q.GetOptionD() ?? string.Empty;

            if (_correctDropdown != null)
            {
                string correct = q.correct_option?.ToLower() ?? "a";
                int idx = _correctDropdown.choices.IndexOf(correct);
                _correctDropdown.index = idx >= 0 ? idx : 0;
            }
        }

        // ── Sil (soft delete) ─────────────────────────────────────────
        private async Task OnDeleteQuestionAsync(QuestionData q)
        {
            if (q == null || string.IsNullOrEmpty(q.id)) return;
            if (SupabaseClient.Instance == null)
            { SetFeedback("Sunucu bağlantısı kurulamadı."); return; }

            SetFeedback("Siliniyor...");

            var payload = new DeactivatePayload { is_active = false };
            string json = JsonUtility.ToJson(payload);
            string endpoint = $"questions?id=eq.{q.id}";

            string resp;
            try { resp = await SupabaseClient.Instance.PatchAsync(endpoint, json); }
            catch (System.Exception e)
            {
                Debug.LogError("[TeacherPanel] Soru silme PATCH hatası: " + e.Message);
                SetFeedback("Soru silinemedi.");
                return;
            }

            if (string.IsNullOrEmpty(resp)) { SetFeedback("Soru silinemedi."); return; }

            Debug.Log("[TeacherPanel] Soru pasifleştirildi.");
            SetFeedback("Soru silindi.");

            // Düzenlenen soru silindiyse moddan çık
            if (_isEditMode && _editingQuestionId == q.id)
            {
                ExitEditMode();
                ClearQuestionForm();
            }

            await LoadQuestionsForClassAsync();
        }

        // ── Yardımcılar ───────────────────────────────────────────────
        private void ClearQuestionForm()
        {
            if (_questionInput != null) _questionInput.value = string.Empty;
            if (_optAInput != null) _optAInput.value = string.Empty;
            if (_optBInput != null) _optBInput.value = string.Empty;
            if (_optCInput != null) _optCInput.value = string.Empty;
            if (_optDInput != null) _optDInput.value = string.Empty;
            if (_correctDropdown != null) _correctDropdown.index = 0;
        }

        private void ExitEditMode()
        {
            _isEditMode = false;
            _editingQuestionId = null;
            if (_btnSave != null) _btnSave.text = "Soruyu Kaydet";
            _btnCancelEdit?.AddToClassList("hidden");
        }

        private async Task<string> FindClassIdAsync(string className)
        {
            string encoded = UnityWebRequest.EscapeURL(className);
            string endpoint = $"classes?class_name=eq.{encoded}&is_archived=eq.false&limit=1";

            string resp;
            try { resp = await SupabaseClient.Instance.GetAsync(endpoint); }
            catch (System.Exception e)
            {
                Debug.LogError("[TeacherPanel] Sınıf GET hatası: " + e.Message);
                return null;
            }

            if (string.IsNullOrEmpty(resp)) return null;

            ClassDataList list = null;
            try { list = ClassDataList.FromJson(resp); }
            catch (System.Exception e) { Debug.LogError("[TeacherPanel] Sınıf parse hatası: " + e.Message); }

            if (list?.items != null && list.items.Count > 0)
            {
                Debug.Log($"[TeacherPanel] Sınıf bulundu: {className}");
                return list.items[0].id;
            }
            return null;
        }

        private async Task<string> ResolveClassIdAsync(string className)
        {
            string existingId = await FindClassIdAsync(className);
            if (!string.IsNullOrEmpty(existingId)) return existingId;

            var cPayload = new NewClassPayload
            {
                class_name  = className,
                teacher_pin = string.IsNullOrEmpty(teacherPinForNewClass) ? "1234" : teacherPinForNewClass,
                student_pin = "1111",
                is_archived = false
            };
            Debug.Log($"[TeacherPanel] Yeni sınıf oluşturma payload: " +
                      $"class_name={cPayload.class_name}, student_pin={cPayload.student_pin}, " +
                      $"teacher_pin={cPayload.teacher_pin}, is_archived={cPayload.is_archived}");

            string cJson = JsonUtility.ToJson(cPayload);

            string cResp;
            try { cResp = await SupabaseClient.Instance.PostAsync("classes", cJson); }
            catch (System.Exception e)
            {
                Debug.LogError("[TeacherPanel] Sınıf POST hatası: " + e.Message);
                return null;
            }

            if (string.IsNullOrEmpty(cResp)) return null;

            ClassDataList created = null;
            try { created = ClassDataList.FromJson(cResp); }
            catch (System.Exception e) { Debug.LogError("[TeacherPanel] Yeni sınıf parse hatası: " + e.Message); }

            if (created?.items != null && created.items.Count > 0)
            {
                Debug.Log($"[TeacherPanel] Yeni sınıf oluşturuldu: {className}");
                return created.items[0].id;
            }
            return null;
        }

        private void SetFeedback(string msg)
        {
            if (_feedback != null) _feedback.text = msg;
        }

        [System.Serializable]
        private class NewClassPayload
        {
            public string class_name;
            public string teacher_pin;
            public string student_pin;
            public bool   is_archived;
        }

        [System.Serializable]
        private class NewQuestionPayload
        {
            public string          class_id;
            public string          question_text;
            public QuestionOptions options;
            public string          correct_option;
            public bool            is_active;
        }

        [System.Serializable]
        private class UpdateQuestionPayload
        {
            public string          question_text;
            public QuestionOptions options;
            public string          correct_option;
            public bool            is_active;
        }

        [System.Serializable]
        private class DeactivatePayload
        {
            public bool is_active;
        }
    }
}