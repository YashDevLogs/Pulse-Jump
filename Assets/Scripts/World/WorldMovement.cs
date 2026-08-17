using UnityEngine;

public class WorldMovement : MonoBehaviour
{
    public static WorldMovement Instance { get; private set; }

    [Header("Movement")]
    [SerializeField] private float baseSpeed = 8f;
    [SerializeField] private float maxSpeed = 14f;

    public float CurrentSpeed { get; private set; }

    private bool isMoving;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CurrentSpeed = baseSpeed;
        Debug.Log("WorldMovement: Initialized with base speed " + baseSpeed);
    }

    private void Update()
    {
        if (!isMoving)
            return;

        float speed = CurrentSpeed;

        transform.position += Vector3.back * (speed * Time.deltaTime);
    }

    public void StartMoving()
    {
        isMoving = true;
        Debug.Log("WorldMovement: Started moving.");
    }

    public void StopMoving()
    {
        isMoving = false;
        Debug.Log("WorldMovement: Stopped moving.");
    }

    public void SetSpeed(float speed)
    {
        CurrentSpeed = Mathf.Clamp(speed, 0f, maxSpeed);
    }
}