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
    public class GeomStructure
    {
        protected List<Vertex> _vertices;
        protected List<Edge> _edges;
        protected List<Face> _faces;
        protected double _avrgEdgeLength;
        protected BoundingBox _bb;
        protected Mesh _mesh;
        protected bool _isStiched;
        protected bool _isPreProcessed;
        protected bool _isDarwable;


        public void Initialise() 
        {
            _vertices = new List<Vertex>();
            _edges = new List<Edge>();
            _faces = new List<Face>();
            _bb = new BoundingBox();
            _mesh = new Mesh();
            _isStiched = false;
            _isDarwable = false;
        }


        //--------------------------------- Constructors -----------------------------------//
        #region

        public GeomStructure() 
        {
            Initialise();
        }

        public GeomStructure(List<Vertex> vetrices, List<Edge> edges)
        {
            Initialise();
            StichExternalObjects(_vertices, _edges);
        }

        public GeomStructure(List<Vertex> vetrices, List<Edge> edges, List<Face> faces) 
        {
            Initialise();
            StichExternalObjects(_vertices, _edges, _faces);
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //-------------------------- Properties and add methods ----------------------------//
        #region

        public void AddVertex(Vertex v) 
        {
            _vertices.Add(v);
        }

        public void AddVertices(List<Vertex> vs)
        {
            _vertices.AddRange(vs);
        }

        public void AddEdge(Edge e)
        {
            _edges.Add(e);
        }

        public void AddEdges(List<Edge> es)
        {
            _edges.AddRange(es);
        }

        public void AddFace(Face f)
        {
            _faces.Add(f);
        }

        public void AddFacees(List<Face> fs)
        {
            _faces.AddRange(fs);
        }

        public List<Vertex> GetVertices()
        {
            return _vertices;
        }

        public List<Edge> GetEdges() 
        {
            return _edges;
        }

        public List<Face> GetFaces() 
        {
            return _faces;
        }

        public double AvrgEdgeLength 
        { 
            get{ return _avrgEdgeLength; }
            set { _avrgEdgeLength = value; } 
        }

        public BoundingBox GetBoundingBox() 
        {
            return _bb;
        }

        public Mesh GetMesh() 
        {
            return _mesh;
        }

        public bool IsStiched
        {
            get { return _isStiched; }
            set { _isStiched = value; }
        }

        public bool IsDrawable
        {
            get { return _isDarwable; }
            set { _isDarwable = value; }
        }

        public bool IsPreProcessed
        {
            get { return _isPreProcessed; }
            set { _isPreProcessed = value; }
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //------------------------------ Creating objects ----------------------------------//
        #region

        /* Vertices and edges from lines */
        public virtual void CreateObjectsFromLines(List<RhinoObject> selLine)
        {
            List<Vertex> vertices = new List<Vertex>();
            List<Edge> edges = new List<Edge>();
            int nodeCounter = 0;

            for (int i = 0; i < selLine.Count; i++)
            {
                int index = -1;
                Curve crv = selLine[i].Geometry as Curve;

                Point3d ptS = crv.PointAtStart;
                Point3d ptE = crv.PointAtEnd;
                Vertex vs, ve;

                //Check if there is a vertex at the start point
                if (!Util.ContainsVertex(vertices, ptS, out index))
                {
                    vs = new Vertex(ptS);
                    vs.ID = nodeCounter;
                    nodeCounter++;
                    vertices.Add(vs);
                }
                else
                    vs = vertices[index];

                //Check if there is a vertex at the edn point
                if (!Util.ContainsVertex(vertices, ptE, out index))
                {
                    ve = new Vertex(ptE);
                    ve.ID = nodeCounter;
                    nodeCounter++;
                    vertices.Add(ve);
                }
                else
                    ve = vertices[index];

                Edge e = new Edge(vs, ve);
                e.ID = i;

                vs.AddEdge(e);
                ve.AddEdge(e);

                edges.Add(e);
            }

            _vertices.AddRange(vertices);
            _edges.AddRange(edges);
        }

        /* Preprocess/sorting vertices and edges */
        public void PreprocessGeometry1()
        {
            CalcEdgeLength();

            CalcEdgeInitialLength();

            CalcAvrgEdgeLength();

            CalcEdgeMidPoint();

            SortNodes();
        }

        /* Creating faces, edges must be sorted first */
        public virtual bool CreateFacesFromEdges()
        {
            if (_vertices.Count < 1) { return false; }

            Edge e1, e2;
            Vertex v1, v2;
            Vertex commonVertex;
            List<Edge> fEdges = new List<Edge>(4);
            List<Vertex> fVertices = new List<Vertex>(4);
            List<Edge> missingEdges = new List<Edge>();
            int faceCounter = 0;
            double angle;

            foreach (Vertex v in _vertices)
            {
                List<Edge> edges = v.GetEdges();
                List<double> edgeAngles = v.EdgeAngles;

                for (int i = 0; i < edges.Count; i++)
                {
                    if (v.GetEdges().Count < 3) break;

                    //Check angle, if greater than 180, no face should be created.
                    if (i == 0)
                        angle = edgeAngles[0];
                    else if (i == edges.Count - 1)
                        angle = 360 - edgeAngles[i - 1];
                    else
                        angle = Math.Abs(edgeAngles[i] - edgeAngles[i - 1]);
                    if (angle > 180) break;

                    if (i == (edges.Count - 1))
                    {
                        e1 = edges[i];
                        e2 = edges[0];
                    }
                    else
                    {
                        e1 = edges[i];
                        e2 = edges[i + 1];
                    }

                    v1 = Util.GetOtherEnd(e1, v);
                    v2 = Util.GetOtherEnd(e2, v);

                    fEdges.Clear();
                    fVertices.Clear();
                    missingEdges.Clear();

                    fVertices.Add(v);
                    fVertices.Add(v1);
                    fEdges.Add(e1);

                    commonVertex = new Vertex(-1);

                    if (FindMissingEdge(v, v1, v2, missingEdges, ref commonVertex))
                    {
                        if (commonVertex.ID != -1 && missingEdges.Count == 2)
                        {   //Quad
                            fVertices.Add(commonVertex);
                            fVertices.Add(v2);
                            fEdges.Add(missingEdges[0]);
                            fEdges.Add(missingEdges[1]);
                            fEdges.Add(e2);
                        }
                        else
                        {   //Triangle 
                            fVertices.Add(v2);
                            fEdges.Add(missingEdges[0]);
                            fEdges.Add(e2);
                        }

                        if (!Util.ContainsFace(_faces, fVertices))
                        {
                            //Create face object, assign all topology, and add to list of faces.
                            _faces.Add(CreateFace(fVertices, fEdges, faceCounter));
                            faceCounter++;
                        }
                    }
                }
            }
            return true;
        }

        /* Preprocess faces-dependent geometry (normals etc) */
        public void PreprocessGeometry2()
        {
            CalculateFaceMidPoints();

            CalculateFaceNormals();

            CompleteFaceTopology();

            UnifyNormals();

            CalculateVertexNormals();

            UnifyNodesSorting();

            BuildMesh();

            _isPreProcessed = true;
        }

        private Face CreateFace(List<Vertex> fVertices, List<Edge> fEdges, int id)
        {
            Face face = new Face(id);
            face.AddVertices(fVertices);
            face.AddEdges(fEdges);

            foreach (Edge e in fEdges)
                e.AddFace(face);

            foreach (Vertex v in fVertices)
                v.AddFace(face);

            return face;
        }

        protected bool FindMissingEdge(Vertex v, Vertex v1, Vertex v2, List<Edge> missingEdges, ref Vertex commonVertex) 
        {
            List<Edge> neigEdg = new List<Edge>();
            neigEdg.AddRange(v1.GetEdges());
            neigEdg.AddRange(v2.GetEdges());

            //Check if there is one edge that joins the vertices, i.e triangular face.
            foreach (Edge e in neigEdg)
            {   
                if (e.GetStart().ID == v1.ID && e.GetEnd().ID == v2.ID || e.GetEnd().ID == v1.ID && e.GetStart().ID == v2.ID)
                {
                    missingEdges.Add(e);
                    return true;
                }
            }

            //Check if there are 2 edge that connect to v1 and v2 and have one vertex incommon. I.e quad face.
            foreach (Edge e1 in v1.GetEdges()) 
            {
                Vertex v11 = Util.GetOtherEnd(e1, v1);
                foreach (Edge e2 in v2.GetEdges()) 
                {
                    Vertex v22 = Util.GetOtherEnd(e2, v2);
                    if (v11.ID!= v.ID && v22.ID != v.ID && v11.ID == v22.ID && v11.ID != v1.ID) 
                    {
                        missingEdges.Add(e1);
                        missingEdges.Add(e2);
                        commonVertex = v11;
                        return true;
                    }
                }
            }
            return false;
        }

        private void CompleteFaceTopology() 
        {
            foreach (Edge e in _edges) 
                e.CompleteFaceTopology();
        }

        public void CreateObjectsFromMesh(List<RhinoObject> _selMesh)
        {
            //Yet to be implemented
        }

        //----------------------------- Update/Postprocess ---------------------------------//
        #region

        /* Called to update geometry when changed due to for example analysis. */
        public virtual void UpdateGeometry(Parameters p)
        {
            CalculateFaceNormals();

            CalculateFaceMidPoints();

            CalculateVertexNormals();

            CalcEdgeMidPoint();
        }

        #endregion
        //----------------------------------------------------------------------------------//

        #endregion
        //----------------------------------------------------------------------------------//


        //-------------------- Creating objects from external objects ----------------------//
        #region

        /** If Objects (Edges, Vertices and Faces) are created outside of the dll these have
        * to be topologically stiched together, so that each edge knows about its vertices
        * etc. 
        */
        public void StichExternalObjects(List<Vertex> vs, List<Edge> es)
        {
            StichVerticesEdges(ref vs, ref es);
            
            _vertices = vs;
            _edges = es;

            PreprocessGeometry1();
            CreateFacesFromEdges();
            PreprocessGeometry2();
        }
        
        public void StichExternalObjects(List<Vertex> vs, List<Edge> es, List<Face> fs)
        {
            StichVerticesEdgesFaces(vs, es, fs);

            _vertices = vs;
            _edges = es;
            _faces = fs;

            PreprocessGeometry1();
            PreprocessGeometry2();
        }

        private void StichVerticesEdgesFaces(List<Vertex> vs, List<Edge> es, List<Face> fs)
        {
            StichVerticesEdges(ref vs, ref es);

            int index = -1;

            List<Edge> fEs;

            foreach (Face f in fs)
            {
                fEs = new List<Edge>();

                foreach (Edge e in f.GetEdges())
                    if (Util.ContainsEdge(es, e, out index))
                        fEs.Add(es[index]);

                if (fEs.Count > 0)
                {
                    f.ClearEdges();
                    f.AddEdges(fEs);

                    //Add the edge vertices to the faces, but avoid duplicates.
                    f.ClearVertices();
                    foreach (Edge e in f.GetEdges())
                    {
                        for (int i = 0; i < 2; i++)
                            if (!Util.ContainsVertex(f.GetVertices(), e.GetVertices()[i]))
                                f.GetVertices().Add(e.GetVertices()[i]);
                    }
                }
            }

            _isStiched = true;
        }

        private void StichVerticesEdges(ref List<Vertex> vs, ref List<Edge> es) 
        {
            //Make sure no duplicated vertices exist.
            vs = RemoveDuplicatedVerticesWithPriority(ref vs);

            List<Edge> newEdges = new List<Edge>();

            int index = -1;
            int id = 0;

            //Aviod duplicated edges and fix edge - vertex topology
            foreach (Edge e in es)
            {
                if (Util.ContainsVertex(vs, e.GetStart().Point, out index))
                {
                    e.SetStart(vs[index]);
                    vs[index].AddEdge(e);
                }
                if (Util.ContainsVertex(vs, e.GetEnd().Point, out index))
                {
                    e.SetEnd(vs[index]);
                    vs[index].AddEdge(e);
                }

                if (!Util.ContainsEdge(newEdges, e))
                {
                    e.ID = id;
                    id++;
                    newEdges.Add(e);
                }
            }

            es = newEdges;
            _isStiched = true;
        }
        
        //Removes duplicated vertices but makes sure that Node6Dofs are prioritised.
        private List<Vertex> RemoveDuplicatedVerticesWithPriority(ref List<Vertex> vertices) 
        {
            List<Vertex> newVertices = new List<Vertex>();
            int index = 0;
            int exsitingIndex;

            foreach (Vertex v in vertices)
            {
                exsitingIndex = -1;

                if (!Util.ContainsVertex(newVertices, v.Point, out exsitingIndex))
                {
                    v.ID = index;
                    newVertices.Add(v);
                    index++;
                }
                else
                {
                    //If the new node is of type Node6dof, and the existing node at same place is not. Replace.
                    if (v.GetType() == typeof(Node6Dof) && newVertices[exsitingIndex].GetType() != typeof(Node6Dof))
                    {
                        int id = newVertices[exsitingIndex].ID;
                        v.ID = id;
                        newVertices.RemoveAt(exsitingIndex);
                        newVertices.Insert(exsitingIndex, v);
                    }
                }
            }
            return newVertices;
        }


        #endregion
        //----------------------------------------------------------------------------------//


        //------------------------------- Process geometry ---------------------------------//
        #region

        /** Calculates the counter clockwise Oring for each node. Has to be run before faces  
        *   are created.
        */
        private void SortNodes()
        {
            foreach (Vertex v in _vertices)
                v.SortEdges();
        }

        private void CalcAvrgEdgeLength()
        {
            _avrgEdgeLength = 0;
            foreach (Edge e in _edges)
                _avrgEdgeLength += e.Length / _edges.Count;
        }

        private void CalcEdgeLength()
        {
            foreach (Edge e in _edges)
                e.CalcLength();
        }

        private void CalcEdgeInitialLength() 
        {
            foreach (Edge e in _edges)
                e.CalcInitialLength();
        }

        private void CalcEdgeMidPoint() 
        {
            foreach (Edge e in _edges) 
            {
                e.CalcMidPoint();
            }
        }

        private void CalculateFaceMidPoints()
        {
            foreach (Face f in _faces)
                f.CalcMidPoints();
        }

        private void CalculateFaceNormals()
        {
            foreach (Face f in _faces)
                f.CalcNormal();
        }

        /** Calculate vertex normals, relying on the face normals to be calculated!
        *
        */
        private void CalculateVertexNormals()
        {
            foreach (Vertex v in _vertices)
                v.CalcNormal();
        }


        /** Unifying the direction of face normals
        *
        */
        private void UnifyNormals() 
        {
            if (_faces.Count < 1) return;

            int unifyCounter = 0;
            int iterCounter = 0;
            int index = 0;
            int maxIter = 1000;

            while (unifyCounter < _faces.Count && FindNotUnifiedFace(ref index) && iterCounter < maxIter)
            {
                _faces[index].IsNormalUnified = true;
                UnifyAdjacentNormals(_faces[index], ref unifyCounter);
                iterCounter++;
            }
        }

        private void UnifyAdjacentNormals(Face inFace, ref int unifyCounter)
        {
            foreach (Face f in inFace.GetFaces()) 
            {
                if (!f.IsNormalUnified)
                {

                    if (!Util.IsSameDirection(inFace.Normal, f.Normal))
                        f.ReverseNormal();

                    f.IsNormalUnified = true;
                    unifyCounter++;
                    UnifyAdjacentNormals(f, ref unifyCounter);
                }
            }
        }

        private bool FindNotUnifiedFace(ref int index)
        {
            foreach (Face face in _faces)
            {
                if (!face.IsNormalUnified)
                {
                    index = _faces.IndexOf(face);
                    return true;
                }
            }
            return false;
        }

        private void UnifyNodesSorting() 
        {
            foreach (Vertex v in _vertices)
                v.UnifySorting();
        }

        /** Calculate geometry bounding box
        *
        */
        public void CalcBoundingBox() 
        {
            List<double> min = new List<double>(3) { 10000000000, 10000000000, 10000000000 };
            List<double> max = new List<double>(3) { -10000000000, -10000000000, -10000000000 };

            foreach (Vertex v in _vertices) 
            {
                for(int i = 0; i < 3; i++)
                {
                    if (v.GetPosition()[i] < min[i])
                        min[i] = v.GetPosition()[i];

                    if (v.GetPosition()[i] < max[i])
                        max[i] = v.GetPosition()[i];
                }
            }

            _bb = new BoundingBox(new Point3d(min[0], min[1], min[2]), new Point3d(max[0], max[1], max[2])); 
        }


        #endregion
        //----------------------------------------------------------------------------------//


        //----------------------- Build geometry for Conduit/Baking ------------------------//
        #region

        /** Builds a mesh usfull either for visulaisation or for "baking".
        *   
        */
        public void BuildMesh()
        {
            if (_faces.Count < 1 || _vertices.Count < 1) { return; }

            int nrFaces = _faces.Count;
            int nrVertices = _vertices.Count;
            List<int> vertexIds;
            int nrVerticesFace;
            Mesh mesh = new Mesh();

            for (int j = 0; j < _vertices.Count; j++)
            {
                mesh.Vertices.Add(_vertices[j].Point);
            }

            //Loops over the faces and adds the faces to the mesh. 
            foreach (Face face in _faces)
            {
                nrVerticesFace = face.GetVertices().Count;
                vertexIds = face.GetVertexIds();

                if (nrVerticesFace == 4)
                {
                    mesh.Faces.AddFace(vertexIds[0], vertexIds[1], vertexIds[2], vertexIds[3]);
                }
                else if (nrVerticesFace == 3)
                {
                    mesh.Faces.AddFace(vertexIds[0], vertexIds[1], vertexIds[2]);
                }
            }

            mesh.Compact();
            //mesh.UnifyNormals();

            _mesh = mesh;
        }

        #endregion
        //----------------------------------------------------------------------------------//


    }
}
