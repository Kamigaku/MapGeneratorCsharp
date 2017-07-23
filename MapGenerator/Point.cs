using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenerator {
    public partial class Point {

        private int _x;
        private int _y;

        public int X
        {
            get { return this._x; }
            set { this._x = value; }
        }

        public int Y
        {
            get { return this._y; }
            set { this._y = value; }
        }

        public Point(int x, int y) {
            this._x = x;
            this._y = y;
        }

        public Point(Point p) {
            this._x = p._x;
            this._y = p._y;
        }

    }
}
