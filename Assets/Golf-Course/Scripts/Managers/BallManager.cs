using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Golf_Course.Scripts.Managers
{
    public class BallManager : Singleton<BallManager>
    {
        [SerializeField]
        private GolfBall golfBallPrefab;

        [SerializeField]
        private Transform golfBallsParentTransform;

        [SerializeField]
        private int totalBalls = 10;

        [SerializeField]
        private Terrain terrain;

        private readonly List<GolfBall> _golfBalls = new();
        private readonly Queue<GolfBall> _ballPool = new();

        public Action OnBallsInitialized;

        private void OnEnable()
        {
            if (GameHandler.Instance != null)
            {
                GameHandler.Instance.OnGameStarted += InitializeBalls;
            }
        }

        private void OnDisable()
        {
            if (GameHandler.Instance != null)
            {
                GameHandler.Instance.OnGameStarted -= InitializeBalls;
            }
        }

        private void InitializeBalls()
        {
            if (terrain == null || golfBallPrefab == null || NPCController.Instance == null)
            {
                Debug.LogWarning("Required components are missing.");
                return;
            }

            ClearBalls();
            var terrainData = terrain.terrainData;
            var terrainPosition = terrain.transform.position;

            for (var i = 0; i < totalBalls; i++)
            {
                var hit = new NavMeshHit();
                var validPosition = false;

                while (!validPosition)
                {
                    var randomX = Random.Range(terrainPosition.x, terrainPosition.x + terrainData.size.x);
                    var randomZ = Random.Range(terrainPosition.z, terrainPosition.z + terrainData.size.z);
                    var terrainHeight = terrain.SampleHeight(new Vector3(randomX, 0, randomZ));
                    var randomPosition = new Vector3(randomX, terrainHeight, randomZ);

                    if (NavMesh.SamplePosition(randomPosition, out hit, 2.0f, NavMesh.AllAreas))
                    {
                        validPosition = true;
                    }
                }

                var golfBall = GetOrCreateBall();
                var ballTransform = golfBall.transform;
                ballTransform.position = hit.position;
                ballTransform.rotation = quaternion.identity;
                var distanceToNpc = Vector3.Distance(hit.position, NPCController.Instance.GetNPCPosition());
                var ballLevel = CalculateBallLevelBasedOnDistance(distanceToNpc);
                golfBall.Initialize(ballLevel, hit.position, CalculateBallPoint(ballLevel));
                golfBall.gameObject.SetActive(true);
                _golfBalls.Add(golfBall);
            }

            OnBallsInitialized?.Invoke();
        }

        private GolfBall GetOrCreateBall()
        {
            return _ballPool.Count > 0 ? _ballPool.Dequeue() : Instantiate(golfBallPrefab, golfBallsParentTransform);
        }

        public void RemoveBall(GolfBall golfBall)
        {
            if (!_golfBalls.Remove(golfBall))
            {
                return;
            }

            golfBall.gameObject.SetActive(false);
            golfBall.transform.SetParent(golfBallsParentTransform);
            _ballPool.Enqueue(golfBall);
        }

        private BallLevel CalculateBallLevelBasedOnDistance(float distance)
        {
            return distance switch
            {
                < 20 => BallLevel.Level1,
                < 40 => BallLevel.Level2,
                _ => BallLevel.Level3
            };
        }

        private int CalculateBallPoint(BallLevel ballLevel)
        {
            return ballLevel switch
            {
                BallLevel.Level1 => 10,
                BallLevel.Level2 => 20,
                BallLevel.Level3 => 30,
                _ => 0
            };
        }

        public List<GolfBall> GetGolfBalls()
        {
            return _golfBalls;
        }

        public void SetTotalBalls(int newTotal)
        {
            totalBalls = newTotal;
        }

        private void ClearBalls()
        {
            foreach (var golfBall in _golfBalls)
            {
                golfBall.gameObject.SetActive(false);
                golfBall.transform.SetParent(golfBallsParentTransform);
                _ballPool.Enqueue(golfBall);
            }

            _golfBalls.Clear();
        }
    }
}