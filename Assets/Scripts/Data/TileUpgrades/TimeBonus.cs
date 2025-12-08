using UnityEngine;
using Patchwork.Gameplay;
using System.Collections.Generic;

namespace Patchwork.Data
{
    public class TimeBonus : BaseTileUpgrade
    {
        private readonly float m_BaseTimeToAdd = 0.6f;

        public TimeBonus() : base(1) {}
        public TimeBonus(int level) : base(level) {}
        
        private float m_TimeToAdd => m_Level switch { 1 => m_BaseTimeToAdd, 2 => m_BaseTimeToAdd * 2, _ => m_BaseTimeToAdd * 4 };

        public override string DisplayName => "Time Bonus";
        public override string Description => $"+{m_TimeToAdd:F1} seconds to timer";
        
        protected override Color BaseDisplayColor => Color.magenta;

        public override int ModifyScore(int _baseScore, PlacedTile _tile, Board _board, List<PlacedTile> _otherTiles)
        {
            // Time bonus doesn't modify score
            return _baseScore;
        }

        public override void OnTilePlaced(PlacedTile _tile, Board _board)
        {
            // Add time to the timer
            Timer timer = Object.FindFirstObjectByType<Timer>();
            if (timer != null)
            {
                timer.AddTime(m_TimeToAdd);
            }
        }
    }
}

