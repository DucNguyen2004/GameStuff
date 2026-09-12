using TMPro;
using UnityEngine;

namespace Code.UI
{
    [RequireComponent(typeof(TMP_Text))]
    public class TextWidthFitter : MonoBehaviour
    {
        private const int SearchIterations = 12;
        private const float SearchPrecision = 0.5f;

        [SerializeField] private TMP_Text _text;
        [Space(10)]
        [SerializeField] private float _minWidth = 100f;
        [SerializeField] private float _maxWidth = 600f;
        [SerializeField] private float _horizontalPadding = 20f;
        [SerializeField] private int _maxLines = 5;

        private RectTransform _rectTransform;

        private void Awake()
        {
            EnsureInitialized();
        }

        public void SetText(string text)
        {
            EnsureInitialized();

            _text.text = text;

            Fit();
        }

        [ContextMenu("Fit")]
        public void Fit()
        {
            EnsureInitialized();

            float minWidth = Mathf.Min(_minWidth, _maxWidth);
            float maxWidth = Mathf.Max(_minWidth, _maxWidth);

            if (string.IsNullOrEmpty(_text.text))
            {
                ApplyWidth(minWidth);

                return;
            }

            int visibleLinesLimit = _text.maxVisibleLines;
            _text.maxVisibleLines = int.MaxValue;

            float fittedWidth = CalculateFittedWidth(minWidth, maxWidth);

            _text.maxVisibleLines = visibleLinesLimit;

            ApplyWidth(Mathf.Clamp(fittedWidth + _horizontalPadding, minWidth, maxWidth));
        }

        private void EnsureInitialized()
        {
            if (_text == null)
                _text = GetComponent<TMP_Text>();

            _rectTransform = _text.rectTransform;

            _text.textWrappingMode = TextWrappingModes.Normal;
            _text.maxVisibleLines = Mathf.Max(1, _maxLines);
        }

        // Narrowest width that keeps the text on the fewest lines it can occupy,
        // never going past _maxLines. Height is never touched.
        private float CalculateFittedWidth(float minWidth, float maxWidth)
        {
            int targetLines = MeasureLineCount(maxWidth);

            if (targetLines > Mathf.Max(1, _maxLines))
                return maxWidth;

            if (MeasureLineCount(minWidth) <= targetLines)
                return minWidth;

            float tooNarrow = minWidth;
            float wideEnough = maxWidth;

            for (int i = 0; i < SearchIterations && wideEnough - tooNarrow > SearchPrecision; i++)
            {
                float middle = (tooNarrow + wideEnough) * 0.5f;

                if (MeasureLineCount(middle) <= targetLines)
                    wideEnough = middle;
                else
                    tooNarrow = middle;
            }

            return wideEnough;
        }

        private int MeasureLineCount(float width)
        {
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            _text.ForceMeshUpdate(ignoreActiveState: true);

            return _text.textInfo.lineCount;
        }

        private void ApplyWidth(float width)
        {
            _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            _text.ForceMeshUpdate(ignoreActiveState: true);
        }
    }
}
