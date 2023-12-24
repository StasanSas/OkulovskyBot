using System.Drawing;
using System.Collections;
using Color = System.Drawing.Color;
using System.Xml.Linq;
using Graph.Inf;

namespace DrawerGraphs;

public class Graph<TId, TWeight, TState>
{
    protected readonly Dictionary<TId, NodeVisual<TId, TWeight, TState>> NodesDict = 
        new Dictionary<TId, NodeVisual<TId, TWeight, TState>>();

    protected readonly Dictionary<Tuple<TId, TId>, EdgeVisual<TId, TWeight, TState>> EdgesDict =
        new Dictionary<Tuple<TId, TId>, EdgeVisual<TId, TWeight, TState>>();

    private Observer<TId, TWeight, TState>? observer = null;
    private bool isWithObserver;

    public readonly NodeIndexer Nodes;
    public readonly EdgeIndexer Edges;
    public Graph(bool isWithObserver = false)
    {
        Nodes = new NodeIndexer(this);
        Edges = new EdgeIndexer(this);
        this.isWithObserver = isWithObserver;
        if (isWithObserver)
            observer = new Observer<TId, TWeight, TState>();
    }

    public List<Change<List<TId>, TWeight, TState>> GetChanges()
    {
        if (this.isWithObserver)
            return observer.changes;
        else
            throw new Exception();
    }


    public class NodeIndexer : IEnumerable<NodeVisual<TId, TWeight, TState>>
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
                if (parent.NodesDict.ContainsKey(id))
                    return parent.NodesDict[id];
                throw new KeyNotFoundException();
            }
        }

        public void AddNode(NodeVisual<TId, TWeight, TState> node)
        {
            var addedNode = new NodeVisual<TId, TWeight, TState>(node.Id, node.Weight, node.State, parent.observer);
            parent.NodesDict[node.Id] = addedNode;           
        }

        public IEnumerator<NodeVisual<TId, TWeight, TState>> GetEnumerator()
        {
            foreach (var keyAndValue in parent.NodesDict.AsEnumerable())
                yield return keyAndValue.Value;
        }

        public void RemoveNode(NodeVisual<TId, TWeight, TState> node)
        {
            parent.NodesDict.Remove(node.Id);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class EdgeIndexer : IEnumerable<EdgeVisual<TId, TWeight, TState>>
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
                var IdEdge = Tuple.Create(idStart, idEnd);
                if (parent.EdgesDict.ContainsKey(IdEdge))
                    return parent.EdgesDict[IdEdge];
                throw new KeyNotFoundException();
            }
        }

        public void AddEdge(EdgeVisual<TId, TWeight, TState> edge)
        {
            var IdEdge = Tuple.Create(edge.Start.Id, edge.End.Id);
            if (!parent.NodesDict.ContainsKey(edge.Start.Id) ||
                !parent.NodesDict.ContainsKey(edge.End.Id))
                throw new KeyNotFoundException();
            var startNode = parent.NodesDict[edge.Start.Id];
            var endNode = parent.NodesDict[edge.End.Id];
            var addedEdge = new EdgeVisual<TId, TWeight, TState>(startNode, endNode, edge.Weight, edge.State, parent.observer);
            parent.EdgesDict[IdEdge] = addedEdge;
        }


        public IEnumerator<EdgeVisual<TId, TWeight, TState>> GetEnumerator()
        {
            foreach (var keyAndValue in parent.EdgesDict.AsEnumerable())
                yield return keyAndValue.Value;
        }

        public void RemoveEdge(EdgeVisual<TId, TWeight, TState> edge)
        {
            var IdEdge = Tuple.Create(edge.Start.Id, edge.End.Id);
            if (!parent.EdgesDict.ContainsKey(IdEdge))
                throw new KeyNotFoundException();
            parent.EdgesDict.Remove(IdEdge);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

public interface IPartGraph { }

public class EdgeVisual<TId, TWeight, TState> : IPartGraph
{
    private TWeight weight;
    private TState state;
    public NodeVisual<TId, TWeight, TState> Start { get; private set; }
    public NodeVisual<TId, TWeight, TState> End { get; private set; }

    private Observer<TId, TWeight, TState>? observer = null;

    public Observer<TId, TWeight, TState>? Observer 
    {
        get 
        { 
            return observer; 
        }
        set
        {
            if (observer is null)
                observer = value;
            else
                throw new Exception();
        }    
    }

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
                      TWeight Weight = default(TWeight), 
                      TState State = default(TState),
                      Observer<TId, TWeight, TState>? observer = null)
    {
        weight = Weight;
        state = State;
        Start = start;
        End = end;
        Observer = observer;
        GetSignal(TypeChange.CreateEdge);
    }

    public void Remove()
    {
        GetSignal(TypeChange.RemoveEdge);
    }

    public void GetSignal(TypeChange type)
    {
        if (observer != null)
            observer.AddChange(
                new Change<List<TId>, TWeight, TState>(
                    type, new List<TId>() { Start.Id, End.Id },
                    weight, state));
    }

    public override int GetHashCode()
    {
        return Start.Id.GetHashCode() + End.Id.GetHashCode() * 379;
    }

    public override bool Equals(object? obj)
    {
        if (obj.GetType() != typeof(EdgeVisual<TId, TWeight, TState>))
            return false;
        var objTypeOfNodeVisual = obj as EdgeVisual<TId, TWeight, TState>;
        return Start.Id.Equals(objTypeOfNodeVisual.Start.Id) && End.Id.Equals(objTypeOfNodeVisual.End.Id);
    }
}

public class NodeVisual<TId, TWeight, TState> : IPartGraph
{
    private TWeight weight;
    private TState state;


    private Observer<TId, TWeight, TState>? observer = null;

    public Observer<TId, TWeight, TState>? Observer
    {
        get
        {
            return observer;
        }
        set
        {
            if (observer is null)
                observer = value;
            else
                throw new Exception();
        }
    }

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

    public NodeVisual(TId id,
                      TWeight Weight = default(TWeight),
                      TState State = default(TState),
                      Observer<TId, TWeight, TState>? observer = null)
    {
        Id = id;
        weight = Weight;
        state = State;
        Observer = observer;
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
        if (observer != null)
            observer.AddChange(
                new Change<List<TId>, TWeight, TState>(
                    type, new List<TId>() { Id }, weight, state));
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (obj.GetType() != typeof(NodeVisual<TId, TWeight, TState>))
            return false;
        var objTypeOfNodeVisual = obj as NodeVisual<TId, TWeight, TState>;
        return Id.Equals(objTypeOfNodeVisual.Id);
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

        var graph = new Graph<int, int, N>(true);
        graph.Nodes.AddNode(new NodeVisual<int, int, N>(0));
        graph.Nodes.AddNode(new NodeVisual<int, int, N>(1));
        graph.Nodes.AddNode(new NodeVisual<int, int, N>(2));
        graph.Nodes.AddNode(new NodeVisual<int, int, N>(3));
        graph.Nodes.AddNode(new NodeVisual<int, int, N>(4));
        graph.Edges.AddEdge(graph.Nodes[0].Connect(graph.Nodes[1]));
        graph.Edges.AddEdge(graph.Nodes[1].Connect(graph.Nodes[2]));
        graph.Edges.AddEdge(graph.Nodes[1].Connect(graph.Nodes[3]));
        graph.Edges.AddEdge(graph.Nodes[0].Connect(graph.Nodes[4]));
        graph.Edges.AddEdge(graph.Nodes[2].Connect(graph.Nodes[3]));
        graph.Edges.AddEdge(graph.Nodes[2].Connect(graph.Nodes[4]));
        var firstNode = graph.Nodes[0];
        var secondEdge = graph.Edges[1, 2];
        Algoritms.Alg(graph);
        var viz = new Visualizator<int, int, N>(graph.GetChanges(), ParseColor);
        viz.StartVisualize();
    }

    public static Color ParseColor(N state)
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
    public static int Weight;

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
    Func<TState, Color> parserColor;
    Dictionary<TypeChange, Func<Change<List<TId>, TWeight, TState>, IPartGraph?>> processingByType;


    public Visualizator(List<Change<List<TId>, TWeight, TState>> changes, Func<TState, Color> parserColor)
    {
        visualizedGraph = new Graph<TId, TWeight, TState>();
        this.changes = changes;
        this.parserColor = parserColor;
        dataNodeVisual = new Dictionary<NodeVisual<TId, TWeight, TState>, NodeVisualData>();
        dataEdgeVisual = new Dictionary<EdgeVisual<TId, TWeight, TState>, EdgeVisualData>();
        processingByType = new Dictionary<TypeChange, Func<Change<List<TId>, TWeight, TState>, IPartGraph?>>()
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
        };
    }

    public void StartVisualize()
    {
        
        var counterObject = GetCounterObjectInGraph(changes);
        size = (int)(200 * Math.Sqrt(counterObject));
        NodeVisualData.Radius = (size / (counterObject)) + 1;
        EdgeVisualData.Weight = (size / (10 * counterObject)) + 1;
        foreach(var change in changes)
        {
            bmp = new Bitmap(size, size);
            IPartGraph? changeOnlyOnThisStep = AddChangeInGraph(change);
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


    public IPartGraph? AddChangeInGraph(Change<List<TId>, TWeight, TState> change)
    {
        var result = processingByType[change.TypeChange](change);
        return result;
    }

    public IPartGraph? GetEdge(Change<List<TId>, TWeight, TState> change)
    {
        return visualizedGraph.Edges[change.Id[0], change.Id[1]];
    }

    public IPartGraph? GetNode(Change<List<TId>, TWeight, TState> change)
    {
        return visualizedGraph.Nodes[change.Id[0]];
    }
        

    public IPartGraph? ChangeNode(Change<List<TId>, TWeight, TState> change)
    {
        var node = visualizedGraph.Nodes[change.Id[0]];
        node.Weight = change.Weight;
        node.State = change.State;
        return node;
    }

    public IPartGraph? ChangeEdge(Change<List<TId>, TWeight, TState> change)
    {
        var edge = visualizedGraph.Edges[change.Id[0], change.Id[1]];
        edge.Weight = change.Weight;
        edge.State = change.State;
        return edge;
    }



    public IPartGraph? RemoveEdge(Change<List<TId>, TWeight, TState> change)
    {
        var currEdge = visualizedGraph.Edges[change.Id[0], change.Id[1]];
        dataEdgeVisual.Remove(currEdge);
        visualizedGraph.Edges.RemoveEdge(currEdge);
        return null;
    }

    public IPartGraph? AddEdge(Change<List<TId>, TWeight, TState> change)
    {
        var start = visualizedGraph.Nodes[change.Id[0]];
        var end = visualizedGraph.Nodes[change.Id[1]];
        var newEdge = start.Connect(end, change.Weight, change.State);
        dataEdgeVisual.Add(newEdge, new EdgeVisualData(dataNodeVisual[start], dataNodeVisual[end]));
        visualizedGraph.Edges.AddEdge(newEdge);
        return null;
    }

    public IPartGraph? RemoveNode(Change<List<TId>, TWeight, TState> change)
    {
        var currNode = visualizedGraph.Nodes[change.Id[0]];
        dataNodeVisual.Remove(currNode);
        visualizedGraph.Nodes.RemoveNode(currNode);    
        return null;
    }

    public IPartGraph? AddNode(Change<List<TId>, TWeight, TState> change)
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
        var distanceMin = 4 * NodeVisualData.Radius;
        var distanceMax = 6 * NodeVisualData.Radius;
        double maxAmountEdgeIntersections = 0;
        while (true)
        {
            Console.WriteLine(change.Id[0]);
            Console.WriteLine(change.Id[0]);
            Console.WriteLine(change.Id[0]);
            var randomX = rnd.Next(0 + 2 * NodeVisualData.Radius , size - NodeVisualData.Radius);
            var randomY = rnd.Next(0 + 2 * NodeVisualData.Radius , size  - NodeVisualData.Radius);
            if (!IsInCorrectDistanceFromNodes(randomX, randomY, distanceMin, distanceMax))
            {
                distanceMax += 1;
                continue;
            }
            if (IsInsertNodeIfConnectWithOtherNode(randomX, randomY))
                continue;
            if (AmountInsertEdgeIfConnectWithOtherNode(randomX, randomY) > maxAmountEdgeIntersections)
            {
                Console.WriteLine(maxAmountEdgeIntersections);
                maxAmountEdgeIntersections += 0.1;
                continue;
            }
            dataNodeVisual.Add(newNode, new NodeVisualData(randomX, randomY));
            visualizedGraph.Nodes.AddNode(newNode);
            return null;
        }
    }

    public bool IsInCorrectDistanceFromNodes(int x, int y, int distanceMin, int distanceMax)
    {
        var flag = false;
        foreach (var node in visualizedGraph.Nodes)
        {
            var pointCurrNode = dataNodeVisual[node];
            var distance = CalculatorDistance.GetDistance(x, y, pointCurrNode.X, pointCurrNode.Y);
            if (distanceMin >= distance)
                return false;
            if (distance < distanceMax)
                flag = true;
        }
        return flag;
        
    }

    public bool IsInsertNodeIfConnectWithOtherNode(int x, int y)
    {

        var r = 2 * NodeVisualData.Radius;
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
                Console.WriteLine();
                Console.WriteLine($"{node.Id} {currNode.Id}");
                Console.WriteLine($"{centerCircusX}, {centerCircusY}, {r}, {x1}, {y1}, {x2}, {y2}");
                if (CalculatorDistance.IsCircleIntersectSegment(centerCircusX, centerCircusY, r, x1, y1, x2, y2))
                    return true;
                Console.WriteLine();
            }
        }
        return false;
    }

    public int AmountInsertEdgeIfConnectWithOtherNode(int x, int y)
    {
        var r = 2 * NodeVisualData.Radius;
        var counter = 0;
        foreach (var node in visualizedGraph.Nodes)
        {
            var pointCurrNode = dataNodeVisual[node];
            var x1 = x; var y1 = y; var x2 = pointCurrNode.X; var y2 = pointCurrNode.Y;
            foreach (var edge in visualizedGraph.Edges)
            {
                var x3 = dataEdgeVisual[edge].Start.X; var y3 = dataEdgeVisual[edge].Start.Y;
                var x4 = dataEdgeVisual[edge].End.X; var y4 = dataEdgeVisual[edge].End.Y;
                if (CalculatorDistance.IsSegmentIntersectSegment(x1, y1, x2, y2, x3,y3, x4, y4))
                    counter += 1;
            }
        }
        return counter;
    }




    public void DrawGraph(IPartGraph changeOnlyOnThisStep)
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

    public void DrawEdge(EdgeVisual<TId, TWeight, TState> edge ,IPartGraph changeOnlyOnThisStep, Graphics g)
    {
        Pen pen;
        if (edge == changeOnlyOnThisStep)
            pen = new Pen(parserColor(edge.State), EdgeVisualData.Weight * 3);
        else
            pen = new Pen(parserColor(edge.State), EdgeVisualData.Weight);
        g.DrawLine(pen, dataEdgeVisual[edge].Start.X, dataEdgeVisual[edge].Start.Y,
                   dataEdgeVisual[edge].End.X, dataEdgeVisual[edge].End.Y);
        var xLetter = (dataEdgeVisual[edge].Start.X + dataEdgeVisual[edge].End.X) / 2;
        var yLetter = (dataEdgeVisual[edge].Start.Y + dataEdgeVisual[edge].End.Y) / 2;

        Font drawFont = new Font("Arial", EdgeVisualData.Weight * 3);
        SolidBrush drawBrush = new SolidBrush(Color.BlueViolet);

        // Рисуем цифру 5 в координатах (10, 10) на изображении
        g.DrawString(edge.Weight.ToString(), drawFont, drawBrush, new PointF(xLetter, yLetter));
    }

    public void DrawNode(NodeVisual<TId, TWeight, TState> node, IPartGraph changeOnlyOnThisStep, Graphics g)
    {
        SolidBrush brush = new SolidBrush(parserColor(node.State));
        int size;
        if (node == changeOnlyOnThisStep)
            size = (int)(1.5 * NodeVisualData.Radius);
        else
            size = NodeVisualData.Radius;
        brush.Color = parserColor(node.State);
        var pointNode = dataNodeVisual[node];
        g.FillEllipse(brush, pointNode.X - size, pointNode.Y - size, 2 * size, 2 * size);
        Font drawFont = new Font("Arial", EdgeVisualData.Weight * 3);
        SolidBrush drawBrush = new SolidBrush(Color.BlueViolet);

        // Рисуем цифру 5 в координатах (10, 10) на изображении
        g.DrawString(node.Weight.ToString(), drawFont, drawBrush, new PointF(pointNode.X, pointNode.Y));
    }


}







