// BandSetupManager.cs
using ALWTTT;
using ALWTTT.Data;
using ALWTTT.Managers;
using ALWTTT.Utils;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BandSetupManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private BandSetupCanvas setupCanvas;

    [Header("Nav")]
    [SerializeField] private SceneChanger sceneChanger;

    private GameManager GM => GameManager.Instance;

    private void Start()
    {
        var gd = GM.GameplayData;
        var pd = GM.PersistentGameplayData;

        // Build candidate pool X
        var all = gd.AllMusiciansList ?? new List<ALWTTT.Characters.Band.MusicianBase>();
        var allData = all
            .Select(m => m?.MusicianCharacterData)
            .Where(d => d != null)
            .ToList();

        int poolSize = gd.SetupPoolSize < 0 ? 
            allData.Count : 
            Mathf.Min(gd.SetupPoolSize, allData.Count);

        var pool = TakeRandom(allData, poolSize);

        // Ensure persistent starts clean for this flow
        pd.ResetBandForSetup(all);

        int pickCount = Mathf.Clamp(gd.SetupPickCount, 1, pool.Count);

        setupCanvas.Show(
            pool,
            pickCount,
            onConfirm: chosen => OnBandChosen(chosen));
    }

    private void OnBandChosen(List<MusicianCharacterData> chosen)
    {
        var pd = GM.PersistentGameplayData;

        foreach (var m in chosen)
            pd.AddMusicianToBand(m); // grants base cards + health + removes from available

        // Jump into the Sector Map
        sceneChanger.OpenMapScene();
    }

    private static List<T> TakeRandom<T>(List<T> src, int count)
    {
        if (src == null) return new List<T>();
        // simple in-place Fisher–Yates
        for (int i = 0; i < src.Count; i++)
        {
            int j = Random.Range(i, src.Count);
            (src[i], src[j]) = (src[j], src[i]);
        }
        return src.Take(Mathf.Clamp(count, 0, src.Count)).ToList();
    }
}
