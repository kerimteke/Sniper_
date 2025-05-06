using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class MobileSniperScope : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject scopeOverlay;
    [SerializeField] private RectTransform scopeButton;
    [SerializeField] private Image crosshair;
    [SerializeField] private Text ammoText;

    [Header("Camera Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float scopedFOV = 30f;
    [SerializeField] private float normalFOV = 60f;
    [SerializeField] [Range(1f, 80f)] private float maxLookAngle = 60f;

    [Header("Weapon Settings")]
    [SerializeField] private int maxMagazine = 5;
    [SerializeField] private float recoilAmount = 1.5f;
    [SerializeField] private float recoilResetSpeed = 8f;
    [SerializeField] private float reloadTime = 1.5f;

    [Header("Events")]
    public UnityEvent OnScopeOpen;
    public UnityEvent OnShoot;
    public UnityEvent OnReloadStart;
    public UnityEvent OnReloadComplete;


    // Private variables
    private int currentAmmo;
    private bool isScoped;
    private bool isReloading;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private Vector2 touchStartPosition;

    void Start()
    {
        InitializeWeapon();
        SetupCrosshair();
    }

    void InitializeWeapon()
    {
        currentAmmo = maxMagazine;
        originalCameraPosition = mainCamera.transform.localPosition;
        originalCameraRotation = mainCamera.transform.localRotation;
        UpdateAmmoUI();
    }

    void SetupCrosshair()
    {
        crosshair.gameObject.SetActive(false);
        crosshair.rectTransform.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
    }

    void Update()
    {
        if (isReloading) return;

        HandleTouchInput();
        HandleScopeState();
    }

    void HandleTouchInput()
    {
        foreach (Touch touch in Input.touches)
        {
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId)) continue;

            // Scope activation
            if (touch.phase == TouchPhase.Began && IsTouchOnScopeButton(touch.position))
            {
                StartScoping();
                touchStartPosition = touch.position;
            }

            // Aim adjustment while scoped
            if (isScoped && touch.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch.deltaPosition * 0.08f; // Sensitivity adjustment
                AdjustAim(delta.x, delta.y);
            }

            // Fire when released
            if (isScoped && touch.phase == TouchPhase.Ended)
            {
                if (currentAmmo > 0) FireWeapon();
                else StopScoping();
            }
        }
    }

    bool IsTouchOnScopeButton(Vector2 touchPos)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(scopeButton, touchPos);
    }

    void StartScoping()
    {
        if (isScoped || isReloading) return;

        isScoped = true;
        scopeOverlay.SetActive(true);
        crosshair.gameObject.SetActive(true);
        mainCamera.fieldOfView = scopedFOV;
        OnScopeOpen?.Invoke();
    }

    void StopScoping()
    {
        isScoped = false;
        scopeOverlay.SetActive(false);
        crosshair.gameObject.SetActive(false);
        mainCamera.fieldOfView = normalFOV;
        ResetCameraRotation();
    }

    void AdjustAim(float xDelta, float yDelta)
    {
        Vector3 currentRotation = mainCamera.transform.localEulerAngles;
        float newXRotation = currentRotation.x - yDelta;
        float newYRotation = currentRotation.y + xDelta;

        // Clamp vertical rotation
        newXRotation = Mathf.Clamp(newXRotation, -maxLookAngle, maxLookAngle);

        mainCamera.transform.localEulerAngles = new Vector3(
            newXRotation,
            newYRotation,
            0f
        );
    }

    void FireWeapon()
    {
        currentAmmo--;
        UpdateAmmoUI();

        PerformRaycast();
        OnShoot?.Invoke();
        StartCoroutine(RecoilEffect());

        if (currentAmmo <= 0) StartCoroutine(Reload());
    }

    void PerformRaycast()
    {
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
          /*  if (hit.collider.CompareTag("Enemy"))
            {
                hit.collider.GetComponent<EnemyHealth>()?.TakeDamage();
                Debug.Log("enemy canı düştü");
            }
          */
        }
    }
 
    IEnumerator RecoilEffect()
    {
        // Apply recoil
        Vector3 recoilVector = new Vector3(
            Random.Range(-recoilAmount, recoilAmount),
            recoilAmount,
            Random.Range(-recoilAmount, recoilAmount)
        );

        mainCamera.transform.localPosition += recoilVector;

        // Smoothly reset position
        float elapsed = 0f;
        Vector3 startPos = mainCamera.transform.localPosition;

        while (elapsed < 0.2f)
        {
            mainCamera.transform.localPosition = Vector3.Lerp(
                startPos,
                originalCameraPosition,
                elapsed / 0.2f
            );
            elapsed += Time.deltaTime;
            yield return null;
        }

        mainCamera.transform.localPosition = originalCameraPosition;
    }

    IEnumerator Reload()
    {
        isReloading = true;
        StopScoping();
        OnReloadStart?.Invoke();

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxMagazine;
        UpdateAmmoUI();
        isReloading = false;
        OnReloadComplete?.Invoke();
    }

    void ResetCameraRotation()
    {
        mainCamera.transform.localRotation = originalCameraRotation;
    }

    void UpdateAmmoUI()
    {
        ammoText.text = $"{currentAmmo}/{maxMagazine}";
    }

    void HandleScopeState()
    {
        if (isScoped)
        {
            // Keep crosshair centered
            crosshair.rectTransform.position = mainCamera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 0));
        }
    }
}