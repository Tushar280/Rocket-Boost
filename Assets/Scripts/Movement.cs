using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [SerializeField] InputAction thrust;
    [SerializeField] InputAction rotation;

    private Rigidbody rb;
    private AudioSource audioSource;
    [SerializeField] private AudioClip thrustSound;

    [SerializeField] private ParticleSystem vfxLaunch;

    [SerializeField] float thrustPower;
    [SerializeField] float rotationPower;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        SettingsManager.EnsureInstance();
    }

    private void OnEnable()
    {
        thrust.Enable();
        rotation.Enable();
    }

    private void FixedUpdate()
    {
        ProcessThrust();
    }

    private void Update()
    {
        ProcessRotation();
    }

    private void ProcessRotation()
    {
        float rotationValue = rotation.ReadValue<float>();
        if (rotationValue != 0 && rb != null)
        {
            rb.freezeRotation = true;
            transform.Rotate(rotationValue * Vector3.forward * rotationPower * Time.deltaTime);
            rb.freezeRotation = false;
        }
    }

    private void ProcessThrust()
    {
        if (thrust.IsPressed())
        {
            float sfxVol = SettingsManager.Instance != null ? SettingsManager.Instance.SFXVolume : 0.8f;

            if (audioSource != null)
            {
                audioSource.volume = sfxVol;
                if (!audioSource.isPlaying)
                {
                    audioSource.Play();
                }
            }

            if (vfxLaunch != null && !vfxLaunch.isPlaying)
            {
                vfxLaunch.Play();
            }

            if (rb != null)
            {
                rb.AddRelativeForce(Vector3.up * thrustPower * Time.deltaTime);
            }
        }
        else
        {
            if (vfxLaunch != null) vfxLaunch.Stop();
            if (audioSource != null && audioSource.isPlaying) audioSource.Stop();
        }
    }
}
