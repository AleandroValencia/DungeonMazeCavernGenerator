using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class room
{
    public int Difficulty;
    public Vector2 Size;
    public GameObject RoomObj;
    [HideInInspector]
    public bool FoldOut = false;

    public room(int _Difficulty, Vector2 _Size, GameObject _RoomObj)
    {
        Difficulty = _Difficulty;
        Size = _Size;
        RoomObj = _RoomObj;
    }
}
