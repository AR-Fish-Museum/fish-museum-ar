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

        // ── Durum ─────────────────────────────────────────────────
        private string       _currentSelectedCreatureId;
        private QuestionData _currentQuestion;
        private bool         _initialized;

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
            _currentTabLabel = Require<Label>(root, "current-tab-label");
            _creaturesScroll = Require<ScrollView>(root, "creatures-scroll-view");

            _btnCreatures   = Require<Button>(root, "btn-tab-creatures");
            _btnLeaderboard = Require<Button>(root, "btn-tab-leaderboard");
            _btnAbout       = Require<Button>(root, "btn-tab-about");

            _detailOverlay        = Require<VisualElement>(root, "creature-detail-overlay");
            _detailCreatureName   = Require<Label>(root, "detail-creature-name");
            _detailCreatureDesc   = Require<Label>(root, "detail-creature-desc");
            _detailCreatureWeight = Require<Label>(root, "detail-creature-weight");
            _btnBackToList        = Require<Button>(root, "btn-back-to-list");
            _btnFeedCreature      = Require<Button>(root, "btn-feed-creature");

            _questionOverlay   = Require<VisualElement>(root, "question-overlay");
            _questionText      = Require<Label>(root, "question-text");
            _questionFeedback  = Require<Label>(root, "question-feedback");
            _btnOptA           = Require<Button>(root, "btn-opt-a");
            _btnOptB           = Require<Button>(root, "btn-opt-b");
            _btnOptC           = Require<Button>(root, "btn-opt-c");
            _btnOptD           = Require<Button>(root, "btn-opt-d");
            _btnCloseQuestion  = Require<Button>(root, "btn-close-question");

            // ── Event bağlantıları (bir kez bağlanır) ─────────────
            if (_btnCreatures   != null) _btnCreatures.clicked   += () => SwitchTab(TAB_CREATURES);
            if (_btnLeaderboard != null) _btnLeaderboard.clicked += () => SwitchTab(TAB_LEADERBOARD);
            if (_btnAbout       != null) _btnAbout.clicked       += () => SwitchTab(TAB_ABOUT);

            if (_btnBackToList    != null) _btnBackToList.clicked    += HideCreatureDetail;
            if (_btnFeedCreature  != null) _btnFeedCreature.clicked  += () => _ = OnFeedCreature();
            if (_btnCloseQuestion != null) _btnCloseQuestion.clicked += HideQuestionOverlay;

            _initialized = true;
            Debug.Log("[MainAppScreenController] Start tamamlandı — referanslar ve eventler hazır.");

            // İlk ekrana git
            ResetToInitialState();
            _ = LoadCreaturesAsync();
        }

        // ══════════════════════════════════════════════════════════
        //  OnEnable — GameObject her SetActive(true) aldığında
        // ══════════════════════════════════════════════════════════

        private void OnEnable()
        {
            Debug.Log("[MainAppScreenController] Ekran aktifleşti.");

            // Start henüz çalışmadıysa _initialized false olur; o zaman Start halleder
            if (!_initialized) return;

            // Yeniden açılınca temiz duruma getir ve listeyi tazele
            ResetToInitialState();
            _ = LoadCreaturesAsync();
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

            // Her soru açılışında şıkları temizleyip yeniden doldur
            SetOptionButton(_btnOptA, "a", question.option_a);
            SetOptionButton(_btnOptB, "b", question.option_b);
            SetOptionButton(_btnOptC, "c", question.option_c);
            SetOptionButton(_btnOptD, "d", question.option_d);

            if (_questionFeedback != null)
            {
                _questionFeedback.text = string.Empty;
                _questionFeedback.RemoveFromClassList("feedback-correct");
                _questionFeedback.RemoveFromClassList("feedback-wrong");
            }

            if (_btnCloseQuestion != null) _btnCloseQuestion.text = "Kapat";

            _questionOverlay.RemoveFromClassList(CSS_HIDDEN);
            Debug.Log($"[MainAppScreenController] Soru gösteriliyor: {question.question_text}");
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

        private void CheckAnswer(string chosen)
        {
            if (_currentQuestion == null) return;

            string correct = _currentQuestion.correct_option?.ToLower() ?? string.Empty;
            bool   isRight = chosen == correct;

            // Tüm şıkları kilitle ve renk ver
            MarkOption(_btnOptA, "a", chosen, correct);
            MarkOption(_btnOptB, "b", chosen, correct);
            MarkOption(_btnOptC, "c", chosen, correct);
            MarkOption(_btnOptD, "d", chosen, correct);

            if (_questionFeedback != null)
            {
                _questionFeedback.RemoveFromClassList("feedback-correct");
                _questionFeedback.RemoveFromClassList("feedback-wrong");

                if (isRight)
                {
                    _questionFeedback.text = "Doğru! 🎉";
                    _questionFeedback.AddToClassList("feedback-correct");
                }
                else
                {
                    _questionFeedback.text = "Yanlış! 😢";
                    _questionFeedback.AddToClassList("feedback-wrong");
                }
            }

            if (_btnCloseQuestion != null) _btnCloseQuestion.text = "Devam Et";

            Debug.Log($"[MainAppScreenController] Seçilen: {chosen} | Doğru: {correct} | {(isRight ? "✓ Doğru" : "✗ Yanlış")}");
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
    }
}
