using System.Collections;
using TMPro;
using UnityEngine;

namespace ALWTTT.UI
{
    public class FloatingText : MonoBehaviour
    {
        [SerializeField] private float duration = 1;
        [SerializeField] private AnimationCurve scaleCurve;
        [SerializeField] private AnimationCurve yCurve;
        [SerializeField] private AnimationCurve xCurve;
        [SerializeField] private TextMeshProUGUI textField;

        public void PlayText(string text, int xDir, int yDir = -1)
        {
            textField.text = text;
            StartCoroutine(TextRoutine(xDir, yDir));
        }

        private IEnumerator TextRoutine(int xDir, int yDir)
        {
            var waitFrame = new WaitForEndOfFrame();
            var timer = 0f;

            var initalScale = transform.localScale;

            while (timer <= duration)
            {
                timer += Time.deltaTime;
                transform.localScale = scaleCurve.Evaluate(timer / duration) * initalScale;
                var pos = transform.position;
                pos.x += xCurve.Evaluate(timer / duration) * xDir * Time.deltaTime;
                pos.y += yCurve.Evaluate(timer / duration) * yDir * Time.deltaTime;
                transform.position = pos;
                yield return waitFrame;
            }
            Destroy(gameObject);
        }
    }
}