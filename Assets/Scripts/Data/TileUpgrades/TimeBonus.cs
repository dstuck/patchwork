using UnityEngine;
using Patchwork.Gameplay;
using System.Collections.Generic;

namespace Patchwork.Data
{
    public class TimeBonus : ITileUpgrade
    {
        private readonly float m_TimeToAdd;

        public TimeBonus() : this(0.6f) {}
        public TimeBonus(float timeToAdd)
        {
            m_TimeToAdd = Mathf.Max(0f, timeToAdd);
        }

        public string DisplayName => "Time Bonus";
        public string Description => $"+{m_TimeToAdd:F1} seconds to timer";
        public Color DisplayColor => Color.magenta;

        public int ModifyScore(int _baseScore, PlacedTile _tile, Board _board, List<PlacedTile> _otherTiles)
        {
            // Time bonus doesn't modify score
            return _baseScore;
        }

        public void OnTilePlaced(PlacedTile _tile, Board _board)
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

