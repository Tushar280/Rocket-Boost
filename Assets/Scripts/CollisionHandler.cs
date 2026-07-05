using UnityEngine;

public class CollisionHandler : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Friendly")
        {
            Debug.Log("This is a friendly object");
        }
        else
        {
            Debug.Log("This is an enemy object");
        }
    }
}
