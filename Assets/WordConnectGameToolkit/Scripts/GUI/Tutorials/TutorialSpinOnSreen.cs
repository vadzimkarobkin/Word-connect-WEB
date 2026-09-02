// // ©2015 - 2025 Candy Smith
// // All rights reserved
// // Redistribution of this software is strictly not allowed.
// // Copy of this software can be obtained from unity asset store only.
// // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// // FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
// // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// // THE SOFTWARE.

using DG.Tweening;
using UnityEngine;
using WordsToolkit.Scripts.GUI.Buttons;

namespace WordsToolkit.Scripts.GUI.Tutorials
{
    public class TutorialSpinOnScreen : TutorialPopupUI
    {
        [SerializeField]
        private Vector2 arrowStartOffsetPixels = new Vector2(120f, 0f);

        [SerializeField]
        private Vector2 arrowEndOffsetPixels = new Vector2(30f, 0f);

        [SerializeField]
        private float arrowMoveDuration = 1f;

        [SerializeField]
        private Ease arrowMoveEase = Ease.InOutSine;

        [SerializeField]
        private CustomButton closePopupButton;

        private Tween arrowTween;

        public new void SetTitle(string getText)
        {
        }

        protected override void Awake()
        {
            base.Awake();

            if (closePopupButton != null)
            {
                closePopupButton.onClick.AddListener(HandleCloseButtonClicked);
            }
        }

        protected virtual void OnDestroy()
        {
            if (closePopupButton != null)
            {
                closePopupButton.onClick.RemoveListener(HandleCloseButtonClicked);
            }
        }

        public override void AfterShowAnimation()
        {
            base.AfterShowAnimation();

            if (arrow == null || targetObject == null)
            {
                return;
            }

            if (arrowAnimation != null)
            {
                StopCoroutine(arrowAnimation);
                arrowAnimation = null;
            }

            if (arrowTween != null && arrowTween.IsActive())
            {
                arrowTween.Kill();
            }

            arrowTween = null;

            arrow.SetActive(true);

            float offsetDirection = Mathf.Sign(arrowOffset);
            if (Mathf.Approximately(offsetDirection, 0f))
            {
                offsetDirection = 1f;
            }

            float pixelsToUnits = GetPixelsToUnitsFactor();

            Vector2 startOffsetUnits = new Vector2(
                offsetDirection * Mathf.Abs(arrowStartOffsetPixels.x) * pixelsToUnits,
                arrowStartOffsetPixels.y * pixelsToUnits);

            arrowOffset = startOffsetUnits.x;
            UpdateArrowPosition();

            Vector3 startPosition = arrow.transform.position + new Vector3(0f, startOffsetUnits.y, 0f);
            arrow.transform.position = startPosition;
            Vector2 adjustedEndOffset = GetAdjustedEndOffset(offsetDirection, pixelsToUnits);
            Vector3 endPosition = startPosition + new Vector3(adjustedEndOffset.x, adjustedEndOffset.y, 0f);

            if (Vector3.Distance(startPosition, endPosition) <= 0.001f)
            {
                return;
            }

            arrowTween = arrow.transform
                .DOMove(endPosition, Mathf.Max(0.01f, arrowMoveDuration))
                .SetEase(arrowMoveEase)
                .SetLoops(-1, LoopType.Yoyo);
        }

        public override void AfterHideAnimation()
        {
            if (arrowTween != null && arrowTween.IsActive())
            {
                arrowTween.Kill();
                arrowTween = null;
            }

            base.AfterHideAnimation();
        }

        private Vector2 GetAdjustedEndOffset(float offsetDirection, float pixelsToUnits)
        {
            Vector2 offset = arrowEndOffsetPixels;
            offset.x *= offsetDirection;
            return offset * pixelsToUnits;
        }

        private float GetPixelsToUnitsFactor()
        {
            Canvas canvas = arrow != null ? arrow.GetComponentInParent<Canvas>() : null;
            float scaleFactor = canvas != null ? canvas.scaleFactor : 1f;
            return Mathf.Approximately(scaleFactor, 0f) ? 1f : 1f / scaleFactor;
        }

        private void HandleCloseButtonClicked()
        {
            Close();
        }
    }
}
