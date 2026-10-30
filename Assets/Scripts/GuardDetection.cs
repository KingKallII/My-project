using UnityEngine;
using UnityEngine.UI;

public class GuardDetection : MonoBehaviour
{
    public float detection = 0f;

    [Header("Detection Settings")]
    public float detectionTime = 0.5f;
    public float recoveryTime = 1.5f;

    public Slider detectionBar;

    private GuardVision guardVision;
    private GuardPatrol guardPatrol;

    void Start()
    {
        guardVision = GetComponent<GuardVision>();
        guardPatrol = GetComponent<GuardPatrol>();

        if (detectionBar != null)
        {
            detectionBar.minValue = 0f;
            detectionBar.maxValue = 100f;
            detectionBar.value = 0f;
        }
    }

    void Update()
    {
        if (guardVision == null)
            return;

        // =========================================
        // PLAYER TERLIHAT
        // =========================================

        if (guardVision.PlayerIsVisible())
        {
            detection += (100f / detectionTime) * Time.deltaTime;
        }
        else
        {
            // =========================================
            // PLAYER TIDAK TERLIHAT
            // =========================================

            detection -= (100f / recoveryTime) * Time.deltaTime;
        }

        detection = Mathf.Clamp(detection, 0f, 100f);

        // Update Detection Bar
        if (detectionBar != null)
        {
            detectionBar.value = detection;
        }

        // =========================================
        // DETECTION PENUH
        // =========================================

        if (detection >= 100f)
        {
            StartGuardChase();
        }

        // =========================================
        // DETECTION HABIS
        // =========================================

        if (detection <= 0f)
        {
            StopGuardChase();
        }
    }

    void StartGuardChase()
    {
        if (guardPatrol != null &&
            !guardPatrol.IsChasing())
        {
            guardPatrol.StartChase();

            Debug.Log("GUARD MULAI MENGEJAR!");
        }
    }

    void StopGuardChase()
    {
        if (guardPatrol != null &&
            guardPatrol.IsChasing())
        {
            guardPatrol.StopChase();

            Debug.Log("GUARD KEHILANGAN PLAYER!");
        }
    }
}