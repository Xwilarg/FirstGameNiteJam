using UnityEngine;

namespace TouhouPrideGameJam5.SO
{
    [CreateAssetMenu(menuName = "ScriptableObject/GameInfo", fileName = "GameInfo")]
    public class GameInfo : ScriptableObject
    {
        public float RoundDuration;
        public float TimeBeforeRound;
        public int PointsBeforeVictory;
    }
}