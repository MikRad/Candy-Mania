using UnityEngine;

public class CameraWrapper : MonoBehaviour
{
    public static CameraWrapper Instance { get; private set; }
    private Camera MainCamera { get; set; }

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
        return MainCamera.ScreenToWorldPoint(screenPosition);
    }
    
    public Ray ScreenPointToRay(Vector3 screenPosition)
    {
        return MainCamera.ScreenPointToRay(screenPosition);
    }
    
    private void InitView()
    {
        MainCamera = Camera.main;
        
        if (MainCamera == null)
        {
            Debug.LogError($"{nameof(CameraWrapper)}:{nameof(InitView)} - main camera is null !");
            return;
        }

        Transform mainCameraTransform = MainCamera.transform;

        Vector2 screenSizeWorld = MainCamera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        
        Vector3 cameraPos = mainCameraTransform.position;
        cameraPos.x = screenSizeWorld.x;
        cameraPos.y = -screenSizeWorld.y;

        mainCameraTransform.position = cameraPos;
    }
}
