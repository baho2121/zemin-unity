using UnityEngine;
using System.Collections.Generic;

public class PetManager : MonoBehaviour
{
    public static PetManager Instance;

    public List<PetFollower> myPets = new List<PetFollower>();

    void Awake()
    {
        Instance = this;
    }

    // Called when we click on a target
    public void AttackTarget(BreakableObject target)
    {
        if (myPets.Count == 0) return;

        // Simple logic: Send ALL pets to attack
        foreach (var pet in myPets)
        {
            if (pet != null)
                pet.SetTarget(target);
        }
    }

    public void RegisterPet(PetFollower pet)
    {
        if (!myPets.Contains(pet))
            myPets.Add(pet);
    }

    public void UnregisterPet(PetFollower pet)
    {
        if (myPets.Contains(pet))
            myPets.Remove(pet);
    }
}
