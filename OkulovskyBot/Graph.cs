using Newtonsoft.Json.Linq;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Collections;
using Color = System.Drawing.Color;

namespace DrawerGraphs
{

    public class Graph<TId, TWeight, TState>
    {
        public Dictionary<TId, NodeVisual<TId, TWeight, TState>> nodesDict = 
            new Dictionary<TId, NodeVisual<TId, TWeight, TState>>();

        public Dictionary<Tuple<TId, TId>, EdgeVisual<TId, TWeight, TState>> edgesDict =
            new Dictionary<Tuple<TId, TId>, EdgeVisual<TId, TWeight, TState>>();

        public NodeIndexer<TId, TWeight, TState> Nodes;
        public EdgeIndexer<TId, TWeight, TState> Edges;
        public Graph()
        {
            Nodes = new NodeIndexer<TId, TWeight, TState>(this);
            Edges = new EdgeIndexer<TId, TWeight, TState>(this);
        }


        public class NodeIndexer<TId, TWeight, TState> : IEnumerable<NodeVisual<TId, TWeight, TState>>
        {
            public Graph<TId, TWeight, TState> parent;
            public NodeIndexer(Graph<TId, TWeight, TState> parent)
            {
                this.parent = parent;
            }

            public NodeVisual<TId, TWeight, TState> this[TId id]
            {
                get
                {
                    if (parent.nodesDict.ContainsKey(id))
                        return parent.nodesDict[id];
                    throw new KeyNotFoundException();
                }
            }

            public void AddNode(NodeVisual<TId, TWeight, TState> node)
            {
                parent.nodesDict[node.Id] = node;
            }

            public IEnumerator<NodeVisual<TId, TWeight, TState>> GetEnumerator()
            {
                foreach (var keyAndValue in parent.nodesDict.AsEnumerable())
                    yield return keyAndValue.Value;
            }

            public void RemoveNode(NodeVisual<TId, TWeight, TState> node)
            {
                parent.nodesDict.Remove(node.Id);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public class EdgeIndexer<TId, TWeight, TState> : IEnumerable<EdgeVisual<TId, TWeight, TState>>
        {
            public Graph<TId, TWeight, TState> parent;
            public EdgeIndexer(Graph<TId, TWeight, TState> parent)
            {
                this.parent = parent;
            }

            public EdgeVisual<TId, TWeight, TState> this[TId idStart, TId idEnd]
            {
                get
                {
                    var idEnge = Tuple.Create(idStart, idEnd);
                    if (parent.edgesDict.ContainsKey(idEnge))
                        return parent.edgesDict[idEnge];
                    throw new KeyNotFoundException();
                }
            }

            public void AddEdge(EdgeVisual<TId, TWeight, TState> edge)
            {
                var idEnge = Tuple.Create(edge.Start.Id, edge.End.Id);
                if (!parent.nodesDict.ContainsKey(edge.Start.Id) ||
                    !parent.nodesDict.ContainsKey(edge.End.Id))
                    throw new KeyNotFoundException();
                parent.edgesDict[idEnge] = edge;
            }


            public IEnumerator<EdgeVisual<TId, TWeight, TState>> GetEnumerator()
            {
                foreach (var keyAndValue in parent.edgesDict.AsEnumerable())
                    yield return keyAndValue.Value;
            }

            public void RemoveEdge(EdgeVisual<TId, TWeight, TState> edge)
            {
                var idEnge = Tuple.Create(edge.Start.Id, edge.End.Id);
                if (!parent.edgesDict.ContainsKey(idEnge))
                    throw new KeyNotFoundException();
                parent.edgesDict.Remove(idEnge);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }

    public interface PartGraph { }

    public class EdgeVisual<TId, TWeight, TState> : PartGraph
    {
        private TWeight weight;
        private TState state;
        public NodeVisual<TId, TWeight, TState> Start { get; private set; }
        public NodeVisual<TId, TWeight, TState> End { get; private set; }

        public static Observer<TId, TWeight, TState> observer;

        public static bool flag = true;
        public TWeight Weight
        {
            get
            {
                GetSignal(TypeChange.GetEdgeWeight);
                return weight;
            }
            set
            {
                weight = value;
                GetSignal(TypeChange.ChangeEdgeWeight);
            }
        }
        public TState State
        {
            get
            {
                GetSignal(TypeChange.GetEdgeState);
                return state;
            }
            set
            {
                state = value;
                GetSignal(TypeChange.ChangeEdgeState);
            }
        }

        public EdgeVisual(NodeVisual<TId, TWeight, TState> start, 
                          NodeVisual<TId, TWeight, TState> end,
                          TWeight Weight = default(TWeight), TState State = default(TState))
        {
            weight = Weight;
            state = State;
            Start = start;
            End = end;
            GetSignal(TypeChange.CreateEdge);
        }

        public void Remove()
        {
            GetSignal(TypeChange.RemoveEdge);
        }

        public void GetSignal(TypeChange type)
        {
            if (flag)
                observer.AddChange(
                    new Change<List<TId>, TWeight, TState>(
                        type, new List<TId>() { Start.Id, End.Id },
                        weight, state));
        }
    }

    public class NodeVisual<TId, TWeight, TState> : PartGraph
    {
        private TWeight weight;
        private TState state;


        public static Observer<TId, TWeight, TState> observer;

        public static bool flag = true;
        public TId Id { get; private set; }
        public TWeight Weight
        {
            get
            {
                GetSignal(TypeChange.GetNodeWeight);
                return weight;
            }
            set
            {
                weight = value;
                GetSignal(TypeChange.ChangeNodeState);
            }
        }
        public TState State
        {
            get
            {
                GetSignal(TypeChange.GetNodeState);
                return state;
            }
            set
            {
                state = value;
                GetSignal(TypeChange.ChangeNodeState);
            }
        }

        public List<EdgeVisual<TId, TWeight, TState>> AdjacentEdges = 
            new List<EdgeVisual<TId, TWeight, TState>>();

        public NodeVisual(TId id, TWeight Weight = default(TWeight), TState State = default(TState))
        {
            Id = id;
            weight = Weight;
            state = State;
            GetSignal(TypeChange.CreateNode);
        }

        public void Remove()
        {
            foreach (var edge in AdjacentEdges)
            {
                if (edge.Start.Id.Equals(Id))
                    edge.Start.AdjacentEdges.Remove(edge);
                else if (edge.End.Id.Equals(Id))
                    edge.End.AdjacentEdges.Remove(edge);
            }
            AdjacentEdges.Clear();
            GetSignal(TypeChange.RemoveNode);
        }

        public EdgeVisual<TId, TWeight, TState> Connect(NodeVisual<TId, TWeight, TState> other,
                                                        TWeight weightEdge = default(TWeight),
                                                        TState stateEdge = default(TState))
        {
            var edge = new EdgeVisual<TId, TWeight, TState>(this, other, weightEdge, stateEdge);
            this.AdjacentEdges.Add(edge);
            other.AdjacentEdges.Add(edge);
            return edge;
        }


        public void GetSignal(TypeChange type)
        {
            if (flag)
                observer.AddChange(
                    new Change<List<TId>, TWeight, TState>(
                        type, new List<TId>() { Id }, weight, state));
        }
    }

    public enum N
    {
        None,
        Is
    }



    class Algoritms
    {
        public static void Alg(Graph<int, int, N> graph)
        {
            for (var i = 0; i <= 4; i++)
            {
                if (graph.Nodes[i].State == N.Is)
                    continue;
                foreach (var edges in graph.Nodes[i].AdjacentEdges)
                {
                    edges.End.Weight += 1;
                    edges.End.State = N.Is;
                }
            }

        }

        public static void DS(NodeVisual<int, int, N> Start)
        {
            object a = null;
            a = 5;
        }
    }

    public class Program
    {
        public static void Main()
        {
            Bitmap bmp = new Bitmap(400, 400);

            // Создаем объект Graphics для рисования на изображении
            Graphics g = Graphics.FromImage(bmp);

            // Рисуем красный квадрат
            g.FillRectangle(Brushes.Red, 100, 100, 100, 100);

            // Рисуем синий круг
            g.FillEllipse(Brushes.Blue, 200, 200, 100, 100);


            // Рисуем зеленый треугольник
            Point[] points = { new Point(50, 50), new Point(150, 50), new Point(100, 150) };
            g.FillPolygon(Brushes.Green, points);

            // Сохраняем изображение в файл
            bmp.Save("geometric_shapes.png");

            // Освобождаем ресурсы
            g.Dispose();
            bmp.Dispose();

            var observer = new Observer<int, int, N>();
            NodeVisual<int, int, N>.observer = observer;
            EdgeVisual<int, int, N>.observer = observer;

            var graph = new Graph<int, int, N>();
            graph.Nodes.AddNode(new NodeVisual<int, int, N>(0));
            graph.Nodes.AddNode(new NodeVisual<int, int, N>(1));
            graph.Nodes.AddNode(new NodeVisual<int, int, N>(2));
            graph.Nodes.AddNode(new NodeVisual<int, int, N>(3));
            graph.Nodes.AddNode(new NodeVisual<int, int, N>(4));
            graph.Edges.AddEdge(graph.Nodes[0].Connect(graph.Nodes[1]));
            graph.Edges.AddEdge(graph.Nodes[1].Connect(graph.Nodes[2]));
            graph.Edges.AddEdge(graph.Nodes[1].Connect(graph.Nodes[3]));
            graph.Edges.AddEdge(graph.Nodes[0].Connect(graph.Nodes[4]));
            var firstNode = graph.Nodes[0];
            var secondEdge = graph.Edges[1, 2];
            Algoritms.Alg(graph);
            NodeVisual<int, int, N>.flag = false;
            EdgeVisual<int, int, N>.flag = false;
            var viz = new Visualizator<int, int, N>(observer.changes, ParseCollor);
            viz.StartVisualize();
        }

        public static Color ParseCollor(N state)
        {
            if (state == N.None)
                return Color.White;
            else return Color.Red;
        }
    }

    public enum TypeChange
    {
        ChangeNodeWeight,
        ChangeNodeState,
        GetNodeWeight,
        GetNodeState,

        ChangeEdgeWeight,
        ChangeEdgeState,
        GetEdgeWeight,
        GetEdgeState,

        CreateNode,
        CreateEdge,
        RemoveNode,
        AddEdge,
        RemoveEdge,
    }



    public interface IChange<out TId, out TWeight, out TState> 
    {
        TypeChange TypeChange { get; }
        TId Id { get; }
        TWeight Weight { get; }
        TState State { get; }
    }


    public class Change<TId, TWeight, TState> : IChange<TId, TWeight, TState>       
    {
        public TypeChange TypeChange { get; }

        public TId Id { get; }

        public TWeight Weight { get; }
        public TState State { get; }

        public Change(TypeChange type, TId id, TWeight weight, TState state)
        {
            TypeChange = type; Id = id; Weight = weight; State = state;
        }
    }





    public class Observer<TId, TWeight, TState>
    {
        public List<Change<List<TId>, TWeight, TState>> changes = 
            new List<Change<List<TId>, TWeight, TState>>();


        public void AddChange(Change<List<TId>, TWeight, TState> typeChange)
        { changes.Add(typeChange); }
    }

    public class NodeVisualData
    {
        public int X { get; private set; } 
        public int Y { get; private set; }
        public static int Radius;
        public static bool isFirstNode = true;

        public NodeVisualData(int x, int y)
        { X = x; Y = y; }
    }

    public class EdgeVisualData
    {
        public NodeVisualData Start { get; private set; }
        public NodeVisualData End { get; private set; }
        public static int Weidth;

        public EdgeVisualData(NodeVisualData start, NodeVisualData end)
        { Start = start; End = end; }
    }


    public class Visualizator<TId, TWeight, TState>

    {
        Graph<TId, TWeight, TState> visualizedGraph;
        List<Change<List<TId>, TWeight, TState>> changes;
        Bitmap bmp;
        Dictionary<NodeVisual<TId, TWeight, TState>, NodeVisualData> dataNodeVisual;
        Dictionary<EdgeVisual<TId, TWeight, TState>, EdgeVisualData> dataEdgeVisual;
        int size;
        int counterForSave = 0;
        Func<TState, Color> parserCollor;
        

        public Visualizator(List<Change<List<TId>, TWeight, TState>> changes, Func<TState, Color> parserCollor)
        {
            visualizedGraph = new Graph<TId, TWeight, TState>();
            this.changes = changes;
            this.parserCollor = parserCollor;
            dataNodeVisual = new Dictionary<NodeVisual<TId, TWeight, TState>, NodeVisualData>();
            dataEdgeVisual = new Dictionary<EdgeVisual<TId, TWeight, TState>, EdgeVisualData>();
        }

        public void StartVisualize()
        {
            
            var counterObject = GetCounterObjectInGraph(changes);
            size = (int)(200 * Math.Sqrt(counterObject));
            NodeVisualData.Radius = (size / (2 * counterObject)) + 1;
            EdgeVisualData.Weidth = (size / (10 * counterObject)) + 1;
            foreach(var change in changes)
            {
                bmp = new Bitmap(size, size);
                PartGraph? changeOnlyOnThisStep = AddChangeInGraph(change);
                DrawGraph(changeOnlyOnThisStep);
            }
        }

        public int GetCounterObjectInGraph(List<Change<List<TId>, TWeight, TState>> changes)
        {
            var maxCounterObject = 0;
            var counterObject = 0;
            foreach (var change in changes)
            {
                if (change.TypeChange == TypeChange.CreateNode)
                    counterObject += 1;
                if (change.TypeChange == TypeChange.RemoveNode)
                    counterObject -= 1;
                if (change.TypeChange == TypeChange.CreateEdge)
                    counterObject += 3;
                if (change.TypeChange == TypeChange.RemoveEdge)
                    counterObject -= 3;
                if (maxCounterObject < counterObject)
                    maxCounterObject = counterObject;
            }
            return maxCounterObject;
        }


        public PartGraph? AddChangeInGraph(Change<List<TId>, TWeight, TState> change)
        {
            var d = new Dictionary<TypeChange, Func<Change<List<TId>, TWeight, TState>, PartGraph?>>
            { 
                [TypeChange.CreateNode] = AddNode, 
                [TypeChange.RemoveNode] = RemoveNode,
                [TypeChange.CreateEdge] = AddEdge,
                [TypeChange.RemoveEdge] = RemoveEdge,

                [TypeChange.GetNodeWeight] = GetNode,
                [TypeChange.ChangeNodeWeight] = ChangeNode,
                [TypeChange.GetNodeState] = GetNode,
                [TypeChange.ChangeNodeState] = ChangeNode,

                [TypeChange.GetEdgeWeight] = GetEdge,
                [TypeChange.ChangeEdgeWeight] = ChangeEdge,
                [TypeChange.GetEdgeState] = GetEdge,
                [TypeChange.ChangeEdgeState] = ChangeEdge,
            } ;
            var result = d[change.TypeChange](change);
            return result;
        }

        public PartGraph? GetEdge(Change<List<TId>, TWeight, TState> change)
            => visualizedGraph.edgesDict[Tuple.Create(change.Id[0], change.Id[1])];

        public PartGraph? GetNode(Change<List<TId>, TWeight, TState> change)
            => visualizedGraph.nodesDict[change.Id[0]];

        public PartGraph? ChangeNode(Change<List<TId>, TWeight, TState> change)
        {
            var node = visualizedGraph.nodesDict[change.Id[0]];
            node.Weight = change.Weight;
            node.State = change.State;
            return node;
        }

        public PartGraph? ChangeEdge(Change<List<TId>, TWeight, TState> change)
        {
            var edge = visualizedGraph.edgesDict[Tuple.Create(change.Id[0], change.Id[1])];
            edge.Weight = change.Weight;
            edge.State = change.State;
            return edge;
        }



        public PartGraph? RemoveEdge(Change<List<TId>, TWeight, TState> change)
        {
            var currEdge = visualizedGraph.edgesDict[Tuple.Create(change.Id[0], change.Id[1])];
            dataEdgeVisual.Remove(currEdge);
            visualizedGraph.Edges.RemoveEdge(currEdge);
            return null;
        }

        public PartGraph? AddEdge(Change<List<TId>, TWeight, TState> change)
        {
            var start = visualizedGraph.nodesDict[change.Id[0]];
            var end = visualizedGraph.nodesDict[change.Id[1]];
            var newEdge = start.Connect(end, change.Weight, change.State);
            dataEdgeVisual.Add(newEdge, new EdgeVisualData(dataNodeVisual[start], dataNodeVisual[end]));
            visualizedGraph.Edges.AddEdge(newEdge);
            return null;
        }

        public PartGraph? RemoveNode(Change<List<TId>, TWeight, TState> change)
        {
            var currNode = visualizedGraph.nodesDict[change.Id[0]];
            dataNodeVisual.Remove(currNode);
            visualizedGraph.Nodes.RemoveNode(currNode);    
            return null;
        }

        public PartGraph? AddNode(Change<List<TId>, TWeight, TState> change)
        {
            var newNode = new NodeVisual<TId, TWeight, TState>(change.Id[0], change.Weight, change.State);
            Random rnd = new Random();
            if (NodeVisualData.isFirstNode)
            {             
                var randomX = rnd.Next(0 + 2 * NodeVisualData.Radius + 1, (size/2) - NodeVisualData.Radius);
                var randomY = rnd.Next(0 + 2 * NodeVisualData.Radius + 1, (size / 2) - NodeVisualData.Radius);
                dataNodeVisual.Add(newNode, new NodeVisualData(randomX, randomY));
                visualizedGraph.Nodes.AddNode(newNode);
                NodeVisualData.isFirstNode = false;
                return null;
            }
            var distanceMin = 3 * NodeVisualData.Radius;
            var distanceMax = 5 * NodeVisualData.Radius;
            double maxAmountEdgeIntersections = 0;
            while (true)
            {
                var randomX = rnd.Next(0 + 2 * NodeVisualData.Radius , size - NodeVisualData.Radius);
                var randomY = rnd.Next(0 + 2 * NodeVisualData.Radius , size  - NodeVisualData.Radius);
                if (!IsInCorrectDistanceFromNodes(randomX, randomY, distanceMin, distanceMax))
                {
                    distanceMax += NodeVisualData.Radius + 1;
                    continue;
                }
                if (!IsNotInsertNodeIfConnectWithOtherNode(randomX, randomY))
                    continue;
                if (AmountInsertEdgeIfConnectWithOtherNode(randomX, randomY) > maxAmountEdgeIntersections)
                {
                    maxAmountEdgeIntersections += 0.25;
                    continue;
                }
                dataNodeVisual.Add(newNode, new NodeVisualData(randomX, randomY));
                visualizedGraph.Nodes.AddNode(newNode);
                return null;
            }
        }

        public bool IsInCorrectDistanceFromNodes(int x, int y, int distanceMin, int distanceMax)
        {
            foreach (var node in visualizedGraph.Nodes)
            {
                var pointCurrNode = dataNodeVisual[node];
                var distance = Infrastructure.GetDistance(x, y, pointCurrNode.X, pointCurrNode.Y);
                if ( distanceMin<= distance && distance <= distanceMax)
                    return true;
            }
            return false;
        }

        public bool IsNotInsertNodeIfConnectWithOtherNode(int x, int y)
        {
            var r = NodeVisualData.Radius;
            foreach (var node in visualizedGraph.Nodes)
            {
                var pointCurrNode = dataNodeVisual[node];
                var x1 = x; var y1 = y; var x2 = pointCurrNode.X; var y2 = pointCurrNode.Y;
                foreach (var currNode in visualizedGraph.Nodes)
                {
                    if (node.Id.Equals(currNode.Id))
                        continue;
                    var centerCircusX = dataNodeVisual[currNode].X;
                    var centerCircusY = dataNodeVisual[currNode].Y;
                    if (Infrastructure.IsCircleIntersectSegment(centerCircusX, centerCircusY, r, x1, y1, x2, y2))
                        return false;
                }
            }
            return true;
        }

        public int AmountInsertEdgeIfConnectWithOtherNode(int x, int y)
        {
            var r = NodeVisualData.Radius;
            var counter = 0;
            foreach (var node in visualizedGraph.Nodes)
            {
                var pointCurrNode = dataNodeVisual[node];
                var x1 = x; var y1 = y; var x2 = pointCurrNode.X; var y2 = pointCurrNode.Y;
                foreach (var edge in visualizedGraph.Edges)
                {
                    var x3 = dataEdgeVisual[edge].Start.X; var y3 = dataEdgeVisual[edge].Start.Y;
                    var x4 = dataEdgeVisual[edge].End.X; var y4 = dataEdgeVisual[edge].End.Y;
                    if (Infrastructure.IsSegmentIntersectSegment(x1, y1, x2, y2, x3,y3, x4, y4))
                        counter += 1;
                }
            }
            return counter;
        }




        public void DrawGraph(PartGraph changeOnlyOnThisStep)
        {
            var g = Graphics.FromImage(bmp);
            foreach (var edge in visualizedGraph.Edges)
                DrawEdge(edge, changeOnlyOnThisStep, g);
            foreach (var node in visualizedGraph.Nodes)
                DrawNode(node, changeOnlyOnThisStep, g);
            bmp.Save($"graph_shapes{counterForSave}.png");
            counterForSave += 1;
            g.Dispose();
            bmp.Dispose();
        }

        public void DrawEdge(EdgeVisual<TId, TWeight, TState> edge ,PartGraph changeOnlyOnThisStep, Graphics g)
        {
            Pen pen;
            if (edge == changeOnlyOnThisStep)
                pen = new Pen(parserCollor(edge.State), EdgeVisualData.Weidth * 3);
            else
                pen = new Pen(parserCollor(edge.State), EdgeVisualData.Weidth);
            g.DrawLine(pen, dataEdgeVisual[edge].Start.X, dataEdgeVisual[edge].Start.Y,
                       dataEdgeVisual[edge].End.X, dataEdgeVisual[edge].End.Y);
            var xLetter = (dataEdgeVisual[edge].Start.X + dataEdgeVisual[edge].End.X) / 2;
            var yLetter = (dataEdgeVisual[edge].Start.Y + dataEdgeVisual[edge].End.Y) / 2;

            Font drawFont = new Font("Arial", EdgeVisualData.Weidth * 3);
            SolidBrush drawBrush = new SolidBrush(Color.Black);

            // Рисуем цифру 5 в координатах (10, 10) на изображении
            g.DrawString(edge.Weight.ToString(), drawFont, drawBrush, new PointF(xLetter, yLetter));
        }

        public void DrawNode(NodeVisual<TId, TWeight, TState> node, PartGraph changeOnlyOnThisStep, Graphics g)
        {
            SolidBrush brush = new SolidBrush(parserCollor(node.State));
            int size;
            if (node == changeOnlyOnThisStep)
                size = (int)(1.5 * NodeVisualData.Radius);
            else
                size = NodeVisualData.Radius;
            brush.Color = parserCollor(node.State);
            var pointNode = dataNodeVisual[node];
            g.FillEllipse(brush, pointNode.X - size, pointNode.Y - size, 2 * size, 2 * size);
            Font drawFont = new Font("Arial", EdgeVisualData.Weidth * 3);
            SolidBrush drawBrush = new SolidBrush(Color.Black);

            // Рисуем цифру 5 в координатах (10, 10) на изображении
            g.DrawString(node.Weight.ToString(), drawFont, drawBrush, new PointF(pointNode.X, pointNode.Y));
        }


    }






    public static class Infrastructure
    {
        public static double GetDistance(int x1, int y1, int x2, int y2)
        {
            var distance = Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
            return distance ;
        }

        public static bool IntersectsCircleWithCircle(int xCenter1, int yCenter1, int radius1,
                                               int xCenter2, int yCenter2, int radius2)
        {
            double distance = Math.Sqrt(Math.Pow((xCenter2 - xCenter1), 2) + Math.Pow((yCenter2 - yCenter1), 2));
            return distance <= (radius1 + radius2);
        }

        public static bool IsCircleIntersectSegment(double centerCirculx, double centerCircuy, double radius,
            double x1, double y1, double x2, double y2)
        {
            double closestX = 0;
            double closestY = 0;
            double dx = x2 - x1;
            double dy = y2 - y1;

            double t = ((centerCirculx - x1) * dx + (centerCircuy - y1) * dy) / (dx * dx + dy * dy);

            if (t < 0) 
            {
                closestX = x1;
                closestY = y1;
            }
            else if (t > 1) 
            {
                closestX = x2;
                closestY = y2;
            }
            else 
            {
                closestX = x1 + t * dx;
                closestY = y1 + t * dy;
            }
            double distance = Math.Sqrt((closestX - centerCirculx) * (closestX - centerCirculx) + 
                (closestY - centerCircuy) * (closestY - centerCircuy));
            return distance <= radius;
        }

        public static bool IsSegmentIntersectSegment(double x1, double y1, double x2, double y2,
                                                     double x3, double y3, double x4, double y4)
        {
            double a1 = y2 - y1;
            double b1 = x1 - x2;
            double c1 = a1 * x1 + b1 * y1;

            double a2 = y4 - y3;
            double b2 = x3 - x4;
            double c2 = a2 * x3 + b2 * y3;

            double determinant = a1 * b2 - a2 * b1;

            if (determinant == 0)
            {
                return false; // Отрезки параллельны
            }
            else
            {
                double intersectionX = (b2 * c1 - b1 * c2) / determinant;
                double intersectionY = (a1 * c2 - a2 * c1) / determinant;

                bool onSegment1 = IsPointOnSegment(intersectionX, intersectionY, x1, y1, x2, y2);
                bool onSegment2 = IsPointOnSegment(intersectionX, intersectionY, x3, y3, x4, y4);

                return onSegment1 && onSegment2;
            }
        }

        public static bool IsPointOnSegment(double px, double py, double x1, double y1, double x2, double y2)
        {
            bool betweenX = (px >= x1 && px <= x2) || (px >= x2 && px <= x1);
            bool betweenY = (py >= y1 && py <= y2) || (py >= y2 && py <= y1);
            return betweenX && betweenY;
        }
    }
}