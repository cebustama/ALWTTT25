using ALWTTT.Data;
using ALWTTT.Managers;
using ALWTTT.Musicians;
using System;
using System.Collections;
using UnityEngine;

namespace ALWTTT.Map
{
    /// <summary>
    /// Object provided to resolvers so they can act without hard-coding singletons.
    /// </summary>
    public class NodeResolveContext
    {
        public readonly SectorMapManager Manager;
        public readonly SectorMapData MapData;
        public readonly PersistentGameplayData Persistent;
        public readonly SectorMapState MapState;
        public readonly SectorMapVisual Visual;
        public readonly System.Random Rng;

        public NodeResolveContext(
            SectorMapManager manager,
            SectorMapData mapData,
            PersistentGameplayData persistent,
            SectorMapState mapState,
            SectorMapVisual visual,
            System.Random rng = null)
        {
            Manager = manager;
            MapData = mapData;
            Persistent = persistent;
            MapState = mapState;
            Visual = visual;
            Rng = rng ?? new System.Random();
        }

        // ---------- Helpers (stubbed; swap with real systems later) ----------

        public IEnumerator ShowRehearsalMenu(Action<string> onChoice)
        {
            // TODO hook your real Rehearsal canvas here
            Debug.Log("[Resolve] Rehearsal menu opened (Compose/Relax/BandTalk).");
            yield return new WaitForSeconds(0.1f);
            onChoice?.Invoke("Compose"); // default stub choice
        }

        public IEnumerator ShowRandomEvent(Action<Events.RandomEventOption> onChosen)
        {
            yield return Manager.ShowRandomEvent(onChosen);
        }

        public IEnumerator ShowRecruit(Action<MusicianCharacterData> onChosen)
        {
            yield return Manager.ShowRecruit(onChosen);
        }

        public void RefreshHUD()
        {
            Manager.RefreshHUD();
        }

        public IEnumerator RunGig(bool isBoss, Action<bool, int> onFinished)
        {
            // TODO jump to Gig scene and await result; int is fans gained
            Debug.Log(isBoss ? "[Resolve] Boss Gig..." : "[Resolve] Gig...");
            yield return new WaitForSeconds(0.1f);
            onFinished?.Invoke(true, isBoss ? 5 : 3); // stub reward
        }

        public void TravelToNextSector()
        {
            // TODO real sector advance (bump Persistent.CurrentSectorId, regenerate, save, etc.)
            Debug.Log("[Resolve] Traveling to next sector...");
            Manager.RegenerateImmediate();
        }

        public void GameOver()
        {
            // TODO real game over
            Debug.LogWarning("[Resolve] Game Over.");
        }
    }
}