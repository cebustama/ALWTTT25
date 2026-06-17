using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.UI
{
    /// <summary>
    /// Handles the row of loop progress bars ("|||" style) shown during a jam.
    /// - BuildBars(loopCount): instantiates that many bar sections.
    /// - SetProgress(currentLoopIdx0, progress01): fills bars accordingly.
    /// </summary>
    public class LoopsTimerUI : MonoBehaviour
    {
        [Header("Template / Hierarchy")]
        [SerializeField] private RectTransform limitMarkerTemplate;
        [SerializeField] private RectTransform barSectionTemplate;
        [SerializeField] private Image fillImageInTemplate;
        [SerializeField] private RectTransform markerTemplate;

        // keep both the root clone and the fill image for each bar we spawn
        private struct LoopBarInstance
        {
            public RectTransform sectionRoot; // the cloned BarSection
            public Image fillImage;           // the cloned FillImage inside it
        }

        private readonly List<RectTransform> _spawnedLimitMarkers = new();
        private readonly List<LoopBarInstance> _bars = new();
        private readonly List<RectTransform> _spawnedMiddleMarkers = new();

        private void Awake()
        {
            // Hide templates so they don't render themselves
            if (limitMarkerTemplate != null)
                limitMarkerTemplate.gameObject.SetActive(false);

            if (barSectionTemplate != null)
                barSectionTemplate.gameObject.SetActive(false);

            if (markerTemplate != null)
                markerTemplate.gameObject.SetActive(false);
        }

        /// <summary>
        /// Destroy previously spawned bars (but NOT the template),
        /// then spawn 'loopCount' new bars from barSectionTemplate.
        /// </summary>
        public void BuildBars(int loopCount)
        {
            // Clear old bars
            foreach (var bar in _bars)
            {
                if (bar.sectionRoot != null)
                    Destroy(bar.sectionRoot.gameObject);
            }
            _bars.Clear();

            // Clear old in-between markers
            foreach (var m in _spawnedMiddleMarkers)
            {
                if (m != null)
                    Destroy(m.gameObject);
            }
            _spawnedMiddleMarkers.Clear();

            // Clear old limit markers
            foreach (var lm in _spawnedLimitMarkers)
            {
                if (lm != null)
                    Destroy(lm.gameObject);
            }
            _spawnedLimitMarkers.Clear();

            if (barSectionTemplate == null || fillImageInTemplate == null)
                return;

            // --- Spawn leading limit marker ---
            if (limitMarkerTemplate != null)
            {
                var startLimit = Instantiate(
                    limitMarkerTemplate,
                    transform,
                    worldPositionStays: false);

                startLimit.gameObject.SetActive(true);
                _spawnedLimitMarkers.Add(startLimit);
            }

            // --- Spawn each bar + an in-between marker after it ---
            for (int i = 0; i < loopCount; i++)
            {
                // Clone the whole BarSection under LoopsTimerArea
                var cloneSection = Instantiate(
                    barSectionTemplate,
                    transform,              // parent = LoopsTimerArea
                    worldPositionStays: false);

                cloneSection.gameObject.SetActive(true);

                // Find / clone FillImage inside this BarSection clone
                Image cloneFill = null;
                var allImgs = cloneSection.GetComponentsInChildren<Image>(includeInactive: true);
                foreach (var img in allImgs)
                {
                    if (img.name == fillImageInTemplate.name)
                    {
                        cloneFill = img;
                        break;
                    }
                }

                if (cloneFill == null)
                {
                    // fallback: manually copy the template FillImage
                    cloneFill = Instantiate(fillImageInTemplate, cloneSection);
                    cloneFill.gameObject.SetActive(true);
                }

                // Configure the fill bar for horizontal fill
                cloneFill.type = Image.Type.Filled;
                cloneFill.fillMethod = Image.FillMethod.Horizontal;
                cloneFill.fillOrigin = 0; // fill left-to-right
                cloneFill.fillAmount = 0f;

                _bars.Add(new LoopBarInstance
                {
                    sectionRoot = cloneSection,
                    fillImage = cloneFill
                });

                // If this is NOT the last bar, spawn a marker after it
                // so the final layout becomes: Bar | Marker | Bar | Marker | Bar
                if (i < loopCount - 1 && markerTemplate != null)
                {
                    var markerClone = Instantiate(
                        markerTemplate,
                        transform,
                        worldPositionStays: false);

                    markerClone.gameObject.SetActive(true);
                    _spawnedMiddleMarkers.Add(markerClone);
                }
            }

            // --- Spawn trailing limit marker ---
            if (limitMarkerTemplate != null)
            {
                var endLimit = Instantiate(
                    limitMarkerTemplate,
                    transform,
                    worldPositionStays: false);

                endLimit.gameObject.SetActive(true);
                _spawnedLimitMarkers.Add(endLimit);
            }
        }

        /// <summary>
        /// Update current loop visual progress.
        /// loopIndex0 = which loop (0-based) is currently playing.
        /// loopProgress01 = 0..1 progress for that loop.
        ///
        /// Bars before loopIndex0 are forced to filled (1),
        /// current loopIndex0 uses loopProgress01,
        /// bars after are 0.
        /// </summary>
        public void SetProgress(int loopIndex0, float loopProgress01)
        {
            for (int i = 0; i < _bars.Count; i++)
            {
                var bar = _bars[i];
                if (bar.fillImage == null) continue;

                if (i < loopIndex0)
                {
                    // Past loops: fully filled
                    bar.fillImage.fillAmount = 1f;
                }
                else if (i == loopIndex0)
                {
                    // Current loop: partial fill
                    bar.fillImage.fillAmount = Mathf.Clamp01(loopProgress01);
                }
                else
                {
                    // Future loops: empty
                    bar.fillImage.fillAmount = 0f;
                }
            }
        }

        /// <summary>
        /// Optional convenience to zero everything out without rebuilding.
        /// </summary>
        public void ClearProgress()
        {
            SetProgress(0, 0f);
        }

        /// <summary>
        /// Show/hide all spawned bars and markers
        /// </summary>
        public void SetBarsVisible(bool visible)
        {
            foreach (var bar in _bars)
            {
                if (bar.sectionRoot != null)
                    bar.sectionRoot.gameObject.SetActive(visible);
            }

            foreach (var m in _spawnedMiddleMarkers)
            {
                if (m != null)
                    m.gameObject.SetActive(visible);
            }

            foreach (var lm in _spawnedLimitMarkers)
            {
                if (lm != null)
                    lm.gameObject.SetActive(visible);
            }
        }
    }
}