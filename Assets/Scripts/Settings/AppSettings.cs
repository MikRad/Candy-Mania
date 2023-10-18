using UnityEngine;

[CreateAssetMenu(menuName = "ApplicationSettings/AppSettings")]
public class AppSettings : ScriptableObject
{
    public Vector2 _gameFieldOffset;
    public float _cellSize = 1f;
    public int _maxRowsNumber;
    public int _maxColumnsNumber;
    public int _minMatchNumber = 3;
    public string _defaultLevelName = "level1";
    public int _levelsNumberTotal;
    public float _minDragDelta = 0.35f;
    public float _levelTimeAlarm = 30f;
}
