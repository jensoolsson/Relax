using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.DocObjects;

namespace Structures
{
    public class Spline
    {
        protected Polyline _pLine;
        protected List<Edge> _edges;
        protected List<Vertex> _vertices;
        protected eSplineType _type;

        public void Initialise() 
        {
            _vertices = new List<Vertex>();
            _edges = new List<Edge>();
        }

        public Spline(Polyline pLine, eSplineType type)
        {
            Initialise();
            _pLine = pLine;
            _type = type;
        }

        public List<Edge> GetEdges() 
        {
            return _edges;
        }

        public List<Vertex> GetVertices()
        {
            return _vertices;
        }
       
        /* Bend-Splines */
        public void CreateObjects()
        {
            _vertices.Clear();
            _edges.Clear();

            List<Node3Dof> nodes = new List<Node3Dof>();
            List<SplineB> sBars = new List<SplineB>();
            Line line0, line1, line2;
            SplineB sb0, sb1, sb2;

            List<Line> lines = SplitPolylineToLines();

            for (int i = 0; i < lines.Count; i++)
            {
                if (i == 0 && lines.Count > 1)
                {
                    line1 = lines[i];
                    line2 = lines[i + 1];

                    sb1 = CreateOrGetObject(line1, nodes, sBars);
                    sb2 = CreateOrGetObject(line2, nodes, sBars);

                    sb1.NextSplineBar = sb2;
                    sb1.NextSplineBarIsSet = true;
                }
                else if (i < (lines.Count - 1))
                {
                    line0 = lines[i - 1];
                    line1 = lines[i];
                    line2 = lines[i + 1];

                    sb0 = CreateOrGetObject(line0, nodes, sBars);
                    sb1 = CreateOrGetObject(line1, nodes, sBars);
                    sb2 = CreateOrGetObject(line2, nodes, sBars);

                    sb1.PreviousSplineBar = sb0;
                    sb1.NextSplineBar = sb2;
                    sb1.PreviousSplineBarIsSet = true;
                    sb1.NextSplineBarIsSet = true;
                }
                else if (i == (lines.Count - 1))
                {
                    line0 = lines[i - 1];
                    line1 = lines[i];

                    sb0 = CreateOrGetObject(line0, nodes, sBars);
                    sb1 = CreateOrGetObject(line1, nodes, sBars);

                    sb1.PreviousSplineBar = sb0;
                    sb1.PreviousSplineBarIsSet = true;
                }
            }

            _vertices.AddRange(Util.CastToSuperclass<Vertex, Node3Dof>(nodes));
            _edges.AddRange(Util.CastToSuperclass<Edge, SplineB>(sBars));
        }

        public SplineB CreateOrGetObject(Line line, List<Node3Dof> nodes, List<SplineB> sBars)
        {
            Point3d ptS = line.From;
            Point3d ptE = line.To;
            Node3Dof ns, ne;
            SplineB sb = new SplineB();
            int index = -1;

            if (!Util.ContainsVertex(Util.CastToSuperclass<Vertex, Node3Dof>(nodes), ptS, out index))
            {
                ns = new Node3Dof(ptS);
                nodes.Add(ns);
            }
            else
                ns = nodes[index];

            if (!Util.ContainsVertex(Util.CastToSuperclass<Vertex, Node3Dof>(nodes), ptE, out index))
            {
                ne = new Node3Dof(ptE);
                nodes.Add(ne);
            }
            else
                ne = nodes[index];

            index = -1;

            if (!Util.ContainsEdge(Util.CastToSuperclass<Edge, SplineB>(sBars), (Vertex)ns, (Vertex)ne, out index))
            {
                if (_type == eSplineType.BendingSpline)
                    sb = new SplineB(ns, ne);
                
                else if (_type == eSplineType.BendingTorsionSpline)
                    sb = (SplineB)new SplineBT(ns, ne);

                ns.AddEdge(sb);
                ne.AddEdge(sb);
                sBars.Add(sb);
            }
            else
                sb = sBars[index];

            return sb;
        }


        public List<Line> SplitPolylineToLines()
        {
            List<Line> lines = new List<Line>();
            if (_pLine != null)
                for (int i = 0; i < _pLine.Count - 1; i++)
                    lines.Add(new Line(_pLine[i], _pLine[i + 1]));

            return lines;
        }
        
    }
}
