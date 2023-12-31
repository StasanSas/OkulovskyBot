﻿using Graph.Dom;
using Graph.Inf;
using ImageMagick;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graph.App
{
    public class Visualizator<TId, TWeight, TState>

    {
        Graph<TId, TWeight, TState> visualizedGraph;
        List<Change<List<TId>, TWeight, TState>> changes;
        Dictionary<NodeVisual<TId, TWeight, TState>, NodeVisualData> dataNodeVisual;
        Dictionary<EdgeVisual<TId, TWeight, TState>, EdgeVisualData> dataEdgeVisual;
        int size;
        Func<TState, Color> parserColor;
        Dictionary<TypeChange, Func<Change<List<TId>, TWeight, TState>, IPartGraph?>> processingByType;
        private AntiStaticVisualData _antiStaticVisualData;
        private int counterAttemptsPutNode = 0;
        int c = 0;


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

                [TypeChange.UnidentifiedNode] = GetNode,
                [TypeChange.UnidentifiedEdge] = GetEdge,
            };
            _antiStaticVisualData = new AntiStaticVisualData();
        }

        public void StartVisualize(int delay, string outputPath)
        {
            var counterObject = GetCounterObjectInGraph(changes);
            size = (int)(1000 * Math.Sqrt(counterObject));
            _antiStaticVisualData.NodeRadius = (size / (counterObject + 1)) + 1;
            _antiStaticVisualData.EdgeWidth = (size / (10 * counterObject)) + 1;
            var counterAttemptsVisualize = 0;
            while (counterAttemptsVisualize < 100)
            {
                if (TryVisualize(delay, outputPath))
                    break;
            }
        }

        public bool TryVisualize(int delay, string outputPath)
        {
            using (var magickImages = new MagickImageCollection())
            {
                foreach (var change in changes)
                {
                    
                    IPartGraph? changeOnlyOnThisStep = AddChangeInGraph(change);

                    if (change.TypeChange == TypeChange.CreateNode && counterAttemptsPutNode > 10000)
                    {
                        visualizedGraph = new Graph<TId, TWeight, TState>();
                        dataNodeVisual = new Dictionary<NodeVisual<TId, TWeight, TState>, NodeVisualData>();
                        dataEdgeVisual = new Dictionary<EdgeVisual<TId, TWeight, TState>, EdgeVisualData>();
                        _antiStaticVisualData.WaitFirstNode = true;
                        counterAttemptsPutNode = 0;
                        return false;
                    }

                    using (var bmp = new Bitmap(size, size))
                    {
                        var pictureStep = DrawGraph(changeOnlyOnThisStep, bmp);

                        using (var ms = new MemoryStream())
                        {
                            pictureStep.Save(ms, ImageFormat.Jpeg);
                            ms.Position = 0;

                            var magickImage = new MagickImage(ms);
                            magickImage.AnimationDelay = delay;
                            magickImages.Add(magickImage);
                        }
                    }
                }
                magickImages.OptimizeTransparency();
                magickImages.Write(outputPath);

                return true;
            }
        }

        public int GetCounterObjectInGraph(List<Change<List<TId>, TWeight, TState>> changes)
        {
            var maxCounterObject = 0;
            var counterObject = 0;
            foreach (var change in changes)
            {
                if (change.TypeChange == TypeChange.CreateNode)
                    counterObject += 2;
                if (change.TypeChange == TypeChange.RemoveNode)
                    counterObject -= 2;
                if (change.TypeChange == TypeChange.CreateEdge)
                    counterObject += 1;
                if (change.TypeChange == TypeChange.RemoveEdge)
                    counterObject -= 1;
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
            var r = _antiStaticVisualData.NodeRadius;

            counterAttemptsPutNode = 0;
            var bigRadius = (int)(1.5 * r);

            if (_antiStaticVisualData.WaitFirstNode)
            {          
                var randomX = rnd.Next(0 + bigRadius, size - bigRadius);
                var randomY = rnd.Next(0 + bigRadius, size - bigRadius);
                dataNodeVisual.Add(newNode, new NodeVisualData(randomX, randomY));
                visualizedGraph.Nodes.AddNode(newNode);
                _antiStaticVisualData.WaitFirstNode = false;
                return null;
            }
            var distanceMin = 4 * r;
            var distanceMax = 8 * r;
            double maxAmountEdgeIntersections = 0;
            while (true)
            {
                if (counterAttemptsPutNode > 10000)
                    return null;
                var randomX = rnd.Next(0 + bigRadius, size - bigRadius);
                var randomY = rnd.Next(0 + bigRadius, size - bigRadius);
                counterAttemptsPutNode += 1;
                if (!IsInCorrectDistanceFromNodes(randomX, randomY, distanceMin, distanceMax))
                {
                    distanceMax += (r / 10) + 1;
                    continue;
                }
                if (IsInsertNodeIfConnectWithOtherNode(randomX, randomY))
                {
                    distanceMax += (r / 10) + 1;
                    continue;
                }
                if (AmountInsertEdgeIfConnectWithOtherNode(randomX, randomY) > maxAmountEdgeIntersections)
                {
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

            var r = 2 * _antiStaticVisualData.NodeRadius;
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
                    if (CalculatorDistance.IsCircleIntersectSegment(centerCircusX, centerCircusY, r, x1, y1, x2, y2))
                        return true;
                }
            }
            return false;
        }

        public int AmountInsertEdgeIfConnectWithOtherNode(int x, int y)
        {
            var r = 2 * _antiStaticVisualData.NodeRadius;
            var counter = 0;
            foreach (var node in visualizedGraph.Nodes)
            {
                var pointCurrNode = dataNodeVisual[node];
                var x1 = x; var y1 = y; var x2 = pointCurrNode.X; var y2 = pointCurrNode.Y;
                foreach (var edge in visualizedGraph.Edges)
                {
                    var x3 = dataEdgeVisual[edge].Start.X; var y3 = dataEdgeVisual[edge].Start.Y;
                    var x4 = dataEdgeVisual[edge].End.X; var y4 = dataEdgeVisual[edge].End.Y;
                    if (CalculatorDistance.IsSegmentIntersectSegment(x1, y1, x2, y2, x3, y3, x4, y4))
                        counter += 1;
                }
            }
            return counter;
        }




        public Bitmap DrawGraph(IPartGraph changeOnlyOnThisStep, Bitmap bmp)
        {

            using (var g = Graphics.FromImage(bmp))
            {
                foreach (var edge in visualizedGraph.Edges)
                    if (changeOnlyOnThisStep == edge)
                        DrawEdge(edge, 3 * _antiStaticVisualData.EdgeWidth, g);
                    else
                        DrawEdge(edge, _antiStaticVisualData.EdgeWidth, g);
                foreach (var node in visualizedGraph.Nodes)
                    if (changeOnlyOnThisStep == node)
                        DrawNode(node, 1.5f * _antiStaticVisualData.NodeRadius, g);
                    else
                        DrawNode(node, _antiStaticVisualData.NodeRadius, g);
            }
            return bmp;
        }

        public void DrawEdge(EdgeVisual<TId, TWeight, TState> edge, float edgeWidth, Graphics g)
        {
            var pen = new Pen(parserColor(edge.State), edgeWidth);
            g.DrawLine(pen, dataEdgeVisual[edge].Start.X, dataEdgeVisual[edge].Start.Y,
                       dataEdgeVisual[edge].End.X, dataEdgeVisual[edge].End.Y);
            var xLetter = (dataEdgeVisual[edge].Start.X + dataEdgeVisual[edge].End.X) / 2;
            var yLetter = (dataEdgeVisual[edge].Start.Y + dataEdgeVisual[edge].End.Y) / 2;

            Font drawFont = new Font("Arial", edgeWidth * 3);
            SolidBrush drawBrush = new SolidBrush(Color.BlueViolet);

            // Рисуем цифру 5 в координатах (10, 10) на изображении
            g.DrawString(edge.Weight.ToString(), drawFont, drawBrush, new PointF(xLetter, yLetter));
        }

        public void DrawNode(NodeVisual<TId, TWeight, TState> node, float size, Graphics g)
        {
            SolidBrush brush = new SolidBrush(parserColor(node.State));  
            brush.Color = parserColor(node.State);
            var pointNode = dataNodeVisual[node];
            g.FillEllipse(brush, pointNode.X - size, pointNode.Y - size, 2 * size, 2 * size);
            Font drawFont = new Font("Arial", _antiStaticVisualData.EdgeWidth * 3);
            SolidBrush drawBrush = new SolidBrush(Color.BlueViolet);

            // Рисуем цифру 5 в координатах (10, 10) на изображении
            g.DrawString(node.Weight.ToString(), drawFont, drawBrush, new PointF(pointNode.X, pointNode.Y));
        }
    }
}