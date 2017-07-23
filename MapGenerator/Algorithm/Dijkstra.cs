using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenerator.Dijkstra
{
    public class Algorithm
    {

        #region Member variables
        private char[,] _map;
        private List<Rule> _rules;
        private Node[] _nodes;
        #endregion Member variables

        #region Properties
        public Node[] Nodes {
            get { return _nodes; }
        }
        #endregion Properties

        #region Constructors
        public Algorithm(char[,] theMap)
        {
            _map = theMap;
            _rules = new List<Rule>();
            _nodes = new Node[_map.GetLength(1) * _map.GetLength(0)];
        }

        public Algorithm(Node[] nodes)
        {
            _map = null;
            _rules = null;
            _nodes = nodes;
        }

        #endregion Constructors

        #region Public methods

        /**
         * Créer les noeuds pour le tableau passé
         * @param doAngles (boolean) : si TRUE, l'algorithme passera aussi dans les angles haut gauche, haut droite, bas gauche et bas droite.
         */
        public void CreateNodes(bool doAngles)
        {
            for (int y = 0; y < _map.GetLength(0); y++)
            {
                for (int x = 0; x < _map.GetLength(1); x++)
                {
                    if (IsValidObject(_map[y, x]))
                    {
                        int value = XYValue(x, y, _map.GetLength(1));
                        _nodes[value] = new Node(value);
                        if (x + 1 < _map.GetLength(1) && ValidateARule(_map[y, x], _map[y, x + 1], true)) // Droite
                            _nodes[value].addNeighbors(XYValue(x + 1, y, _map.GetLength(1)));

                        if (x - 1 >= 0 && ValidateARule(_map[y, x], _map[y, x - 1], true)) // Gauche
                            _nodes[value].addNeighbors(XYValue(x - 1, y, _map.GetLength(1)));

                        if (y + 1 < _map.GetLength(0) && ValidateARule(_map[y, x], _map[y + 1, x], true)) // Haut
                            _nodes[value].addNeighbors(XYValue(x, y + 1, _map.GetLength(1)));

                        if (y - 1 >= 0 && ValidateARule(_map[y, x], _map[y - 1, x], true)) // Bas
                            _nodes[value].addNeighbors(XYValue(x, y - 1, _map.GetLength(1)));

                        if (doAngles)
                        {
                            if (y + 1 < _map.GetLength(0) && x - 1 >= 0 &&
                                    ValidateARule(_map[y, x], _map[y + 1, x - 1], false)) // Haut gauche
                                _nodes[value].addNeighbors(XYValue(x - 1, y + 1, _map.GetLength(1)));

                            if (y + 1 < _map.GetLength(0) && x + 1 < _map.GetLength(1) &&
                                    ValidateARule(_map[y, x], _map[y + 1, x + 1], false)) // Haut droite
                                _nodes[value].addNeighbors(XYValue(x + 1, y + 1, _map.GetLength(1)));

                            if (y - 1 >= 0 && x - 1 >= 0 &&
                                    ValidateARule(_map[y, x], _map[y - 1, x - 1], false)) // Bas gauche
                                _nodes[value].addNeighbors(XYValue(x - 1, y - 1, _map.GetLength(1)));

                            if (y - 1 >= 0 && x + 1 < _map.GetLength(1) &&
                                    ValidateARule(_map[y, x], _map[y - 1, x + 1], false)) // Bas droite
                                _nodes[value].addNeighbors(XYValue(x + 1, y - 1, _map.GetLength(1)));
                        }
                    }
                }
            }
        }

        /***
         * Renvoi le chemin complet d'un point de départ jusqu'à l'arrivée.
         * @param from Le point de départ
         * @param to Le point d'arrivé
         * @return Le chemin ou null si le chemin n'existe pas
         */
        public List<Point> ShortestPathFromTo(Point from, Point to)
        {
            int valueOrigine = Algorithm.XYValue(from.X, from.Y, _map.GetLength(1));
            int valueTarget = Algorithm.XYValue(to.X, to.Y, _map.GetLength(1));

            ResetDistance();
            ResetFetch();

            Node currentNode = _nodes[valueOrigine];
            currentNode.Previous = valueOrigine;
            bool found = false;

            List<Node> ordereredDistance = new List<Node>();
            while (!found)
            {
                int shortDistance = currentNode.ShortestDistance + 1; // La valeur la plus petite possible
                for (int i = 0; i < currentNode.Neighbors.Count; i++)
                { // Je parcours tous mes voisins
                    Node neighbor = _nodes[currentNode.Neighbors[i]];
                    if (!neighbor.Fetched)
                    { // Est-ce qu'il n'a jamais été parcouru ?
                        if (neighbor.ShortestDistance > 0)
                        { // Est-ce que le noeud voisin a déjà été parcouru ?
                            if (neighbor.ShortestDistance > shortDistance)
                            { // Le noeud voisin possède un parcours qui était plus long
                                neighbor.ShortestDistance = shortDistance;
                                neighbor.Previous = currentNode.Value;
                            }
                        }
                        else
                        { // Le noeud voisin n'a jamais été parcouru, j'initialise sa valeur et son précédent
                            neighbor.ShortestDistance = shortDistance;
                            neighbor.Previous = currentNode.Value;
                            ordereredDistance.Add(neighbor);
                        }
                    }
                }
                _nodes[currentNode.Value].Fetched = true;
                ordereredDistance.Sort(delegate (Node n1, Node n2)
                {
                    return n1.ShortestDistance - n2.ShortestDistance;
                });
                if (ordereredDistance.Count == 0)
                    return null;
                currentNode = ordereredDistance[0];
                ordereredDistance.RemoveAt(0);
                if (currentNode.Value == valueTarget)
                {
                    found = true;
                }
            }
            // Parcours inversé
            List<Point> reversePath = new List<Point>();
            while (currentNode.Value != valueOrigine) {
                int yPrevious = Algorithm.YValue(currentNode.Value, _map.GetLength(1));
                int xPrevious = Algorithm.XValue(currentNode.Value, _map.GetLength(1));
                reversePath.Add(new Point(xPrevious, yPrevious));
                currentNode = _nodes[currentNode.Previous];
            }
            return reversePath;
        }

        /**
         * Retourne le nombre de points accessibles à partir d'un noeud d'origine
         * @param origin (Point) Les coordonnées du point d'origine
         * @return Le nombre de point accessible
         */
        public int NumberOfAccessiblePoint(Point origin)
        {
            return AllPossiblePath(origin).Count;
        }

        /**
         * Retourne le nombre de points accessibles à partir d'un noeud d'origine
         * @param origin (int) Le valeur du point d'origine
         * @return Le nombre de point accessible
         */
        public int NumberOfAccessiblePoint(int origin)
        {
            return AllPossiblePath(origin).Count;
        }

        /**
         * Retourne la position du premier élément correspondant à la recherche par rapport à la position
         * @param origin (Point) Les coordonnées du point d'origine
         * @param toFind Le caractère recherché
         * @return Le premier point trouvé
         */
        public Point FindFirst(Point origin, char toFind)
        {
            int valueOrigine = Algorithm.XYValue(origin.X, origin.Y, _map.GetLength(1));
            ResetDistance();

            Node currentNode = _nodes[valueOrigine];
            currentNode.Previous = valueOrigine;
            bool found = false;

            List<Node> ordereredDistance = new List<Node>();
            while (!found)
            {
                int shortDistance = currentNode.ShortestDistance + 1; // La valeur la plus petite possible
                for (int i = 0; i < currentNode.Neighbors.Count; i++)
                { // Je parcours tous mes voisins
                    Node neighbor = _nodes[currentNode.Neighbors[i]];
                    if (neighbor != null && !neighbor.Fetched)
                    { // Est-ce qu'il n'a jamais été parcouru ?
                        if (neighbor.ShortestDistance > 0)
                        { // Est-ce que le noeud voisin a déjà été parcouru ?
                            if (neighbor.ShortestDistance > shortDistance)
                            { // Le noeud voisin possède un parcours qui était plus long
                                neighbor.ShortestDistance = shortDistance;
                                neighbor.Previous = currentNode.Value;
                            }
                        }
                        else
                        { // Le noeud voisin n'a jamais été parcouru, j'initialise sa valeur et son précédent
                            neighbor.ShortestDistance = shortDistance;
                            neighbor.Previous = currentNode.Value;
                            ordereredDistance.Add(neighbor);
                        }
                    }
                }

                _nodes[currentNode.Value].Fetched = true;
                ordereredDistance.Sort(delegate (Node n1, Node n2)
                {
                    return n1.ShortestDistance - n2.ShortestDistance;
                });
                if (ordereredDistance.Count == 0)
                    return new Point(-1, -1);
                currentNode = ordereredDistance[0];
                ordereredDistance.RemoveAt(0);
                int xMap = Algorithm.XValue(currentNode.Value, _map.GetLength(1));
                int yMap = Algorithm.YValue(currentNode.Value, _map.GetLength(1));
                if (_map[yMap, xMap] == toFind)
                {
                    return new Point(xMap, yMap);
                }
            }
            return null;
        }
    
        /** 
         * Retourne tous les points accessibles à partir d'un noeud d'origine.
         * @param origin (Point) Le noeud d'origine
         * @return Tous les points accessibles
         */
        public List<Point> AllPossiblePath(Point origin)
        {
            List<Point> allPath = new List<Point>();
            ResetDistance();
            UnfetchNodes();
            Node originNode = _nodes[XYValue(origin.X, origin.Y, _map.GetLength(0))];
            AllPossiblePath_fetch(originNode, allPath);
            return allPath;
        }

        /** 
         * Retourne tous les points accessibles à partir d'un noeud d'origine.
         * @param origin (int) Le noeud d'origine
         * @return Tous les points accessibles
         */
        public List<Point> AllPossiblePath(int origin)
        {
            List<Point> allPath = new List<Point>();
            ResetDistance();
            UnfetchNodes();
            Node originNode = _nodes[origin];
            AllPossiblePath_fetch(originNode, allPath);
            return allPath;
        }

        public List<List<int>> GetTreePath(int origin)
        {
            List<List<int>> path = new List<List<int>>();
            List<int> values = new List<int>();
            TreeFetch(_nodes[origin], path, values);
            return path;
        }

        public void AddRule(Rule r)
        {
            _rules.Add(r);
        }

        #endregion Public methods

        #region Private methods

        private void UnfetchNodes()
        {
            for (int j = 0; j < _nodes.Length; j++)
                if (_nodes[j] != null) _nodes[j].Fetched = false;
        }

        private void UnfetchNode(int x, int y)
        {
            if (_nodes[XYValue(x, y, _map.GetLength(1))] != null)
                _nodes[XYValue(x, y, _map.GetLength(1))].Fetched = false;
        }

        private void AllPossiblePath_fetch(Node origin, List<Point> path)
        {
            origin.Fetched = true;
            for (int i = 0; i < origin.Neighbors.Count; i++)
            {
                if (!_nodes[origin.Neighbors[i]].Fetched    )
                {
                    if (_map != null)
                        path.Add(new Point(XValue(origin.Neighbors[i], _map.GetLength(1)),
                                YValue(origin.Neighbors[i], _map.GetLength(1))));
                    else
                        path.Add(new Point(origin.Neighbors[i], -1));
                    AllPossiblePath_fetch(_nodes[origin.Neighbors[i]], path);
                }
            }
        }
        
        private void TreeFetch(Node origin, List<List<int>> tree, List<int> values)
        {
            // trouvé un autre moyen pour le chemin
            values.Add(origin.Value);
            origin.Fetched = true;
            for (int i = 0; i < origin.Neighbors.Count; i++)
            {
                if (!_nodes[origin.Neighbors[i]].Fetched    )
                {
                    TreeFetch(_nodes[origin.Neighbors[i]], tree, values);
                }
            }
            tree.Add(new List<int>(values));
            values.Remove(origin.Value);
        }

        private bool ValidateARule(Object o1, Object o2, bool line)
        {
            for (int i = 0; i < _rules.Count; i++)
            {
                if (_rules[i].isValid(o1, o2, line))
                    return true;
            }
            return false;
        }

        private bool IsValidObject(Object o1)
        {
            for (int i = 0; i < _rules.Count; i++)
            {
                if (_rules[i].getSource() == o1)
                    return true;
            }
            return false;
        }

        private void ResetDistance()
        {
            for (int i = 0; i < _nodes.Length; i++)
                if (_nodes[i] != null)
                {
                    _nodes[i].ShortestDistance = 0;
                    _nodes[i].Previous = -1;
                }
        }

        private void ResetFetch()
        {
            for (int i = 0; i < _nodes.Length; i++)
            {
                if (_nodes[i] != null)
                {
                    _nodes[i].Fetched = false;
                }
            }
        }

        #endregion Private methods
        
        #region Static methods

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

        #endregion Static methods
    }
}
