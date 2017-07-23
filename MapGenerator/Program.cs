using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenerator {
    class Program {
        static void Main(string[] args) {

            Configuration.RoomPlacement = Configuration.RoomDistribution.IN_EMPTY_SPACE;

            for (int i = 0; i < 100; i++)
            {
                Map m = Map.GenerateMap();
                Console.WriteLine("Done creating the map");
            }
            Console.ReadLine();
        }
    }
}
