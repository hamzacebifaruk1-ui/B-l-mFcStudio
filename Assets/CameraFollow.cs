using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;            // Takip edilecek araç objesi

    [Header("Distance & Offset")]
    public float distance = 7.0f;       // Kameranın araca uzaklığı
    public float minDistance = 3.0f;    // Yakınlaşma sınırı
    public float maxDistance = 15.0f;   // Uzaklaşma sınırı
    public float heightOffset = 1.8f;   // Odak yüksekliği

    [Header("Sensitivity & Speed")]
    public float xSensitivity = 350.0f; // Fare X hassasiyeti (Yükseltildi)
    public float ySensitivity = 250.0f; // Fare Y hassasiyeti (Yükseltildi)
    public float zoomSensitivity = 3.0f;
    public float followSpeed = 20.0f;   // Kamera takip hızı

    [Header("Limits")]
    public float yMinLimit = -5f;       // Aşağı bakış sınırı
    public float yMaxLimit = 60f;       // Yukarı bakış sınırı

    private float rotX = 0.0f;
    private float rotY = 20.0f;

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        rotX = angles.y;
        rotY = angles.x;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Fare Girdilerini Alma
        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            
            // Hassasiyet çarpanı artırıldı
            rotX += mouseDelta.x * xSensitivity * Time.deltaTime;
            rotY -= mouseDelta.y * ySensitivity * Time.deltaTime;

            rotY = Mathf.Clamp(rotY, yMinLimit, yMaxLimit);

            float scroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                distance -= Mathf.Sign(scroll) * zoomSensitivity;
                distance = Mathf.Clamp(distance, minDistance, maxDistance);
            }
        }

        // Pozisyon ve Rotasyon Hesaplama
        Quaternion targetRotation = Quaternion.Euler(rotY, rotX, 0);
        Vector3 targetCenter = target.position + (Vector3.up * heightOffset);
        Vector3 desiredPosition = targetCenter - (targetRotation * Vector3.forward * distance);

        // Hızlı ve Akıcı Takip
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * followSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * followSpeed);
    }
}