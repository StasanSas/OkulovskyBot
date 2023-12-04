using System.ComponentModel.Design;
using System.Data;
using System.Drawing;



namespace DrawerGraphs
{
    /* 
    {

public class NodeVisual 
{
    private int weight;
    private N state;

    public readonly int Id;
    public int Weight {get; set;}

    public N State {get; set;}


    public List<NodeVisual> AdjacentNodes = new List<NodeVisual>();

    public NodeVisual(int id, int Weight = 0, N State = N.None)
    {
        Id = id;
        weight = Weight;
        state = State;
    }

    public void Remove()
    {
        foreach (var node in AdjacentNodes)
        {
            node.AdjacentNodes.Remove(this);
        }
    }

    public void Connect(NodeVisual other)
    {
        this.AdjacentNodes.Add(other);
        other.AdjacentNodes.Add(this);
    }

    public void RemoveEdge(NodeVisual other)
    {
        this.AdjacentNodes.Remove(other);
        other.AdjacentNodes.Remove(this);
    }
}

    */
    public class NodeVisual 
    {
        private int weight;
        private N state;


        public static Observer observer;
        public readonly int Id;
        public int Weight 
        {
            get
            {
                GetSignalGetWeight();
                return weight;
            }
            set
            {
                GetSignalSetWeight(value);
                weight = value;
            } 
        }
        public N State 
        { 
            get
            {
                GetSignalGetState();
                return state;
            }
            set
            {
                GetSignalSetState(value);
                state = value;
            }
        }

        public List<NodeVisual> AdjacentNodes = new List<NodeVisual>();

        public NodeVisual(int id, int Weight=0 , N State = N.None)
        {
            Id = id;
            weight = Weight;
            state = State;
            GetSignalCreateNode();
        }

        public void Remove()
        {
            foreach(var node in AdjacentNodes)
            {
                node.AdjacentNodes.Remove(this);
            }
            GetSignalRemoveNode();
        }

        public void Connect(NodeVisual other)
        {
            this.AdjacentNodes.Add(other);
            other.AdjacentNodes.Add(this);
            GetSignalAddEdge(other);
        }

        public void RemoveEdge(NodeVisual other)
        {
            this.AdjacentNodes.Remove(other);
            other.AdjacentNodes.Remove(this);
            GetSignalRemoveEdge(other);
        }

        public void GetSignalCreateNode() 
        {
            observer.AddChange(new NodeCreateOrRemove(TypeChange.CreateNode, this));
        }
        public void GetSignalRemoveNode() 
        {
            observer.AddChange(new NodeCreateOrRemove(TypeChange.CreateNode, this));
        }
        public void GetSignalRemoveEdge(NodeVisual other) 
        {
            observer.AddChange(
                new EdgeCreateOrRemove(TypeChange.RemoveEdge, Tuple.Create(this, other)));
        }
        public void GetSignalAddEdge(NodeVisual other) 
        {
            observer.AddChange(
                new EdgeCreateOrRemove(TypeChange.CreateEdge, Tuple.Create(this, other)));
        }
        public void GetSignalGetWeight() 
        {
            observer.AddChange(
                new PropertyChangeOrViewing(TypeChange.GetWeight, weight));
        }
        public void GetSignalSetWeight(int newWeight) 
        {
            observer.AddChange(
                new PropertyChangeOrViewing(TypeChange.ChangeWeight, newWeight));
        }
        public void GetSignalGetState() 
        {
            observer.AddChange(
                new PropertyChangeOrViewing(TypeChange.GetState, state));
        }
        public void GetSignalSetState(N newState) 
        {
            observer.AddChange(
                new PropertyChangeOrViewing(TypeChange.ChangeState, newState));
        }

    }

    public enum N
    {
        None,
        Is        
    }



    class Algoritms
    {
        public static void DFS(NodeVisual Start) 
        {
            Start.Weight += 5;
            foreach(var node in Start.AdjacentNodes) 
            { 
                if (node.State == N.None)
                {
                    node.Weight += 3;
                    node.State = N.Is;
                    DFS(node);
                    
                }                    
            }
        }
    }

    // class Program
    // {
    //     static void Main(string[] args)
    //     {
    //         var observer = new Observer();
    //         NodeVisual.observer = observer;
    //         
    //         List<NodeVisual> graph = new List<NodeVisual>();
    //         var a = new NodeVisual(0);
    //         var b = new NodeVisual(1);
    //         var c = new NodeVisual(3);
    //         var d = new NodeVisual(4);
    //         var e = new NodeVisual(5);
    //         a.Connect(b);
    //         a.Connect(e); 
    //         b.Connect(c); 
    //         b.Connect(d);
    //         Algoritms.DFS(a);
    //         var z = 0;
    //     }
    // }

    public enum TypeChange
    {
        ChangeWeight,
        ChangeState,
        GetWeight,
        GetState,
        CreateNode,
        CreateEdge,
        RemoveNode,
        AddEdge,
        RemoveEdge,
    }


    public interface NodeChanges<out T> 
    {
        TypeChange TypeChange { get; }
        T Change { get; }
    }

    public class NodeCreateOrRemove : NodeChanges<NodeVisual>
    {
        public TypeChange TypeChange { get; }

        public NodeVisual Change { get; }

        public NodeCreateOrRemove(TypeChange type, NodeVisual change)
        {
            TypeChange = type; Change = change;
        }
    }

    public class EdgeCreateOrRemove : NodeChanges<Tuple<NodeVisual, NodeVisual>>
    {
        public TypeChange TypeChange { get; }
        public Tuple<NodeVisual, NodeVisual> Change { get; }

        public EdgeCreateOrRemove(TypeChange type, Tuple<NodeVisual, NodeVisual> change)
        {
            TypeChange = type; Change = change;
        }
    }

    public class PropertyChangeOrViewing : NodeChanges<object>
    {
        public TypeChange TypeChange { get; }

        public object Change { get; }

        public PropertyChangeOrViewing(TypeChange type, object change)
        {
            TypeChange = type; Change = change;
        }
    }



    public class Observer
    {
        public List<NodeChanges<object>> changes = new List<NodeChanges<object>>();


        public void AddChange(NodeChanges<object> typeChange) 
        { changes.Add(typeChange); }
    }


}