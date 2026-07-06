using UnityEngine;

public class CollisionHandler : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        switch (collision.gameObject.tag)
        {
            case "Friendly":
                Debug.Log("This is a friendly object");
                break;
            case "Finish":
                Debug.Log("This is the finish");
                break;
            case "Fuel":
                Debug.Log("This is fuel");
                break;
            default:
                Debug.Log("This is an enemy object");
                break;
        }
    }
}
