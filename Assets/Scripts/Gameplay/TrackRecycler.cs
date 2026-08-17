using UnityEngine;

public class TrackRecycler : MonoBehaviour
{
    [Header("Track Setup")]
    [SerializeField] private Transform[] trackSegments;
    [SerializeField] private float segmentLength = 90f;

    [Header("Recycling")]
    [SerializeField] private float recycleZ = -90f;

    private void Update()
    {

        if (GameManager.Instance != null &&
    GameManager.Instance.CurrentState != GameState.Playing)
            return;

        if (trackSegments == null || trackSegments.Length == 0)
            return;

        for (int i = 0; i < trackSegments.Length; i++)
        {
            Transform segment = trackSegments[i];

            if (segment.position.z <= recycleZ)
            {
                RecycleSegment(segment);
            }
        }
    }

    private void RecycleSegment(Transform segment)
    {
        Transform lastSegment = GetFarthestSegment();

        Vector3 newPosition = lastSegment.position;
        newPosition.z += segmentLength;

        segment.position = newPosition;

        Debug.Log(
            $"Track recycled: {segment.name} → Z {newPosition.z}"
        );
    }

    private Transform GetFarthestSegment()
    {
        Transform farthest = trackSegments[0];

        for (int i = 1; i < trackSegments.Length; i++)
        {
            if (trackSegments[i].position.z > farthest.position.z)
            {
                farthest = trackSegments[i];
            }
        }

        return farthest;
    }
}