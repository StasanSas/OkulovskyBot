using Graph.Inf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graph.Dom
{
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
}
