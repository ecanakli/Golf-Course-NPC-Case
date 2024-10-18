using UnityEngine;

namespace Golf_Course.Scripts
{
    public class BillboardEffect : MonoBehaviour
    {
        [SerializeField]
        private Camera mainCamera;

        private void LateUpdate()
        {
            if (mainCamera == null)
            {
                return;
            }

            var rotation = mainCamera.transform.rotation;
            transform.LookAt(transform.position + rotation * Vector3.forward,
                rotation * Vector3.up);
        }
    }
}