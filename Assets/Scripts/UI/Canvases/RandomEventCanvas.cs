using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace ALWTTT.Events
{
    public class RandomEventCanvas : MonoBehaviour
    {
        [Header("Event Info")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image eventImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        [Header("Options")]
        [SerializeField] private LayoutGroup optionsRoot;
        [SerializeField] private Button optionButtonPrefab;

        private Action<RandomEventOption> _onChosen;

        public void Show(RandomEventData data, Action<RandomEventOption> onChosen)
        {
            gameObject.SetActive(true);
            _onChosen = onChosen;

            if (backgroundImage) backgroundImage.sprite = data.BackgroundSprite;
            if (eventImage) eventImage.sprite = data.EventSprite;
            if (titleText) titleText.text = data.Title;
            if (descriptionText) descriptionText.text = data.Description;

            BuildOptions(data.Options);
        }

        public void Hide()
        {
            ClearOptions();
            gameObject.SetActive(false);
            _onChosen = null;
        }

        private void BuildOptions(List<RandomEventOption> options)
        {
            ClearOptions();

            foreach (var opt in options)
            {
                var btn = Instantiate(optionButtonPrefab, optionsRoot.transform);
                var label = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (label) label.text = opt.text;

                btn.onClick.AddListener(() =>
                {
                    _onChosen?.Invoke(opt);
                    Hide();
                });
            }
        }

        private void ClearOptions()
        {
            var root = optionsRoot ? optionsRoot.transform : transform;
            for (int i = root.childCount - 1; i >= 0; i--)
                Destroy(root.GetChild(i).gameObject);
        }
    }
}