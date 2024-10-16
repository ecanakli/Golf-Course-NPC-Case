using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace Golf_Course.Scripts.Managers
{
    public class NPCController : Singleton<NPCController>
    {
        [Header("NPC References")]
        [SerializeField]
        private NavMeshAgent npcNavMeshAgent;

        public NavMeshAgent NpcNavMeshAgent => npcNavMeshAgent;

        [SerializeField]
        private NPCDecisionSystem npcDecisionSystem;

        [SerializeField]
        private Transform golfCart;

        public Transform GolfCart => golfCart;

        [SerializeField]
        private Transform pickUpTransform;

        [SerializeField]
        private Transform dropTransform;

        [Header("NPC Adjustments")]
        [SerializeField]
        private float maxHealth = 100f;

        public float MaxHealth => maxHealth;

        [SerializeField]
        private float healthDecreaseRate = 1;

        public float HealthDecreaseRate => healthDecreaseRate;

        private float _health;
        private NPCStatus _npcStatus = NPCStatus.Stopped;
        private GolfBall _currentBall;
        private CancellationTokenSource _moveCancellationTokenSource;

        public void StartNpcLifeCycle()
        {
            _health = maxHealth;
            _moveCancellationTokenSource?.Cancel();
            _moveCancellationTokenSource = new CancellationTokenSource();
            var moveToken = _moveCancellationTokenSource.Token;
            StartReducingHealth(moveToken).Forget();
            StartNPCLoop(moveToken).Forget();
        }

        private async UniTask StartNPCLoop(CancellationToken token)
        {
            while (_health > 0 && !token.IsCancellationRequested && _npcStatus != NPCStatus.Finished)
            {
                if (_npcStatus == NPCStatus.Stopped)
                {
                    _currentBall = npcDecisionSystem.MakeDecision(transform.position, _health);
                    if (_currentBall == null)
                    {
                        if (BallManager.Instance.GetGolfBalls().Count == 0)
                        {
                            _npcStatus = NPCStatus.Finished;
                            break;
                        }

                        await UniTask.Delay(500, cancellationToken: token);
                        continue;
                    }

                    _npcStatus = NPCStatus.MovingToBall;
                    await MoveToBall(_currentBall, token);

                    _npcStatus = NPCStatus.PickingUp;
                    await PickUp(_currentBall, token);

                    _npcStatus = NPCStatus.MovingToCart;
                    await MoveToGolfCart(token);

                    _npcStatus = NPCStatus.DroppingOff;
                    await DropOff(_currentBall, token);

                    BallManager.Instance.RemoveBall(_currentBall);
                    _currentBall = null;
                    _npcStatus = NPCStatus.Stopped;
                    await UniTask.Delay(500, cancellationToken: token);
                }
                else
                {
                    await UniTask.Yield(token);
                }
            }

            _npcStatus = NPCStatus.Finished;
            OnGameFinished();
        }

        public Vector3 GetNPCPosition()
        {
            return transform.position;
        }

        private async UniTask StartReducingHealth(CancellationToken token)
        {
            while (_health > 0 && !token.IsCancellationRequested)
            {
                _health -= healthDecreaseRate * Time.deltaTime;
                if (_health <= 0)
                {
                    _npcStatus = NPCStatus.Finished;
                    break;
                }

                await UniTask.Yield(token);
            }
        }

        private async UniTask MoveToBall(GolfBall ball, CancellationToken token)
        {
            npcNavMeshAgent.SetDestination(ball.BallPosition);
            await UniTask.WaitUntil(IsDestinationReached, cancellationToken: token);
        }

        private async UniTask MoveToGolfCart(CancellationToken token)
        {
            npcNavMeshAgent.SetDestination(golfCart.position);
            await UniTask.WaitUntil(IsDestinationReached, cancellationToken: token);
        }

        private async UniTask PickUp(GolfBall ball, CancellationToken token)
        {
            await UniTask.Delay(500, cancellationToken: token);
            ball.OnPickUp(pickUpTransform);
        }

        private async UniTask DropOff(GolfBall ball, CancellationToken token)
        {
            await UniTask.Delay(500, cancellationToken: token);
            ball.OnDropOff(dropTransform);
        }

        private bool IsDestinationReached()
        {
            if (npcNavMeshAgent.pathPending)
            {
                return false;
            }

            if (!(npcNavMeshAgent.remainingDistance <= npcNavMeshAgent.stoppingDistance))
            {
                return false;
            }

            return !npcNavMeshAgent.hasPath || npcNavMeshAgent.velocity.sqrMagnitude == 0f;
        }

        private void OnGameFinished()
        {
            Debug.Log("Finished");
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _moveCancellationTokenSource?.Cancel();
            _moveCancellationTokenSource?.Dispose();
        }
    }
}