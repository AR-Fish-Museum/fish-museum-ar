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
        [SerializeField] private GameObject mainAppUIObject;
        [SerializeField] private SceneLoader sceneLoader;
        [SerializeField] private string      tableEndpoint = "classes";
        [SerializeField] private string      teacherMasterPin = "1234";

        // ── Ekran grupları ────────────────────────────────────────────
        private VisualElement _mainMenu;
        private VisualElement _classSelection;
        private VisualElement _pinRegistration;
        private VisualElement _teacherPin;
        private VisualElement _teacherPanel;

        // ── Sınıf listesi ─────────────────────────────────────────────
        private ScrollView _classListScroll;
        private Label      _classFeedback;

        // ── Öğrenci giriş ekranı ──────────────────────────────────────
        private TextField _fullNameInput;
        private TextField _pinInput;
        private Label     _pinFeedback;

        // ── Sabit butonlar ────────────────────────────────────────────
        private Button _btnRoleStudent;
        private Button _btnRoleTeacher;
        private Button _btnConfirm;
        private Button _btnBackToMenu;
        private Button _btnStudentBack;

        private TextField _teacherPinInput;
        private Label     _teacherPinFeedback;
        private Button    _btnTeacherConfirm;
        private Button    _btnTeacherBack;
        private Button    _btnTeacherPanelBack;

        // ── Durum ─────────────────────────────────────────────────────
        public ClassData CurrentClass { get; private set; }

        // Öğrenci giriş ekranında girilen geçici değerler
        private string _pendingStudentFullName;
        private string _pendingStudentPin;

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
                Debug.LogError("[LoginScreensController] rootVisualElement null.");
                return;
            }

            // ── Ekran grupları ────────────────────────────────────────
            _mainMenu        = Require<VisualElement>(root, "main-menu");
            _classSelection  = Require<VisualElement>(root, "class-selection");
            _pinRegistration = Require<VisualElement>(root, "pin-registration");
            _teacherPin      = Require<VisualElement>(root, "teacher-pin");
            _teacherPanel    = Require<VisualElement>(root, "teacher-panel");

            // ── Sınıf listesi ─────────────────────────────────────────
            _classListScroll = Require<ScrollView>(root, "class-list-scroll");
            _classFeedback   = Require<Label>(root, "class-feedback");

            // ── Öğrenci giriş ekranı ──────────────────────────────────
            _fullNameInput = Require<TextField>(root, "full-name-input");
            _pinInput      = Require<TextField>(root, "pin-input");
            _pinFeedback   = Require<Label>(root, "pin-feedback");

            // ── Sabit butonlar ────────────────────────────────────────
            _btnRoleStudent = Require<Button>(root, "btn-role-student");
            _btnRoleTeacher = Require<Button>(root, "btn-role-teacher");
            _btnConfirm     = Require<Button>(root, "btn-confirm");
            _btnBackToMenu  = Require<Button>(root, "btn-back-to-menu");
            _btnStudentBack = Require<Button>(root, "btn-student-back");

            _teacherPinInput     = Require<TextField>(root, "teacher-pin-input");
            _teacherPinFeedback  = Require<Label>(root, "teacher-pin-feedback");
            _btnTeacherConfirm   = Require<Button>(root, "btn-teacher-confirm");
            _btnTeacherBack      = Require<Button>(root, "btn-teacher-back");
            _btnTeacherPanelBack = Require<Button>(root, "btn-teacher-panel-back");

            // ── Event bağlantıları ────────────────────────────────────
            if (_btnRoleStudent != null) _btnRoleStudent.clicked += ShowStudentLogin;
            if (_btnRoleTeacher != null) _btnRoleTeacher.clicked += ShowTeacherPin;

            if (_btnConfirm     != null) _btnConfirm.clicked     += OnStudentContinue;
            if (_btnBackToMenu  != null) _btnBackToMenu.clicked  += ShowMainMenu;
            if (_btnStudentBack != null) _btnStudentBack.clicked += ShowMainMenu;

            if (_btnTeacherConfirm   != null) _btnTeacherConfirm.clicked   += OnTeacherConfirm;
            if (_btnTeacherBack      != null) _btnTeacherBack.clicked      += ShowMainMenu;
            if (_btnTeacherPanelBack != null) _btnTeacherPanelBack.clicked += ShowMainMenu;

            Debug.Log("[LoginScreensController] Start tamamlandı.");
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
            _teacherPin?.AddToClassList("hidden");
            _teacherPanel?.AddToClassList("hidden");
            active?.RemoveFromClassList("hidden");
        }

        private void ShowMainMenu() => ShowScreen(_mainMenu);

        private void ShowClassSelection()
        {
            if (_classFeedback != null) _classFeedback.text = string.Empty;
            ShowScreen(_classSelection);
        }

        // ── Öğrenci giriş ekranı (önce isim + PIN) ────────────────────
        private void ShowStudentLogin()
        {
            if (_fullNameInput != null) _fullNameInput.value = string.Empty;
            if (_pinInput      != null) _pinInput.value      = string.Empty;
            if (_pinFeedback   != null) _pinFeedback.text    = string.Empty;

            _pendingStudentFullName = string.Empty;
            _pendingStudentPin      = string.Empty;

            ShowScreen(_pinRegistration);
        }

        private void OnStudentContinue()
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

            _pendingStudentFullName = fullName;
            _pendingStudentPin      = pin;

            _ = LoadActiveClassesAsync();
        }

        // ── Öğretmen akışı ────────────────────────────────────────────
        private void ShowTeacherPin()
        {
            if (_teacherPinInput    != null) _teacherPinInput.value   = string.Empty;
            if (_teacherPinFeedback != null) _teacherPinFeedback.text = string.Empty;
            ShowScreen(_teacherPin);
        }

        private void ShowTeacherPanel() => ShowScreen(_teacherPanel);

        private void OnTeacherConfirm()
        {
            string pin = _teacherPinInput?.value.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(pin))
            {
                if (_teacherPinFeedback != null) _teacherPinFeedback.text = "Lütfen PIN kodunu girin.";
                return;
            }

            if (pin != teacherMasterPin)
            {
                if (_teacherPinFeedback != null) _teacherPinFeedback.text = "PIN hatalı. Lütfen tekrar deneyin.";
                if (_teacherPinInput    != null) _teacherPinInput.value   = string.Empty;
                return;
            }

            Debug.Log("[LoginScreensController] Öğretmen girişi başarılı.");
            ShowTeacherPanel();
        }

        // ══════════════════════════════════════════════════════════════
        //  Supabase — aktif sınıfları yükle (PIN ön kontrolü ile)
        // ══════════════════════════════════════════════════════════════

        private async Task LoadActiveClassesAsync()
        {
            if (_classListScroll == null)
            {
                Debug.LogError("[LoginScreensController] _classListScroll null.");
                return;
            }

            if (SupabaseClient.Instance == null)
            {
                Debug.LogError("[LoginScreensController] SupabaseClient.Instance null.");
                if (_pinFeedback != null) _pinFeedback.text = "Sunucu bağlantısı kurulamadı.";
                return;
            }

            _classListScroll.Clear();
            _classListScroll.style.flexGrow = 1;
            _classListScroll.style.width    = new StyleLength(StyleKeyword.Auto);

            if (_pinFeedback != null) _pinFeedback.text = "Sınıflar yükleniyor...";

            string endpoint = "classes?is_archived=eq.false&order=class_name.asc";

            string json;
            try
            {
                json = await SupabaseClient.Instance.GetAsync(endpoint);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[LoginScreensController] API Hatası: " + e.Message);
                if (_pinFeedback != null) _pinFeedback.text = "Beklenmedik bir hata oluştu.";
                return;
            }

            if (json == null)
            {
                if (_pinFeedback != null) _pinFeedback.text = "Bağlantı hatası. Lütfen tekrar deneyin.";
                return;
            }

            ClassDataList result;
            try
            {
                result = ClassDataList.FromJson(json);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[LoginScreensController] JSON parse hatası: " + e.Message);
                if (_pinFeedback != null) _pinFeedback.text = "Veri işlenirken hata oluştu.";
                return;
            }

            if (result?.items == null || result.items.Count == 0)
            {
                if (_pinFeedback != null) _pinFeedback.text = "Henüz aktif sınıf bulunamadı.";
                return;
            }

            // ── PIN ön kontrolü: en az bir sınıfın student_pin'i eşleşmeli ──
            bool anyPinMatch = false;
            foreach (var c in result.items)
            {
                if (c != null && c.student_pin == _pendingStudentPin)
                {
                    anyPinMatch = true;
                    break;
                }
            }

            if (!anyPinMatch)
            {
                if (_pinFeedback != null) _pinFeedback.text = "Öğrenci PIN hatalı.";
                return;
            }

            if (_pinFeedback != null) _pinFeedback.text = string.Empty;

            // ── Sınıf butonları ───────────────────────────────────────
            foreach (var classData in result.items)
            {
                if (classData == null) continue;

                var captured = classData;
                Button btn;
                try
                {
                    btn = new Button();
                    btn.text = captured.class_name ?? "(isimsiz sınıf)";
                    btn.clicked += () => OnStudentSelectedClass(captured);
                    btn.AddToClassList("class-item-btn");
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[LoginScreensController] Buton oluşturma hatası: " + e.Message);
                    continue;
                }

                _classListScroll.Add(btn);
            }

            ShowClassSelection();
        }

        // ══════════════════════════════════════════════════════════════
        //  Sınıf seçimi — PIN sınıfla eşleşiyorsa quiz başlat
        // ══════════════════════════════════════════════════════════════

        private void OnStudentSelectedClass(ClassData selected)
        {
            CurrentClass = selected;

            string pin = string.IsNullOrEmpty(_pendingStudentPin)
                ? (_pinInput?.value.Trim() ?? string.Empty)
                : _pendingStudentPin;

            if (selected == null || selected.student_pin != pin)
            {
                if (_classFeedback != null)
                    _classFeedback.text = "Bu sınıfın PIN'i girdiğiniz PIN ile eşleşmiyor.";
                return;
            }

            // PIN eşleşti — mevcut kayıt/oturum/quiz başlatma akışını çalıştır
            OnConfirmPin();
        }

        // ══════════════════════════════════════════════════════════════
        //  Misafir girişi (dead code — menüden kaldırıldı, dokunma)
        // ══════════════════════════════════════════════════════════════

        private void OnJoinAsGuest()
        {
            Debug.Log("[LoginScreensController] Misafir girişi yapıldı, balık galerisine geçiliyor...");

            if (GameSession.Instance != null)
            {
                GameSession.Instance.SetGuestSession();
            }
            else
            {
                Debug.LogError("[LoginScreensController] GameSession.Instance null.");
            }

            if (sceneLoader != null)
            {
                sceneLoader.LoadFishGalleryScene();
            }
            else
            {
                Debug.LogError("[LoginScreensController] sceneLoader null.");
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  PIN doğrulama + öğrenci kaydı + ana ekrana geçiş (değişmedi)
        // ══════════════════════════════════════════════════════════════

        private async void OnConfirmPin()
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
                Debug.LogError("[LoginScreensController] GameSession.Instance null.");
            }

            try
            {
                string firstName;
                string lastName;

                int spaceIndex = fullName.IndexOf(' ');
                if (spaceIndex > 0)
                {
                    firstName = fullName.Substring(0, spaceIndex).Trim();
                    lastName  = fullName.Substring(spaceIndex + 1).Trim();
                    if (string.IsNullOrEmpty(lastName)) lastName = "-";
                }
                else
                {
                    firstName = fullName;
                    lastName  = "-";
                }

                if (SupabaseClient.Instance == null)
                {
                    Debug.LogError("[LoginScreensController] SupabaseClient.Instance null — öğrenci kaydı oluşturulamadı.");
                }
                else
                {
                    var payload = new NewStudentPayload
                    {
                        class_id   = CurrentClass.id,
                        first_name = firstName,
                        last_name  = lastName,
                        role       = "student"
                    };

                    string json = JsonUtility.ToJson(payload);
                    string response = await SupabaseClient.Instance.PostAsync("users", json);

                    if (string.IsNullOrEmpty(response))
                    {
                        Debug.LogError("[LoginScreensController] users POST boş yanıt döndü.");
                    }
                    else
                    {
                        UserDataList result = UserDataList.FromJson(response);
                        if (result?.items != null && result.items.Count > 0)
                        {
                            string newUserId = result.items[0].id;
                            GameSession.Instance?.SetUserId(newUserId);
                            Debug.Log($"[LoginScreensController] Öğrenci kaydı oluşturuldu. UserId: {newUserId}");
                        }
                        else
                        {
                            Debug.LogError("[LoginScreensController] users POST yanıtı parse edilemedi: " + response);
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("[LoginScreensController] Öğrenci kaydı sırasında hata: " + e.Message);
            }

            if (_pinFeedback != null) _pinFeedback.text = $"Hoş geldin, {fullName}!";

            if (mainAppUIObject != null)
            {
                mainAppUIObject.SetActive(true);
                this.gameObject.SetActive(false);
            }
            else
            {
                Debug.LogError("[LoginScreensController] mainAppUIObject null.");
            }
        }

        // ══════════════════════════════════════════════════════════════
        //  Yardımcı
        // ══════════════════════════════════════════════════════════════

        private T Require<T>(VisualElement root, string name) where T : VisualElement
        {
            var el = root.Q<T>(name);
            if (el == null)
                Debug.LogError($"[LoginScreensController] '{name}' ({typeof(T).Name}) UXML'de bulunamadı.");
            return el;
        }

        [System.Serializable]
        private class NewStudentPayload
        {
            public string class_id;
            public string first_name;
            public string last_name;
            public string role;
        }
    }
}