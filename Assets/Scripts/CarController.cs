using UnityEngine;
using UnityEngine.InputSystem;

public class CarController : MonoBehaviour
{
    [System.Serializable]
    public struct Wheel
    {
        public WheelCollider collider;
        public Transform transform;
        public bool isFront;
    }

    public Wheel[] wheels;

    [Header("Car Power & Speed")]
    public float motorTorque = 4500f; // Gücü artırdık (Varsayılan: 4500)
    public float brakeTorque = 5000f; // El freni gücü
    public float maxSteerAngle = 35f;

    private float currentSteerAngle;
    private Rigidbody rb;

   private void Start()
{
    rb = GetComponent<Rigidbody>();
    if (rb != null)
    {
        // Aracın fiziki olarak "uykuya" geçmesini (donmasını) engeller:
        rb.sleepThreshold = 0.0f;

        // Ağırlık merkezini tabana indirir (Automatic Center Of Mass kapalı olmalı)
        rb.centerOfMass = new Vector3(0, -0.8f, 0); 
    }
}

    private void FixedUpdate()
    {
        float inputAccel = 0f;
        float inputSteer = 0f;
        bool isBraking = false;

        if (Keyboard.current != null)
        {
            // İleri / Geri (W / S)
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) inputAccel = 1f;
            else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) inputAccel = -1f;

            // Sağ / Sol (A / D)
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) inputSteer = 1f;
            else if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) inputSteer = -1f;

            // Fren (Space Tuşu)
            isBraking = Keyboard.current.spaceKey.isPressed;
        }

        currentSteerAngle = maxSteerAngle * inputSteer;

        foreach (var wheel in wheels)
        {
            // Direksiyon Döndürme
            if (wheel.isFront)
            {
                wheel.collider.steerAngle = currentSteerAngle;
            }

            // Motor Gücü & Fren Uygulaması
            if (isBraking)
            {
                wheel.collider.motorTorque = 0f;
                wheel.collider.brakeTorque = brakeTorque;
            }
            else
            {
                wheel.collider.brakeTorque = 0f;
                wheel.collider.motorTorque = inputAccel * motorTorque;
            }

            // Görsel Hizalama (Amortisör & Dönüşler)
            UpdateWheelTransform(wheel.collider, wheel.transform);
        }
    }

    private void UpdateWheelTransform(WheelCollider col, Transform trans)
    {
        Vector3 pos;
        Quaternion rot;
        col.GetWorldPose(out pos, out rot);

        trans.position = pos;
        trans.rotation = rot;
    }
}