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

using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;
using WordsToolkit.Scripts.Audio;
using WordsToolkit.Scripts.Enums;
using WordsToolkit.Scripts.System;
using WordsToolkit.Scripts.System.Haptic;

namespace WordsToolkit.Scripts.GUI.Buttons
{
    [RequireComponent(typeof(Animator))]
    public class CustomButton : Button
    {
        public AudioClip overrideClickSound;
        public RuntimeAnimatorController overrideAnimatorController;
        private bool isClicked;
        private readonly float cooldownTime = .5f; // Cooldown time in seconds
        public new ButtonClickedEvent onClick;
        private new Animator animator;
        public bool noSound;

        private static bool blockInput;

        public static CustomButton latestClickedButton;
        private IAudioService audioService;
        [Inject]
        public void Construct(IAudioService audioService)
        {
            this.audioService = audioService;
        }

        protected override void OnEnable()
        {
            isClicked = false;
            // run only in runtime
            if (Application.isEditor)
            {
                return;
            }
            base.OnEnable();
            animator = GetComponent<Animator>();
            if (overrideAnimatorController != null)
            {
                animator.runtimeAnimatorController = overrideAnimatorController;
            }
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (blockInput || isClicked || !interactable)
            {
                return;
            }

            if (transition != Transition.Animation)
            {
                Pressed();
            }

            isClicked = true;
            if(!noSound)
                audioService.PlayClick(overrideClickSound);
            HapticFeedback.TriggerHapticFeedback(HapticFeedback.HapticForce.Light);
            // Start cooldown
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(Cooldown());
            }

            base.OnPointerClick(eventData);
        }

        public void Pressed()
        {
            if (blockInput || !interactable)
            {
                return;
            }
            latestClickedButton = this;
            onClick?.Invoke();
            EventManager.GetEvent<CustomButton>(EGameEvent.ButtonClicked).Invoke(this);
        }

        private IEnumerator Cooldown()
        {
            yield return new WaitForSeconds(cooldownTime);
            isClicked = false;
        }


        private bool IsAnimationPlaying()
        {
            var stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            return stateInfo.loop || stateInfo.normalizedTime < 1;
        }

        public static void BlockInput(bool block)
        {
            blockInput = block;
        }
    }
}