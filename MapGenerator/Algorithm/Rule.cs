using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapGenerator.Dijkstra
{
    public class Rule
    {

        private Object _source;
        private Object _neighbor;
        private bool _line;
        private bool _diagonal;
    
        public Rule(Object source, Object neighbor, bool line, bool diagonal)
        {
            _line = line;
            _diagonal = diagonal;
            _source = source;
            _neighbor = neighbor;
        }

        public bool isValid(Object source, Object neighbor, bool line)
        {
            if (source == _source && neighbor == _neighbor)
            {
                if (line)
                    return _line;
                else
                    return _diagonal;
            }
            return false;
        }

        public Object getSource()
        {
            return _source;
        }

    }
}
