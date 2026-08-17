using System.Collections;
using UnityEngine;

public class MainMenuPlayerAnimation : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;

    [Header("Positions")]
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform centerPoint;
    [SerializeField] private Transform rightPoint;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float jumpDuration = 0.8f;

    [Header("Center")]
    [SerializeField] private float centerWaitDuration = 0.2f;

    [Header("Pulse")]
    [SerializeField] private Shockwave shockwave;
    [SerializeField] private float pulseAtNormalizedTime = 0.5f;

    [Header("Rolling")]
    [SerializeField] private float rollSpeed = 360f;

    [Header("Gameplay Scripts To Disable")]
    [SerializeField] private Behaviour[] gameplayScripts;


    private void Start()
    {
        DisableGameplay();

        StartCoroutine(MenuLoop());
    }


    private void DisableGameplay()
    {
        foreach (Behaviour script in gameplayScripts)
        {
            if (script != null)
            {
                script.enabled = false;
            }
        }

        Rigidbody rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }


    private IEnumerator MenuLoop()
    {
        while (true)
        {
            // -----------------------------------------
            // RESET
            // -----------------------------------------

            transform.position = leftPoint.position;


            // -----------------------------------------
            // MOVE LEFT → CENTER
            // -----------------------------------------

            yield return MoveTo(
                centerPoint.position
            );


            // -----------------------------------------
            // SMALL WAIT
            // -----------------------------------------

            yield return new WaitForSeconds(
                centerWaitDuration
            );


            // -----------------------------------------
            // JUMP + PULSE
            // -----------------------------------------

            yield return JumpAndPulse();


            // -----------------------------------------
            // MOVE CENTER → RIGHT
            // -----------------------------------------

            yield return MoveTo(
                rightPoint.position
            );


            // -----------------------------------------
            // LOOP
            // -----------------------------------------

            yield return null;
        }
    }


    private IEnumerator MoveTo(Vector3 target)
    {
        while (
            Vector3.Distance(
                transform.position,
                target
            ) > 0.02f
        )
        {
            transform.position =
                Vector3.MoveTowards(
                    transform.position,
                    target,
                    moveSpeed * Time.deltaTime
                );


            // -----------------------------------------
            // ROLL
            // -----------------------------------------
            // Movement is along Z, so roll around X.
            // -----------------------------------------

            transform.Rotate(
                Vector3.right,
                rollSpeed * Time.deltaTime,
                Space.Self
            );

            yield return null;
        }

        transform.position = target;
    }


    private IEnumerator JumpAndPulse()
    {
        Vector3 startPosition =
            transform.position;

        float elapsed = 0f;

        bool pulseTriggered = false;


        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / jumpDuration
                );


            // -----------------------------------------
            // HORIZONTAL POSITION
            // -----------------------------------------

            transform.position =
                startPosition;


            // -----------------------------------------
            // PARABOLIC JUMP
            // -----------------------------------------

            float height =
                Mathf.Sin(
                    t * Mathf.PI
                ) * jumpHeight;


            transform.position +=
                Vector3.up * height;


            // -----------------------------------------
            // ROLL IN AIR
            // -----------------------------------------

            transform.Rotate(
                Vector3.right,
                rollSpeed * Time.deltaTime,
                Space.Self
            );


            // -----------------------------------------
            // PULSE AT APEX
            // -----------------------------------------

            if (!pulseTriggered &&
                t >= pulseAtNormalizedTime)
            {
                pulseTriggered = true;

                if (shockwave != null)
                {
                    shockwave.Launch();

                    Debug.Log(
                        "Main Menu: Shockwave triggered."
                    );
                }
            }

            yield return null;
        }


        // Make absolutely sure we return
        // to the original ground position.

        transform.position =
            startPosition;
    }
}