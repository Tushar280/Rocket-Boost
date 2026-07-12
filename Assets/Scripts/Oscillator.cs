using UnityEngine;

public class Oscillator : MonoBehaviour
{
    [SerializeField] Vector3 movementVector;
    [SerializeField] float speed;

    Vector3 startPosition;
    Vector3 endPosition;
    float movementFactor;

    void Start()
    {
       startPosition = transform.position;
       endPosition = startPosition + movementVector;
    }

    void Update()
    {
       
    }

    
}
