namespace Golf_Course.Scripts.Managers
{
    public class PointManager : Singleton<PointManager>
    {
        public int GetPoints(BallLevel ballLevel)
        {
            return ballLevel switch
            {
                BallLevel.Level1 => 10,
                BallLevel.Level2 => 20,
                BallLevel.Level3 => 30,
                _ => 0
            };
        }
    }
}
