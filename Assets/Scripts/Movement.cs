using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [SerializeField] InputAction thrust;
    [SerializeField] private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        thrust.Enable();
    }
    
    private void OnDisable()
    {
        thrust.Disable();
    }

    private void FixedUpdate()
    {
        if(thrust.IsPressed())
        {
            rb.AddRelativeForce(Vector3(0, 1, 0) * Time.deltaTime);
        }
    }
}
