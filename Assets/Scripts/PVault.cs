using UnityEngine;

// Single responsibility: detect vaultable obstacles and execute the vault.
// Knows nothing about climbing or movement.
public class PlayerVault : MonoBehaviour
{
    [Header("Vault Settings")]
    [SerializeField] private float vaultCheckDistance = 1.2f;
    [SerializeField] private float vaultHeight        = 1.0f;
    [SerializeField] private float vaultSpeed         = 6f;
    [SerializeField] private LayerMask vaultLayerMask;

    private const float VaultArrivalThreshold = 0.1f;
    private const float VaultLandingOffset    = 1.1f;

    private CharacterController cc;
    private Vector3 vaultTargetPosition;

    public void Initialize(CharacterController characterController)
    {
        cc = characterController;
    }
// ── Timer that controls how often we raycast ──
// Add this with your other private variables at the top of PlayerVault.cs
        private float vaultCheckTimer = 0f;
        private const float VaultCheckInterval = 0.05f; // check 20 times/sec not 60
    // PlayerController calls this to ask:
    // "Can I vault right now?" — also sets up the target position
    public bool CanVault(float verticalInput)
{
    // ── GATE 1: Is the player even pressing forward? ──
    // Cheapest possible check. No physics involved.
    // If not pressing W, skip everything immediately.
    if (verticalInput <= 0.1f) 
    {
         // reset timer so vault is responsive
        vaultCheckTimer = 0f;
        return false;         // when player does press W
    }

    // ── GATE 2: Timer — only raycast every 0.05 seconds ──
    // Instead of 60 raycasts per second we do 20.
    // 0.05f interval = 20 checks per second.
    // Fast enough to feel instant. Cheap enough to not matter.
    vaultCheckTimer -= Time.deltaTime;
    if (vaultCheckTimer > 0f) return false;
    vaultCheckTimer = VaultCheckInterval; // reset for next interval

    // ── RAY 1: obstacle at chest height? ──
    // Only reaches here 20 times per second maximum
    Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
    if (!Physics.Raycast(rayOrigin, transform.forward,
                         out RaycastHit hitLow,
                         vaultCheckDistance, vaultLayerMask))
        return false;

    // ── GATE 3: Is it actually tagged Vaultable? ──
    // Check tag BEFORE firing Ray 2.
    // Tag check costs almost nothing vs a second Raycast.
    if (!hitLow.collider.CompareTag("Vaultable")) return false;

    // ── RAY 2: surface on top to land on? ──
    // Only fires if Ray 1 AND tag check both passed.
    // In most frames Ray 2 never fires at all.
    Vector3 aboveObstacle = transform.position
                          + transform.forward * vaultCheckDistance
                          + Vector3.up * vaultHeight;

    if (!Physics.Raycast(aboveObstacle, Vector3.down,
                         out RaycastHit hitTop,
                         vaultHeight + 0.5f, vaultLayerMask))
        return false;

    // ── Valid vault ──
    vaultTargetPosition = hitTop.point + Vector3.up * VaultLandingOffset;
    return true;
}

    // PlayerController calls this every frame during Vaulting state
    // Returns true when vault is complete
    public bool HandleVault()
    {
        Vector3 current = transform.position;
        Vector3 newPos  = Vector3.MoveTowards(current, vaultTargetPosition,
                                              vaultSpeed * Time.deltaTime);
        cc.Move(newPos - current);

        return Vector3.Distance(transform.position, vaultTargetPosition)
               < VaultArrivalThreshold;
    }

    // Gizmos helper — called from PlayerGizmos
    public (Vector3 origin, Vector3 forward, float distance,
            Vector3 abovePoint, float height) GetGizmosData()
    {
        Vector3 origin     = transform.position + Vector3.up * 0.5f;
        Vector3 abovePoint = transform.position
                           + transform.forward * vaultCheckDistance
                           + Vector3.up * vaultHeight;
        return (origin, transform.forward, vaultCheckDistance,
                abovePoint, vaultHeight + 0.5f);
    }
}