using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenerator {
    public abstract partial class Utility {
        
        public static int nextInt(Random random, int min, int max) {
            if (min == max || max < min)
                return 0;
            return random.Next(max - min) + min;
        }

        public static void displayArrayReverseY(char[,] array) {
            for (int y = array.GetLength(0) - 1; y >= 0; y--) {
                for (int x = 0; x < array.GetLength(1); x++) {
                    Console.Write(array[y, x]);
                }
                Console.WriteLine();
            }
        }

        public static double angleBetweenTwoPoints(Point p1, Point p2) {
            float xDiff = p2.X - p1.X;
            float yDiff = p2.Y - p1.Y;
            return Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI;
        }

        public static bool checkXorYSurrondings(char[,] map, int x_origin,
                                            int y_origin, char search) {
            bool xAxis = checkXSurrondings(map, x_origin, y_origin, search);
            bool yAxis = checkYSurrondings(map, x_origin, y_origin, search);
            return (xAxis || yAxis);
        }

        /**
        * Check if the caracther appear on the Y axis of the position. ( -1 || 1 )
        * @param map The map where to look
        * @param x_origin The x position
        * @param y_origin The y position
        * @param search The character to search
        * @return TRUE if the character is present once, false if he is not present
        */
        public static bool checkYSurrondings(char[,] map, int x_origin, int y_origin,
                                                char search) {
            return (y_origin - 1 >= 0 && map[y_origin - 1, x_origin] == search) ||
                   (y_origin + 1 < map.GetLength(0) && map[y_origin + 1, x_origin] == search);
        }

        /**
         * Check if the caracthere either on the X axis of the position. ( -1 || 1 )
         * @param map The map where to look
         * @param x_origin The x position
         * @param y_origin The y position
         * @param search The character to search
         * @return TRUE if the character is present once, false if he is not present
         */
        public static bool checkXSurrondings(char[,] map, int x_origin, int y_origin,
                                                char search) {
            return (x_origin - 1 >= 0 && map[y_origin, x_origin - 1] == search) ||
                   (x_origin + 1 < map.GetLength(1) && map[y_origin, x_origin + 1] == search);
        }

        /// <summary>
        /// Permet de vérifier si les cases adjacentes et les diagonales adjacents possèdent le caractère recherché
        /// </summary>
        /// <param name="map">La map</param>
        /// <param name="x_origin">La coordonnée d'origine X</param>
        /// <param name="y_origin">La coordonnée d'origine Y</param>
        /// <param name="search">Le caractère recherché</param>
        /// <returns>TRUE si le caractère est présent dans une des cases, sinon FALSE</returns>
        public static bool checkSquareSurrondings(char[,] map, int x_origin,
                                                 int y_origin, char search) {
            if (x_origin - 1 >= 0) {
                if (y_origin - 1 >= 0 && map[y_origin - 1, x_origin - 1] == search)
                    return true;
                if (y_origin + 1 < map.GetLength(0) && map[y_origin + 1, x_origin - 1] == search)
                    return true;
            }
            if (x_origin + 1 < map.GetLength(1)) {
                if (y_origin - 1 >= 0 && map[y_origin - 1, x_origin + 1] == search)
                    return true;
                if (y_origin + 1 < map.GetLength(0) && map[y_origin + 1, x_origin + 1] == search)
                    return true;
            }
            return Utility.checkXorYSurrondings(map, x_origin, y_origin, search);
        }

        /// <summary>
        /// Rotate an array with an angle value
        /// </summary>
        /// <param name="m"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static char[,] rotate(char[,] m, int r)
        {
            int W = m.GetLength(0);
            int H = m.GetLength(1);
            char[,] ret = new char[H, W];
            for (int i = 0; i < ret.GetLength(0); i++)
            {
                for (int j = 0; j < ret.GetLength(1); j++)
                {
                    ret[i, j] = m[W - j - 1, i];
                }
            }
            return ret;
        }
        
        /// <summary>
        /// Fill an array with a specific object
        /// </summary>
        /// <param name="array">The array to fill</param>
        /// <param name="filler">The object that will fill the array</param>
        /// <returns>The array filled</returns>
        public static T[,] fillArrayWith<T>(T[,] array, T filler)
        {
            for (int y = 0; y < array.GetLength(0); y++)
            {
                for (int x = 0; x < array.GetLength(1); x++)
                {
                    array[y, x] = filler;
                }
            }
            return array;
        }

    }
}
