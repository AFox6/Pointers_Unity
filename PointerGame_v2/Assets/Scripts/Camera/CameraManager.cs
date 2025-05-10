using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;

    [SerializeField] private GameObject targetPrefab;
    [SerializeField] private Color mainMenuBGColor;
    private CinemachineCamera virtualCam;
    private Camera cam;


    void Awake(){
        if(instance != null){
            Destroy(instance.gameObject);
        }
        else{
            instance = this;
        }
    }

    private void Start()
    {
        virtualCam = GetComponentInChildren<CinemachineCamera>();
        cam = GetComponentInChildren<Camera>();
    }

    public void SetupCamera(){
        GameObject newTarget = Instantiate(targetPrefab, transform);
        virtualCam.Target.TrackingTarget = newTarget.transform;
    }

    public void UpdateEnvironment(Color _color){
        if(cam == null) cam = GetComponentInChildren<Camera>();
        cam.backgroundColor = _color;
    }
    
    public void RemoveFollowTarget(){
        if(virtualCam == null) virtualCam = GetComponentInChildren<CinemachineCamera>();

        if(virtualCam.Target.TrackingTarget != null){
            Destroy(virtualCam.Target.TrackingTarget.gameObject);
            virtualCam.Target.TrackingTarget = null;
        }
        UpdateEnvironment(mainMenuBGColor);
    }
}
