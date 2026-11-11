using ALWTTT.Data;
using ALWTTT.Managers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.UI
{
    /// <summary>
    /// [S5 NAVE-1] Cohesion as hearts outside the gig (linear map, ship hub).
    /// Same rendering as GigCanvas.SetCohesion (COH-1 / GIG-UI-1): one pip per
    /// point up to MaxCohesion, more if cohesion exceeds it; empty pips use the
    /// empty sprite, or the full sprite tinted. Put it on the CohesionPips root
    /// (the one with the Layout Group) copied from GigCanvas.
    /// Debt: GigCanvas keeps its own copy of this logic; unify after the demo.
    /// </summary>
    public class CohesionPipsView : MonoBehaviour
    {
        private const string Tag = "[S5]";

        [SerializeField, Tooltip("Parent of the pips (has the Layout Group). Empty = this object.")]
        private RectTransform pipRoot;
        [SerializeField] private Image pipPrefab;
        [SerializeField] private Sprite pipFull;
        [SerializeField] private Sprite pipEmpty;
        [SerializeField] private Color pipEmptyTint = new Color(1f, 1f, 1f, 0.25f);

        private readonly List<Image> _pips = new List<Image>();

        private void OnEnable()
        {
            if (pipRoot == null) pipRoot = (RectTransform)transform;

            // Pips copied over from the gig prefab would duplicate the row.
            for (int i = pipRoot.childCount - 1; i >= 0; i--)
                Destroy(pipRoot.GetChild(i).gameObject);
            _pips.Clear();

            PersistentGameplayData.BandCohesionChanged += Refresh;
            var gm = GameManager.Instance;
            var pd = gm != null ? gm.PersistentGameplayData : null;
            if (pd != null)
            {
                Refresh(pd.BandCohesion);
                Debug.Log($"{Tag} Cohesion hearts | {pd.BandCohesion}/{MaxCohesion} | {gameObject.scene.name}");
            }
        }

        private void OnDisable()
        {
            PersistentGameplayData.BandCohesionChanged -= Refresh;
        }

        private static int MaxCohesion
        {
            get
            {
                var gm = GameManager.Instance;
                return gm != null && gm.GameplayData != null ? gm.GameplayData.MaxCohesion : 10;
            }
        }

        public void Refresh(int value)
        {
            if (pipRoot == null || pipPrefab == null) return;

            int count = Mathf.Max(MaxCohesion, value);
            while (_pips.Count < count)
                _pips.Add(Instantiate(pipPrefab, pipRoot));

            Color baseColor = pipPrefab.color;
            for (int i = 0; i < _pips.Count; i++)
            {
                var pip = _pips[i];
                if (pip == null) continue;
                pip.gameObject.SetActive(i < count);
                pip.raycastTarget = false;
                bool full = i < value;
                if (full)
                {
                    if (pipFull != null) pip.sprite = pipFull;
                    pip.color = baseColor;
                }
                else if (pipEmpty != null)
                {
                    pip.sprite = pipEmpty;
                    pip.color = baseColor;
                }
                else
                {
                    if (pipFull != null) pip.sprite = pipFull;
                    pip.color = baseColor * pipEmptyTint;
                }
            }
        }
    }
}
