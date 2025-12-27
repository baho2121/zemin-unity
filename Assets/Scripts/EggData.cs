using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Egg", menuName = "Pet Simulator/Egg Data")]
public class EggData : ScriptableObject
{
    public string eggName;
    public int price = 100;
    
    [Header("Drop Table")]
    public List<PetDropChance> availablePets;
}

[System.Serializable]
public class PetDropChance
{
    public PetData pet;
    [Range(0, 100)]
    public float dropChance; // Percentage (e.g., 50 for 50%)
}
