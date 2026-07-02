using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class AspectRatioController : MonoBehaviour
{
    [SerializeField] private float targetAspect = 16 / 9f;
    [SerializeField] private float cameraAspect;

    private void Start()
    {
        cameraAspect = GetCameraAspect();

        ApplyLetteringBox();
    }

    private void LateUpdate()
    {
        float newCameraAspect = GetCameraAspect();
        if (cameraAspect != newCameraAspect)
        {
            ApplyLetteringBox();
        }
    }

    private void ApplyLetteringBox()
    {
        float targetToCamera = targetAspect / cameraAspect;
        if (targetToCamera < 1f)
        {
            // 카메라 width가 원본보다 크다
            // width를 줄여야 한다
            Camera.main.rect = new Rect((1 - targetToCamera) / 2, 0f, targetToCamera, 1f);
        }
        else
        {
            float cameraToTarget = 1 / targetToCamera;
            Camera.main.rect = new Rect(0f, (1 - cameraToTarget) / 2, 1f, cameraToTarget);
        }
    }

    private float GetCameraAspect()
    {
        int width = Camera.main.pixelWidth;
        int height = Camera.main.pixelHeight;
        return width / (float)height;
    }

}
