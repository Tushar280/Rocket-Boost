using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [SerializeField] InputAction thrust;
    [SerializeField] InputAction rotation;
    [SerializeField] private Rigidbody rb;
    [SerializeField] float thrustPower;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        thrust.Enable();
        rotation.Enable();
    }
    
    private void OnDisable()
    {
        thrust.Disable();
        rotation.Disable();
    }

    private void FixedUpdate(){
        ProcessThrust();
        
    }

    private void Update()
    {
        float rotationValue = rotation.ReadValue<float>();
        Debug.Log("The Rotation Value is" + rotationValue);
    }

    private void ProcessThrust()
    {
        if(thrust.IsPressed())
        {
            rb.AddRelativeForce(Vector3.up* thrustPower * Time.deltaTime);
        }
    }

    
}
