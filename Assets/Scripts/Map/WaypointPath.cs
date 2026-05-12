using UnityEngine;

/// <summary>
/// 사각형 루프(Square Loop) 형태의 웨이포인트 경로를 정의한다.
/// 적들은 이 경로를 따라 11시 → 7시 → 5시 → 1시 방향으로 순환한다.
/// </summary>
public class WaypointPath : MonoBehaviour
{
    [Header("웨이포인트 배열 (순서대로 배치)")]
    [Tooltip("사각형 모서리 + 중간 지점들을 순서대로 배치")]
    public Transform[] waypoints;

    [Header("설정")]
    public bool isLoop = true; // 순환 경로 여부
    public Color gizmoColor = Color.yellow;

    /// <summary>
    /// 다음 웨이포인트 인덱스를 반환한다 (순환 처리).
    /// </summary>
    public int GetNextIndex(int currentIndex)
    {
        int next = currentIndex + 1;
        if (isLoop && next >= waypoints.Length)
            return 0; // 순환
        return Mathf.Min(next, waypoints.Length - 1);
    }

    /// <summary>
    /// 특정 인덱스의 웨이포인트 위치를 반환한다.
    /// </summary>
    public Vector3 GetWaypointPosition(int index)
    {
        if (waypoints == null || waypoints.Length == 0)
            return Vector3.zero;
        index = Mathf.Clamp(index, 0, waypoints.Length - 1);
        return waypoints[index].position;
    }

    /// <summary>
    /// 전체 경로 길이를 계산한다.
    /// </summary>
    public float GetTotalPathLength()
    {
        float total = 0f;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            total += Vector3.Distance(waypoints[i].position, waypoints[i + 1].position);
        }
        if (isLoop && waypoints.Length > 1)
        {
            total += Vector3.Distance(waypoints[waypoints.Length - 1].position, waypoints[0].position);
        }
        return total;
    }

    /// <summary>
    /// 에디터에서 경로를 시각적으로 표시한다.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = gizmoColor;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                Gizmos.DrawSphere(waypoints[i].position, 0.3f);
            }
        }

        // 마지막 웨이포인트
        if (waypoints[waypoints.Length - 1] != null)
        {
            Gizmos.DrawSphere(waypoints[waypoints.Length - 1].position, 0.3f);
        }

        // 순환 경로면 마지막 → 첫 번째 연결
        if (isLoop && waypoints.Length > 1)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(waypoints[waypoints.Length - 1].position, waypoints[0].position);
        }
    }
}
