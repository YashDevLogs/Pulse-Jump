using System;
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class PerfectCinematicController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private CinemachineCamera cinematicCamera;

    [Header("Cinemachine Priority")]
    [SerializeField] private int normalPriority = 0;
    [SerializeField] private int cinematicPriority = 20;

    [Header("Cinematic")]
    [SerializeField] private float slowMotionScale = 0.25f;
    [SerializeField] private float cameraBlendTime = 0.15f;
    [SerializeField] private float cinematicDuration = 1.2f;

    private Coroutine cinematicCoroutine;

    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;

    private float originalFixedDeltaTime;

    [SerializeField]
    private PerfectImpactPostProcess perfectImpactPostProcess;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (cinemachineBrain == null && mainCamera != null)
        {
            cinemachineBrain =
                mainCamera.GetComponent<CinemachineBrain>();
        }

        if (cinematicCamera == null)
        {
            cinematicCamera =
                GetComponent<CinemachineCamera>();
        }

        originalFixedDeltaTime =
            Time.fixedDeltaTime;

        // Cinematic camera is inactive during normal gameplay.
        if (cinematicCamera != null)
        {
            cinematicCamera.Priority =
                normalPriority;
        }

        // Normal gameplay is handled directly by Main Camera.
        if (cinemachineBrain != null)
        {
            cinemachineBrain.enabled = false;
        }
    }

    public void PlayPerfectImpact(Action impactAction)
    {
        if (cinematicCoroutine != null)
        {
            StopCoroutine(cinematicCoroutine);

            // Safety restoration if another cinematic was interrupted.
            RestoreCameraState();
        }

        cinematicCoroutine =
            StartCoroutine(
                PerfectImpactRoutine(
                    impactAction
                )
            );
    }

    private IEnumerator PerfectImpactRoutine(
        Action impactAction)
    {
        Debug.Log(
            "PERFECT CINEMATIC START"
        );

        if (mainCamera == null ||
            cinemachineBrain == null ||
            cinematicCamera == null)
        {
            Debug.LogError(
                "PerfectCinematicController: " +
                "Missing camera reference."
            );

            cinematicCoroutine = null;
            yield break;
        }

        // ==================================================
        // SAVE MAIN CAMERA
        // ==================================================

        originalCameraPosition =
            mainCamera.transform.position;

        originalCameraRotation =
            mainCamera.transform.rotation;

        // ==================================================
        // ENABLE CINEMACHINE
        // ==================================================

        cinemachineBrain.enabled = true;

        cinematicCamera.Priority =
            cinematicPriority;

        Debug.Log(
            "Perfect Cinematic Camera activated."
        );

        // ==================================================
        // SLOW MOTION
        // ==================================================

        Time.timeScale =
            slowMotionScale;

        Time.fixedDeltaTime =
            originalFixedDeltaTime *
            slowMotionScale;

        // ==================================================
        // ALLOW CAMERA TO SWITCH
        // ==================================================

        yield return new WaitForSecondsRealtime(
            cameraBlendTime
        );

        // ==================================================
        // IMPACT
        // ==================================================

        Debug.Log("PERFECT CINEMATIC IMPACT!");

        if (perfectImpactPostProcess != null)
        {
            perfectImpactPostProcess.PlayImpact();
        }
        else
        {
            Debug.LogError(
                "PerfectCinematicController: PerfectImpactPostProcess reference is missing."
            );
        }

        impactAction?.Invoke();

        // ==================================================
        // HOLD
        // ==================================================

        yield return new WaitForSecondsRealtime(
            cinematicDuration
        );

        // ==================================================
        // STOP CINEMATIC CAMERA
        // ==================================================

        cinematicCamera.Priority =
            normalPriority;

        // Allow Cinemachine to finish its transition.
        yield return new WaitForSecondsRealtime(
            cameraBlendTime
        );

        // ==================================================
        // DISABLE CINEMACHINE
        // ==================================================

        cinemachineBrain.enabled = false;

        // ==================================================
        // RESTORE EXACT MAIN CAMERA
        // ==================================================

        mainCamera.transform.SetPositionAndRotation(
            originalCameraPosition,
            originalCameraRotation
        );

        // ==================================================
        // RESTORE TIME
        // ==================================================

        Time.timeScale = 1f;

        Time.fixedDeltaTime =
            originalFixedDeltaTime;

        Debug.Log(
            "PERFECT CINEMATIC END"
        );

        cinematicCoroutine = null;
    }

    private void RestoreCameraState()
    {
        if (cinematicCamera != null)
        {
            cinematicCamera.Priority =
                normalPriority;
        }

        if (cinemachineBrain != null)
        {
            cinemachineBrain.enabled = false;
        }

        if (mainCamera != null)
        {
            mainCamera.transform.SetPositionAndRotation(
                originalCameraPosition,
                originalCameraRotation
            );
        }

        Time.timeScale = 1f;

        Time.fixedDeltaTime =
            originalFixedDeltaTime;
    }
}