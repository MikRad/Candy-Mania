using System;
using UnityEngine;

[Serializable]
public class SfxInfo
{
    [HideInInspector]
    [SerializeField] private string _name;
    public AudioClip _clip;
    public SfxType _type;
    public float _volume = 1f;
    public float _pitch = 1f;

    public void UpdateName()
    {
        _name = _type.ToString();
    }
}
