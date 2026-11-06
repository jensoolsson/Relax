using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.DocObjects;

namespace Structures
{
    public class Rectangular : ElementMesh
    {

        public Rectangular() : base()
        {

        }


        //public override void BuildMesh(Bar bar)
        //{
        //    Vector3d vec = bar.GetEnd().Point - bar.GetStart().Point;
        //    CoordSystem[] css = new CoordSystem[2];

        //    css[0] = bar.BaseCS;
        //    css[1] = bar.BaseCS;

        //    BuildMeshFromCS(css, bar);
        //}


        public override void BuildMesh(Bar bar)
        {
            Vector3d vec = bar.GetEnd().Point - bar.GetStart().Point;

            CoordSystem[] css = new CoordSystem[2];

            if (bar.GetFaces().Count > 0)
                css = ConstructMeshNormalCS(bar, vec);
            else
                css = ConstructPerpCS(vec);
            
            BuildMeshFromCS(css, bar);
        }

        public override void BuildMesh(Beam beam)
        {
            Vector3d vec = beam.GetEnd().Point - beam.GetStart().Point;
            CoordSystem[] css = new CoordSystem[2];

            css[0] = beam.CoordSystem1;
            css[1] = beam.CoordSystem2;

            Bar bar = (Bar)beam;

            BuildMeshFromCS(css, bar);
        }

        private void BuildMeshFromCS(CoordSystem[] css, Bar bar) 
        {
            //TODO: Make more generic so it works with an arbitrary nr of sections..
            Point3d pt1, pt2, pt3, pt4;

            _mesh.Vertices.Clear();
            _mesh.Faces.Clear();

            for (int i = 0; i < 2; i++)         //Looping over the sections
            {
                pt1 = new Point3d(bar.GetVertices()[i].Point);
                pt2 = new Point3d(bar.GetVertices()[i].Point);
                pt3 = new Point3d(bar.GetVertices()[i].Point);
                pt4 = new Point3d(bar.GetVertices()[i].Point);

                pt1 += css[i].X * (bar.CrossSection.Width / 2);
                pt1 += css[i].Y * (bar.CrossSection.Height / 2);
                pt2 += css[i].X * -(bar.CrossSection.Width / 2);
                pt2 += css[i].Y * (bar.CrossSection.Height / 2);
                pt3 += css[i].X * -(bar.CrossSection.Width / 2);
                pt3 += css[i].Y * -(bar.CrossSection.Height / 2);
                pt4 += css[i].X * (bar.CrossSection.Width / 2);
                pt4 += css[i].Y * -(bar.CrossSection.Height / 2);

                _mesh.Vertices.Add(pt1);
                _mesh.Vertices.Add(pt2);
                _mesh.Vertices.Add(pt3);
                _mesh.Vertices.Add(pt4);

                if (i == 1)
                {
                    _mesh.Faces.AddFace(0, 1, 5, 4);
                    _mesh.Faces.AddFace(1, 2, 6, 5);
                    _mesh.Faces.AddFace(2, 3, 7, 6);
                    _mesh.Faces.AddFace(3, 0, 4, 7);
                }
            }
        }


        public CoordSystem[] ConstructPerpCS(Vector3d elemZ)
        {
            Vector3d zVec = new Vector3d(0, 0, 1);
            Vector3d elemX = Vector3d.CrossProduct(elemZ, zVec);
            Vector3d elemY = Vector3d.CrossProduct(elemZ, elemX);

            CoordSystem cs1 = new CoordSystem(elemX, elemY, elemZ);
            CoordSystem cs2 = new CoordSystem(elemX, elemY, elemZ);
            CoordSystem[] css = new CoordSystem[2];

            css[0] = cs1;
            css[1] = cs1;

            return css;
        }



        public CoordSystem[] ConstructMeshNormalCS(Bar b, Vector3d elemZ)
        {
            List<Face> faces = b.GetFaces();
            Vector3d normal = new Vector3d(0,0,0);

            foreach (Face f in faces)
                normal += f.Normal;

            normal.Unitize();

            Vector3d elemX = Vector3d.CrossProduct(elemZ, normal);
            Vector3d elemY = Vector3d.CrossProduct(elemZ, elemX);

            CoordSystem cs1 = new CoordSystem(elemX, elemY, elemZ);
            CoordSystem cs2 = new CoordSystem(elemX, elemY, elemZ);
            CoordSystem[] css = new CoordSystem[2];

            css[0] = cs1;
            css[1] = cs1;

            return css;
        }










        public override void ColorMesh(Bar bar, double realMin, double realMax, double showMin, double showMax)
        {
            _color1 = Util.ConvertNumberToColor(bar.ColorBy[0], realMin, realMax, showMin, showMax);
            _color2 = Util.ConvertNumberToColor(bar.ColorBy[1], realMin, realMax, showMin, showMax);

            int vertCount = _mesh.Vertices.Count;

            _mesh.VertexColors.Clear();

            for (int i = 0; i < vertCount; i++)
            {
                if (i > 3)
                    _mesh.VertexColors.Add(_color2);      //vertex ring 2
                else
                    _mesh.VertexColors.Add(_color1); ;      //vertex ring 1     
            }
        }






    }
}
