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
        
        // Auto register with manager
        if (PetManager.Instance != null)
            PetManager.Instance.RegisterPet(this);
            
        // Disable collider to prevent physics issues
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    // State
    private BreakableObject currentTarget;
    private bool isAttacking = false;
    private float attackTimer;
    public float damage = 10f;
    public float attackRate = 1.0f; // Seconds between hits

    // Removed Duplicate Start Method
    
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

    void OnDestroy()
    {
        if (PetManager.Instance != null)
            PetManager.Instance.UnregisterPet(this);
    }

    public void SetTarget(BreakableObject target)
    {
        currentTarget = target;
        isAttacking = (target != null);
        
        // Random jump to start attack animation
        jumpPhase = Random.Range(0f, 10f);
    }

    void Update()
    {
        if (isAttacking && currentTarget != null)
        {
            HandleAttackLogic();
        }
        else
        {
            // Reset if target died
            if (isAttacking && currentTarget == null)
            {
                isAttacking = false;
            }
            
            HandleFollowLogic();
        }
    }

    void HandleAttackLogic()
    {
        // Circular Attack Formation
        float attackRadius = 1.5f; // Radius of the circle around the coin
        
        // Calculate angle based on index (distribute evenly)
        float angle = ((float)petIndex / Mathf.Max(totalPets, 1)) * Mathf.PI * 2f;
        
        Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
        Vector3 targetPos = currentTarget.transform.position + (direction * attackRadius);
        
        // Ground the target pos just in case
        // targetPos.y = currentTarget.transform.position.y; // Keep same height as coin? Or ground?
        // Better: Keep pet's Y logic separate (bounce)

        float dist = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(targetPos.x, 0, targetPos.z));
        float stopDistance = 0.5f; // How close to the "slot" should they get?

        if (dist > stopDistance)
        {
            // Move fast to assigned slot
            Vector3 movePos = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed * 2f);
            transform.position = new Vector3(movePos.x, transform.position.y, movePos.z);
            
            // Look at the COIN, not the slot
            transform.LookAt(currentTarget.transform.position);
            
            // Running animation height (baseHeight)
             transform.position = new Vector3(transform.position.x, baseHeight, transform.position.z);
        }
        else
        {
            // We are in position, Attack!
            // Bounce animation aggressive
            jumpPhase += Time.deltaTime * jumpSpeed * 2f;
            float bounceTitle = Mathf.Abs(Mathf.Sin(jumpPhase)) * 0.5f; 
            
            // Stay in slot but bounce Y
            transform.position = new Vector3(targetPos.x, baseHeight + bounceTitle, targetPos.z);
            transform.LookAt(currentTarget.transform.position);

            // Deal Damage
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackRate)
            {
                currentTarget.TakeDamage(damage);
                attackTimer = 0f;
            }
        }
    }

    void HandleFollowLogic()
    {
        if (player == null) return;

        // Auto-update index based on list position
        petIndex = allPets.IndexOf(this);
        totalPets = allPets.Count;

        // 1. TARGET CALCULATION (Grid/U-Formation)
        int maxPetsPerRow = 5;
        float spacing = 1.3f;      // Horizontal distance
        float rowDepth = 1.5f;     // Vertical distance between rows
        float curveBias = 0.5f;    // How much it curves back (U-shape)

        int rowIndex = petIndex / maxPetsPerRow;
        int indexInRow = petIndex % maxPetsPerRow;
        
        int petsInFullRows = (totalPets / maxPetsPerRow) * maxPetsPerRow;
        int petsInThisRow = maxPetsPerRow;
        
        int totalRows = Mathf.CeilToInt((float)totalPets / maxPetsPerRow);
        if (rowIndex == totalRows - 1) // Is last row?
        {
             int remainder = totalPets % maxPetsPerRow;
             if (remainder > 0) petsInThisRow = remainder;
        }

        float rowCenterOffset = (petsInThisRow - 1) * 0.5f;
        float lateralOffset = (indexInRow - rowCenterOffset) * spacing;
        
        // Base depth behind player
        float startDepth = 3.5f; 
        
        // Calculate U-Curve
        float extraDepthCurve = Mathf.Abs(lateralOffset) * curveBias;
        
        // Invert the curve
        float depthOffset = startDepth + (rowIndex * rowDepth) - extraDepthCurve;

        // Calculate world position
        Vector3 targetPos = player.position 
                            - (player.forward * depthOffset) 
                            + (player.right * lateralOffset);

        targetPos.y = 0f; // Ground level

        // 2. SMOOTH FOLLOW
        Vector3 currentPos = transform.position;
        Vector3 smoothPos = Vector3.Lerp(currentPos, targetPos, Time.deltaTime * followSpeed);
        
        transform.position = new Vector3(smoothPos.x, transform.position.y, smoothPos.z);

        // 3. JUMP ANIMATION
        jumpPhase += Time.deltaTime * jumpSpeed;
        float newY = baseHeight + Mathf.Abs(Mathf.Sin(jumpPhase + (petIndex * 0.5f))) * jumpHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // 4. LOOK AT PLAYER
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
    }
}
