using MapGenerator.Dijkstra;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenerator {
    public class Map {

        private Dictionary<int, Room> _rooms;
        private int _mapSeed;
        private Random _random;
        private char[,] _map;
        private int[,] _idMap;
        private int _roomCount;

        public char[,] MapArray {
            get 
            {
                return _map;
            }
        }

        private Map(int mapSeed) {
            _rooms = new Dictionary<int, Room>();
            _mapSeed = mapSeed;
            _random = new Random(_mapSeed);
            LoadRooms();
        }

        #region Private methods

        private void LoadRooms() {

            #region Determine the number of rooms
            int numberOfRooms = Configuration.NumberOfRooms;
            if(numberOfRooms <= 0)
                numberOfRooms = (int.Parse("" + _mapSeed.ToString()[_mapSeed.ToString().Length - 1]) +
                               int.Parse("" + _mapSeed.ToString()[_mapSeed.ToString().Length - 2]) * 10) + 10;
            #endregion Determine the number of rooms

            #region Create all the rooms and add them to the map
            for (int i = 0; i < numberOfRooms; i++) {
                Room room = new Room(_random.Next());
                _rooms.Add(_roomCount, room);
                room.Id = _roomCount;
                _roomCount += 1;
                AddRoomToMap(room);
            }
            #endregion Create all the rooms and add them to the map

            CreateCorridors();

            using (FileStream fs = new FileStream("export.txt", FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs)) {
                    for (int y = 0; y < _map.GetLength(0); y++)
                    {
                        string line = string.Empty;
                        for (int x = 0; x < _map.GetLength(1); x++)
                        {
                            line += _map[y, x];
                        }
                        sw.WriteLine(line);
                    }
                }
            }
        }

        private void AddRoomToMap(Room room) {

            Point originRoom = new Point(0, 0);

            #region First room
            if(_map == null)
            {
                _map = new char[room.Map.GetLength(0), room.Map.GetLength(1)];
                _idMap = new int[room.Map.GetLength(0), room.Map.GetLength(1)];
                Utility.fillArrayWith(_map, Configuration.Void);
                Utility.fillArrayWith(_idMap, -1);
            }
            #endregion First room

            #region Other rooms
            else
            {
                #region Selecting all the available spots for the origin of the room
                List<Point> availableSpots = new List<Point>();
                if (Configuration.RoomPlacement != Configuration.RoomDistribution.RANDOM)
                {
                    for (int y = 0; y < _map.GetLength(0); y++)
                    {
                        for (int x = 0; x < _map.GetLength(1); x++)
                        {
                            switch (Configuration.RoomPlacement)
                            {
                                case Configuration.RoomDistribution.BIND:
                                    if (_map[y, x] == Configuration.Wall)
                                        availableSpots.Add(new Point(x, y));
                                    break;
                                case Configuration.RoomDistribution.IN_EMPTY_SPACE:
                                    if (_map[y, x] == Configuration.Void)
                                        availableSpots.Add(new Point(x, y));
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    availableSpots.Add(new Point(Utility.nextInt(_random, 0, _map.GetLength(0)), 
                                                 Utility.nextInt(_random, 0, _map.GetLength(1))));
                }
                #endregion Selecting all the available spots for the origin of the room

                if (availableSpots.Count == 0)
                {
                    originRoom = new Point(_map.GetLength(1), _map.GetLength(0));
                }
                else
                {
                    originRoom = availableSpots[_random.Next() % availableSpots.Count()];
                }
                int newWidth = room.Map.GetLength(1) + originRoom.X;
                int newHeight = room.Map.GetLength(0) + originRoom.Y;

                newWidth  = newWidth < _map.GetLength(1) ? _map.GetLength(1) : newWidth;
                newHeight = newHeight < _map.GetLength(0) ? _map.GetLength(0) : newHeight;

                if (newWidth != _map.GetLength(1) || newHeight != _map.GetLength(0))
                {
                    char[,] tempNewMap = new char[newHeight, newWidth];
                    int[,] tempNewIdMap = new int[newHeight, newWidth];
                    tempNewMap = Utility.fillArrayWith(tempNewMap, Configuration.Void);
                    tempNewIdMap = Utility.fillArrayWith(tempNewIdMap, -1);
                    for (int y = 0; y < _map.GetLength(0); y++)
                    {
                        for (int x = 0; x < _map.GetLength(1); x++)
                        {
                            tempNewMap[y, x] = _map[y, x];
                            tempNewIdMap[y, x] = _idMap[y, x];
                        }
                    }
                    _map = tempNewMap;
                    _idMap = tempNewIdMap;
                    tempNewMap = null;
                    tempNewIdMap = null;
                }
            }
            #endregion Other rooms

            bool addedAGround = false;
            room.Origin = new Point(originRoom);
            for (int y = 0; y < room.Map.GetLength(0); y++)
            {
                for (int x = 0; x < room.Map.GetLength(1); x++)
                {
                    if (_map[y + originRoom.Y, x + originRoom.X] == Configuration.Void)
                    {
                        _map[y + originRoom.Y, x + originRoom.X] = room.Map[y, x];
                        _idMap[y + originRoom.Y, x + originRoom.X] = room.Id;
                        if (room.Map[y, x] == Configuration.Ground)
                            addedAGround = true;
                    }
                    else
                    {
                        room.Map[y, x] = Configuration.Void;
                        int idTile = _idMap[y + originRoom.Y, x + originRoom.X];
                        if (idTile != -1 && !room.Neighbors.ContainsKey(idTile)) {
                            room.Neighbors.Add(idTile, _rooms[idTile]);
                            _rooms[idTile].Neighbors.Add(room.Id, room);
                        }
                    }
                }
            }
            if (!addedAGround)
            {
                Console.WriteLine("[Warning] A room without ground has been deleted.");
                foreach(int neighborKey in room.Neighbors.Keys)
                {
                    room.Neighbors[neighborKey].Neighbors.Remove(room.Id);
                }
                for (int y = 0; y < room.Map.GetLength(0); y++)
                {
                    for (int x = 0; x < room.Map.GetLength(1); x++)
                    {
                        if(_idMap[y + room.Origin.Y, x + room.Origin.X] == room.Id)
                        {
                            _map[y + room.Origin.Y, x + room.Origin.X] = Configuration.Void;
                            _idMap[y + room.Origin.Y, x + room.Origin.X] = -1;
                        }
                    }
                }
                _rooms.Remove(room.Id);
            }
        }

        private void CreateCorridors()
        {
            if(_rooms.Count > 1) {
                foreach (int key in _rooms.Keys)
                {
                    if (_rooms[key].Neighbors.Count < _rooms.Count - 2)
                    {
                        List<int> notNeighborsKey = _rooms.Keys.ToList(); // contains all not non neighbors of the current room
                        List<int> neighborsKey = _rooms[key].Neighbors.Keys.ToList(); // contains the neighbors of the current and the current room
                        foreach (int neighbor in _rooms[key].Neighbors.Keys)
                        {
                            notNeighborsKey.Remove(neighbor);
                        }
                        notNeighborsKey.Remove(_rooms[key].Id);
                        neighborsKey.Add(_rooms[key].Id);

                        char[,] corridorMap = new char[_map.GetLength(0), _map.GetLength(1)];
                        for (int y = 0; y < _map.GetLength(0); y++)
                        {
                            for (int x = 0; x < _map.GetLength(1); x++)
                            {
                                corridorMap[y, x] = _map[y, x];
                            }
                        }

                        for(int i = 0; i < neighborsKey.Count; i++)
                        {
                            Room neighborRoom = _rooms[neighborsKey[i]];
                            for(int y = 0; y < neighborRoom.Map.GetLength(0); y++)
                            {
                                for(int x = 0; x < neighborRoom.Map.GetLength(1); x++)
                                {
                                    corridorMap[y + neighborRoom.Origin.Y, x + neighborRoom.Origin.X] = Configuration.Void;
                                }
                            }
                        }

                        Room roomPicked = _rooms[Utility.nextInt(_random, 0, neighborsKey.Count)];

                        Algorithm d = new Algorithm(corridorMap);
                        d.AddRule(new Rule())

                        // Prochain objectif, choisir une salle au hasard, un point de type mur au hasard de la salle choisie
                        // effectuer un A* pour trouver la salle la plus proche

                        /*int roomToJoin = Utility.nextInt(_random, 0, allKeys.Count - 1);

                        Point p1 = _rooms[key].Origin;
                        Point p2 = _rooms[allKeys[roomToJoin]].Origin;

                        Room r1 = _rooms[key];
                        Room r2 = _rooms[allKeys[roomToJoin]];

                        char[,] corridorMap = new char[_map.GetLength(0), _map.GetLength(1)];
                        for(int y = 0; y < _map.GetLength(0); y++)
                        {
                            for(int x = 0; x < _map.GetLength(1); x++)
                            {
                                corridorMap[y, x] = _map[y, x];
                            }
                        }

                        for (int y = 0; y < r1.Map.GetLength(0); y++)
                        {
                            for (int x = 0; x < r1.Map.GetLength(1); x++)
                            {
                                char tileValue = corridorMap[y + r1.Origin.Y, x + r1.Origin.X];
                                if (tileValue == Configuration.Wall) {
                                    corridorMap[y + r1.Origin.Y, x + r1.Origin.X] = Configuration.Ground;
                                }
                            }
                        }

                        for (int y = 0; y < r2.Map.GetLength(0); y++)
                        {
                            for (int x = 0; x < r2.Map.GetLength(1); x++)
                            {
                                char tileValue = corridorMap[y + r2.Origin.Y, x + r2.Origin.X];
                                if (tileValue == Configuration.Wall)
                                {
                                    corridorMap[y + r2.Origin.Y, x + r2.Origin.X] = Configuration.Ground;
                                }
                            }
                        }




                        r1.Neighbors.Add(r2.Id, r2);
                        r2.Neighbors.Add(r1.Id, r1);
                        */
                    }
                }
            }
        }

        #endregion Private methods

        #region Static methods

        public static Map GenerateMap()
        {
            return new Map(new Random().Next());
        }

        public static Map GenerateMap(int seed)
        {
            return new Map(seed);
        }

        #endregion Static methods

        #region Public methods
        
        public void AddRoom(int roomSeed)
        {
            Room room = new Room(roomSeed);
            AddRoom(room);
        }

        public void AddRoom(Room room)
        {
            _rooms.Add(_roomCount, room);
            room.Id = _roomCount;
            _roomCount += 1;
            AddRoomToMap(room);
            CreateCorridors();
        }

        #endregion Public methods

    }
}
