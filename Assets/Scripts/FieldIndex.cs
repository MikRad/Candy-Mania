using System;
using System.Collections.Generic;

[Serializable]
public class FieldIndex
{
    public int _i;
    public int _j;

    public FieldIndex(int i, int j)
    {
        _i = i;
        _j = j;
    }

    public bool IsEqual(FieldIndex fIdx)
    {
        return ((_i == fIdx._i) && (_j == fIdx._j));
    }

    public bool IsListContains(IEnumerable<FieldIndex> indexes)
    {
        foreach (FieldIndex idx in indexes)
        {
            if (IsEqual(idx))
                return true;
        }

        return false;
    }

    public override string ToString()
    {
        return $"i = {_i} j = {_j}";
    }
}
