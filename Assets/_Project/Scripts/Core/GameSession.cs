using UnityEngine;

public class GameSession : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────
    public static GameSession Instance { get; private set; }

    // ── Oturum bilgileri (Inspector'dan görülebilir) ──────────────
    [Header("Oturum Durumu")]
    [field: SerializeField] public bool IsGuest { get; private set; }
    [field: SerializeField] public string StudentName { get; private set; }
    [field: SerializeField] public string ClassId { get; private set; }
    [field: SerializeField] public string ClassName { get; private set; }
    [field: SerializeField] public int Score { get; private set; }

    // ── Quiz istatistikleri ───────────────────────────────────────
    [Header("Quiz Durumu")]
    [field: SerializeField] public string UserId { get; private set; }
    [field: SerializeField] public int CorrectCount { get; private set; }
    [field: SerializeField] public int WrongCount { get; private set; }
    [field: SerializeField] public int AnsweredQuestionCount { get; private set; }

    [Header("Sahne Dönüş Durumu")]
    [field: SerializeField] public bool ShouldShowQuizResultOnReturn { get; private set; }
    [field: SerializeField] public int LastQuizTotalQuestionCount { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Oturum kurma metotları ────────────────────────────────────
    public void SetGuestSession()
    {
        IsGuest = true;
        StudentName = "Misafir";
        ClassId = string.Empty;
        ClassName = string.Empty;
        Score = 0;

        UserId = string.Empty;
        CorrectCount = 0;
        WrongCount = 0;
        AnsweredQuestionCount = 0;
    }

    public void SetClassSession(string studentName, string classId, string className)
    {
        IsGuest = false;
        StudentName = studentName;
        ClassId = classId;
        ClassName = className;
        Score = 0;

        UserId = string.Empty;
        CorrectCount = 0;
        WrongCount = 0;
        AnsweredQuestionCount = 0;
    }

    // ── Kullanıcı kimliği ─────────────────────────────────────────
    public void SetUserId(string userId)
    {
        UserId = userId;
    }

    // ── Puanlama ──────────────────────────────────────────────────
    public void AddScore(int amount)
    {
        Score += amount;
    }

    // ── Cevap kaydı ───────────────────────────────────────────────
    public void RegisterAnswer(bool isCorrect)
    {
        AnsweredQuestionCount++;

        if (isCorrect)
        {
            CorrectCount++;
            AddScore(10);
        }
        else
        {
            WrongCount++;
        }
    }

    public void SaveQuizResultState(int totalQuestionCount)
    {
        LastQuizTotalQuestionCount = Mathf.Max(0, totalQuestionCount);
    }

    public void RequestShowQuizResultOnReturn()
    {
        ShouldShowQuizResultOnReturn = true;
        Debug.Log("[GameSession] Quiz sonuç ekranına dönüş istendi.");
    }

    public bool ConsumeShowQuizResultOnReturn()
    {
        bool value = ShouldShowQuizResultOnReturn;
        ShouldShowQuizResultOnReturn = false;
        return value;
    }

    public bool PeekShowQuizResultOnReturn()
    {
        return ShouldShowQuizResultOnReturn;
    }

    // ── Sıfırlama ─────────────────────────────────────────────────
    public void ResetSession()
    {
        IsGuest = false;
        StudentName = string.Empty;
        ClassId = string.Empty;
        ClassName = string.Empty;
        Score = 0;

        UserId = string.Empty;
        CorrectCount = 0;
        WrongCount = 0;
        AnsweredQuestionCount = 0;
        ShouldShowQuizResultOnReturn = false;
        LastQuizTotalQuestionCount = 0;
    }
}