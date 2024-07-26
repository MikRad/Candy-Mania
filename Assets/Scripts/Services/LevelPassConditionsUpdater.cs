using System.Collections.Generic;

public class LevelPassConditionsUpdater
{
    private Dictionary<LevelPassCondition.Type, int> _levelPassConditionsMap = new Dictionary<LevelPassCondition.Type, int>();

    public void Init(IEnumerable<LevelPassCondition> levelPassConditions)
    {
        _levelPassConditionsMap = new Dictionary<LevelPassCondition.Type, int>();
        
        foreach (LevelPassCondition lpCon in levelPassConditions)
        {
            _levelPassConditionsMap.Add(lpCon._levelPassConditionType, lpCon._numberNeededToComplete);
        }
    }

    public void UpdateVictoryCondition(LevelPassCondition.Type lpcType)
    {
        if(_levelPassConditionsMap.ContainsKey(lpcType))
        {
            _levelPassConditionsMap[lpcType]--;            
            
            EventBus.Get.RaiseEvent(this, new LevelPassConditionUpdatedEvent(lpcType, _levelPassConditionsMap[lpcType]));

            if (_levelPassConditionsMap[lpcType] == 0)
            {
                _levelPassConditionsMap.Remove(lpcType);
                
                if (_levelPassConditionsMap.Count == 0)
                {
                    EventBus.Get.RaiseEvent(this, new AllLevelPassConditionsReachedEvent());
                }
            }
        }
    }
}
