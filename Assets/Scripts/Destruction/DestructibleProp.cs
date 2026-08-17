using UnityEngine;

public class DestructibleProp : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private BarrierType barrierType;

    [Header("References")]
    [SerializeField] private GameObject intactVisual;
    [SerializeField] private GameObject fracturedVisual;
    [SerializeField] private Collider gameplayCollider;

    [Header("Explosion")]
    [SerializeField] private float explosionForce = 8f;
    [SerializeField] private float explosionRadius = 4f;
    [SerializeField] private float upwardModifier = 1f;

    private bool destroyed;

    private Rigidbody[] fracturedRigidbodies;

    private Vector3[] originalPositions;
    private Quaternion[] originalRotations;
    private Vector3[] originalScales;

    private bool[] originalKinematicStates;

    private bool[] originalColliderStates;

    public enum BarrierType
    {
        Glass,
        Wood
    }



    private void Awake()
    {
        if (gameplayCollider == null)
        {
            gameplayCollider =
                GetComponent<Collider>();
        }

        if (fracturedVisual != null)
        {
            fracturedRigidbodies =
                fracturedVisual.GetComponentsInChildren<Rigidbody>(true);

            originalPositions =
                new Vector3[fracturedRigidbodies.Length];

            originalRotations =
                new Quaternion[fracturedRigidbodies.Length];

            originalScales =
                new Vector3[fracturedRigidbodies.Length];

            originalKinematicStates =
                new bool[fracturedRigidbodies.Length];

            originalColliderStates =
                new bool[fracturedRigidbodies.Length];

            for (int i = 0;
                 i < fracturedRigidbodies.Length;
                 i++)
            {
                Rigidbody rb =
                    fracturedRigidbodies[i];

                Transform piece =
                    rb.transform;

                originalPositions[i] =
                    piece.localPosition;

                originalRotations[i] =
                    piece.localRotation;

                originalScales[i] =
                    piece.localScale;

                originalKinematicStates[i] =
                    rb.isKinematic;

                Collider pieceCollider =
                    rb.GetComponent<Collider>();

                if (pieceCollider != null)
                {
                    originalColliderStates[i] =
                        pieceCollider.enabled;
                }
            }
        }
    }


    public PulseTiming DestroyProp(
     Vector3 hitPosition,
     Vector3 playerPosition,
     PulseTiming? forcedTiming = null)
    {
        if (destroyed)
            return PulseTiming.None;

        destroyed = true;

        // Play barrier breaking sound once.
        if (AudioManager.Instance != null)
        {
            if (barrierType == BarrierType.Glass)
            {
                AudioManager.Instance.PlayGlassBreak(
                    transform.position
                );
            }
            else if (barrierType == BarrierType.Wood)
            {
                AudioManager.Instance.PlayWoodBreak(
                    transform.position
                );
            }
        }

        PulseTiming timing;

        // --------------------------------------------------
        // GET TIMING
        // --------------------------------------------------

        if (forcedTiming.HasValue)
        {
            timing = forcedTiming.Value;
        }
        else
        {
            timing = PulseTiming.Good;

            PulseGate gate =
                GetComponent<PulseGate>();

            if (gate != null)
            {
                timing =
                    gate.EvaluateTiming(playerPosition);
            }
        }

        // --------------------------------------------------
        // SCORE
        // --------------------------------------------------

        if (ScoreManager.Instance != null)
        {
            bool triggerCinematic =
                !forcedTiming.HasValue;

            ScoreManager.Instance.AddBarrierScore(
                timing,
                triggerCinematic
            );
        }

        // --------------------------------------------------
        // DISABLE ONLY THE GAMEPLAY COLLIDER
        // --------------------------------------------------

        if (gameplayCollider != null)
        {
            gameplayCollider.enabled = false;
        }

        // --------------------------------------------------
        // HIDE INTACT VERSION
        // --------------------------------------------------

        if (intactVisual != null)
        {
            intactVisual.SetActive(false);
        }

        // --------------------------------------------------
        // SHOW FRACTURED VERSION
        // --------------------------------------------------

        if (fracturedVisual != null)
        {
            fracturedVisual.SetActive(true);
        }

        // --------------------------------------------------
        // ACTIVATE FRACTURED PHYSICS
        // --------------------------------------------------

        if (fracturedRigidbodies != null)
        {
            foreach (Rigidbody rb in fracturedRigidbodies)
            {
                rb.isKinematic = false;

                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                rb.AddExplosionForce(
                    explosionForce,
                    hitPosition,
                    explosionRadius,
                    upwardModifier,
                    ForceMode.Impulse
                );
            }
        }

        Debug.Log(
            $"DestructibleProp destroyed: {gameObject.name} | " +
            $"Timing: {timing}"
        );

        return timing;
    }


    public void ResetDestruction()
    {
        destroyed = false;


        // --------------------------------------------------
        // COLLISION
        // --------------------------------------------------

        if (gameplayCollider != null)
        {
            gameplayCollider.enabled = true;
        }


        // --------------------------------------------------
        // VISUALS
        // --------------------------------------------------

        if (intactVisual != null)
        {
            intactVisual.SetActive(true);
        }

        if (fracturedVisual != null)
        {
            fracturedVisual.SetActive(false);
        }


        // --------------------------------------------------
        // RESET FRACTURED PIECES
        // --------------------------------------------------

        if (fracturedRigidbodies != null)
        {
            for (int i = 0;
                 i < fracturedRigidbodies.Length;
                 i++)
            {
                Rigidbody rb =
                    fracturedRigidbodies[i];

                rb.isKinematic = false;

                rb.linearVelocity =
                    Vector3.zero;

                rb.angularVelocity =
                    Vector3.zero;

                Transform piece =
                    rb.transform;

                piece.localPosition =
                    originalPositions[i];

                piece.localRotation =
                    originalRotations[i];

                piece.localScale =
                    originalScales[i];

                rb.isKinematic =
                    originalKinematicStates[i];

                Collider pieceCollider =
                    rb.GetComponent<Collider>();

                if (pieceCollider != null)
                {
                    pieceCollider.enabled =
                        originalColliderStates[i];
                }
            }
        }

        PulseGate gate =
            GetComponent<PulseGate>();

        if (gate != null)
        {
            gate.ResetBarrier();
        }


        Debug.Log(
            "DestructibleProp reset: " +
            gameObject.name
        );
    }

    public void PrepareForPerfectCinematic()
    {
        if (destroyed)
            return;

        // Disable ONLY the gameplay collision.
        //
        // Do NOT disable every collider in the hierarchy.
        // The fractured pieces need their own physics colliders
        // once the barrier is destroyed.
        if (gameplayCollider != null)
        {
            gameplayCollider.enabled = false;
        }

        Debug.Log(
            "Barrier prepared for Perfect cinematic: " +
            gameObject.name
        );
    }
}