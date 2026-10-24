using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // Takip edilecek araç objesi

    [Header("Distance & Offset")]
    public float distance = 7.0f;       // Kameranın araca uzaklığı
    public float minDistance = 3.0f;    // En yakın mesafe
    public float maxDistance = 15.0f;   // En uzak mesafe
    public float heightOffset = 2.0f;   // Aracın merkezinden yükseklik payı

    [Header("Sensitivity & Smoothing")]
    public float xSensitivity = 15.0f;  // X Eksen Hassasiyeti (Yeni ölçek)
    public float ySensitivity = 15.0f;  // Y Eksen Hassasiyeti (Yeni ölçek)
    public float zoomSensitivity = 2.0f;
    public float rotationSmoothTime = 0.05f; // Açı yumuşatma süresi
    public float followSmoothTime = 0.1f;   // Pozisyon yumuşatma süresi

    [Header("Limits")]
    public float yMinLimit = -5f;       // Kameranın aşağı inme sınırı
    public float yMaxLimit = 60f;       // Kameranın yukarı çıkma sınırı

    private float x = 0.0f;
    private float y = 20.0f;

    private float currentX;
    private float currentY;
    private float xVelocity;
    private float yVelocity;
    private Vector3 currentVelocity;

    private void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
        currentX = x;
        currentY = y;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        if (!target) return;

        // Fare Girdilerini Alma ve Time.deltaTime ile Çarpma (Dengeli Hassasiyet)
        if (Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            x += mouseDelta.x * xSensitivity * Time.deltaTime;
            y -= mouseDelta.y * ySensitivity * Time.deltaTime;

            y = ClampAngle(y, yMinLimit, yMaxLimit);

            float scroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                distance -= Mathf.Sign(scroll) * zoomSensitivity;
                distance = Mathf.Clamp(distance, minDistance, maxDistance);
            }
        }

        // Açıyı Yumuşatma (SmoothDamp Angle)
        currentX = Mathf.SmoothDampAngle(currentX, x, ref xVelocity, rotationSmoothTime);
        currentY = Mathf.SmoothDampAngle(currentY, y, ref yVelocity, rotationSmoothTime);

        // Hedef Pozisyonu Hesaplama
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 targetPosition = target.position + Vector3.up * heightOffset;
        Vector3 desiredPosition = targetPosition - (rotation * Vector3.forward * distance);

        // Pozisyonu Yumuşatarak Takip Etme (SmoothDamp Vector3)
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, followSmoothTime);
        transform.rotation = rotation;
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F) angle += 360F;
        if (angle > 360F) angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}