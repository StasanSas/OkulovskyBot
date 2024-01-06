using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graph.App
{
    public class NodeVisualData
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        // public static int Radius;
        // public static bool isFirstNode = true;

        public NodeVisualData(int x, int y)
        { X = x; Y = y; }
    }

    public class EdgeVisualData
    {
        public NodeVisualData Start { get; private set; }
        public NodeVisualData End { get; private set; }
        // public static int Weight;

        public EdgeVisualData(NodeVisualData start, NodeVisualData end)
        { Start = start; End = end; }
    }

    public class AntiStaticVisualData
    {
        public int NodeRadius;
        public bool WaitFirstNode = true;
        public int EdgeWidth;
    }
}
