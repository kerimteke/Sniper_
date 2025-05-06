using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class SniperTouchController:MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    // Animator: Sniper modeline bağlı Animator Controller, trigger'lar "OnScope" ve "OutScope" tanımlı.
    public Animator animatorum;

   // public Animation OnAnimation;
    // public Animation OffAnimation;
    public GameObject SniperGovde;
    public GameObject ScopeImage;
    
    
    private float currentYaw = 0f;      // Kamera dönüş açılarının tutulması (yaw ve pitch)
    private float currentPitch = 0f;
    private Vector2 previousPointerPosition;// Dokunma sırasında eski pozisyonu tutmak için

    public Camera SniperCamera;// Raycast için
    public float raycastMaxDistance = 100f;// Raycast’in maksimum mesafe.
    public float rotationSensitivity = 0.1f;// Parmağın hareketine bağlı olarak dönüş hassasiyeti.

    
    // Scope aktif olup olmadığını takip eder.
   //  private bool isScoping = false;

    public void OnEventTriggered()
    {
        StartCoroutine(ExecuteSequence());
        StartCoroutine(ExecuteSequence2());
    }

    IEnumerator ExecuteSequence()
    {
       // Debug.Log("girdi mi");

        animatorum.SetTrigger("OnScope");
        // Debug.Log("KARA1");
        yield return new WaitForSeconds(0.2f);
        // Debug.Log("KARA2");

        ScopeImage.SetActive(true);
        //Debug.Log("KARA3");

        SniperGovde.SetActive(false);
    }
    IEnumerator ExecuteSequence2()
    {
        Debug.Log("kokoş mi");
        SniperGovde.SetActive(true);
        ScopeImage.SetActive(false);
        animatorum.SetTrigger("OffScope");
        yield return StartCoroutine(Karman());
        /*
        yield return new WaitForSeconds(0.2f);
        animatorum.SetTrigger("OutScope");
       */

    }

    IEnumerator Karman()
    {
        animatorum.SetTrigger("OffScope");
        yield return new WaitForEndOfFrame();
    }


    public void OnDrag(PointerEventData eventData)
    {

    }
    
    
    public void OnPointerDown(PointerEventData eventData)
     {
        //Debug.Log("girMEdi mi");

        StartCoroutine(ExecuteSequence());

        previousPointerPosition = eventData.position;


     }

     public void OnPointerUp(PointerEventData eventData)
     {
        StartCoroutine(ExecuteSequence2());

     }
    
}


