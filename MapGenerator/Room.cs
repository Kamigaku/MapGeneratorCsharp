using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenerator {
    public class Room {

        private int    _seed;
        private int   _width;
        private int  _height;
        private int  _id;
        private char[,] _map;
        private Random _random;
        private Point _origin;
        private Dictionary<int, Room> _neighbors;

        #region Getter & Setter

        public char[,] Map
        {
            get { return this._map; }
            set { _map = value; }
        }

        public Point Origin {
            get { return _origin; }
            set { _origin = value; }
        }

        public Dictionary<int, Room> Neighbors {
            get { return _neighbors; }
        }

        public int Id {
            get { return _id; }
            set { if(_id == -1) _id = value; }
        }

        #endregion Getter & Setter

        public Room(int seed) {
            this._seed = seed;
            this._random = new Random(_seed);
            this._neighbors = new Dictionary<int, Room>();
            this._id = -1;
            generateRoom();            
        }

        public Room(int seed, char[,] map)
        {
            this._seed = seed;
            this._random = new Random(_seed);
            this._map = map;
            this._neighbors = new Dictionary<int, Room>();
        }

        private void generateRoom() {
            this._height = Utility.nextInt(_random, Configuration.MinRoomHeight, Configuration.MaxRoomHeight);
            this._width = Utility.nextInt(_random, Configuration.MinRoomWidth, Configuration.MaxRoomWidth);
            this._map = new char[this._height, this._width];

            #region Fill the map with #
            _map = Utility.fillArrayWith(_map, Configuration.Void);
            #endregion

            #region Generate the important points
            int numberOfPoints = 0;
            if (this._width + this._height < 10)      numberOfPoints = 3;
            else if (this._width + this._height < 30) numberOfPoints = 6;
            else                                      numberOfPoints = 9;
            List<Point> points = new List<Point>();
            for (int i = 0; i < numberOfPoints; i++) {
                int cX = Utility.nextInt(_random, 1, _width - 1);
                int cY = Utility.nextInt(_random, 1, _height - 1);
                bool found = false;
                for (int j = 0; j < points.Count; j++) {
                    if (points[j].X == cX && points[j].Y == cY) {
                        i--;
                        found = true;
                    }
                }
                if (!found) {
                    points.Add(new Point(cX, cY));
                    _map[cY, cX] = i.ToString()[0];
                }
            }
            #endregion

            #region Connects all points
            Dictionary<int, double> indexAndWeight = new Dictionary<int, double>();
            Point center = new Point((int)Math.Round(points.Average(p => (double)p.X)),
                                (int)Math.Round(points.Average(p => (double)p.Y))); // Détermination du centre de gravité
            for (int i = 0; i < points.Count; i++) { // Calcul d'angle par rapport au centre de gravité
                double angle = Utility.angleBetweenTwoPoints(center, points[i]);
                if (angle < 0)
                    angle = 180 + (180 + angle);
                indexAndWeight.Add(i, angle);
            }
            var myList = indexAndWeight.ToList();
            myList.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value)); // Tri par angle
            for (int i = 0; i < myList.Count; i++) {
                drawBetween2Points(_random, points[myList[i].Key], points[myList[(i + 1) % points.Count].Key], Configuration.Ground);
            }
            myList.Clear();
            #endregion

            #region Fill the empty space inside the polygon
            for (int y = 0; y < this._map.GetLength(0); y++) {
                bool foundX = false;
                bool startManuver = false;
                Point init = null;
                for (int x = 0; x < this._map.GetLength(1); x++) {
                    if (this._map[y, x] == Configuration.Ground) {
                        if (foundX && startManuver) {
                            for (int i = init.X + 1; i < x; i++) {
                                this._map[y, i] = Configuration.Ground;
                            }
                            startManuver = false;
                            init = new Point(x, y);
                        }
                        else {
                            foundX = true;
                            init = new Point(x, y);
                        }
                    }
                    if (foundX && this._map[y, x] == Configuration.Void) {
                        startManuver = true;
                    }
                }
            }
            #endregion

            #region Add the wall around the map
            for (int y = 0; y < this._map.GetLength(0); y++) {
                for (int x = 0; x < this._map.GetLength(1); x++) {
                    if (this._map[y, x] == Configuration.Void && Utility.checkSquareSurrondings(this._map, x, y, Configuration.Ground))
                        this._map[y, x] = Configuration.Wall;
                }
            }
            #endregion

            #region Reduce the room
            int heightMinRoom = -1;
            int heightMaxRoom = this._map.GetLength(0);
            int widthMinRoom = -1;
            int widthMaxRoom = -1;
            char[,] tempMap = new char[this._map.GetLength(0), this._map.GetLength(1)];

            #region Copy array in a temp array
            for (int y = 0; y < this._map.GetLength(0); y++) {
                for (int x = 0; x < this._map.GetLength(1); x++) {
                    tempMap[y, x] = this._map[y, x];
                }
            }
            #endregion

            for (int y = 0; y < this._map.GetLength(0); y++) {
                for (int x = 0; x < this._map.GetLength(1); x++) {
                    if (this._map[y, x] == Configuration.Wall) {
                        if (heightMinRoom == -1)
                            heightMinRoom = y;
                        if (widthMinRoom == -1)
                            widthMinRoom = x;
                        heightMaxRoom = y;
                        if (x < widthMinRoom) widthMinRoom = x;
                        if (x > widthMaxRoom) widthMaxRoom = x;
                    }
                }
            }
            this._map = new char[heightMaxRoom - heightMinRoom + 1, widthMaxRoom - widthMinRoom + 1];
            for (int y = heightMinRoom; y <= heightMaxRoom; y++) {
                for (int x = widthMinRoom; x <= widthMaxRoom; x++) {
                    this._map[y - heightMinRoom, x - widthMinRoom] = tempMap[y, x];
                }
            }
            tempMap = null;
            this._height = this._map.GetLength(0);
            this._width = this._map.GetLength(1);
            #endregion
        }

        private void drawBetween2Points(Random random, Point p1ori, Point p2ori, char charToDraw) {
            Point p1 = new Point(p1ori);
            Point p2 = new Point(p2ori);
            int directionY = (p2.Y - p1.Y) < 0 ? -1 : ((p2.Y - p1.Y) == 0 ? 0 : 1);
            int directionX = (p2.X - p1.X) < 0 ? -1 : ((p2.X - p1.X) == 0 ? 0 : 1);
            this._map[p1.Y, p1.X] = Configuration.Ground;
            while (directionX != 0 && directionY != 0) {
                int xOrY = Utility.nextInt(random, 0, 2);
                int indicX = 0;
                int indicY = 0;
                if (xOrY == 1)
                    indicX = Utility.nextInt(random, 0, 2) * directionX;
                else
                    indicY = Utility.nextInt(random, 0, 2) * directionY;
                p1.X += indicX;
                p1.Y += indicY;
                directionY = (p2.Y - p1.Y) < 0 ? -1 : ((p2.Y - p1.Y) == 0 ? 0 : 1);
                directionX = (p2.X - p1.X) < 0 ? -1 : ((p2.X - p1.X) == 0 ? 0 : 1);
                this._map[p1.Y, p1.X] = charToDraw;
            }
            if (directionX == 0) {
                while ((p2.Y - p1.Y) != 0) {
                    p1.Y += (p2.Y - p1.Y) / Math.Abs((p2.Y - p1.Y));
                    this._map[p1.Y, p1.X] = charToDraw;
                }
            }
            else {
                while ((p2.X - p1.X) != 0) {
                    p1.X += (p2.X - p1.X) / Math.Abs((p2.X - p1.X));
                    this._map[p1.Y, p1.X] = charToDraw;
                }
            }
        }
        

    }
}
