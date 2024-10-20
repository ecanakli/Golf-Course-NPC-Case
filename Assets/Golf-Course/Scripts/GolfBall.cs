using UnityEngine;

namespace Golf_Course.Scripts
{
    public class GolfBall : MonoBehaviour
    {
        public BallLevel BallLevel { get; private set; }
        public Vector3 BallPosition { get; private set; }
        public int BallPoint { get; private set; }

        [SerializeField]
        private Collider ballCollider;

        [SerializeField]
        private Rigidbody ballRigidbody;

        [SerializeField]
        private Transform ballTransform;
        
        [SerializeField]
        private Renderer  ballRenderer;

        public void Initialize(BallLevel ballLevel, Vector3 ballPosition, int ballPoint, Material material)
        {
            BallLevel = ballLevel;
            BallPosition = ballPosition;
            BallPoint = ballPoint;
            ballRenderer.material = material;
            transform.position = BallPosition;
        }

        public void OnPickUp(Transform pickUpTransform)
        {
            ballCollider.enabled = false;
            ballRigidbody.isKinematic = true;

            ballTransform.SetParent(pickUpTransform);
            ballTransform.localPosition = Vector3.zero;
        }

        public void OnDropOff(Transform dropTransform)
        {
            ballTransform.SetParent(dropTransform);
            ballCollider.enabled = true;
            ballRigidbody.isKinematic = false;
        }

        public void ResetBall()
        {
            gameObject.SetActive(false);
            ballCollider.enabled = true;
            ballRigidbody.isKinematic = true;
        }
    }
}