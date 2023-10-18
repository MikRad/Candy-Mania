using System;

[Serializable]
public class CellData
{
    public FieldIndex _fieldIndex;
    public int _detonationsToClear;
    public GameItemType _gameItemType;

    public CellData(FieldIndex fIdx, int detonationsToClear = 0, GameItemType gItemType = GameItemType.Empty)
    {
        _fieldIndex = fIdx;
        _detonationsToClear = detonationsToClear;
        _gameItemType = gItemType;
    }
}
