using Golf_Course.Scripts.Managers;
using UnityEngine;

namespace Golf_Course.Scripts
{
    public class GolfBall : MonoBehaviour
    {
        public BallLevel BallLevel { get; private set; }
        public Vector3 BallPosition { get; private set; }
        public int BallPoints { get; private set; }

        [SerializeField]
        private Collider ballCollider;

        [SerializeField]
        private Rigidbody ballRigidbody;

        [SerializeField]
        private Transform ballTransform;

        public void Initialize(BallLevel ballLevel, Vector3 ballPosition)
        {
            BallLevel = ballLevel;
            BallPosition = ballPosition;
            transform.position = BallPosition;
            BallPoints = PointManager.Instance.GetPoints(BallLevel);
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
            ballTransform.SetParent(null);
            ballCollider.enabled = true;
            ballRigidbody.isKinematic = false;
        }
    }
}