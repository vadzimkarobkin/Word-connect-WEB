using GameAnalyticsSDK;

public static class AdsSettings
{
    public static int InterCooldown;
    public static int RewardedCooldown;
    public static int RewardedTipCooldown;
    public static float InterInsteadRewarded;
    public static int InterFirstShowDelay; 
    public static int InterMinLevel;
    public static string InterPlacement;
}

public static class Analytics
{
    public static void GameLoad()
    {
        GameAnalytics.NewDesignEvent("game_load");
    }

    public static void SessionStart()
    {
        GameAnalytics.NewDesignEvent("session_start");
    }

    public static void SessionEnd(int sessionDuration, int levelsPlayed)
    {
        GameAnalytics.NewDesignEvent($"session_end:session_duration:{sessionDuration}");
        GameAnalytics.NewDesignEvent($"session_end:levels_played:{levelsPlayed}");
    }

    public static void TutorialStart(string tutorialId)
    {
        GameAnalytics.NewDesignEvent($"tutorial_start:tutorial_id:{tutorialId}");
    }

    public static void TutorialComplete(string tutorialId)
    {
        GameAnalytics.NewDesignEvent($"tutorial_complete:tutorial_id:{tutorialId}");
    }

    public static void LevelStart(int level)
    {
        GameAnalytics.NewDesignEvent($"level_start:level:{level}");
    }

    public static void LevelComplete(int level, int timeOnLevel)
    {
        GameAnalytics.NewDesignEvent($"level_complete:level:{level}:time_on_level:{timeOnLevel}");
    }

    public static void HintUse(int level, string type)
    {
        GameAnalytics.NewDesignEvent($"hint_use:level:{level}:type:{type}");
    }

    public static void HintRewardedCtaClick(int level)
    {
        GameAnalytics.NewDesignEvent($"hint_rewarded_cta_click:level:{level}");
    }

    public static void CoinsEarned(int level, int coins)
    {
        GameAnalytics.NewDesignEvent($"coins_earned:level:{level}:coins:{coins}");
    }

    public static void CoinsSpent(int level, int coins)
    {
        GameAnalytics.NewDesignEvent($"coins_spent:level:{level}:coins:{coins}");
    }

    public static void AdRequest(string adType, string placement)
    {
        GameAnalytics.NewDesignEvent($"ad_request:{adType}:{placement}");
    }

    public static void AdImpression(string adType, string placement)
    {
        GameAnalytics.NewDesignEvent($"ad_impression:{adType}:{placement}");
    }

    public static void AdClose(string adType, string placement)
    {
        GameAnalytics.NewDesignEvent($"ad_close:{adType}:{placement}");
    }

    public static void AdRewardComplete(string adType, string placement, string closeReason)
    {
        GameAnalytics.NewDesignEvent($"ad_reward_complete:{adType}:{placement}:{closeReason}");
    }
}
