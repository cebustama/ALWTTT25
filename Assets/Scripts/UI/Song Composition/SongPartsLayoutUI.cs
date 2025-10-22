using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.UI
{
    public class SongPartsLayoutUI : MonoBehaviour
    {
        [SerializeField] private Transform musicianIconsRoot;
        [SerializeField] private Transform partsRoot;

        [SerializeField] private Image musicianIconTemplate;
        [SerializeField] private SongPartElementUI partPrefab;

        public Transform ContentRoot => partsRoot != null ? partsRoot : transform;
        public Image MusicianIconTemplate => musicianIconTemplate;
        public Transform MusicianIconsRoot => musicianIconsRoot;
        public SongPartElementUI PartPrefab => partPrefab;

        private void Awake()
        {
            // Make templates inactive so they don't show as sample rows
            if (musicianIconTemplate != null)
                musicianIconTemplate.gameObject.SetActive(false);
        }
    }
}