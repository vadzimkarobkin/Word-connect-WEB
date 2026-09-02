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

using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using WordsToolkit.Scripts.Data;
using WordsToolkit.Scripts.System;

namespace WordsToolkit.Scripts.GUI.Buttons.Boosts
{
    public abstract class BaseBoostButton : BaseGUIButton
    {
        public ResourceObject resourceToPay;
        public ResourceObject resourseToHoldBoost;
        public TextMeshProUGUI price;
        public TextMeshProUGUI countText;
        public GameObject priceObject;
        public GameObject countTextObject;
        protected int count;

        [SerializeField]
        private ParticleSystem waves;

        private CanvasGroup canvasGroup;
        private bool isActive;


        protected override void OnEnable()
        {
            base.OnEnable();
            if (!Application.isPlaying)
            {
                return;
            }
            SubscribeToHeldResource();
            InitializePrice();
            UpdatePriceDisplay();
            onClick.AddListener(OnClick);
            var main = waves.main;
            main.prewarm = true; // Start particles in grown state
            main.loop = true;
            waves.Stop(); // Ensure it's not auto-playing
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            onClick.RemoveListener(OnClick);
            UnsubscribeFromHeldResource();
        }

        protected abstract void InitializePrice();

        public virtual void UpdatePriceDisplay()
        {
            // If we have resources in the hold boost, show that count
            if (resourseToHoldBoost != null && resourseToHoldBoost.GetValue() > 0)
            {
                countTextObject.gameObject.SetActive(true);
                priceObject.gameObject.SetActive(false);
                countText.text = resourseToHoldBoost.GetValue().ToString();
            }
            else
            {
                countTextObject.gameObject.SetActive(false);
                priceObject.gameObject.SetActive(true);
                price.text = count.ToString();
            }
        }

        protected void OnClick()
        {
            if (isActive)
            {
                Refund();
                DeactivateBoost();
                return;
            }
            if (resourceManager.Consume(resourseToHoldBoost, 1))
            {
                ActivateBoost();
            }
            // If not, consume from the regular resource
            else if (resourceManager.ConsumeWithEffects(resourceToPay, count))
            {
                resourseToHoldBoost.Add(gameSettings.countOfBoostsToBuy);
                UpdatePriceDisplay();
                
                // Analytics: Coins Spent
                var levelManager = FindObjectOfType<WordsToolkit.Scripts.Gameplay.Managers.LevelManager>();
                if (levelManager != null)
                {
                    Analytics.CoinsSpent(levelManager.currentLevel, count);
                }
            }
        }

        private void Refund()
        {
            resourseToHoldBoost.Add(1);
            
            // Analytics: Coins Earned (if it's a coin resource)
            var levelManager = FindObjectOfType<WordsToolkit.Scripts.Gameplay.Managers.LevelManager>();
            if (levelManager != null && resourseToHoldBoost.name.ToLower().Contains("coin"))
            {
                Analytics.CoinsEarned(levelManager.currentLevel, 1);
            }
            
            UpdatePriceDisplay();
        }

        protected virtual void ActivateBoost(bool hideButtons = true)
        {
            UpdatePriceDisplay();
            if(hideButtons)
                buttonViewController.HideOtherButtons(this);
            PulseAnimation();
            waves.Play();
            isActive = true;
            priceObject.SetActive(false);
            countTextObject.SetActive(false);
        }
        protected virtual void DeactivateBoost()
        {
            isActive = false;
            buttonViewController.ShowButtons();
            waves.Clear();
            waves.Stop();
            DOTween.Complete(transform);
            DOTween.Kill(transform);
            transform.localScale = Vector3.one;
            UpdatePriceDisplay();
        }



        private void PulseAnimation()
        {
            animator.enabled = false;
            transform.DOScale(Vector3.one * 0.9f, 0.5f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        private void SubscribeToHeldResource()
        {
            if (resourseToHoldBoost != null)
            {
                resourseToHoldBoost.OnResourceUpdate += HandleHeldResourceChanged;
            }
        }

        private void UnsubscribeFromHeldResource()
        {
            if (resourseToHoldBoost != null)
            {
                resourseToHoldBoost.OnResourceUpdate -= HandleHeldResourceChanged;
            }
        }

        private void HandleHeldResourceChanged(int _)
        {
            UpdatePriceDisplay();
        }
    }
}