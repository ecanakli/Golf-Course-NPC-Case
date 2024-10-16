using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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

        private void Start()
        {
            InitializeBalls();
        }

        private void InitializeBalls()
        {
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

                var golfBall = Instantiate(golfBallPrefab, hit.position, Quaternion.identity, golfBallsParentTransform);
                var distanceToNpc = Vector3.Distance(hit.position, NPCController.Instance.GetNPCPosition());
                var ballLevel = CalculateBallLevelBasedOnDistance(distanceToNpc);
                golfBall.Initialize(ballLevel, hit.position);
                _golfBalls.Add(golfBall);
            }

            NPCController.Instance.StartNpcLifeCycle();
        }

        public void RemoveBall(GolfBall golfBall)
        {
            _golfBalls.Remove(golfBall);
            Destroy(golfBall.gameObject);
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

        public List<GolfBall> GetGolfBalls()
        {
            return _golfBalls;
        }
    }
}