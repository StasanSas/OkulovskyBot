

namespace Graph.Inf
{
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
}