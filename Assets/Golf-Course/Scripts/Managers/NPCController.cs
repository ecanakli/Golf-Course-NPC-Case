using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

namespace Golf_Course.Scripts.Managers
{
    /// <summary>
    /// Controls the NPC behavior, including movement, health management, and interaction with golf balls.
    /// </summary>
    public class NPCController : Singleton<NPCController>
    {
        [Header("NPC References")]
        [SerializeField]
        private NavMeshAgent npcNavMeshAgent;

        [SerializeField]
        private Slider npcHealthBarSlider;

        [SerializeField]
        private Transform golfCartTransform;

        [SerializeField]
        private Transform pickUpTransform;

        [SerializeField]
        private Animator npcAnimator;

        [Header("NPC Adjustments")]
        [SerializeField]
        private float maxHealth = 100f;

        [SerializeField]
        private float healthDecreaseRate = 1;

        public float MaxHealth
        {
            get => maxHealth;
            set => maxHealth = value;
        }

        public float HealthDecreaseRate
        {
            get => healthDecreaseRate;
            set => healthDecreaseRate = value;
        }

        public Action<float> OnHealthChanged;
        public Action<int> OnPointsEarned;
        public Action OnSuccessful;
        public Action OnFail;

        private float _health;
        private NPCStatus _npcStatus = NPCStatus.Stopped;
        private GolfBall _currentBall;
        private Vector3 _npcStartPosition;
        private CancellationTokenSource _moveCancellationTokenSource;

        [Header("Animation String Hashes")]
        private static readonly int RunningTrigger = Animator.StringToHash("Running");

        private static readonly int PickingUpTrigger = Animator.StringToHash("PickingUp");
        private static readonly int PuttingDownTrigger = Animator.StringToHash("PuttingDown");
        private static readonly int IdleTrigger = Animator.StringToHash("Idle");

        private void OnEnable()
        {
            if (BallManager.Instance != null)
            {
                BallManager.Instance.OnBallsInitialized += StartNpcLifeCycle;
            }
        }

        private void OnDisable()
        {
            if (BallManager.Instance != null)
            {
                BallManager.Instance.OnBallsInitialized -= StartNpcLifeCycle;
            }
        }

        private void Awake()
        {
            _npcStartPosition = transform.position;
        }

        /// <summary>
        /// Starts the NPC life cycle when balls are initialized.
        /// </summary>
        private void StartNpcLifeCycle()
        {
            _moveCancellationTokenSource?.Cancel();
            _moveCancellationTokenSource = new CancellationTokenSource();
            var moveToken = _moveCancellationTokenSource.Token;

            _npcStatus = NPCStatus.Stopped;
            _currentBall = null;
            transform.position = _npcStartPosition;
            SetHealthValues();
            StartReducingHealth(moveToken).Forget();
            StartNPCLoop(moveToken).Forget();
        }

        private void SetHealthValues()
        {
            _health = maxHealth;
            npcHealthBarSlider.maxValue = maxHealth;
            npcHealthBarSlider.value = _health;
        }

        /// <summary>
        /// Main NPC loop that controls the NPC behavior and state transitions.
        /// </summary>
        private async UniTask StartNPCLoop(CancellationToken token)
        {
            while (_health > 0 && !token.IsCancellationRequested && _npcStatus != NPCStatus.Finished)
            {
                if (_npcStatus == NPCStatus.Stopped)
                {
                    _currentBall = MakeDecision(transform.position, _health);
                    if (_currentBall == null)
                    {
                        PlayAnimation(IdleTrigger);
                        if (BallManager.Instance.GetGolfBalls().Count == 0)
                        {
                            _npcStatus = NPCStatus.Finished;
                            OnSuccessful?.Invoke();
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
            OnSuccessful?.Invoke();
        }

        /// <summary>
        /// Decision-making algorithm to determine the most optimal ball to collect,
        /// dynamically balancing between score and distance based on the NPC's current health.
        /// </summary>
        private GolfBall MakeDecision(Vector3 npcPosition, float currentHealth)
        {
            GolfBall bestBall = null;
            var bestScore = float.MinValue;
            var remainingTime = CalculateRemainingTime(currentHealth);

            foreach (var golfBall in BallManager.Instance.GetGolfBalls())
            {
                // Calculate the distance to the ball and to the cart
                var distanceFromNpcToBall = CalculateNavMeshDistance(npcPosition, golfBall.BallPosition);
                var timeToReachBall = distanceFromNpcToBall / npcNavMeshAgent.speed;

                var distanceFromBallToCart =
                    CalculateNavMeshDistance(golfBall.BallPosition, golfCartTransform.position);
                var timeToReachGolfCart = distanceFromBallToCart / npcNavMeshAgent.speed;

                var totalTime = timeToReachBall + timeToReachGolfCart;
                // If the total time exceeds remaining time, skip this ball
                if (totalTime > remainingTime)
                {
                    continue;
                }

                var ballPoints = golfBall.BallPoint;
                var healthPercentage = currentHealth / maxHealth;
                // Adjust weight dynamically based on current health
                var dynamicPointWeight =
                    Mathf.Lerp(1.5f, 0.5f, 1 - healthPercentage); // High health → prioritize points
                var dynamicTimeWeight =
                    Mathf.Lerp(0.5f, 1.5f, 1 - healthPercentage); // Low health → prioritize distance

                // Calculate the final score for the ball
                var score = (ballPoints * dynamicPointWeight) - (totalTime * dynamicTimeWeight);

                // Choose the ball with the highest score
                if (!(score > bestScore))
                {
                    continue;
                }

                bestScore = score;
                bestBall = golfBall;
            }

            return bestBall;
        }


        private float CalculateRemainingTime(float currentHealth)
        {
            return currentHealth / healthDecreaseRate;
        }

        /// <summary>
        /// Calculates the distance along the NavMesh between two points.
        /// </summary>
        private float CalculateNavMeshDistance(Vector3 startPosition, Vector3 targetPosition)
        {
            var path = new NavMeshPath();
            if (!NavMesh.CalculatePath(startPosition, targetPosition, NavMesh.AllAreas, path))
            {
                return float.MaxValue;
            }

            var totalDistance = 0f;
            for (var i = 1; i < path.corners.Length; i++)
            {
                totalDistance += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            }

            return totalDistance;
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
                OnHealthChanged?.Invoke(_health);
                npcHealthBarSlider.value = _health;
                if (_health <= 0)
                {
                    _npcStatus = NPCStatus.Finished;
                    CancelLoop();
                    OnFail?.Invoke();
                    break;
                }

                await UniTask.Yield(token);
            }
        }

        private async UniTask MoveToBall(GolfBall ball, CancellationToken token)
        {
            StopAgent();
            PlayAnimation(RunningTrigger);
            await RotateTowards(ball.BallPosition, token);
            ResumeAgent();
            npcNavMeshAgent.SetDestination(ball.BallPosition);
            await UniTask.WaitUntil(IsDestinationReached, cancellationToken: token);
        }

        private async UniTask MoveToGolfCart(CancellationToken token)
        {
            StopAgent();
            PlayAnimation(RunningTrigger);
            await RotateTowards(golfCartTransform.position, token);
            ResumeAgent();
            npcNavMeshAgent.SetDestination(golfCartTransform.position);
            await UniTask.WaitUntil(IsDestinationReached, cancellationToken: token);
        }

        private async UniTask PickUp(GolfBall ball, CancellationToken token)
        {
            StopAgent();
            PlayAnimation(PickingUpTrigger);
            await UniTask.Delay(TimeSpan.FromSeconds(0.08f), cancellationToken: token);
            ball.OnPickUp(pickUpTransform);
            await WaitForAnimationComplete(PickingUpTrigger, token);
            ResumeAgent();
        }

        private async UniTask DropOff(GolfBall ball, CancellationToken token)
        {
            StopAgent();
            PlayAnimation(PuttingDownTrigger);
            await UniTask.Delay(TimeSpan.FromSeconds(0.2f), cancellationToken: token);
            ball.OnDropOff(golfCartTransform);
            await WaitForAnimationComplete(PuttingDownTrigger, token);
            OnPointsEarned?.Invoke(ball.BallPoint);
            ResumeAgent();
        }

        private async UniTask RotateTowards(Vector3 targetPosition, CancellationToken token)
        {
            var direction = (targetPosition - transform.position).normalized;
            direction.y = 0;

            if (direction == Vector3.zero)
            {
                return;
            }

            var targetRotation = Quaternion.LookRotation(direction);

            while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    180 * Time.deltaTime
                );

                await UniTask.Yield(token);
            }
        }

        private void PlayAnimation(int trigger)
        {
            npcAnimator.SetTrigger(trigger);
        }

        private async UniTask WaitForAnimationComplete(int stateHash, CancellationToken token)
        {
            while (npcAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash != stateHash)
            {
                await UniTask.Yield(token);
            }

            while (npcAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash == stateHash &&
                   npcAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            {
                await UniTask.Yield(token);
            }
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

        private void StopAgent()
        {
            npcNavMeshAgent.isStopped = true;
            npcNavMeshAgent.ResetPath();
        }

        private void ResumeAgent()
        {
            npcNavMeshAgent.isStopped = false;
        }

        private void CancelLoop()
        {
            PlayAnimation(IdleTrigger);
            _moveCancellationTokenSource?.Cancel();
            npcNavMeshAgent.ResetPath();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            _moveCancellationTokenSource?.Cancel();
            _moveCancellationTokenSource?.Dispose();
        }
    }
}