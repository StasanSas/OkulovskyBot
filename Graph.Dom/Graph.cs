using Graph.Inf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graph.Dom
{
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

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
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

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }

    public interface IPartGraph { }
}
