using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenerator
{
    public abstract class Configuration
    {

        public enum RoomDistribution
        {
            RANDOM,
            BIND,
            IN_EMPTY_SPACE
        }

        public enum CorridorDistribution
        {
            STRAIGHT,
            RANDOM
        }

        public static int MaxRoomWidth = 30;
        public static int MinRoomWidth = 5;
        public static int MaxRoomHeight = 30;
        public static int MinRoomHeight = 5;
        public static int NumberOfRooms = 100; // can be 0 if you want it to be seed driven
        public static RoomDistribution RoomPlacement;
        public static CorridorDistribution CorridorPlacement;

        public static readonly char Wall   = 'W';
        public static readonly char Ground = '.';
        public static readonly char Void   = '#';

    }
}
