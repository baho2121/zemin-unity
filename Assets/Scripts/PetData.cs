using UnityEngine;

[CreateAssetMenu(fileName = "New Pet", menuName = "Pet Simulator/Pet Data")]
public class PetData : ScriptableObject
{
    public string petName;
    public GameObject petPrefab;
    public float damage = 10f;
    public float moveSpeed = 5f;
}
