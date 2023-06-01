using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class CameraViewportHandler : MonoBehaviour
{
    public enum Constraint { Landscape, Portrait }

    #region FIELDS
    public Color wireColor = Color.white;
    public float UnitsSize = 1; // size of your scene in unity units
    public Constraint constraint = Constraint.Portrait;
    public static CameraViewportHandler Instance;
    public new Camera camera;

    public bool executeInUpdate;

    private float _width;
    private float _height;
    //*** bottom screen
    private Vector3 _bottomLeft;
    private Vector3 _bottomCenter;
    private Vector3 _bottomRight;
    //*** middle screen
    private Vector3 _middleLeft;
    private Vector3 _middleCenter;
    private Vector3 _middleRight;
    //*** top screen
    private Vector3 _topLeft;
    private Vector3 _topCenter;
    private Vector3 _topRight;
    #endregion

    #region PROPERTIES
    public float Width
    {
        get
        {
            return _width;
        }
    }
    public float Height
    {
        get
        {
            return _height;
        }
    }

    // helper points:
    public Vector3 BottomLeft
    {
        get
        {
            return _bottomLeft;
        }
    }
    public Vector3 BottomCenter
    {
        get
        {
            return _bottomCenter;
        }
    }
    public Vector3 BottomRight
    {
        get
        {
            return _bottomRight;
        }
    }
    public Vector3 MiddleLeft
    {
        get
        {
            return _middleLeft;
        }
    }
    public Vector3 MiddleCenter
    {
        get
        {
            return _middleCenter;
        }
    }
    public Vector3 MiddleRight
    {
        get
        {
            return _middleRight;
        }
    }
    public Vector3 TopLeft
    {
        get
        {
            return _topLeft;
        }
    }
    public Vector3 TopCenter
    {
        get
        {
            return _topCenter;
        }
    }
    public Vector3 TopRight
    {
        get
        {
            return _topRight;
        }
    }
    #endregion

    #region METHODS
    private void Awake()
    {
        camera = GetComponent<Camera>();
        Instance = this;
        GetResolutionFromAspectRatio();
    }

    private void GetResolutionFromAspectRatio()
    {
        float leftX, rightX, topY, bottomY;

        if (constraint == Constraint.Landscape)
        {
            camera.orthographicSize = 1f / camera.aspect * UnitsSize / 2f;
        }
        else
        {
            camera.orthographicSize = UnitsSize / 2f;
        }

        _height = 2f * camera.orthographicSize;
        _width = _height * camera.aspect;

        float cameraX, cameraY;
        cameraX = camera.transform.position.x;
        cameraY = camera.transform.position.y;

        leftX = cameraX - _width / 2;
        rightX = cameraX + _width / 2;
        topY = cameraY + _height / 2;
        bottomY = cameraY - _height / 2;

        //*** bottom
        _bottomLeft = new Vector3(leftX, bottomY, 0);
        _bottomCenter = new Vector3(cameraX, bottomY, 0);
        _bottomRight = new Vector3(rightX, bottomY, 0);
        //*** middle
        _middleLeft = new Vector3(leftX, cameraY, 0);
        _middleCenter = new Vector3(cameraX, cameraY, 0);
        _middleRight = new Vector3(rightX, cameraY, 0);
        //*** top
        _topLeft = new Vector3(leftX, topY, 0);
        _topCenter = new Vector3(cameraX, topY, 0);
        _topRight = new Vector3(rightX, topY, 0);
    }

    private void Update()
    {
#if UNITY_EDITOR

        if (executeInUpdate)
            GetResolutionFromAspectRatio();

#endif
    }

    void OnDrawGizmos()
    {
        Gizmos.color = wireColor;

        Matrix4x4 temp = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        if (camera.orthographic)
        {
            float spread = camera.farClipPlane - camera.nearClipPlane;
            float center = (camera.farClipPlane + camera.nearClipPlane) * 0.5f;
            Gizmos.DrawWireCube(new Vector3(0, 0, center), new Vector3(camera.orthographicSize * 2 * camera.aspect, camera.orthographicSize * 2, spread));
        }
        else
        {
            Gizmos.DrawFrustum(Vector3.zero, camera.fieldOfView, camera.farClipPlane, camera.nearClipPlane, camera.aspect);
        }
        Gizmos.matrix = temp;
    }
    #endregion

}