using UnityEngine;

/// <summary>
/// Fish Gallery kartına eklenen buton davranışı. Karta tıklanınca
/// seçili balığı SelectedFishSession'a yazar ve AR sahnesine geçer.
/// </summary>
public class FishGalleryCardButton : MonoBehaviour
{
    [Header("Balık Bilgisi")]
    [SerializeField] private string fishId;
    [SerializeField] private string fishName;
    [SerializeField] private GameObject fishPrefab;

    [Header("Sahne Geçişi")]
    [SerializeField] private SceneLoader sceneLoader;

    /// <summary>
    /// Kart tıklandığında çağrılır. (Button'ın OnClick event'ine bağlanmalı.)
    /// </summary>
    public void OnCardClicked()
    {
        if (fishPrefab == null)
        {
            Debug.LogError($"[FishGalleryCardButton] fishPrefab null (fishId: {fishId}) — " +
                           "Inspector'dan balık prefab'ı bağlanmamış. Seçim yine de kaydediliyor.");
        }

        SelectedFishSession.SelectFish(fishId, fishName, fishPrefab);
        Debug.Log($"[FishGalleryCardButton] Balık seçildi: {fishName} (id: {fishId})");

        if (sceneLoader != null)
        {
            sceneLoader.LoadRealARScene();
        }
        else
        {
            Debug.LogError("[FishGalleryCardButton] sceneLoader null — " +
                           "Inspector'dan SceneLoader referansı bağlanmamış. AR sahnesine geçilemedi.");
        }
    }
}