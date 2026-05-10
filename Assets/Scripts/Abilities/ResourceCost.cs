using System;
using UnityEngine;

[Serializable]
public class ResourceCost
{
    [Header("Resource Costs")] [Tooltip("Health cost (usually 0 for most abilities)")]
    [field:SerializeField] public float Health { get; protected set; }

    [Tooltip("Energy / Stamina cost")]
    [field:SerializeField]public float Energy  { get; protected set; }

    [Tooltip("Mana cost")]
    [field:SerializeField]public float Mana  { get; protected set; }
}