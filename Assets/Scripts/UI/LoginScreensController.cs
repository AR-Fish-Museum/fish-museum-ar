using System.Threading.Tasks;
using FishMuseum.Core;
using FishMuseum.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace FishMuseum.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class LoginScreensController : MonoBehaviour
    {
        // ── Inspector referansları ────────────────────────────────────
        [SerializeField] private GameObject mainAppUIObject; // Yeni ana ekran objemiz
        [SerializeField] private SceneLoader sceneLoader;
        [SerializeField] private string      tableEndpoint = "classes";

        // ── Ekran grupları ────────────────────────────────────────────
        private VisualElement _mainMenu;
        private VisualElement _classSelection;
        private VisualElement _pinRegistration;

        // ── Sınıf listesi ─────────────────────────────────────────────
        private ScrollView _classListScroll;
        private Label      _classFeedback;

        // ── PIN ekranı ────────────────────────────────────────────────
        private Label     _selectedClassLabel;
        private TextField _fullNameInput;
        private TextField _pinInput;
        private Label     _pinFeedback;

        // ── Sabit butonlar ────────────────────────────────────────────
        private Button _btnJoinGroup;
        private Button _btnJoinGuest;
        private Button _btnConfirm;
        private Button _btnBackToMenu;
        private Button _btnBackToClass;

        // ── Durum ─────────────────────────────────────────────────────
        public ClassData CurrentClass { get; private set; }

        // ══════════════════════════════════════════════════════════════
        //  Start — UIDocument.Awake()'i bitirdikten SONRA çalışır;
        //  bu sayede rootVisualElement'in UXML ağacı dolu olur.
        //  (Awake içinde Q<>() çağrısı UIDocument hazır olmadan
        //   boş ağaç döndürerek tüm event bağlamalarını sessizce
        //   atlatıyordu — timing bug düzeltildi.)
        // ══════════════════════════════════════════════════════════════

        private void Start()
        {
            var doc = GetComponent<UIDocument>();
            if (doc == null)
            {
                Debug.LogError("[LoginScreensController] UIDocument bileşeni bulunamadı.");
                return;
            }

            var root = doc.rootVisualElement;
            if (root == null)
            {
                Debug.LogError("[LoginScreensController] rootVisualElement null. " +
                               "UIDocument 'Source Asset' alanına LoginScreens.uxml atandığından emin olun.");
                return;
            }

            // ── Ekran grupları ────────────────────────────────────────
            _mainMenu        = Require<VisualElement>(root, "main-menu");
            _classSelection  = Require<VisualElement>(root, "class-selection");
            _pinRegistration = Require<VisualElement>(root, "pin-registration");

            // ── Sınıf listesi ─────────────────────────────────────────
            _classListScroll = Require<ScrollView>(root, "class-list-scroll");
            _classFeedback   = Require<Label>(root, "class-feedback");

            // ── PIN ekranı ────────────────────────────────────────────
            _selectedClassLabel = Require<Label>(root, "selected-class-label");
            _fullNameInput      = Require<TextField>(root, "full-name-input");
            _pinInput           = Require<TextField>(root, "pin-input");
            _pinFeedback        = Require<Label>(root, "pin-feedback");

            // ── Sabit butonlar ────────────────────────────────────────
            _btnJoinGroup   = Require<Button>(root, "btn-join-group");
            _btnJoinGuest   = Require<Button>(root, "btn-join-guest");
            _btnConfirm     = Require<Button>(root, "btn-confirm");
            _btnBackToMenu  = Require<Button>(root, "btn-back-to-menu");
            _btnBackToClass = Require<Button>(root, "btn-back-to-class");

            // ── Event bağlantıları ────────────────────────────────────
            if (_btnJoinGroup != null)
                _btnJoinGroup.clicked += () =>
                {
                    Debug.Log("[LoginScreensController] btn-join-group tıklandı.");
                    _ = LoadActiveClassesAsync();
                };

            if (_btnJoinGuest   != null) _btnJoinGuest.clicked   += OnJoinAsGuest;
            if (_btnConfirm     != null) _btnConfirm.clicked     += OnConfirmPin;
            if (_btnBackToMenu  != null) _btnBackToMenu.clicked  += ShowMainMenu;
            if (_btnBackToClass != null) _btnBackToClass.clicked += ShowClassSelection;

            Debug.Log("[LoginScreensController] Start tamamlandı — tüm event'ler bağlandı.");

            ShowMainMenu();
        }

        // ══════════════════════════════════════════════════════════════
        //  Ekran geçişleri
        // ══════════════════════════════════════════════════════════════

        private void ShowScreen(VisualElement active)
        {
            _mainMenu?.AddToClassList("hidden");
            _classSelection?.AddToClassList("hidden");
            _pinRegistration?.AddToClassList("hidden");
            active?.RemoveFromClassList("hidden");
        }

        private void ShowMainMenu() => ShowScreen(_mainMenu);

        private void ShowClassSelection()
        {
            if (_classFeedback != null) _classFeedback.text = string.Empty;
            ShowScreen(_classSelection);
        }

        private void ShowPinRegistration(ClassData selected)
        {
            CurrentClass = selected;

            if (_selectedClassLabel != null)
                _selectedClassLabel.text = selected.class_name;

            if (_fullNameInput != null) _fullNameInput.value = string.Empty;
            if (_pinInput      != null) _pinInput.value      = string.Empty;
            if (_pinFeedback   != null) _pinFeedback.text    = string.Empty;

            ShowScreen(_pinRegistration);
        }

        // ══════════════════════════════════════════════════════════════
        //  Supabase — aktif sınıfları yükle
        // ══════════════════════════════════════════════════════════════

        private async Task LoadActiveClassesAsync()
        {
            Debug.Log("[LoginScreensController] Sınıflar çekiliyor...");

            // ── ScrollView guard ──────────────────────────────────────
            if (_classListScroll == null)
            {
                Debug.LogError("[LoginScreensController] _classListScroll null — " +
                               "UXML'de 'class-list-scroll' ScrollView bulunamadı.");
                return;
            }

            // ── SupabaseClient guard ──────────────────────────────────
            if (SupabaseClient.Instance == null)
            {
                Debug.LogError("[LoginScreensController] SupabaseClient.Instance null. " +
                               "SupabaseClient bileşeninin sahneye eklendiğinden ve " +
                               "Bootstrap/LoginScene hiyerarşisinde var olduğundan emin olun.");
                if (_classFeedback != null) _classFeedback.text = "Sunucu bağlantısı kurulamadı.";
                return;
            }

            _classListScroll.Clear();
            _classListScroll.style.flexGrow = 1;
            _classListScroll.style.width    = new StyleLength(StyleKeyword.Auto);

            if (_classFeedback != null) _classFeedback.text = "Sınıflar yükleniyor...";

            ShowClassSelection();

            // ── API isteği ────────────────────────────────────────────
            // Sadece tablo adı — rest/v1/ ön ekini SupabaseClient.BuildUrl ekler
            string endpoint = "classes?is_archived=eq.false&order=class_name.asc";
            Debug.Log("[LoginScreensController] Endpoint: " + endpoint);

            string json;
            try
            {
                json = await SupabaseClient.Instance.GetAsync(endpoint);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[LoginScreensController] API Hatası: " + e.Message +
                               "\nStackTrace: " + e.StackTrace);
                if (_classFeedback != null) _classFeedback.text = "Beklenmedik bir hata oluştu.";
                return;
            }

            if (json == null)
            {
                if (_classFeedback != null) _classFeedback.text = "Bağlantı hatası. Lütfen tekrar deneyin.";
                return;
            }

            Debug.Log($"[LoginScreensController] Ham JSON yanıtı: {json}");

            // ── JSON parse ────────────────────────────────────────────
            ClassDataList result;
            try
            {
                result = ClassDataList.FromJson(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[LoginScreensController] JSON parse hatası: " + e.Message +
                               "\nStackTrace: " + e.StackTrace);
                if (_classFeedback != null) _classFeedback.text = "Veri işlenirken hata oluştu.";
                return;
            }

            Debug.Log($"[LoginScreensController] Gelen sınıf sayısı: {result?.items?.Count ?? 0}");

            if (result?.items == null || result.items.Count == 0)
            {
                if (_classFeedback != null) _classFeedback.text = "Henüz aktif sınıf bulunamadı.";
                return;
            }

            if (_classFeedback != null) _classFeedback.text = string.Empty;

            // ── Buton oluşturma ───────────────────────────────────────
            foreach (var classData in result.items)
            {
                if (classData == null)
                {
                    Debug.LogWarning("[LoginScreensController] Listede null ClassData kaydı atlandı.");
                    continue;
                }

                var captured = classData;

                Button btn;
                try
                {
                    btn = new Button();
                    btn.text = captured.class_name ?? "(isimsiz sınıf)";
                    btn.clicked += () => ShowPinRegistration(captured);
                    btn.AddToClassList("class-item-btn");
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[LoginScreensController] Buton oluşturma hatası: " + e.Message +
                                   "\nStackTrace: " + e.StackTrace);
                    continue;
                }

                _classListScroll.Add(btn);
                Debug.Log($"[LoginScreensController] Sınıf butonu eklendi: {captured.class_name}");
            }

            Debug.Log($"[LoginScreensController] Toplam {result.items.Count} sınıf listelendi.");
        }

        // ══════════════════════════════════════════════════════════════
        //  Misafir girişi
        // ══════════════════════════════════════════════════════════════

        private void OnJoinAsGuest()
        {
            Debug.Log("[LoginScreensController] Misafir girişi yapıldı, balık galerisine geçiliyor...");

            // Oturumu misafir olarak işaretle (IsGuest=true, StudentName="Misafir", Score=0)
            if (GameSession.Instance != null)
            {
                GameSession.Instance.SetGuestSession();
            }
            else
            {
                Debug.LogError("[LoginScreensController] GameSession.Instance null — " +
                               "LoginScene içinde GameSession objesinin bulunduğundan emin olun. " +
                               "Misafir oturumu kaydedilemeden devam ediliyor.");
            }

            // Misafir → AR balık keşif sahnesine geç (MainApp_UI açılmaz, login UI kapatılmaz)
            if (sceneLoader != null)
            {
                sceneLoader.LoadFishGalleryScene();
            }
            else
            {
                Debug.LogError("[LoginScreensController] sceneLoader null — " +
                               "Inspector'dan SceneLoader referansı bağlanmamış. " +
                               "Misafir AR sahnesine geçemedi.");
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  PIN doğrulama
        // ══════════════════════════════════════════════════════════════

        private void OnConfirmPin()
        {
            string fullName = _fullNameInput?.value.Trim() ?? string.Empty;
            string pin      = _pinInput?.value.Trim()      ?? string.Empty;

            if (string.IsNullOrEmpty(fullName))
            {
                if (_pinFeedback != null) _pinFeedback.text = "Lütfen adınızı ve soyadınızı girin.";
                return;
            }

            if (string.IsNullOrEmpty(pin))
            {
                if (_pinFeedback != null) _pinFeedback.text = "Lütfen PIN kodunu girin.";
                return;
            }

            if (CurrentClass == null)
            {
                if (_pinFeedback != null) _pinFeedback.text = "Beklenmedik bir hata oluştu.";
                return;
            }

            if (pin != CurrentClass.student_pin)
            {
                if (_pinFeedback != null) _pinFeedback.text = "PIN hatalı. Lütfen tekrar deneyin.";
                if (_pinInput    != null) _pinInput.value   = string.Empty;
                return;
            }

            // PIN doğru — grup oturumunu kaydet
            if (GameSession.Instance != null)
            {
                string studentName = string.IsNullOrEmpty(fullName) ? "Öğrenci" : fullName;
                GameSession.Instance.SetClassSession(
                    studentName,
                    CurrentClass.id,
                    CurrentClass.class_name);
            }
            else
            {
                Debug.LogError("[LoginScreensController] GameSession.Instance null — " +
                               "LoginScene içinde GameSession objesinin bulunduğundan emin olun. " +
                               "Grup oturumu kaydedilemeden devam ediliyor.");
            }

            if (_pinFeedback != null) _pinFeedback.text = $"Hoş geldin, {fullName}!";

            // AR sahnesine geçilmez — aynı sahnede ana uygulama UI'ına geçilir
            if (mainAppUIObject != null)
            {
                mainAppUIObject.SetActive(true);  // Öğrenci/quiz ekranını aç
                this.gameObject.SetActive(false); // Login UI'ını (kendini) gizle
            }
            else
            {
                Debug.LogError("[LoginScreensController] mainAppUIObject null — " +
                               "Ana uygulama UI objesi Inspector'dan bağlanmamış. " +
                               "Grup girişi sonrası ekran açılamadı.");
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  Yardımcı: güvenli element bulucu
        // ══════════════════════════════════════════════════════════════

        private T Require<T>(VisualElement root, string name) where T : VisualElement
        {
            var el = root.Q<T>(name);
            if (el == null)
                Debug.LogError($"[LoginScreensController] '{name}' ({typeof(T).Name}) " +
                               "UXML'de bulunamadı — name attribute'unu ve UXML'i kontrol edin.");
            return el;
        }
    }
}
