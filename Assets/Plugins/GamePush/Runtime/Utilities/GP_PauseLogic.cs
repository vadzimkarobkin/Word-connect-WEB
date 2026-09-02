using UnityEngine;

namespace GamePush
{
    public class GP_PauseLogic : MonoBehaviour
    {
        public static int LevelsPlayed;
        private static bool _sessionStarted;
        private static float _sessionTime;
        private static bool _tempMute;
        private static bool _gamePause;
        private static bool _adPause;
    
        private void OnEnable()
        {
            GP_Game.OnPause += PauseGame;
            GP_Game.OnResume += UnpauseGame;
    
            GP_Ads.OnPreloaderStart += AdStart;
            GP_Ads.OnFullscreenStart += AdStart;
            GP_Ads.OnRewardedStart += AdStart;
    
            GP_Ads.OnAdsClose += AdClose;

            if (!_sessionStarted)
            { 
                Analytics.SessionStart();
                _sessionStarted = true;
            }
                
        }
    
        private void OnDisable()
        {
            GP_Game.OnPause -= PauseGame;
            GP_Game.OnResume -= UnpauseGame;
    
            GP_Ads.OnPreloaderStart -= AdStart;
            GP_Ads.OnFullscreenStart -= AdStart;
            GP_Ads.OnRewardedStart -= AdStart;
    
            GP_Ads.OnAdsClose -= AdClose;
        }
    
        void OnApplicationFocus(bool hasFocus)
        {
            if (hasFocus)
                UnpauseGame();
            else
                PauseGame();
        }
    
        private static void PauseGame()
        {
            if (_gamePause) return;
            _gamePause = true;
    
            GP_Logger.Log($"Game On Pause: {_gamePause}");

            if (_sessionStarted)
            {
                Analytics.SessionEnd((int)_sessionTime, LevelsPlayed);
                _sessionStarted = false;
                _sessionTime = 0;
                LevelsPlayed = 0;
            }
            

            
            _tempMute = AudioListener.pause;
            MusicOff();
            Time.timeScale = 0f;
        }
    
        private static void UnpauseGame()
        {
            if (!_gamePause || _adPause) return;
            _gamePause = false;

            if (!_sessionStarted)
            {
                Analytics.SessionStart();
                _sessionStarted = true;
            }
            
            GP_Logger.Log($"Game On Pause: {_gamePause}");
    
            if (!_tempMute)
                MusicOn();
            Time.timeScale = 1;
        }
    
        private static void MusicOff() => AudioListener.pause = true;
        private static void MusicOn() => AudioListener.pause = false;
    
        private static void AdStart()
        {
            GP_Logger.Log($"Ad Start");
    
            _adPause = true;
            PauseGame();
        }
    
        private static void AdClose(bool succes)
        {
            GP_Logger.Log($"Ad Close: {succes}");
    
            _adPause = false;
            UnpauseGame();
        }

        private void Update()
        {
            _sessionTime += Time.deltaTime;
        }
    }

}
