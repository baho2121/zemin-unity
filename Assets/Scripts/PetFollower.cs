using UnityEngine;

public class PetFollower : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform player; // The player to follow
    public int petIndex = 0; // Which number pet is this? (0, 1, 2...)
    public int totalPets = 1; // Total number of pets you have

    [Header("Movement Settings")]
    public float followSpeed = 5f; // Lerp speed (approx 0.08 per frame in 60fps is ~5.0 with deltaTime)
    public float jumpHeight = 0.5f;
    public float jumpSpeed = 10f; // Speed of the sine wave
    public float baseHeight = 0.3f; // Minimum Y height

    private float jumpPhase;

    void Start()
    {
        // Randomize jump phase slightly so they don't all jump in perfect unison if you have many
        jumpPhase = Random.Range(0f, 10f);
        
        // If player is not assigned, try to find it automatically
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        // Fix: Disable collider to prevent pushing the player
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
    }

    // Auto-indexing system
    public static System.Collections.Generic.List<PetFollower> allPets = new System.Collections.Generic.List<PetFollower>();

    void OnEnable()
    {
        if (!allPets.Contains(this)) allPets.Add(this);
    }

    void OnDisable()
    {
        if (allPets.Contains(this)) allPets.Remove(this);
    }

    void Update()
    {
        if (player == null) return;

        // Auto-update index based on list position
        petIndex = allPets.IndexOf(this);
        totalPets = allPets.Count;

        // 1. TARGET CALCULATION (Symmetrical V/U Formation)
        int maxPetsPerRow = 5;
        float spacing = 1.3f;      // Horizontal distance
        float rowDepth = 1.5f;     // Vertical distance between rows
        float curveBias = 0.5f;    // How much it curves back (U-shape)

        int rowIndex = petIndex / maxPetsPerRow;
        int indexInRow = petIndex % maxPetsPerRow;
        
        // Calculate how many pets are in THIS specific row
        // If total is 7:
        // Row 0: 5 pets
        // Row 1: 2 pets
        // We need to know where 'this' row starts and ends to center it.
        
        int petsInFullRows = (totalPets / maxPetsPerRow) * maxPetsPerRow;
        int petsInThisRow = maxPetsPerRow;
        
        // If we are in the last row, count might be less
        if (rowIndex == totalPets / maxPetsPerRow)
        {
            petsInThisRow = totalPets % maxPetsPerRow;
            if (petsInThisRow == 0) petsInThisRow = maxPetsPerRow; // Full last row case
        }
        // Actually simpler:
        int totalRows = Mathf.CeilToInt((float)totalPets / maxPetsPerRow);
        if (rowIndex == totalRows - 1) // Is last row?
        {
             int remainder = totalPets % maxPetsPerRow;
             if (remainder > 0) petsInThisRow = remainder;
        }

        // Standard Centering Formula: (i - (N-1)/2)
        // Example N=4: (0-1.5)=-1.5, (1-1.5)=-0.5, (2-1.5)=0.5, (3-1.5)=1.5 -> Centered!
        // Example N=3: (0-1)=-1, (1-1)=0, (2-1)=1 -> Centered!
        
        float rowCenterOffset = (petsInThisRow - 1) * 0.5f;
        float lateralOffset = (indexInRow - rowCenterOffset) * spacing;
        
        // Base depth behind player
        // For "Ters U" (Inverted U), the sides are CLOSER to the player than the center?
        // OR the center is further back?
        // Let's assume standard "Encircle" behavior: Sides come forward (closer to player).
        float startDepth = 3.5f; // Push start back so sides have room to come forward
        
        // Calculate U-Curve
        float extraDepthCurve = Mathf.Abs(lateralOffset) * curveBias;
        
        // Invert the curve: Center is furthest back (relative to the arc), Sides are closer
        float depthOffset = startDepth + (rowIndex * rowDepth) - extraDepthCurve;

        // Calculate world position relative to player rotation
        Vector3 targetPos = player.position 
                            - (player.forward * depthOffset) 
                            + (player.right * lateralOffset);

        targetPos.y = 0f; // Ground level

        // 2. SMOOTH FOLLOW (Lerp)
        Vector3 currentPos = transform.position;
        Vector3 smoothPos = Vector3.Lerp(currentPos, targetPos, Time.deltaTime * followSpeed);
        
        // Keep X and Z smoothed
        transform.position = new Vector3(smoothPos.x, transform.position.y, smoothPos.z);

        // 3. JUMP ANIMATION (Sine Wave)
        jumpPhase += Time.deltaTime * jumpSpeed;
        
        float newY = baseHeight + Mathf.Abs(Mathf.Sin(jumpPhase + (petIndex * 0.5f))) * jumpHeight; // Offset jump phase by index for wave effect
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // 4. LOOK AT PLAYER
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
    }
}
