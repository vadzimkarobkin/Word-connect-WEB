using System;
using GamePush;
using UnityEngine;
using VContainer;
using WordsToolkit.Scripts.Gameplay.Managers;
using WordsToolkit.Scripts.System;
using WordsToolkit.Scripts.Services.Ads;

namespace WordsToolkit.Scripts.Infrastructure.DI
{
    public static class Ads
    {
        public static void ShowRewarded(string placement, Action<string> onRewarded, Action onStart, Action<bool> onClose)
        {
            // Проверяем перезарядку rewarded рекламы (только если это не первый показ)
            if (!AdCooldownManager.Instance.CanShowRewarded())
            {
                Debug.Log($"Rewarded ad on cooldown. Remaining: {AdCooldownManager.Instance.GetRewardedCooldownRemaining():F1}s");
                return;
            }

            // Отмечаем показ rewarded рекламы
            AdCooldownManager.Instance.MarkRewardedShown();
            
            GP_Ads.ShowRewarded(placement, onRewarded, onStart, onClose);
        }

        public static void ShowInter(Action onStart, Action<bool> onClose)
        {
            // Проверяем перезарядку interstitial рекламы (только если это не первый показ)
            if (!AdCooldownManager.Instance.CanShowInterstitial())
            {
                Debug.Log($"Interstitial ad on cooldown. Remaining: {AdCooldownManager.Instance.GetInterstitialCooldownRemaining():F1}s");
                return;
            }

            // Отмечаем показ interstitial рекламы
            AdCooldownManager.Instance.MarkInterstitialShown();
            
            GP_Ads.ShowFullscreen(onStart, onClose);
        }
    }
}