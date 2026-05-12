using UnityEngine;

/// <summary>
/// 탑다운 뷰 카메라 컨트롤러.
/// 마우스 휠로 줌, 화살표 키로 이동 가능.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("설정")]
    public float moveSpeed = 20f;
    public float zoomSpeed = 5f;
    public float minZoom = 15f;
    public float maxZoom = 50f;

    [Header("초기 위치")]
    public Vector3 defaultPosition = new Vector3(0f, 30f, -15f);
    public Vector3 defaultRotation = new Vector3(60f, 0f, 0f);

    private void Start()
    {
        transform.position = defaultPosition;
        transform.eulerAngles = defaultRotation;
    }

    private void Update()
    {
        // 이동
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(h, 0f, v) * moveSpeed * Time.deltaTime;
        transform.Translate(move, Space.World);

        // 줌
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            Vector3 pos = transform.position;
            pos.y -= scroll * zoomSpeed;
            pos.y = Mathf.Clamp(pos.y, minZoom, maxZoom);
            transform.position = pos;
        }
    }
}
