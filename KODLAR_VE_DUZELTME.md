# Unity Derleme Hatası: CS0101 / CS0111 / CS0579 — “already contains a definition”

> **Tek cümlelik özet:** Bu hatalar kodun kendisinde bir yazım hatası olduğu için **değil**, aynı sınıfların (`CameraFollow`, `CarController`, `BuggiController`, `BuggiAntiRollBar`, `BuggiCrashController`) projede **iki kez** bulunması yüzünden çıkıyor. Kodda hiçbir değişiklik yapmadan, fazladan kopyaları silmek hatayı çözer.

---

## 1. Hatalar ne anlama geliyor?

| Hata | Anlamı |
|---|---|
| `CS0101: The namespace '<global namespace>' already contains a definition for 'CameraFollow'` | Bu isimde bir sınıf zaten var; sen ikinci kez tanımlıyorsun. |
| `CS0111: Type 'CameraFollow' already defines a member called 'Start' with the same parameter types` | Aynı `Start()` metodu aynı sınıfın içinde iki kez var. (CS0101’in devamıdır; kod iki kez derlendiği için her metot “iki kez tanımlı” görünür.) |
| `CS0579: Duplicate 'System.Serializable' attribute` | `CarController` içindeki `Wheel` struct’ı iki kez tanımlı olduğu için `[System.Serializable]` etiketi çakışıyor. |

Yani listede 20+ hata görünüyor ama **hepsinin tek bir sebebi var: dosyalar çift.**

---

## 2. Neden oluyor? (En sık 3 sebep)

1. **Aynı script iki farklı klasörde duruyor.** Örneğin hem `Assets/CameraFollow.cs` hem `Assets/Scripts/CameraFollow.cs`.
   → Senin hata satırlarında yol `Assets\Scripts\CameraFollow.cs` görünüyor, yani projede en az iki kopya var.
2. **Aynı Asset Store paketi iki kez import edilmiş.** `LastGuns` paketini iki kez içe aktardıysan `BuggiController` gibi script’ler `Base/Scripts/Buggy/` dışında başka bir klasörde de oluşur.
3. **Kod aynı dosyanın içine iki kez yapıştırılmış.** (Dosyada `class CameraFollow` metnini arattığında 2 kez bulunuyorsa bu durumdur.)

> ⚠️ Dosyayı yeniden adlandırmak (örn. `CameraFollow2.cs`) **işe yaramaz**; hata dosya adına değil, **sınıf adına** bakıyor. Ya fazla dosyayı silmelisin ya da içindeki sınıfın adını değiştirmelisin.

---

## 3. Adım adım çözüm

### Adım 1 — Kopyaları bul
**Unity içinden:** Project penceresindeki arama kutusuna sırayla şunları yaz:
`CameraFollow` · `CarController` · `BuggiController` · `BuggiAntiRollBar` · `BuggiCrashController`

Aynı isim 2 farklı klasörde çıkıyorsa sebep bulundu.

**Windows’tan (Unity kapalıyken) hızlı tarama:** PowerShell aç ve proje yolunu yaz:

```powershell
Get-ChildItem -Path "C:\PROJE_YOLUN\Assets" -Recurse -File -Include CameraFollow.cs,CarController.cs,BuggiController.cs,BuggiAntiRollBar.cs,BuggiCrashController.cs |
    Select-Object -ExpandProperty FullName
```

### Adım 2 — Sadece **bir** kopya bırak
Projenin doğru (tek kopya) hâli şu yapıdır:

```
Assets/
├── Scripts/
│   ├── CameraFollow.cs            <-- 1 kopya
│   └── CarController.cs           <-- 1 kopya
└── RetroStyleGames/LastGuns/Base/Scripts/Buggy/
    ├── BuggiController.cs         <-- 1 kopya
    ├── BuggiAntiRollBar.cs        <-- 1 kopya
    └── BuggiCrashController.cs    <-- 1 kopya
```

- Fazla olan dosyaları **sil** (silmeden önce gözünün önünde dursun istiyorsan proje dışına, örn. `C:\Yedek\` klasörüne taşı — `Assets` içinde bırakma, Unity orayı da derler).
- Aynı dosya içinde kod iki kez yapıştırılmışsa: dosyayı aç, ikinci `class ...` bloğunu baştan sonuna kadar sil.
- `LastGuns` paketini iki kez import ettiysen, **ikinci gelen tüm klasörü** sil (scriptler + materyaller).

### Adım 3 — Yeniden derle
Unity’de `Ctrl + S` (veya `Assets > Refresh`). Console’daki hataların hepsi aynı anda kaybolmalı. Kalmadıysa: `Console > Clear` → hatasız derlenene kadar yukarıdaki adımı tekrarla.

### Hiçbiri işe yaramadıysa
- `Assets` içinde projenin kendisinin bir kopyası olabilir: `Assets/B-l-mFcStudio-main/...`, `Assets/LastGuns 1/...` gibi. Bu klasörü komple sil.
- Son çare: Unity’yi kapat → projedeki `Library` klasörünü sil → projeyi yeniden aç (scriptler yeniden derlenir).

---

## 4. Temiz (eski) kodlar — kopyala / yapıştır

Aşağıdakiler projedeki **tek kopya, hatasız** sürümlerdir. Bozuk gördüğün dosyanın içeriğini tamamen bunlarla değiştirebilirsin (dosya adı ve klasör yolu yukarıdaki yapıyla aynı olsun).

### 4.1 `Assets/Scripts/CameraFollow.cs`

```csharp
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
```

### 4.2 `Assets/Scripts/CarController.cs`

```csharp
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
```

### 4.3 `Assets/RetroStyleGames/LastGuns/Base/Scripts/Buggy/BuggiController.cs`

```csharp
using UnityEngine;

namespace Gadd420
{
    public class BuggiController : MonoBehaviour
    {
        public enum DriveType { FWD, RWD, AWD }
        public enum WheelVisualRotationMode { UseWheelColliderPose, ManualRPM }
        public enum ManualWheelSpinAxis { X, Y, Z }

        [Header("Drive")]
        public DriveType driveType = DriveType.RWD;

        [Range(0f, 1f)]
        public float awdFrontTorqueSplit = 0.5f; 

        [Header("Input")]
        public bool useInputManagerIfPresent = true;
        public Input_Manager inputManager; 

        [Header("Units")]
        public bool useKmph = false;

        [Header("Engine (speed in MPH or KMPH)")]
        public float maxSpeed = 160f;
        public float maxReverseSpeed = 20f;

        [Header("Torque")]
        public bool isAutomatic = true;
        public float firstGearTorque = 1500f;
        public float topGearTorque = 800f;

        [Header("Gears")]
        public int numGears = 6;

        [Header("Engine Braking")]
        public bool useEngineBraking = true;
        public float engineBrakingDrag = 0.25f;

        [Header("Steering")]
        public float minSteerAngle = 6f;
        public float maxSteerAngle = 28f;
        public float speedForMinSteer = 90f; 

        [Header("Brakes")]
        public float brakeTorque = 2500f;
        public float handbrakeTorque = 4000f;

        [Header("Wheels (0 FL, 1 FR, 2 RL, 3 RR)")]
        public WheelCollider[] wheelColliders = new WheelCollider[4];
        public Transform[] wheelVisuals = new Transform[4];

        [Header("Wheel Visuals")]
        public WheelVisualRotationMode wheelVisualRotationMode = WheelVisualRotationMode.UseWheelColliderPose;
        public ManualWheelSpinAxis manualSpinAxis = ManualWheelSpinAxis.X;

        [Header("Debug")]
        public float currentSpeed; 
        [HideInInspector] public bool isCrashed;

        Rigidbody rb;

        // internal
        float maxSpeedMs;
        float maxReverseSpeedMs;
        float speedForMinSteerMs;

        float currentSpeedMs;
        float forwardSpeedMs;
        float ogDrag;

        float ogTorque;
        float torqueDifference;
        int currentGear = 1;
        float currentGearPercent;
        float rpm;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            ogDrag = rb.linearDamping;

            if (useInputManagerIfPresent && !inputManager)
                inputManager = GetComponent<Input_Manager>();

            ogTorque = firstGearTorque;

            if (!isAutomatic)
            {
                numGears = Mathf.Max(1, numGears);
                torqueDifference = (firstGearTorque - topGearTorque) / Mathf.Max(1, (numGears - 1));
            }
            else
            {
                numGears = 1;
            }

            float unitsToMs = useKmph ? (1f / 3.6f) : (1f / 2.237f);
            maxSpeedMs = maxSpeed * unitsToMs;
            maxReverseSpeedMs = maxReverseSpeed * unitsToMs;
            speedForMinSteerMs = speedForMinSteer * unitsToMs;
        }

        void Update()
        {
            currentSpeedMs = rb.linearVelocity.magnitude;
            forwardSpeedMs = Vector3.Dot(rb.linearVelocity, transform.forward);

            float msToUnits = useKmph ? 3.6f : 2.237f;
            currentSpeed = currentSpeedMs * msToUnits;

            UpdateWheelVisuals();
        }

        void FixedUpdate()
        {
            if (isCrashed)
            {
                ApplyBrakes(1f, 1f);
                ApplyMotorTorque(0f);
                return;
            }

            ReadInput(out float steer, out float throttle, out float brake, out float handbrake);

            float steerAngle = CalculateSteerAngle(steer);
            ApplySteering(steerAngle);

            float engineTorque = CalculateEngineTorque(throttle);
            ApplyDrive(throttle, engineTorque, brake, handbrake);

            EngineBraking(throttle);
        }

        void ReadInput(out float steer, out float throttle, out float brake, out float handbrake)
        {
            if (useInputManagerIfPresent && inputManager)
            {
                steer = Mathf.Clamp(inputManager.HzInput, -1f, 1f);
                throttle = Mathf.Clamp(inputManager.VInput, -1f, 1f);
                brake = Mathf.Clamp01(inputManager.FrontBreakInput);
            }
            else
            {
                steer = Mathf.Clamp(Input_Compat.GetHorizontal(), -1f, 1f);
                throttle = Mathf.Clamp(Input_Compat.GetVertical(), -1f, 1f);
                brake = Input_Compat.GetBrakeHeld() ? 1f : 0f;
            }

            handbrake = Input_Compat.GetHandbrakeHeld() ? 1f : 0f;
        }

        float CalculateSteerAngle(float steerInput)
        {
            float t = (speedForMinSteerMs <= 0.01f) ? 1f : Mathf.Clamp01(currentSpeedMs / speedForMinSteerMs);
            float angle = Mathf.Lerp(maxSteerAngle, minSteerAngle, t) * steerInput;
            return angle;
        }

        float CalculateEngineTorque(float throttleInput)
        {
            if (Mathf.Abs(throttleInput) < 0.001f) return 0f;

     
            if (throttleInput > 0f && currentSpeedMs > maxSpeedMs) return 0f;

        
            if (throttleInput < 0f && Mathf.Abs(forwardSpeedMs) > maxReverseSpeedMs) return 0f;

           
            if (isAutomatic)
            {
                float torqueLerp = Mathf.Lerp(ogTorque, topGearTorque, (maxSpeedMs <= 0.01f) ? 1f : (currentSpeedMs / maxSpeedMs));
                firstGearTorque = torqueLerp;
            }
            else
            {
                if (currentGear == 0) firstGearTorque = ogTorque;
                else if (currentGear == numGears - 1) firstGearTorque = topGearTorque;
                else firstGearTorque = ogTorque - torqueDifference * currentGear;

                UpdateManualGear();
            }

            return firstGearTorque * throttleInput;
        }

        void UpdateManualGear()
        {
            float speedPercent = (maxSpeedMs <= 0.01f) ? 0f : Mathf.Abs(currentSpeedMs / maxSpeedMs);
            float gearPercentage = 1f / (float)numGears;

            float upperGearMax = gearPercentage * (currentGear + 1);
            float downGearMax = gearPercentage * currentGear;

            if (currentGear > 0 && speedPercent < downGearMax) currentGear--;
            if (speedPercent > upperGearMax && currentGear < (numGears - 1)) currentGear++;

            float targetGearFactor = Mathf.InverseLerp(
                gearPercentage * currentGear,
                gearPercentage * (currentGear + 1),
                speedPercent
            );

            currentGearPercent = Mathf.Lerp(currentGearPercent, targetGearFactor, Time.fixedDeltaTime * 5f);
            float gearNumFactor = currentGear / (float)numGears;
            rpm = Mathf.Lerp(gearNumFactor, 1f, currentGearPercent);
        }

        void ApplySteering(float steerAngle)
        {
    
            if (wheelColliders[0]) wheelColliders[0].steerAngle = steerAngle;
            if (wheelColliders[1]) wheelColliders[1].steerAngle = steerAngle;
        }

        void ApplyDrive(float throttleInput, float engineTorqueSigned, float brakeInput, float handbrakeInput)
        {
            // brake logic (service brake)
            ApplyBrakes(brakeInput, handbrakeInput);

            // if braking hard, don't push motor
            if (brakeInput > 0.01f)
            {
                ApplyMotorTorque(0f);
                return;
            }

            if (throttleInput < -0.01f && forwardSpeedMs > 1f)
            {
                ApplyBrakes(1f, handbrakeInput);
                ApplyMotorTorque(0f);
                return;
            }

            ApplyMotorTorque(engineTorqueSigned);
        }

        void ApplyMotorTorque(float torque)
        {
            float fl = 0f, fr = 0f, rl = 0f, rr = 0f;

            if (driveType == DriveType.FWD)
            {
                fl = torque * 0.5f;
                fr = torque * 0.5f;
            }
            else if (driveType == DriveType.RWD)
            {
                rl = torque * 0.5f;
                rr = torque * 0.5f;
            }
            else // AWD
            {
                float front = torque * Mathf.Clamp01(awdFrontTorqueSplit);
                float rear = torque - front;

                fl = front * 0.5f;
                fr = front * 0.5f;
                rl = rear * 0.5f;
                rr = rear * 0.5f;
            }

            if (wheelColliders[0]) wheelColliders[0].motorTorque = fl;
            if (wheelColliders[1]) wheelColliders[1].motorTorque = fr;
            if (wheelColliders[2]) wheelColliders[2].motorTorque = rl;
            if (wheelColliders[3]) wheelColliders[3].motorTorque = rr;
        }

        void ApplyBrakes(float brakeInput, float handbrakeInput)
        {
            float bt = brakeTorque * Mathf.Clamp01(brakeInput);

            // service brake on all wheels
            for (int i = 0; i < wheelColliders.Length; i++)
            {
                if (wheelColliders[i]) wheelColliders[i].brakeTorque = bt;
            }

            // handbrake only on rear axle: RL, RR
            float hbt = handbrakeTorque * Mathf.Clamp01(handbrakeInput);
            if (wheelColliders[2]) wheelColliders[2].brakeTorque = Mathf.Max(wheelColliders[2].brakeTorque, hbt);
            if (wheelColliders[3]) wheelColliders[3].brakeTorque = Mathf.Max(wheelColliders[3].brakeTorque, hbt);
        }

        void EngineBraking(float throttleInput)
        {
            if (useEngineBraking && Mathf.Abs(throttleInput) < 0.001f && IsAnyWheelGrounded())
                rb.linearDamping = engineBrakingDrag;
            else
                rb.linearDamping = ogDrag;
        }

        bool IsAnyWheelGrounded()
        {
            for (int i = 0; i < wheelColliders.Length; i++)
            {
                if (wheelColliders[i] && wheelColliders[i].isGrounded) return true;
            }
            return false;
        }

        void UpdateWheelVisuals()
        {
            for (int i = 0; i < 4; i++)
            {
                var wc = wheelColliders[i];
                var wv = wheelVisuals[i];
                if (!wc || !wv) continue;

                wc.GetWorldPose(out Vector3 pos, out Quaternion rot);
                wv.position = pos;

                if (wheelVisualRotationMode == WheelVisualRotationMode.UseWheelColliderPose)
                {
                    wv.rotation = rot;
                }
                else
                {
                    float wheelRPM = wc.rpm;
                    if (rb.linearVelocity.sqrMagnitude > 0.0001f)
                    {
                        Vector3 axis = (manualSpinAxis == ManualWheelSpinAxis.X) ? Vector3.right :
                                       (manualSpinAxis == ManualWheelSpinAxis.Y) ? Vector3.up : Vector3.forward;

                        wv.Rotate(axis * wheelRPM * 6f * Time.deltaTime);
                    }
                }
            }
        }
    }
}
```

### 4.4 `Assets/RetroStyleGames/LastGuns/Base/Scripts/Buggy/BuggiAntiRollBar.cs`

```csharp
using UnityEngine;

namespace Gadd420
{
    [RequireComponent(typeof(Rigidbody))]
    public class BuggiAntiRollBar : MonoBehaviour
    {
        [Header("Axles")]
        public WheelCollider frontLeft;
        public WheelCollider frontRight;
        public WheelCollider rearLeft;
        public WheelCollider rearRight;

        [Header("Forces")]
        public float frontAntiRoll = 5000f;
        public float rearAntiRoll = 5000f;

        Rigidbody rb;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        void FixedUpdate()
        {
            ApplyAntiRoll(frontLeft, frontRight, frontAntiRoll);
            ApplyAntiRoll(rearLeft, rearRight, rearAntiRoll);
        }

        void ApplyAntiRoll(WheelCollider left, WheelCollider right, float antiRollForce)
        {
            if (!left || !right) return;

            float travelL = 1f;
            float travelR = 1f;

            WheelHit hit;

            bool groundedL = left.GetGroundHit(out hit);
            if (groundedL)
            {
                travelL = (-left.transform.InverseTransformPoint(hit.point).y - left.radius) / left.suspensionDistance;
            }

            bool groundedR = right.GetGroundHit(out hit);
            if (groundedR)
            {
                travelR = (-right.transform.InverseTransformPoint(hit.point).y - right.radius) / right.suspensionDistance;
            }

            float antiRoll = (travelL - travelR) * antiRollForce;

            if (groundedL)
                rb.AddForceAtPosition(left.transform.up * -antiRoll, left.transform.position);

            if (groundedR)
                rb.AddForceAtPosition(right.transform.up * antiRoll, right.transform.position);
        }
    }
}
```

### 4.5 `Assets/RetroStyleGames/LastGuns/Base/Scripts/Buggy/BuggiCrashController.cs`

```csharp
using UnityEngine;

namespace Gadd420
{
    public class BuggiCrashController : MonoBehaviour
    {
        public string[] crashTag;
        public float decelerationSpeedForCrash = 8f;

        Rigidbody rb;
        BuggiController car;

        float rbSpeed;
        float lateRbSpeed;
        float counter;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            car = GetComponent<BuggiController>();
        }

        void Update()
        {
            if (!rb || !car) return;

            if (counter == 0f) rbSpeed = rb.linearVelocity.magnitude;

            counter += Time.deltaTime;

            if (counter >= 0.05f)
            {
                lateRbSpeed = rb.linearVelocity.magnitude;

                if (rbSpeed - lateRbSpeed > decelerationSpeedForCrash)
                    car.isCrashed = true;

                counter = 0f;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!car) return;

            for (int i = 0; i < crashTag.Length; i++)
            {
                if (other.CompareTag(crashTag[i]))
                    car.isCrashed = true;
            }
        }
    }
}
```

---

## 5. Gelen diğer mesajlar hata değil (FBX uyarıları)

```
Mesh 'Car_Base_3_low.005' has 2 (out of 2036) vertices with no weight and bone assigned ...
Can't calculate tangents, because mesh 'Car_Exterior_D_22_low.003' doesn't contain normals.
```

Bunlar **ImportFBX Warning**’dir, derlemeyi engellemez, kırmızı hata değildir:

- *“vertices with no weight”*: ripli (skinned) modelin 2 köşesine kemik atanmamış. Model durağan (statik) kullanılacaksa hiç önemli değil. Düzeltmek istersen: modeli 3D programında aç, o köşeleri en yakın kemiğe ata (veya `Rig > Animation Type` = **None** yaparak rig’i tamamen kapat).
- *“Can't calculate tangents / doesn't contain normals”*: modelde normal verisi yok, Unity normal map için teğet hesaplayamıyor. Modelin Import ayarlarında **Normals = Calculate** yap, ya da 3D programında normalleri hesaplayıp tekrar export et. Görsel olarak sorun görmüyorsan dokunmadan da bırakabilirsin.

---

## 6. Bonus: Araç hareket etmiyorsa (kod hatası değil, ayar eksik)

`Assets/Scenes/SampleScene.unity` içinde `CarController` bileşeninin **`wheels` dizisi boş** (`wheels: []`). Dizi boşken `FixedUpdate` hiçbir tekere tork vermez, yani araba gaz yemez.

Düzeltmek için sahnede araç objesini seç → Inspector’da **Car Controller** bileşeni:

1. `Wheels` alanının boyutunu **4** yap.
2. Her eleman için:
   - **Collider** → `WC_FL`, `WC_FR`, `WC_RL`, `WC_RR` (WheelCollider)
   - **Transform** → o tekerin görsel mesh’i
   - **Is Front** → ön ikisi `✔`, arka ikisi `✘`
3. Aynı obje üzerinde **Rigidbody** olmalı. Kütlesi makul (örn. 1200–1500), `Interpolate = Interpolate` önerilir.
4. Kamerada **Camera Follow** bileşeninin `Target` alanı araca işaret etmeli.

---

## 7. Bu depodaki (repo) düzenleme

Bu commit’te `CameraFollow.cs` ve `CarController.cs` dosyaları `Assets` kökünden `Assets/Scripts/` klasörüne taşındı; böylece depoda da her sınıfın **tek** kopyası var ve yol, Unity’de gördüğün `Assets\Scripts\...` yoluyla aynı. `.meta` dosyaları korunduğu için sahnedeki script bağlantıları (GUID’ler) bozulmadı.
