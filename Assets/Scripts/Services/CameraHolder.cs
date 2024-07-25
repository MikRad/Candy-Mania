using UnityEngine;

public class CameraHolder : MonoBehaviour
{
    private Camera _mainCamera;
    
    public static CameraHolder Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        InitView();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public Vector3 ScreenToWorldPoint(Vector3 screenPosition)
    {
        return _mainCamera.ScreenToWorldPoint(screenPosition);
    }
    
    public Ray ScreenPointToRay(Vector3 screenPosition)
    {
        return _mainCamera.ScreenPointToRay(screenPosition);
    }
    
    private void InitView()
    {
        _mainCamera = Camera.main;
        
        if (_mainCamera == null)
        {
            Debug.LogError($"{nameof(CameraHolder)}:{nameof(InitView)} - main camera is null !");
            return;
        }

        Transform mainCameraTransform = _mainCamera.transform;

        Vector2 screenSizeWorld = _mainCamera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        
        Vector3 cameraPos = mainCameraTransform.position;
        cameraPos.x = screenSizeWorld.x;
        cameraPos.y = -screenSizeWorld.y;

        mainCameraTransform.position = cameraPos;
    }
}
