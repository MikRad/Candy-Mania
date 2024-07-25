public static class UIEvents
{
    public struct LevelCompletedPanelContinueClicked : IEvent
    {
    }
    
    public struct LevelFailedPanelRestartClicked : IEvent
    {
    }
    
    public struct SettingsPanelContinueClicked : IEvent
    {
    }
    
    public struct SettingsPanelToMainMenuClicked : IEvent
    {
    }
    
    public struct LevelInfoPanelSettingsClicked :IEvent
    {
    }
    
    public struct MainMenuPlayClicked :IEvent
    {
    }
    
    public struct MainMenuSettingsClicked :IEvent
    {
    }
    
    public struct MainMenuExitClicked :IEvent
    {
    }
    
    public struct LevelButtonsPanelBackClicked :IEvent
    {
    }
    
    public struct LevelButtonsPanelLevelSelected :IEvent
    {
        public int LevelNumber { get; private set; }
        
        public LevelButtonsPanelLevelSelected(int levelNumber)
        {
            LevelNumber = levelNumber;
        }
    }
}
