using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Underwater life deluxe paketindeki materyalleri URP'ye uyarlar.
/// Editor-only; runtime'a hiç dokunmaz. Telefonda pembe görünen
/// (shader uyumsuz) materyalleri URP/Lit'e çevirir, texture ve rengi korur.
/// </summary>
public static class UnderwaterMaterialFixer
{
    private const string TargetFolder = "Assets/Underwater life deluxe";
    private const string MenuPath     = "Tools/AR Balık Müzesi/Fix Underwater Materials";

    [MenuItem(MenuPath)]
    public static void FixMaterials()
    {
        Shader urpLit   = Shader.Find("Universal Render Pipeline/Lit");
        Shader standard = Shader.Find("Standard");
        Shader target   = urpLit != null ? urpLit : standard;

        if (target == null)
        {
            Debug.LogError("[UnderwaterMaterialFixer] Ne 'Universal Render Pipeline/Lit' " +
                           "ne de 'Standard' shader bulunamadı. İşlem iptal edildi.");
            return;
        }

        if (!AssetDatabase.IsValidFolder(TargetFolder))
        {
            Debug.LogError($"[UnderwaterMaterialFixer] Klasör bulunamadı: {TargetFolder}");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { TargetFolder });
        Debug.Log($"[UnderwaterMaterialFixer] {guids.Length} material bulundu. Shader hedefi: {target.name}");

        int fixedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) continue;

            // ── Mevcut texture ve rengi yakala (shader değişmeden önce) ──
            Texture mainTex = null;
            if (mat.HasProperty("_BaseMap") && mat.GetTexture("_BaseMap") != null)
                mainTex = mat.GetTexture("_BaseMap");
            else if (mat.HasProperty("_MainTex") && mat.GetTexture("_MainTex") != null)
                mainTex = mat.GetTexture("_MainTex");
            else if (mat.mainTexture != null)
                mainTex = mat.mainTexture;

            bool  hasColor = false;
            Color color    = Color.white;
            if (mat.HasProperty("_BaseColor"))
            {
                color = mat.GetColor("_BaseColor");
                hasColor = true;
            }
            else if (mat.HasProperty("_Color"))
            {
                color = mat.GetColor("_Color");
                hasColor = true;
            }

            // ── Shader'ı değiştir ──
            mat.shader = target;

            // ── Texture'ı geri ata ──
            if (mainTex != null)
            {
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", mainTex);
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", mainTex);
            }

            // ── Rengi geri ata ──
            if (hasColor)
            {
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
                if (mat.HasProperty("_Color"))     mat.SetColor("_Color", color);
            }

            EditorUtility.SetDirty(mat);
            fixedCount++;
            Debug.Log($"[UnderwaterMaterialFixer] Düzeltildi: {path}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[UnderwaterMaterialFixer] Tamamlandı. {fixedCount} material {target.name} " +
                  "shader'ına dönüştürüldü.");
    }
}