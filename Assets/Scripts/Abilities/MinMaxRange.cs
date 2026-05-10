using System;
using UnityEngine;

[Serializable]
public class MinMaxRange
{
    [field: SerializeField] public float MinRange { get; protected set; } = 1f;

    [field: SerializeField] public float MaxRange { get; protected set; } = 3f;


}