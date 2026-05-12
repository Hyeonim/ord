using UnityEngine;

/// <summary>
/// 적 이동 경로를 정의하는 웨이포인트 시스템.
/// </summary>
public class WaypointPath : MonoBehaviour
{
    [Header("웨이포인트")]
    public Transform[] waypoints;

    public int WaypointCount => waypoints != null ? waypoints.Length : 0;

    public Vector3 GetWaypointPosition(int index)
    {
        if (waypoints == null || index < 0 || index >= waypoints.Length)
            return transform.position;
        return waypoints[index].position;
    }

    public float GetTotalPathLength()
    {
        if (waypoints == null || waypoints.Length < 2) return 0f;
        float total = 0f;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            total += Vector3.Distance(waypoints[i].position, waypoints[i + 1].position);
        }
        return total;
    }

    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                Gizmos.DrawSphere(waypoints[i].position, 0.2f);
            }
        }
        if (waypoints[waypoints.Length - 1] != null)
            Gizmos.DrawSphere(waypoints[waypoints.Length - 1].position, 0.2f);
    }
}
