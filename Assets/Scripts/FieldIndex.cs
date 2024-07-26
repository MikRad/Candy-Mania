using System;

[Serializable]
public struct FieldIndex
{
    public int _i;
    public int _j;

    public FieldIndex(int i, int j)
    {
        _i = i;
        _j = j;
    }

    public override string ToString()
    {
        return $"i = {_i} j = {_j}";
    }
}
