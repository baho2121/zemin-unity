using UnityEngine;

public class EggManager : MonoBehaviour
{
    public static EggManager Instance;

    void Awake()
    {
        Instance = this;
    }

    // SERVER LOGIC: Determine what hatches
    public void RequestBuyEgg(EggData egg)
    {
        // 1. Validation (Does player have money?)
        if (EconomyManager.Instance.currentCoins >= egg.price)
        {
            // Deduct Mone
            EconomyManager.Instance.AddCoins(-egg.price);
            
            // 2. RNG Calculation (Roll the dice)
            PetData hatchedPet = CalculateDrop(egg);
            
            if (hatchedPet != null)
            {
                Debug.Log("Server: Hatched " + hatchedPet.petName);
                
                // 3. Client Notification (Visuals)
                // In multiplayer, this would be TargetRpc(player)
                HatchSuccess(hatchedPet);
            }
        }
        else
        {
            Debug.Log("Server: Not enough money!");
        }
    }

    private PetData CalculateDrop(EggData egg)
    {
        float totalWeight = 0;
        foreach (var drop in egg.availablePets)
            totalWeight += drop.dropChance;

        float randomValue = Random.Range(0, totalWeight);
        float refresh = 0;

        foreach (var drop in egg.availablePets)
        {
            refresh += drop.dropChance;
            if (randomValue <= refresh)
            {
                return drop.pet;
            }
        }
        return egg.availablePets[0].pet; // Fallback
    }

    // CLIENT LOGIC: Visuals
    private void HatchSuccess(PetData pet)
    {
        Debug.Log("Client: SHOW HATCH ANIMATION -> " + pet.petName);
        
        // Spawn the pet!
        // For now, we just spawn it next to the player's current pets
        // In the future: Show 3D Egg cracking UI
        SpawnPet(pet);
    }

    private void SpawnPet(PetData petData)
    {
        // For prototype: Just Instantiate the prefab and add PetFollower script
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        GameObject newPetObj = Instantiate(petData.petPrefab, player.transform.position, Quaternion.identity);
        
        // Setup Script
        PetFollower pf = newPetObj.GetComponent<PetFollower>();
        if (pf == null) pf = newPetObj.AddComponent<PetFollower>();
        
        pf.player = player.transform;
        pf.damage = petData.damage;
        // pf.speed = petData.moveSpeed;
        
        // Add to manager
        if (PetManager.Instance != null)
            PetManager.Instance.RegisterPet(pf);
            
        // Rename
        newPetObj.name = petData.petName;
    }
}
