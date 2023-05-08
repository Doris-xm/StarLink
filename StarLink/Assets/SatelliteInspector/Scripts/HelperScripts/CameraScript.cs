using UnityEngine;


/// <summary>
/// Main Camera script
/// </summary>
public class CameraScript : MonoBehaviour
{
    public Vector3 TargetPos;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        TargetPos = Vector3.zero;

        float angle = -65.0f * Time.deltaTime;
        float speed = 10 * Time.deltaTime;
        float zoomspeed = 100 * Time.deltaTime;

        Vector3 view = mainCamera.ScreenToViewportPoint(Input.mousePosition);
        Vector3 centralView = -1 * (Vector3.one - 2 * view);
        centralView[2] = 0.0f;

        transform.Translate(Vector3.forward * (Input.mouseScrollDelta.y * zoomspeed));

        // if (Input.GetKey(KeyCode.Mouse2))
        // if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftCommand))  // Mac
        if (Input.GetMouseButton(0) && Input.GetKey("up")) // Windows
        {
            if (centralView.x < 0)
            {
                transform.RotateAround(TargetPos, transform.TransformDirection(Vector3.up), angle * Mathf.Abs(centralView.x));
            }

            if (centralView.x > 0)
            {
                transform.RotateAround(TargetPos, transform.TransformDirection(Vector3.down), angle * Mathf.Abs(centralView.x));
            }

            if (centralView.y > 0)
            {
                transform.RotateAround(TargetPos, transform.TransformDirection(Vector3.right), angle * Mathf.Abs(centralView.y));
            }

            if (centralView.y < 0)
            {
                transform.RotateAround(TargetPos, transform.TransformDirection(Vector3.left), angle * Mathf.Abs(centralView.y));
            }
        }

        // if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftShift)) // Mac
        if (Input.GetMouseButton(0) && Input.GetKey("down")) // Windows
        {

            if (centralView.x < 0)
            {
                transform.Translate(Vector3.right * (speed * Mathf.Abs(centralView.x)));
            }

            if (centralView.x > 0)
            {
                transform.Translate(Vector3.left * (speed * Mathf.Abs(centralView.x)));
            }

            if (centralView.y > 0)
            {
                transform.Translate(Vector3.down * (speed * Mathf.Abs(centralView.y)));
            }

            if (centralView.y < 0)
            {
                transform.Translate(Vector3.up * (speed * Mathf.Abs(centralView.y)));
            }
        }
    }
}
