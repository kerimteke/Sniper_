using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class SniperScope : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public GameObject Sniper;

    [Header("UI Elements")]
    [SerializeField] private GameObject scopeVisual; // Scope’un UI görseli
    [SerializeField] private Camera mainCamera;      // Ana kamera

    [Header("Camera Settings")]
    [SerializeField] private float scopeFOV = 30f;   // Scope açıkken FOV
    [SerializeField] private float sensitivity = 0.1f; // Nişan alma hassasiyeti
    [SerializeField] private float pitchMin = -55f;  // Minimum dikey açı
    [SerializeField] private float pitchMax = 55f;   // Maksimum dikey açı


    [Header("Recoil Settings")]
    [SerializeField] private Vector2 recoilOffset = new Vector2(-10f, 30f); // Recoil kayma miktarı
    [SerializeField] private float recoilDuration = 0.5f;                // Recoil süresi

    [Header("Audio")]
    [SerializeField] private AudioSource fireSound; // Ateş sesi

    private float defaultFOV;        // Normal FOV
    private bool isScopeActive;      // Scope açık mı?
    private bool isAiming;           // Nişan alıyor mu?
    private float scopeCloseTime;    // Scope’un kapanma zamanı
    private Vector3 originalScopePos; // Scope’un orijinal pozisyonu

    private void Awake()
    {
        defaultFOV = mainCamera.fieldOfView;
        originalScopePos = scopeVisual.transform.localPosition;
        scopeVisual.SetActive(false);
        isScopeActive = false;
        isAiming = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
                                                             Sniper.SetActive(false);
        if (!isScopeActive)
        {
            ActivateScope();
        }
        isAiming = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isAiming)
        {
            Vector2 delta = eventData.delta;
            float yaw = delta.x * sensitivity;
            float pitch = -delta.y * sensitivity;

            Vector3 camRotation = mainCamera.transform.eulerAngles;
            float newPitch = camRotation.x + pitch;
            if (newPitch > 180f) newPitch -= 360f;
            newPitch = Mathf.Clamp(newPitch, pitchMin, pitchMax);
            camRotation.x = newPitch;
            camRotation.y += yaw;
            mainCamera.transform.eulerAngles = camRotation;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (isAiming)
        {
            Fire();
            isAiming = false;
            scopeCloseTime = Time.time + 0.5f;
        }

        Sniper.SetActive(true);

    }

    private void Update()
    {
        if (!isAiming && isScopeActive && Time.time > scopeCloseTime)
        {
            DeactivateScope();
        }
    }

    private void ActivateScope()
    {
        isScopeActive = true;
        scopeVisual.SetActive(true);
        mainCamera.fieldOfView = scopeFOV;
    }

    private void DeactivateScope()
    {
        isScopeActive = false;
        scopeVisual.SetActive(false);
        mainCamera.fieldOfView = defaultFOV;
    }

    private void Fire()
    {
        if (fireSound != null)
        {
            fireSound.Play();
        }
        StartCoroutine(RecoilCoroutine());
       
    }

    private IEnumerator RecoilCoroutine()
    {
        // Kameranın ve scope’un orijinal pozisyonlarını kaydet
        Vector3 originalCameraPos = mainCamera.transform.localPosition;
        Vector3 originalScopePos = scopeVisual.transform.localPosition;

        // Kamera için rastgele bir titreme offset’i oluştur
        Vector3 recoilCameraOffset = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0);

        // Scope için recoil pozisyonunu hesapla
        Vector3 recoilPos = originalScopePos + new Vector3(recoilOffset.x, recoilOffset.y, 0);
        scopeVisual.transform.localPosition = recoilPos;

        float elapsed = 0f;
        float recoilDuration = 0.2f; // Recoil süresini ayarlayabilirsin

        while (elapsed < recoilDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / recoilDuration;

            // Scope’u orijinal pozisyonuna yumuşakça geri döndür
            scopeVisual.transform.localPosition = Vector3.Lerp(recoilPos, originalScopePos, t);

            // Kamerayı titret ve orijinal pozisyonuna geri döndür
            mainCamera.transform.localPosition = originalCameraPos + recoilCameraOffset * (1 - t);

            yield return null;
        }

        // Recoil bittiğinde her şeyi orijinal pozisyonuna sabitle
        scopeVisual.transform.localPosition = originalScopePos;
        mainCamera.transform.localPosition = originalCameraPos;
    }

    /*
    private IEnumerator RecoilCoroutine()
    {
        Vector3 recoilPos = originalScopePos + new Vector3(recoilOffset.x, recoilOffset.y, 0);
        scopeVisual.transform.localPosition = recoilPos;

        float elapsed = 0f;
        while (elapsed < recoilDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / recoilDuration;
            scopeVisual.transform.localPosition = Vector3.Lerp(recoilPos, originalScopePos, t);
            yield return null;
        }
        scopeVisual.transform.localPosition = originalScopePos;
    } */
}