using UnityEngine;

public class DestroyGameObject : MonoBehaviour
{
    public void DestroySelf()
    {
        Destroy(gameObject); // Destroy the GameObject this script is attached to
    }
}
