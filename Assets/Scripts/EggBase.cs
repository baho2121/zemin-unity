using UnityEngine;

public class EggBase : MonoBehaviour
{
    public EggData eggData;
    
    // Logic removed as requested.
    // This script now only serves as a container for EggData.
    public GameObject interactionUI; // Kept to avoid breaking Generator ref immediately, though unused.
}
