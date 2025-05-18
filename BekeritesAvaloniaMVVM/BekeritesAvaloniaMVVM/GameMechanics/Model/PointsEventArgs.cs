using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameMechanics.Model {
    public class PointsEventArgs {
        private int _point1;
        private int _point2;

        public int PlayerOnePoint {
            get => _point1;
        }

        public int PlayerTwoPoint {
            get => _point2;
        }

        public PointsEventArgs(int point1, int point2) {
            _point1 = point1;
            _point2 = point2;
        }
    }
}
