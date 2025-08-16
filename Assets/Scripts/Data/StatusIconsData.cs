using ALWTTT.Enums;
using ALWTTT.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ALWTTT.Data
{
    [CreateAssetMenu(fileName = "Status Icons", menuName = "ALWTTT/Containers/StatusIcons")]
    public class StatusIconsData : ScriptableObject
    {
        [SerializeField] private StatusIconBase statusIconBasePrefab;
        [SerializeField] private List<StatusIconData> statusIconList;

        public StatusIconBase StatusIconBasePrefab => statusIconBasePrefab;
        public List<StatusIconData> StatusIconList => statusIconList;
    }

    [Serializable]
    public class StatusIconData
    {
        [SerializeField] private StatusType iconStatus;
        [SerializeField] private Sprite iconSprite;
        [SerializeField] private List<SpecialKeywords> specialKeywords;
        public StatusType IconStatus => iconStatus;
        public Sprite IconSprite => iconSprite;
        public List<SpecialKeywords> SpecialKeywords => specialKeywords;
    }
}