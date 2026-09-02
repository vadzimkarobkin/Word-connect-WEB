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

using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace WordsToolkit.Scripts.Settings
{
    public class GameSettings : SettingsBase
    {
        [Header("On start")]
        public int coins = 200;
        public int gems = 10;

        [Header("Monetization")]
        public bool enableAds = true;
        public bool enableInApps = true;
        public bool enableLuckySpin = true;

        [Header("Lucky spin settings")]
        [Tooltip("Maximum number of free Lucky Spin attempts that can accumulate over time.")]
        public int maxFreeLuckySpinCount = 3;

        [Tooltip("Number of hours required to generate one additional free Lucky Spin attempt.")]
        public float freeLuckySpinRechargeHours = 24f;

        [Header("GDPR settings")]
        public string privacyPolicyUrl;

        [Header("Continue time settings")]
        public int continuePrice = 15;
        public float continueTime = 15;

        [FormerlySerializedAs("gemsPerExtraWord")]
        [Tooltip("Gems awarded per extra word found")]
        public int gemsForExtraWords = 5;

        [Tooltip("Gems awarded when consuming a gift")]
        public int gemsForGift = 10;

        [Header("Gameplay settings")]
        [Tooltip("Enable shuffling of letters only after the player reaches the specified level number.")]
        public int shuffleLettersFromLevel = 0;

        [Header("Boost settings")]
        public int hammerBoostPrice = 200;
        public int hintBoostPrice = 200;
        public int countOfBoostsToBuy = 3;
        [Header("Boost button available from levels")]
        public BoostLevel[] boostLevels;
    }

    [Serializable]
    public class BoostLevel
    {
        [TagFieldUI]
        public string tag;
        public int level;
    }
}
