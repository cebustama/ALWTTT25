using ALWTTT.Enums;
using ALWTTT.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ALWTTT.Managers
{
    public class FxManager : MonoBehaviour
    {
        public FxManager() { }

        public static FxManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private List<FxBundle> fxList;

        [Header("Floating Text")]
        [SerializeField] private FloatingText floatingTextPrefab;

        public Dictionary<FxType, GameObject> FxDict { get; private set; } =
            new Dictionary<FxType, GameObject>();
        public List<FxBundle> FxList => fxList;

        #region Setup
        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                transform.parent = null;
                Instance = this;
                DontDestroyOnLoad(gameObject);
                for (int i = 0; i < Enum.GetValues(typeof(FxType)).Length; i++)
                {
                    FxDict.Add(
                        (FxType)i, 
                        FxList.FirstOrDefault(x => x.FxType == (FxType)i)?.FxPrefab
                    );
                }
            }
        }
        #endregion

        public void SpawnFloatingText(
            Transform targetTransform, string text, int xDir = 0, int yDir = -1)
        {
            var cloneText = Instantiate(
                floatingTextPrefab, targetTransform.position, Quaternion.identity);

            if (xDir == 0) xDir = Random.value >= 0.5f ? 1 : -1;

            cloneText.PlayText(text, xDir, yDir);
        }

        public void PlayFx(Transform targetTransform, FxType targetFx)
        {
            Instantiate(FxDict[targetFx], targetTransform);
        }
    }

    [Serializable]
    public class FxBundle
    {
        [SerializeField] private FxType fxType;
        [SerializeField] private GameObject fxPrefab;

        public FxType FxType => fxType;
        public GameObject FxPrefab => fxPrefab;
    }
}