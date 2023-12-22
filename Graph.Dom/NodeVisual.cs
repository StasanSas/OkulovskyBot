using Graph.Inf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graph.Dom
{
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
}
