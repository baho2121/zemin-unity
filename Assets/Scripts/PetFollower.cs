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
        // Randomize jump phase
        jumpPhase = Random.Range(0f, 10f);
        
        // Find player if null
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
        
        // Register
        if (PetManager.Instance != null)
            PetManager.Instance.RegisterPet(this);
            
        // Disable collider to prevent physics issues (We use custom raycast now)
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        
        // Clean up Rigidbody if added by mistake
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) Destroy(rb);
    }

    // State
    private BreakableObject currentTarget;
    private bool isAttacking = false;
    private float attackTimer;
    public float damage = 10f;
    public float attackRate = 1.0f; 

    public static System.Collections.Generic.List<PetFollower> allPets = new System.Collections.Generic.List<PetFollower>();

    void OnEnable() { if (!allPets.Contains(this)) allPets.Add(this); }
    void OnDisable() { if (allPets.Contains(this)) allPets.Remove(this); }
    void OnDestroy()
    {
        if (PetManager.Instance != null) PetManager.Instance.UnregisterPet(this);
    }

    public void SetTarget(BreakableObject target)
    {
        currentTarget = target;
        isAttacking = (target != null);
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
            if (isAttacking && currentTarget == null) isAttacking = false;
            HandleFollowLogic();
        }
    }

    void HandleAttackLogic()
    {
        // Circular Attack Formation
        float attackRadius = 1.5f; 
        float angle = ((float)petIndex / Mathf.Max(totalPets, 1)) * Mathf.PI * 2f;
        Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
        Vector3 targetPos = currentTarget.transform.position + (direction * attackRadius);
        
        float dist = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(targetPos.x, 0, targetPos.z));
        float stopDistance = 0.5f;

        if (dist > stopDistance)
        {
            Vector3 movePos = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed * 2f);
            
            // Adjust height to match coin/ground
            float groundY = GetGroundHeight(movePos);
            transform.position = new Vector3(movePos.x, groundY + baseHeight, movePos.z);
            
            transform.LookAt(currentTarget.transform.position);
        }
        else
        {
            // Bounce
            jumpPhase += Time.deltaTime * jumpSpeed * 2f;
            float bounceTitle = Mathf.Abs(Mathf.Sin(jumpPhase)) * 0.5f; 
            
            float groundY = GetGroundHeight(targetPos);
            transform.position = new Vector3(targetPos.x, groundY + baseHeight + bounceTitle, targetPos.z);
            transform.LookAt(currentTarget.transform.position);

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

        petIndex = allPets.IndexOf(this);
        totalPets = allPets.Count;

        // 1. TARGET CALCULATION
        int maxPetsPerRow = 5;
        float spacing = 1.3f;      
        float rowDepth = 1.5f;     
        float curveBias = 0.5f;    

        int rowIndex = petIndex / maxPetsPerRow;
        int indexInRow = petIndex % maxPetsPerRow;
        int petsInThisRow = maxPetsPerRow;
        
        int totalRows = Mathf.CeilToInt((float)totalPets / maxPetsPerRow);
        if (rowIndex == totalRows - 1)
        {
             int remainder = totalPets % maxPetsPerRow;
             if (remainder > 0) petsInThisRow = remainder;
        }

        float rowCenterOffset = (petsInThisRow - 1) * 0.5f;
        float lateralOffset = (indexInRow - rowCenterOffset) * spacing;
        float startDepth = 3.5f; 
        float extraDepthCurve = Mathf.Abs(lateralOffset) * curveBias;
        float depthOffset = startDepth + (rowIndex * rowDepth) - extraDepthCurve;

        Vector3 targetPos = player.position 
                            - (player.forward * depthOffset) 
                            + (player.right * lateralOffset);

        // FIX: Don't force y=0. Find actual ground info.
        float groundY = GetGroundHeight(targetPos);
        targetPos.y = groundY;

        // 2. SMOOTH FOLLOW
        Vector3 currentPos = transform.position;
        Vector3 smoothPos = Vector3.Lerp(currentPos, targetPos, Time.deltaTime * followSpeed);
        
        transform.position = new Vector3(smoothPos.x, transform.position.y, smoothPos.z);

        // 3. JUMP ANIMATION
        jumpPhase += Time.deltaTime * jumpSpeed;
        float newY = groundY + baseHeight + Mathf.Abs(Mathf.Sin(jumpPhase + (petIndex * 0.5f))) * jumpHeight;
        
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // 4. LOOK AT PLAYER
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
    }

    // Helper to find ground height
    float GetGroundHeight(Vector3 pos)
    {
        // Raycast from high up downwards
        RaycastHit hit;
        // Start from player height or high enough (e.g., +5 units from pos.y)
        // If we want to be safe for stairs, cast from a bit above the target position estimate
        if (Physics.Raycast(new Vector3(pos.x, pos.y + 5f, pos.z), Vector3.down, out hit, 10f))
        {
            return hit.point.y;
        }
        // Fallback: use player's Y or 0
        return (player != null) ? player.position.y : 0f;
    }
}
