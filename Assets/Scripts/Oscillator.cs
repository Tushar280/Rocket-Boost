using UnityEngine;

public class Oscillator : MonoBehaviour
{
    [SerializeField] Vector3 movementVector;
    [SerializeField] float period = 2f;

    private void Update()
    {
        float cycle01 = Time.time / period; // goes 0 -> 1 and back to 0
    }

    
}
