using GameMechanics.Persistence;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameMechanics.Model
{
    public class Player
    {

        #region Properties
        public int Points { get; private set; }
        public Color Color { get; private set; }
        public string Name { get; private set; }
        #endregion

        #region Private methods

        #endregion

        #region Constructor
        public Player(int points, Color color, string name) {
            Points = points;
            Color = color;
            Name = name;
        }
        #endregion

        #region Public methods

        public override string? ToString() {
            return $"{Points},{Color.Name},{Name}";
        }

        public void AddToPoint() {
            Points++;
        }

        public void ClearPoints() {
            Points = 0;
        }


        #endregion

    }
}
