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
            PlayText(text, xDir, yDir, Color.white);
        }

        public void PlayText(string text, int xDir, int yDir, Color color)
        {
            textField.text = text;
            textField.color = color;
            StartCoroutine(TextRoutine(xDir, yDir));
        }

        public void PlayText(string text, Vector2 dir, Color color)
        {
            textField.text = text;
            textField.color = color;
            StartCoroutine(TextRoutineVec(dir));
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

        private IEnumerator TextRoutineVec(Vector2 dir)
        {
            var waitFrame = new WaitForEndOfFrame();
            var timer = 0f;
            var initalScale = transform.localScale;
            var d = (dir.sqrMagnitude > 0.0001f) ? dir.normalized : Vector2.up;

            while (timer <= duration)
            {
                timer += Time.deltaTime;
                transform.localScale = scaleCurve.Evaluate(timer / duration) * initalScale;
                var pos = transform.position;
                pos.x += xCurve.Evaluate(timer / duration) * d.x * Time.deltaTime;
                pos.y += yCurve.Evaluate(timer / duration) * d.y * Time.deltaTime;
                transform.position = pos;
                yield return waitFrame;
            }
            Destroy(gameObject);
        }
    }
}