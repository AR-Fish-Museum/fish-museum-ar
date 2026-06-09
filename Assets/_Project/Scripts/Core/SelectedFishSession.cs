using UnityEngine;

/// <summary>
/// Fish Gallery'de seçilen balığın bilgisini sahneler arası taşıyan
/// basit static oturum. AR sahnesi bu değerleri okuyarak doğru balığı
/// spawn edebilir. (Şimdilik MonoBehaviour değil; static yeterli.)
/// </summary>
public static class SelectedFishSession
{
    public static string     SelectedFishId     { get; private set; }
    public static string     SelectedFishName   { get; private set; }
    public static GameObject SelectedFishPrefab { get; private set; }

    public static void SelectFish(string fishId, string fishName, GameObject prefab)
    {
        SelectedFishId     = fishId;
        SelectedFishName   = fishName;
        SelectedFishPrefab = prefab;
    }

    public static void Clear()
    {
        SelectedFishId     = null;
        SelectedFishName   = null;
        SelectedFishPrefab = null;
    }
}