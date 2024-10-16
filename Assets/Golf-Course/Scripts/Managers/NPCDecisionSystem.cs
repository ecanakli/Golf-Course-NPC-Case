using UnityEngine;
using UnityEngine.AI;

namespace Golf_Course.Scripts.Managers
{
    public class NPCDecisionSystem : MonoBehaviour
    {
        public GolfBall MakeDecision(Vector3 npcPosition, float currentHealth)
        {
            GolfBall bestBall = null;
            var bestScore = float.MinValue;
            var remainingTime = CalculateRemainingTime(currentHealth);
            var maxHealth = NPCController.Instance.MaxHealth;

            foreach (var golfBall in BallManager.Instance.GetGolfBalls())
            {
                var distanceFromNpcToBall = CalculateNavMeshDistance(npcPosition, golfBall.BallPosition);
                var timeToReachBall = distanceFromNpcToBall / NPCController.Instance.NpcNavMeshAgent.speed;

                var distanceFromBallToCart =
                    CalculateNavMeshDistance(golfBall.BallPosition, NPCController.Instance.GolfCart.position);
                var timeToReachGolfCart = distanceFromBallToCart / NPCController.Instance.NpcNavMeshAgent.speed;

                var totalTime = timeToReachBall + timeToReachGolfCart;
                if (totalTime > remainingTime)
                {
                    continue;
                }

                var ballPoints = golfBall.BallPoints;
                var healthPercentage = currentHealth / maxHealth;
                var dynamicPointWeight = Mathf.Lerp(1.5f, 0.5f, 1 - healthPercentage);
                var dynamicTimeWeight = Mathf.Lerp(0.5f, 1.5f, 1 - healthPercentage);
                var score = (ballPoints * dynamicPointWeight) - (totalTime * dynamicTimeWeight);

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
            return currentHealth / NPCController.Instance.HealthDecreaseRate;
        }

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
    }
}