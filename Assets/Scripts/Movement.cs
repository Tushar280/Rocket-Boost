using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [SerializeField] InputAction thrust;
    [SerializeField] InputAction rotation;

    [SerializeField] private Rigidbody rb;

    [SerializeField] float thrustPower;
    [SerializeField] float rotationPower;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        thrust.Enable();
        rotation.Enable();
    }

    private void FixedUpdate(){
        ProcessThrust();
        
    }

    private void Update()
    {
        ProcessRotation();
    }

    private void ProcessRotation()
    {
        float rotationValue = rotation.ReadValue<float>();
        if (rotationValue != 0)
        {
            rb.freezeRotation = true;
            transform.Rotate(rotationValue * Vector3.forward * rotationPower * Time.deltaTime);
            rb.freezeRotation = false;
        }
    
    }

    private void ProcessThrust()
    {
        if(thrust.IsPressed())
        {
            rb.AddRelativeForce(Vector3.up* thrustPower * Time.deltaTime);
        }
    }

    
}
