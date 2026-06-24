using System.Collections.Generic;
using System.Threading.Tasks;
using FishMuseum.Core;
using FishMuseum.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace FishMuseum.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class MainAppScreenController : MonoBehaviour
    {
        // ── Ödül balığı (Inspector'dan atanır) ─────────────────────
        [Header("Ödül Balığı")]
        [SerializeField] private GameObject rewardClownfishPrefab;
        [SerializeField] private GameObject rewardSharkPrefab;
        [SerializeField] private int        sharkUnlockCorrectCount = 4;

        [Header("Balık Kartları (Inspector'dan genişletilebilir)")]
        [SerializeField] private List<RewardFishCardData> rewardFishCards = new List<RewardFishCardData>();

        [Header("Sahne Geçişi")]
        [SerializeField] private SceneLoader sceneLoader;


        // ── Sekme sabitleri ───────────────────────────────────────
        private const string TAB_CREATURES   = "Canlılar";
        private const string TAB_LEADERBOARD = "Sıralama";
        private const string TAB_ABOUT       = "Müze";
        private const string CSS_ACTIVE      = "nav-btn-active";
        private const string CSS_HIDDEN      = "hidden";

        // ── Liste alanı ───────────────────────────────────────────
        private Label      _currentTabLabel;
        private ScrollView _creaturesScroll;

        // ── Navigasyon butonları ──────────────────────────────────
        private Button _btnCreatures;
        private Button _btnLeaderboard;
        private Button _btnAbout;

        // ── Canlı detay overlay ───────────────────────────────────
        private VisualElement _detailOverlay;
        private Label         _detailCreatureName;
        private Label         _detailCreatureDesc;
        private Label         _detailCreatureWeight;
        private Button        _btnBackToList;
        private Button        _btnFeedCreature;

        // ── Soru overlay ──────────────────────────────────────────
        private VisualElement _questionOverlay;
        private Label         _questionText;
        private Label         _questionFeedback;
        private Button        _btnOptA;
        private Button        _btnOptB;
        private Button        _btnOptC;
        private Button        _btnOptD;
        private Button        _btnCloseQuestion;
        private Label         _quizStatusLabel;

        // ── Durum ─────────────────────────────────────────────────
        private string       _currentSelectedCreatureId;
        private QuestionData _currentQuestion;
        private bool         _initialized;

        // ── Sınıf soruları (quiz akışı) ───────────────────────────
        private List<QuestionData> _classQuestions;
        private int                _currentQuestionIndex;

        // ── Şık karıştırma: fiziksel buton (A/B/C/D konumu) -> DB key (a/b/c/d) ──
        private readonly string[]        _buttonKeys      = new string[4];
        private readonly System.Action[] _optionHandlers  = new System.Action[4];
        private static readonly string[] OptionDisplayLetters = { "A", "B", "C", "D" };

        // ══════════════════════════════════════════════════════════
        //  Start — UIDocument.Awake() bittikten sonra çalışır
        // ══════════════════════════════════════════════════════════

        private void Start()
        {
            var doc = GetComponent<UIDocument>();
            if (doc == null)
            {
                Debug.LogError("[MainAppScreenController] UIDocument bileşeni bulunamadı.");
                return;
            }

            var root = doc.rootVisualElement;
            if (root == null)
            {
                Debug.LogError("[MainAppScreenController] rootVisualElement null. " +
                               "UIDocument 'Source Asset' alanına MainAppScreen.uxml atandığından emin olun.");
                return;
            }

            // ── Referanslar ───────────────────────────────────────
            _currentTabLabel = root.Q<Label>("current-tab-label");
            _creaturesScroll = Require<ScrollView>(root, "creatures-scroll-view");

            _btnCreatures   = root.Q<Button>("btn-tab-creatures");
            _btnLeaderboard = root.Q<Button>("btn-tab-leaderboard");
            _btnAbout       = root.Q<Button>("btn-tab-about");

            _detailOverlay        = root.Q<VisualElement>("creature-detail-overlay");
            _detailCreatureName   = root.Q<Label>("detail-creature-name");
            _detailCreatureDesc   = root.Q<Label>("detail-creature-desc");
            _detailCreatureWeight = root.Q<Label>("detail-creature-weight");
            _btnBackToList        = root.Q<Button>("btn-back-to-list");
            _btnFeedCreature      = root.Q<Button>("btn-feed-creature");

            _questionOverlay   = Require<VisualElement>(root, "question-overlay");
            _questionText      = Require<Label>(root, "question-text");
            _questionFeedback  = Require<Label>(root, "question-feedback");
            _btnOptA           = Require<Button>(root, "btn-opt-a");
            _btnOptB           = Require<Button>(root, "btn-opt-b");
            _btnOptC           = Require<Button>(root, "btn-opt-c");
            _btnOptD           = Require<Button>(root, "btn-opt-d");
            _btnCloseQuestion  = Require<Button>(root, "btn-close-question");

            _quizStatusLabel = root.Q<Label>("QuizStatusLabel");

            // ── Event bağlantıları (bir kez bağlanır) ─────────────
            if (_btnCreatures   != null) _btnCreatures.clicked   += () => SwitchTab(TAB_CREATURES);
            if (_btnLeaderboard != null) _btnLeaderboard.clicked += () => SwitchTab(TAB_LEADERBOARD);
            if (_btnAbout       != null) _btnAbout.clicked       += () => SwitchTab(TAB_ABOUT);

            if (_btnBackToList    != null) _btnBackToList.clicked    += HideCreatureDetail;
            if (_btnFeedCreature  != null) _btnFeedCreature.clicked  += () => _ = OnFeedCreature();
            if (_btnCloseQuestion != null) _btnCloseQuestion.clicked += OnQuestionContinueClicked;

            _initialized = true;
            Debug.Log("[MainAppScreenController] Start tamamlandı — referanslar ve eventler hazır.");

            // İlk ekrana git
            ResetToInitialState();
            _ = LoadClassQuestionsAsync();
        }

        // ══════════════════════════════════════════════════════════
        //  OnEnable — GameObject her SetActive(true) aldığında
        // ══════════════════════════════════════════════════════════

        private void OnEnable()
        {
            Debug.Log("[MainAppScreenController] Ekran aktifleşti.");

            // Start henüz çalışmadıysa _initialized false olur; o zaman Start halleder
            if (!_initialized) return;

            // Yeniden açılınca temiz duruma getir ve soruları tazele
            ResetToInitialState();
            _ = LoadClassQuestionsAsync();
        }

        // ══════════════════════════════════════════════════════════
        //  Başlangıç durumu — her zaman canlılar listesi görünür,
        //  her iki overlay gizli
        // ══════════════════════════════════════════════════════════

        private void ResetToInitialState()
        {
            // Overlay'leri gizle
            _detailOverlay?.AddToClassList(CSS_HIDDEN);
            _questionOverlay?.AddToClassList(CSS_HIDDEN);

            // Durum değişkenlerini temizle
            _currentSelectedCreatureId = null;
            _currentQuestion           = null;

            // Nav butonlarını sıfırla, Canlılar aktif
            _btnCreatures?.RemoveFromClassList(CSS_ACTIVE);
            _btnLeaderboard?.RemoveFromClassList(CSS_ACTIVE);
            _btnAbout?.RemoveFromClassList(CSS_ACTIVE);
            _btnCreatures?.AddToClassList(CSS_ACTIVE);

            if (_currentTabLabel != null) _currentTabLabel.text = TAB_CREATURES;

            Debug.Log("[MainAppScreenController] Başlangıç durumuna döndü.");
        }

        // ══════════════════════════════════════════════════════════
        //  Sekme değiştirme
        // ══════════════════════════════════════════════════════════

        private void SwitchTab(string tabName)
        {
            Debug.Log($"[MainAppScreenController] Sekme değişti: {tabName}");

            // Her sekme geçişinde açık overlay'leri kapat
            _detailOverlay?.AddToClassList(CSS_HIDDEN);
            _questionOverlay?.AddToClassList(CSS_HIDDEN);

            _btnCreatures?.RemoveFromClassList(CSS_ACTIVE);
            _btnLeaderboard?.RemoveFromClassList(CSS_ACTIVE);
            _btnAbout?.RemoveFromClassList(CSS_ACTIVE);

            switch (tabName)
            {
                case TAB_CREATURES:
                    _btnCreatures?.AddToClassList(CSS_ACTIVE);
                    _ = LoadCreaturesAsync();
                    break;

                case TAB_LEADERBOARD:
                    _btnLeaderboard?.AddToClassList(CSS_ACTIVE);
                    break;

                case TAB_ABOUT:
                    _btnAbout?.AddToClassList(CSS_ACTIVE);
                    break;

                default:
                    Debug.LogWarning($"[MainAppScreenController] Bilinmeyen sekme: {tabName}");
                    return;
            }

            if (_currentTabLabel != null) _currentTabLabel.text = tabName;
        }

        // ══════════════════════════════════════════════════════════
        //  Supabase — canlıları yükle
        // ══════════════════════════════════════════════════════════

        private async Task LoadCreaturesAsync()
        {
            Debug.Log("[MainAppScreenController] Canlılar yükleniyor...");

            if (_creaturesScroll == null)
            {
                Debug.LogError("[MainAppScreenController] _creaturesScroll null.");
                return;
            }

            if (SupabaseClient.Instance == null)
            {
                Debug.LogError("[MainAppScreenController] SupabaseClient.Instance null.");
                return;
            }

            _creaturesScroll.Clear();
            _creaturesScroll.style.flexGrow = 1;
            _creaturesScroll.style.width    = new StyleLength(StyleKeyword.Auto);

            string json;
            try
            {
                json = await SupabaseClient.Instance.GetAsync("creatures?select=*");
            }
            catch (System.Exception e)
            {
                Debug.LogError("[MainAppScreenController] API Hatası: " + e.Message +
                               "\nStackTrace: " + e.StackTrace);
                return;
            }

            if (json == null)
            {
                Debug.LogError("[MainAppScreenController] Canlılar API null döndü.");
                return;
            }

            CreatureDataList result;
            try
            {
                result = CreatureDataList.FromJson(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[MainAppScreenController] JSON parse hatası: " + e.Message);
                return;
            }

            Debug.Log($"[MainAppScreenController] {result?.items?.Count ?? 0} canlı yüklendi.");

            if (result?.items == null || result.items.Count == 0)
            {
                var empty = new Label { text = "Henüz canlı eklenmemiş." };
                empty.AddToClassList("creature-desc");
                _creaturesScroll.Add(empty);
                return;
            }

            foreach (var creature in result.items)
            {
                if (creature == null) continue;
                _creaturesScroll.Add(BuildCreatureCard(creature));
            }
        }

        // ══════════════════════════════════════════════════════════
        //  Supabase — sınıf sorularını yükle (öğrenci quiz akışı)
        // ══════════════════════════════════════════════════════════

        private async Task LoadClassQuestionsAsync()
        {
            Debug.Log("[MainAppScreenController] Sınıf soruları yükleniyor...");

            if (_creaturesScroll == null)
            {
                Debug.LogError("[MainAppScreenController] _creaturesScroll null.");
                return;
            }

            if (GameSession.Instance == null)
            {
                Debug.LogError("[MainAppScreenController] GameSession.Instance null — " +
                               "Sınıf soruları yüklenemedi.");
                return;
            }

            string classId = GameSession.Instance.ClassId;
            if (string.IsNullOrEmpty(classId))
            {
                Debug.LogError("[MainAppScreenController] GameSession.ClassId boş — " +
                               "Öğrenci sınıf oturumu kurulmamış olabilir.");
                return;
            }

            if (SupabaseClient.Instance == null)
            {
                Debug.LogError("[MainAppScreenController] SupabaseClient.Instance null.");
                return;
            }

            string endpoint =
                $"questions?class_id=eq.{classId}&is_active=eq.true&order=created_at.asc";

            Debug.Log("[MainAppScreenController] Soru endpoint: " + endpoint);

            string json;
            try
            {
                json = await SupabaseClient.Instance.GetAsync(endpoint);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[MainAppScreenController] Soru API hatası: " + e.Message);
                return;
            }

            if (json == null)
            {
                Debug.LogError("[MainAppScreenController] Sorular API null döndü.");
                return;
            }

            QuestionDataList result;
            try
            {
                result = QuestionDataList.FromJson(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[MainAppScreenController] Soru parse hatası: " + e.Message);
                return;
            }

            Debug.Log($"[MainAppScreenController] {result?.items?.Count ?? 0} soru yüklendi.");

            // ── Soru yoksa bilgi mesajı göster ────────────────────
            if (result?.items == null || result.items.Count == 0)
            {
                _creaturesScroll.Clear();
                var empty = new Label { text = "Bu sınıf için henüz soru bulunamadı." };
                empty.AddToClassList("creature-desc");
                _creaturesScroll.Add(empty);
                return;
            }

            // ── Sorular var → quiz akışını başlat ─────────────────
            _classQuestions       = result.items;
            _currentQuestionIndex = 0;

            if (_currentTabLabel != null) _currentTabLabel.text = "Quiz";

            _creaturesScroll.Clear();
            _detailOverlay?.AddToClassList(CSS_HIDDEN);

            ShowQuestionOverlay(_classQuestions[0]);
        }

        // ══════════════════════════════════════════════════════════
        //  Canlı kartı — liste öğesi
        // ══════════════════════════════════════════════════════════

        private VisualElement BuildCreatureCard(CreatureData creature)
        {
            var card = new VisualElement();
            card.AddToClassList("creature-card");

            var nameLabel = new Label { text = creature.name ?? "(isimsiz)" };
            nameLabel.AddToClassList("creature-name");

            var descLabel = new Label { text = creature.description ?? string.Empty };
            descLabel.AddToClassList("creature-desc");

            var btn = new Button();
            btn.text = "AR'da Gör / Besle";
            btn.name = "btn-view-ar";
            btn.AddToClassList("creature-ar-btn");
            btn.clicked += () => ShowCreatureDetail(creature);

            card.Add(nameLabel);
            card.Add(descLabel);
            card.Add(btn);

            return card;
        }

        // ══════════════════════════════════════════════════════════
        //  Canlı detay overlay — göster / gizle
        // ══════════════════════════════════════════════════════════

        private void ShowCreatureDetail(CreatureData creature)
        {
            if (_detailOverlay == null) return;

            _currentSelectedCreatureId = creature.id;

            if (_detailCreatureName   != null) _detailCreatureName.text   = creature.name ?? "(isimsiz)";
            if (_detailCreatureDesc   != null) _detailCreatureDesc.text   = creature.description ?? "Açıklama bulunamadı.";
            if (_detailCreatureWeight != null) _detailCreatureWeight.text = $"Mevcut Ağırlık: {creature.base_weight} kg";

            // Soru overlay kapalı olmalı
            _questionOverlay?.AddToClassList(CSS_HIDDEN);

            _detailOverlay.RemoveFromClassList(CSS_HIDDEN);

            Debug.Log($"[MainAppScreenController] Detay gösteriliyor: {creature.name} (ID: {creature.id})");
        }

        private void HideCreatureDetail()
        {
            _detailOverlay?.AddToClassList(CSS_HIDDEN);
            _currentSelectedCreatureId = null;
            Debug.Log("[MainAppScreenController] Detay kapatıldı — liste ekranına dönüldü.");
        }

        // ══════════════════════════════════════════════════════════
        //  Besleme — Supabase'den soru çek
        // ══════════════════════════════════════════════════════════

        private async Task OnFeedCreature()
        {
            Debug.Log("[MainAppScreenController] Besleme tetiklendi!");

            if (string.IsNullOrEmpty(_currentSelectedCreatureId))
            {
                Debug.LogWarning("[MainAppScreenController] _currentSelectedCreatureId boş.");
                return;
            }

            if (SupabaseClient.Instance == null)
            {
                Debug.LogError("[MainAppScreenController] SupabaseClient.Instance null.");
                return;
            }

            // Misafir soruları: class_id null, canlıya özel, aktif
            string endpoint =
                $"questions?creature_id=eq.{_currentSelectedCreatureId}" +
                "&class_id=is.null" +
                "&is_active=eq.true" +
                "&limit=1";

            Debug.Log("[MainAppScreenController] Soru endpoint: " + endpoint);

            string json;
            try
            {
                json = await SupabaseClient.Instance.GetAsync(endpoint);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[MainAppScreenController] Soru API hatası: " + e.Message);
                return;
            }

            if (json == null)
            {
                Debug.LogError("[MainAppScreenController] Soru API null döndü.");
                return;
            }

            Debug.Log("[MainAppScreenController] Soru JSON: " + json);

            QuestionDataList result;
            try
            {
                result = QuestionDataList.FromJson(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[MainAppScreenController] Soru parse hatası: " + e.Message);
                return;
            }

            if (result?.items == null || result.items.Count == 0)
            {
                Debug.Log("[MainAppScreenController] Bu canlı için aktif soru bulunamadı.");
                return;
            }

            ShowQuestionOverlay(result.items[0]);
        }

        // ══════════════════════════════════════════════════════════
        //  Soru overlay — göster / gizle / kontrol
        // ══════════════════════════════════════════════════════════

        private void ShowQuestionOverlay(QuestionData question)
        {
            if (_questionOverlay == null) return;

            _currentQuestion = question;

            if (_questionText != null)
                _questionText.text = question.question_text ?? "Soru yüklenemedi.";

            // Şıklar sabit sırada: A=GetOptionA, B=GetOptionB, C=GetOptionC, D=GetOptionD
            SetOptionButton(_btnOptA, "a", question.GetOptionA());
            SetOptionButton(_btnOptB, "b", question.GetOptionB());
            SetOptionButton(_btnOptC, "c", question.GetOptionC());
            SetOptionButton(_btnOptD, "d", question.GetOptionD());

            if (_questionFeedback != null)
            {
                _questionFeedback.text = string.Empty;
                _questionFeedback.RemoveFromClassList("feedback-correct");
                _questionFeedback.RemoveFromClassList("feedback-wrong");
            }

            if (_btnCloseQuestion != null)
            {
                _btnCloseQuestion.text = "Devam Et";
                _btnCloseQuestion.style.display = DisplayStyle.None;
            }

            _questionOverlay.RemoveFromClassList(CSS_HIDDEN);
            UpdateQuizStatusLabel();
            Debug.Log($"[MainAppScreenController] Soru gösteriliyor: {question.question_text}");
        }

        private void UpdateQuizStatusLabel()
        {
            if (_quizStatusLabel == null) return;

            int x = _currentQuestionIndex + 1;
            int y = _classQuestions != null ? _classQuestions.Count : 0;
            int z = GameSession.Instance != null ? GameSession.Instance.Score : 0;

            _quizStatusLabel.style.display = DisplayStyle.Flex;
            _quizStatusLabel.text = $"Soru {x} / {y}  •  Puan: {z}";
        }

        // ══════════════════════════════════════════════════════════
        //  Şıkları rastgele sırala — DB key korunur, sadece görünüm değişir
        // ══════════════════════════════════════════════════════════

        private void AssignShuffledOptions(QuestionData question)
        {
            Button[] buttons = { _btnOptA, _btnOptB, _btnOptC, _btnOptD };

            // DB key + metin çiftleri (orijinal a/b/c/d korunur)
            var entries = new System.Collections.Generic.List<(string key, string text)>
            {
                ("a", question.GetOptionA()),
                ("b", question.GetOptionB()),
                ("c", question.GetOptionC()),
                ("d", question.GetOptionD()),
            };

            // Fisher-Yates karıştırma
            for (int i = entries.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (entries[i], entries[j]) = (entries[j], entries[i]);
            }

            for (int i = 0; i < buttons.Length; i++)
            {
                Button btn = buttons[i];
                if (btn == null) continue;

                string dbKey         = entries[i].key;   // bu fiziksel butonun DB anahtarı
                string optionText    = entries[i].text;
                string displayLetter = OptionDisplayLetters[i];

                _buttonKeys[i] = dbKey;

                btn.text = $"{displayLetter}) {optionText ?? "—"}";
                btn.SetEnabled(true);
                btn.RemoveFromClassList("option-correct");
                btn.RemoveFromClassList("option-wrong");

                // Önceki handler'ı kaldır (clicked event'lerinin birikmesini önle)
                if (_optionHandlers[i] != null)
                    btn.clicked -= _optionHandlers[i];

                // dbKey her döngüde yeni bir local; closure güvenli
                System.Action handler = () => CheckAnswer(dbKey);
                _optionHandlers[i] = handler;
                btn.clicked += handler;
            }
        }

        /// <summary>DB key'in (a/b/c/d) ekrandaki görünen harfini (A/B/C/D) döndürür.</summary>
        private string GetDisplayLetterForKey(string key)
        {
            for (int i = 0; i < _buttonKeys.Length; i++)
                if (_buttonKeys[i] == key) return OptionDisplayLetters[i];
            return key?.ToUpper() ?? string.Empty;
        }

        /// <summary>
        /// Şık butonunu sıfırlar, listener'ları temizleyip yeniden bağlar.
        /// </summary>
        private void SetOptionButton(Button btn, string key, string optionText)
        {
            if (btn == null) return;

            btn.text = $"{key.ToUpper()}) {optionText ?? "—"}";
            btn.SetEnabled(true);
            btn.RemoveFromClassList("option-correct");
            btn.RemoveFromClassList("option-wrong");

            // Eski listener'ları kaldır (birden fazla çağrıda birikmeyi önle)
            btn.clicked -= OnOptionAClicked;
            btn.clicked -= OnOptionBClicked;
            btn.clicked -= OnOptionCClicked;
            btn.clicked -= OnOptionDClicked;

            switch (key)
            {
                case "a": btn.clicked += OnOptionAClicked; break;
                case "b": btn.clicked += OnOptionBClicked; break;
                case "c": btn.clicked += OnOptionCClicked; break;
                case "d": btn.clicked += OnOptionDClicked; break;
            }
        }

        private void OnOptionAClicked() => CheckAnswer("a");
        private void OnOptionBClicked() => CheckAnswer("b");
        private void OnOptionCClicked() => CheckAnswer("c");
        private void OnOptionDClicked() => CheckAnswer("d");

        private async void CheckAnswer(string chosen)
        {
            if (_currentQuestion == null) return;

            string correct = _currentQuestion.correct_option?.ToLower() ?? string.Empty;
            bool   isRight = chosen == correct;

            // Tüm şıkları kilitle ve renk ver (sabit a/b/c/d eşlemesi)
            MarkOption(_btnOptA, "a", chosen, correct);
            MarkOption(_btnOptB, "b", chosen, correct);
            MarkOption(_btnOptC, "c", chosen, correct);
            MarkOption(_btnOptD, "d", chosen, correct);

            // Cevabı GameSession quiz sayaçlarına/puanına kaydet
            if (GameSession.Instance != null)
            {
                GameSession.Instance.RegisterAnswer(isRight);
            }

            try
            {
                if (SupabaseClient.Instance != null &&
                    GameSession.Instance != null &&
                    !string.IsNullOrEmpty(GameSession.Instance.UserId) &&
                    _currentQuestion != null)
                {
                    var answerPayload = new AnswerPayload
                    {
                        question_id = _currentQuestion.id,
                        user_id = GameSession.Instance.UserId,
                        is_correct = isRight,
                        chosen_option = chosen
                    };

                    string answerJson = JsonUtility.ToJson(answerPayload);

                    await SupabaseClient.Instance.PostAsync("analytics_answers", answerJson);

                    Debug.Log("[MainAppScreenController] Cevap Supabase analytics_answers tablosuna kaydedildi.");
                }
                else
                {
                    Debug.LogWarning("[MainAppScreenController] Cevap Supabase'e kaydedilemedi. SupabaseClient, UserId veya CurrentQuestion eksik.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("[MainAppScreenController] Cevap kaydı sırasında hata: " + e.Message);
            }

            UpdateQuizStatusLabel();

            if (_questionFeedback != null)
            {
                _questionFeedback.RemoveFromClassList("feedback-correct");
                _questionFeedback.RemoveFromClassList("feedback-wrong");

                if (isRight)
                {
                    _questionFeedback.text = "Doğru! 🎉 +10 Puan";
                    _questionFeedback.AddToClassList("feedback-correct");
                }
                else
                {
                    _questionFeedback.text = $"Yanlış! Doğru cevap: {correct.ToUpper()}";
                    _questionFeedback.AddToClassList("feedback-wrong");
                }
            }

            if (_btnCloseQuestion != null)
            {
                _btnCloseQuestion.text = "Devam Et";
                _btnCloseQuestion.style.display = DisplayStyle.Flex;
            }

            int totalScore = GameSession.Instance != null ? GameSession.Instance.Score : 0;
            Debug.Log($"[MainAppScreenController] Seçilen: {chosen} | Doğru: {correct} | " +
                      $"{(isRight ? "✓ Doğru" : "✗ Yanlış")} | Score: {totalScore}");
        }

        private void MarkOption(Button btn, string key, string chosen, string correct)
        {
            if (btn == null) return;
            btn.SetEnabled(false);
            if (key == correct)       btn.AddToClassList("option-correct");
            else if (key == chosen)   btn.AddToClassList("option-wrong");
        }

        private void HideQuestionOverlay()
        {
            // Sorudan çıkınca canlı detay sayfasına döner (detay overlay açık kalır)
            _questionOverlay?.AddToClassList(CSS_HIDDEN);
            _currentQuestion = null;
            Debug.Log("[MainAppScreenController] Soru kapatıldı — canlı detay sayfasına dönüldü.");
        }

        // ══════════════════════════════════════════════════════════
        //  Devam Et — quiz akışında sonraki soruya geç
        // ══════════════════════════════════════════════════════════

        private void OnQuestionContinueClicked()
        {
            // Quiz modunda mıyız?
            if (_classQuestions != null && _classQuestions.Count > 0)
            {
                _currentQuestionIndex++;

                if (_currentQuestionIndex < _classQuestions.Count)
                {
                    ShowQuestionOverlay(_classQuestions[_currentQuestionIndex]);
                    return;
                }

                // Soru kalmadı → quiz tamamlandı sonucunu göster
                ShowQuizResult();
                return;
            }

            // Quiz modu değil → eski davranış
            HideQuestionOverlay();
        }

        // ══════════════════════════════════════════════════════════
        //  Quiz sonucu kartı
        // ══════════════════════════════════════════════════════════

        private void ShowQuizResult()
        {
            // Soru ve detay panellerini kesin kapat
            if (_questionOverlay != null)
            {
                _questionOverlay.style.display = DisplayStyle.None;
            }

            if (_detailOverlay != null)
            {
                _detailOverlay.style.display = DisplayStyle.None;
            }

            if (_currentQuestion != null)
            {
                _currentQuestion = null;
            }

            if (_currentTabLabel != null) _currentTabLabel.text = "Quiz Tamamlandı";

            // Sonuç listesini görünür yap ve temizle
            if (_creaturesScroll != null)
            {
                _creaturesScroll.style.display = DisplayStyle.Flex;
                _creaturesScroll.Clear();
            }

            int score    = GameSession.Instance != null ? GameSession.Instance.Score                 : 0;
            int correct  = GameSession.Instance != null ? GameSession.Instance.CorrectCount          : 0;
            int wrong    = GameSession.Instance != null ? GameSession.Instance.WrongCount            : 0;
            int answered = GameSession.Instance != null ? GameSession.Instance.AnsweredQuestionCount : 0;
            int total    = _classQuestions != null ? _classQuestions.Count : 0;

            var card = new VisualElement();
            card.AddToClassList("quiz-result-card");

            var title = new Label { text = "Quiz Tamamlandı 🎉" };
            title.AddToClassList("quiz-result-title");

            var subtitle = new Label { text = "Tebrikler, tüm soruları tamamladın." };
            subtitle.AddToClassList("quiz-result-subtitle");

            var scoreLabel = new Label { text = $"Toplam Puan: {score}" };
            scoreLabel.AddToClassList("quiz-result-score");

            var correctLabel = new Label { text = $"Doğru Sayısı: {correct}" };
            correctLabel.AddToClassList("quiz-result-stat");

            var wrongLabel = new Label { text = $"Yanlış Sayısı: {wrong}" };
            wrongLabel.AddToClassList("quiz-result-stat");

            var answeredLabel = new Label { text = $"Cevaplanan Soru: {answered} / {total}" };
            answeredLabel.AddToClassList("quiz-result-stat");

            card.Add(title);
            card.Add(subtitle);
            card.Add(scoreLabel);
            card.Add(correctLabel);
            card.Add(wrongLabel);
            card.Add(answeredLabel);

            // ── Balığını Seç — tüm balık kartları (puan kısıtı YOK) ──
            var selectTitle = new Label { text = "Balığını Seç" };
            selectTitle.AddToClassList("fish-select-title");
            card.Add(selectTitle);

            var fishCards = GetEffectiveFishCards();
            if (fishCards.Count == 0)
            {
                var noFish = new Label { text = "Henüz balık eklenmemiş." };
                noFish.AddToClassList("quiz-result-stat");
                card.Add(noFish);
            }
            else
            {
                foreach (var data in fishCards)
                {
                    if (data == null || data.prefab == null) continue; // prefab yoksa kart gösterilmez
                    card.Add(BuildFishCard(data));
                }
            }

            // Baştan başla — sadece soruları yeniden başlatır (puan sıfırlanmaz)
            var restartBtn = new Button { text = "Quiz'e Baştan Başla" };
            restartBtn.AddToClassList("quiz-result-restart-btn");
            restartBtn.clicked += () =>
            {
                if (_classQuestions == null || _classQuestions.Count == 0) return;

                _currentQuestionIndex = 0;

                if (_creaturesScroll != null)
                {
                    _creaturesScroll.Clear();
                    _creaturesScroll.style.display = DisplayStyle.None;
                }

                if (_questionOverlay != null)
                {
                    _questionOverlay.style.display = DisplayStyle.Flex;
                }

                ShowQuestionOverlay(_classQuestions[0]);
            };
            card.Add(restartBtn);

            // Sıralamayı Gör — aynı sınıftaki öğrenci sıralaması
            var leaderboardBtn = new Button { text = "Sıralamayı Gör" };
            leaderboardBtn.AddToClassList("quiz-result-leaderboard-btn");
            leaderboardBtn.clicked += () => _ = LoadLeaderboardAsync();
            card.Add(leaderboardBtn);

            _creaturesScroll?.Add(card);

            Debug.Log($"[MainAppScreenController] Quiz tamamlandı — Puan: {score}, " +
                      $"Doğru: {correct}, Yanlış: {wrong}, Cevaplanan: {answered}/{total}");
        }

        // ══════════════════════════════════════════════════════════
        //  Balığımı Gör — skora göre ödül balığı seç + AR sahnesine geç
        // ══════════════════════════════════════════════════════════

        private void OnViewRewardFishClicked()
        {
            if (GameSession.Instance == null)
            {
                Debug.LogError("[MainAppScreenController] GameSession.Instance null — ödül balığı seçilemedi.");
                return;
            }

            int correctCount = GameSession.Instance.CorrectCount;

            GameObject selectedPrefab;
            string     fishId;
            string     fishName;

            if (correctCount >= sharkUnlockCorrectCount)
            {
                selectedPrefab = rewardSharkPrefab;
                fishId         = "reward_shark";
                fishName       = "Büyük Beyaz Köpekbalığı";
            }
            else
            {
                selectedPrefab = rewardClownfishPrefab;
                fishId         = "reward_clownfish";
                fishName       = "Palyaço Balığı";
            }

            if (selectedPrefab == null)
            {
                Debug.LogError($"[MainAppScreenController] Ödül balığı prefab'ı atanmamış " +
                               $"(correctCount: {correctCount}, fishId: {fishId}). " +
                               "Inspector'dan rewardClownfishPrefab / rewardSharkPrefab atayın.");
                return;
            }

            SelectedFishSession.SelectFish(fishId, fishName, selectedPrefab);
            Debug.Log($"[MainAppScreenController] Ödül balığı seçildi: {fishName} " +
                      $"(id: {fishId}, doğru: {correctCount}/{sharkUnlockCorrectCount}).");

            if (sceneLoader != null)
            {
                sceneLoader.LoadRealARScene();
            }
            else
            {
                Debug.LogError("[MainAppScreenController] sceneLoader null — " +
                               "Inspector'dan SceneLoader referansı bağlanmamış. AR sahnesine geçilemedi.");
            }
        }

        // ══════════════════════════════════════════════════════════
        //  Balık kartları — tüm balıklar (puan kısıtı yok)
        // ══════════════════════════════════════════════════════════

        private List<RewardFishCardData> GetEffectiveFishCards()
        {
            if (rewardFishCards != null && rewardFishCards.Count > 0)
                return rewardFishCards;

            // Geriye dönük uyumluluk: eski iki prefab alanından varsayılan kartlar
            var fallback = new List<RewardFishCardData>();
            if (rewardClownfishPrefab != null)
                fallback.Add(new RewardFishCardData
                {
                    fishId      = "reward_clownfish",
                    fishName    = "Palyaço Balığı",
                    description = "Sevimli ve renkli bir resif balığı.",
                    prefab      = rewardClownfishPrefab
                });
            if (rewardSharkPrefab != null)
                fallback.Add(new RewardFishCardData
                {
                    fishId      = "reward_shark",
                    fishName    = "Büyük Beyaz Köpekbalığı",
                    description = "Okyanusun güçlü avcısı.",
                    prefab      = rewardSharkPrefab
                });
            return fallback;
        }

        private VisualElement BuildFishCard(RewardFishCardData data)
        {
            var fishCard = new VisualElement();
            fishCard.AddToClassList("fish-card");

            if (data.previewImage != null)
            {
                var img = new Image { image = data.previewImage, scaleMode = ScaleMode.ScaleToFit };
                img.AddToClassList("fish-card-image");
                fishCard.Add(img);
            }

            var nameLabel = new Label { text = data.fishName ?? "(isimsiz balık)" };
            nameLabel.AddToClassList("fish-card-name");
            fishCard.Add(nameLabel);

            if (!string.IsNullOrEmpty(data.description))
            {
                var desc = new Label { text = data.description };
                desc.AddToClassList("fish-card-desc");
                fishCard.Add(desc);
            }

            var selectBtn = new Button { text = "AR'da Gör" };
            selectBtn.AddToClassList("fish-card-btn");
            var captured = data;
            selectBtn.clicked += () => OnRewardFishCardSelected(captured);
            fishCard.Add(selectBtn);

            return fishCard;
        }

        private void OnRewardFishCardSelected(RewardFishCardData data)
        {
            if (data == null) return;

            if (data.prefab == null)
            {
                Debug.LogError($"[MainAppScreenController] Balık prefab'ı null (id: {data.fishId}). Seçim iptal.");
                return;
            }

            Debug.Log($"[MainAppScreenController] Balık kartı seçildi: {data.fishName} (id: {data.fishId})");
            SelectedFishSession.SelectFish(data.fishId, data.fishName, data.prefab);

            if (sceneLoader != null)
            {
                sceneLoader.LoadRealARScene();
            }
            else
            {
                Debug.LogError("[MainAppScreenController] sceneLoader null — AR sahnesine geçilemedi.");
            }
        }

        // ══════════════════════════════════════════════════════════
        //  Sıralama — aynı sınıftaki öğrencileri total_score'a göre listele
        // ══════════════════════════════════════════════════════════

        private async Task LoadLeaderboardAsync()
        {
            if (GameSession.Instance == null)
            {
                Debug.LogError("[MainAppScreenController] GameSession.Instance null — sıralama yüklenemedi.");
                return;
            }

            string classId = GameSession.Instance.ClassId;
            if (string.IsNullOrEmpty(classId))
            {
                Debug.LogError("[MainAppScreenController] GameSession.ClassId boş — sıralama yüklenemedi.");
                return;
            }

            if (SupabaseClient.Instance == null)
            {
                Debug.LogError("[MainAppScreenController] SupabaseClient.Instance null — sıralama yüklenemedi.");
                return;
            }

            string endpoint =
                $"users?class_id=eq.{classId}&role=eq.student&is_banned=eq.false&order=total_score.desc";

            string json;
            try
            {
                json = await SupabaseClient.Instance.GetAsync(endpoint);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[MainAppScreenController] Sıralama API hatası: " + e.Message);
                return;
            }

            if (json == null)
            {
                Debug.LogError("[MainAppScreenController] Sıralama API null döndü.");
                return;
            }

            UserDataList result;
            try
            {
                result = UserDataList.FromJson(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[MainAppScreenController] Sıralama parse hatası: " + e.Message);
                return;
            }

            // Ekran durumunu sıralama için hazırla
            if (_questionOverlay != null) _questionOverlay.style.display = DisplayStyle.None;

            if (_creaturesScroll == null)
            {
                Debug.LogError("[MainAppScreenController] _creaturesScroll null — sıralama gösterilemedi.");
                return;
            }

            _creaturesScroll.style.display = DisplayStyle.Flex;
            _creaturesScroll.Clear();

            if (_currentTabLabel != null) _currentTabLabel.text = "Sıralama";

            var card = new VisualElement();
            card.AddToClassList("leaderboard-card");

            var title = new Label { text = "Sınıf Sıralaması 🏆" };
            title.AddToClassList("leaderboard-title");
            card.Add(title);

            if (result?.items == null || result.items.Count == 0)
            {
                var empty = new Label { text = "Henüz sıralama kaydı bulunamadı." };
                empty.AddToClassList("leaderboard-empty");
                card.Add(empty);
            }
            else
            {
                int rank = 1;
                foreach (var user in result.items)
                {
                    if (user == null) continue;

                    var row = new VisualElement();
                    row.AddToClassList("leaderboard-row");

                    var rankLabel = new Label { text = $"{rank}." };
                    rankLabel.AddToClassList("leaderboard-rank");

                    string fullName = $"{user.first_name} {user.last_name}".Trim();
                    if (string.IsNullOrEmpty(fullName)) fullName = "(isimsiz)";
                    var nameLabel = new Label { text = fullName };
                    nameLabel.AddToClassList("leaderboard-name");

                    var scoreLabel = new Label { text = $"{user.total_score} Puan" };
                    scoreLabel.AddToClassList("leaderboard-score");

                    row.Add(rankLabel);
                    row.Add(nameLabel);
                    row.Add(scoreLabel);
                    card.Add(row);

                    rank++;
                }
            }

            var backBtn = new Button { text = "Sonuç Ekranına Dön" };
            backBtn.AddToClassList("leaderboard-back-btn");
            backBtn.clicked += ShowQuizResult;
            card.Add(backBtn);

            _creaturesScroll.Add(card);

            Debug.Log($"[MainAppScreenController] Sıralama yüklendi: {result?.items?.Count ?? 0} öğrenci.");
        }

        // ══════════════════════════════════════════════════════════
        //  Yardımcı: güvenli element bulucu
        // ══════════════════════════════════════════════════════════

        private T Require<T>(VisualElement root, string name) where T : VisualElement
        {
            var el = root.Q<T>(name);
            if (el == null)
                Debug.LogError($"[MainAppScreenController] '{name}' ({typeof(T).Name}) " +
                               "UXML'de bulunamadı — name attribute'unu ve UXML'i kontrol edin.");
            return el;
        }
        [System.Serializable]
        private class AnswerPayload
        {
            public string question_id;
            public string user_id;
            public bool is_correct;
            public string chosen_option;
        }

        [System.Serializable]
        private class RewardFishCardData
        {
            public string     fishId;
            public string     fishName;
            public string     description;
            public GameObject prefab;
            public Texture2D  previewImage;
        }
    }
}
