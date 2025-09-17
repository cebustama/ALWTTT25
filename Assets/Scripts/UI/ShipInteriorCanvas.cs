using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.Managers
{
    public class ShipInteriorCanvas : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Button composeButton;
        [SerializeField] private Button relaxButton;
        [SerializeField] private Button bandTalkButton;

        public void Setup(Action onCompose, Action onRelax, Action onBandTalk)
        {
            gameObject.SetActive(true);

            composeButton.onClick.RemoveAllListeners();
            relaxButton.onClick.RemoveAllListeners();
            bandTalkButton.onClick.RemoveAllListeners();

            composeButton.onClick.AddListener(() => onCompose?.Invoke());
            relaxButton.onClick.AddListener(() => onRelax?.Invoke());
            bandTalkButton.onClick.AddListener(() => onBandTalk?.Invoke());
        }
    }
}
