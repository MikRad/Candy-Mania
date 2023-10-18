using System;
using System.Collections.Generic;

public class VictoryConditionsUpdater
{
    private Dictionary<VictoryCondition.Type, VictoryCondition> _victoryConditionsMap = new Dictionary<VictoryCondition.Type, VictoryCondition>();

    public IEnumerable<KeyValuePair<VictoryCondition.Type, VictoryCondition>> Conditions => _victoryConditionsMap;

    public event Action<VictoryCondition> OnConditionUpdated; 
    public event Action OnAllConditionsReached; 

    public void Init(IEnumerable<VictoryCondition> victoryConditions)
    {
        _victoryConditionsMap = new Dictionary<VictoryCondition.Type, VictoryCondition>();
        foreach (VictoryCondition vCon in victoryConditions)
        {
            _victoryConditionsMap.Add(vCon._victoryConditionType,
                new VictoryCondition(vCon._victoryConditionType, vCon._numberNeededToComplete));
        }
    }

    public void UpdateVictoryCondition(VictoryCondition.Type vcType)
    {
        if(_victoryConditionsMap.ContainsKey(vcType))
        {
            VictoryCondition vCon = _victoryConditionsMap[vcType];
            vCon._numberNeededToComplete--;
            
            OnConditionUpdated?.Invoke(vCon);

            if (vCon._numberNeededToComplete == 0)
            {
                _victoryConditionsMap.Remove(vcType);
                
                if (_victoryConditionsMap.Count == 0)
                {
                    OnAllConditionsReached?.Invoke();                
                }
            }
        }
    }
}
