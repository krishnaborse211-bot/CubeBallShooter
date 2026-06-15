using UnityEngine;

// Single responsibility: draw debug visualization in the Scene view.
// Zero game logic. Can be deleted in a build with zero consequences.
public class PlayerGizmos : MonoBehaviour
{
    private PlayerClimb playerClimb;
    private PlayerVault playerVault;
    private PlayerController playerController;

    private void Awake()
    {
        // Get references to sibling components
        playerClimb      = GetComponent<PlayerClimb>();
        playerVault      = GetComponent<PlayerVault>();
        playerController = GetComponent<PlayerController>();
    }

    private void OnDrawGizmos()
    {
        // Climb ray — red
        if (playerClimb != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position,
                           transform.forward * playerClimb.ClimbCheckDistance);
        }

        // Vault rays — yellow forward, green downward
        if (playerVault != null)
        {
            var data = playerVault.GetGizmosData();
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(data.origin, data.forward * data.distance);

            Gizmos.color = Color.green;
            Gizmos.DrawRay(data.abovePoint, Vector3.down * data.height);
        }

        // State label floating above player
        #if UNITY_EDITOR
        if (playerController != null)
        {
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 2.5f,
                "State: " + playerController.CurrentState.ToString());
        }
        #endif
    }
}