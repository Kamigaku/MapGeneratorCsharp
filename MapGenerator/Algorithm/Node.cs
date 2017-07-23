using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenerator.Dijkstra
{
    public class Node
    {

        #region Member variables
        private int _value;
        private List<int> _neighbors;
        private bool _fetched;
        private bool _isCrossable;
        private int _shortestDistance;
        private int _previous;
        #endregion Member variables

        #region Properties
        public int Value {
            get { return _value; }
        }

        public List<int> Neighbors {
            get { return _neighbors; }
        }

        public bool Fetched {
            get { return _fetched; }
            set { _fetched = value; }
        }

        public int ShortestDistance {
            get { return _shortestDistance; }
            set { _shortestDistance = value; }
        }

        public int Previous {
            get { return _previous; }
            set { _previous = value; }
        }
        #endregion Properties


        public Node(int value)
        {
            _value = value;
            _neighbors = new List<int>();
            _fetched = false;
            _shortestDistance = 0;
            _previous = -1;
        }

        public void addNeighbors(int value)
        {
            if (!_neighbors.Contains(value))
                _neighbors.Add(value);
        }

        public static int XYValue(int x, int y, int size)
        {
            return x + (y * size);
        }

        public static int XValue(int value, int size)
        {
            return value % size;
        }

        public static int YValue(int value, int size)
        {
            return value / size;
        }

    }
}
