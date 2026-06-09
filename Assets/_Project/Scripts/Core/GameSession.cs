using UnityEngine;

public class GameSession : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────
    public static GameSession Instance { get; private set; }

    // ── Oturum bilgileri (Inspector'dan görülebilir) ──────────────
    [Header("Oturum Durumu")]
    [field: SerializeField] public bool   IsGuest     { get; private set; }
    [field: SerializeField] public string StudentName { get; private set; }
    [field: SerializeField] public string ClassId     { get; private set; }
    [field: SerializeField] public string ClassName    { get; private set; }
    [field: SerializeField] public int    Score       { get; private set; }

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
        IsGuest     = true;
        StudentName = "Misafir";
        ClassId     = string.Empty;
        ClassName   = string.Empty;
        Score       = 0;
    }

    public void SetClassSession(string studentName, string classId, string className)
    {
        IsGuest     = false;
        StudentName = studentName;
        ClassId     = classId;
        ClassName   = className;
        Score       = 0;
    }

    // ── Puanlama ──────────────────────────────────────────────────
    public void AddScore(int amount)
    {
        Score += amount;
    }

    // ── Sıfırlama ─────────────────────────────────────────────────
    public void ResetSession()
    {
        IsGuest     = false;
        StudentName = string.Empty;
        ClassId     = string.Empty;
        ClassName   = string.Empty;
        Score       = 0;
    }
}