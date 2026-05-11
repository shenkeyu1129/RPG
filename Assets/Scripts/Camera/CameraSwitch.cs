using UnityEngine;
using Cinemachine;

public class CameraSwitch : MonoBehaviour
{
    [SerializeField] private CinemachineFreeLook thirdPersonCam;
    [SerializeField] private CinemachineVirtualCamera firstPersonCam;
    
    public static bool isFirstPerson = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            SwitchCamera();
            CameraEvents.Center.Trigger(CameraEvent.SwitchCamera);
        }
    }

    void SwitchCamera()
    {
        isFirstPerson = !isFirstPerson;

        if (isFirstPerson)
        {
            firstPersonCam.Priority = 20;
            thirdPersonCam.Priority = 10;
        }
        else
        {
            firstPersonCam.Priority = 10;
            thirdPersonCam.Priority = 20;
        }
    }
}