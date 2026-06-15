using UnityEngine;

public class Target : MonoBehaviour
{
    [Header("Settings")]
    public int scoreValue = 10;

    [Header("Audio")]
    [SerializeField] private AudioClip hitSound;

    void OnCollisionEnter(Collision collision)
    {
        // Only react to balls hitting this target
        if (collision.gameObject.CompareTag("Ball"))
        {
            HitTarget();
        }
    }

    void HitTarget()
    {
        if (hitSound != null)
        AudioSource.PlayClipAtPoint(hitSound, transform.position);

        if (ScoreManager.instance != null)
            ScoreManager.instance.AddScore(scoreValue);

        if (GameManager.instance != null)
          GameManager.instance.TargetDestroyed();

        Destroy(gameObject);
    }
}