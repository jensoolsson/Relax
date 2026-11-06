using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.DocObjects;

namespace Structures
{
    public static class Util
    {
        static double _tol = 0.00001;

        static public double Tol
        {
            get { return _tol; }
        }


        /** Casting list of objects from subclass to superclass
        * 
        */
        public static List<TO> CastToSuperclass<TO, FROM>(List<FROM> subObjs) where FROM : TO
        {
            List<TO> superObjs = new List<TO>(subObjs.Count);
            TO obj;

            for (int i = 0; i < subObjs.Count; i++)
            {
                obj = (TO)subObjs[i];
                if (obj != null)
                    superObjs.Add(obj);
            }

            superObjs.TrimExcess();

            return superObjs;
        }

        /** Casting list of objects from superclass to subclass
        * 
        */
        public static List<TO> CastToSubclass<TO, FROM>(List<FROM> superObjs) where TO : FROM
        {
            List<TO> subObjs = new List<TO>(superObjs.Count);
            TO obj;

            for (int i = 0; i < superObjs.Count; i++)
            {
                try
                {
                    obj = (TO)superObjs[i];
                    if (obj != null)
                        subObjs.Add(obj);
                }
                catch (Exception e) 
                {
                
                }
            }

            subObjs.TrimExcess();

            return subObjs;
        }


        //------------------------------- Vector algebra -----------------------------------//
        #region


        /** In case to vectros have the same direction, returns true. If more similar with one flipped, returns false. 
        *  
        */
        static public bool IsSameDirection(Vector3d dominantVec, Vector3d vec)
        {
            //Reversed vector
            Vector3d revVec = new Vector3d(vec);
            revVec.Reverse();

            double angle1 = Vector3d.VectorAngle(dominantVec, vec);
            double angle2 = Vector3d.VectorAngle(dominantVec, revVec);

            return (angle1 < angle2);
        }

        /** Check if a vector is zero, given a tolerance. 
        * 
        */
        static public bool IsVectorTiny(Vector3d vec, double t)
        {
            if ((Math.Abs(vec.X - 0) + Math.Abs(vec.Y - 0) + Math.Abs(vec.Z - 0) < t))
                return true;

            return false;
        }

        static public Vector3d MyOnCrossProduct(Vector3d vec1, Vector3d vec2)
        {
            Vector3d vec3 = new Vector3d(); ;
            
            //Cross product to create the normal vector
            vec3.X = (vec1.Y * vec2.Z) - (vec1.Z * vec2.Y);
            vec3.Y = (vec1.Z * vec2.X) - (vec1.X * vec2.Z);
            vec3.Z = (vec1.X * vec2.Y) - (vec1.Y * vec2.X);

            return vec3;
        }


        /** calculates the dotproduct of two vectors 
        *
        */
        static public double DotProduct(double[] vec1, double[] vec2)
        {
            double product = 0;
            for (int i = 0; i < vec1.Length; i++)
            {
                product += vec1[i] * vec2[i];
            }

            return product;
        }

        static public double MyDotProd(double[] vector1, double[] vector2)
        {
            double mydotprod = vector1[0] * vector2[0] + vector1[1] * vector2[1] + vector1[2] * vector2[2];
            return mydotprod;
        }

        static public double MyDotProd(Vector3d vector1, Vector3d vector2)
        {
            double mydotprod = vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z;
            return mydotprod;
        }

        static public double DotProduct(Vector3d vec1, Vector3d vec2)
        {
            return vec1.X * vec2.X + vec1.Y * vec2.Y + vec1.Z * vec2.Z;
        }

        static public double VectorLength(double[] vec)
        {
            double length = 0;

            for (int i = 0; i < vec.Length; i++)
            {
                length += Math.Pow(vec[i], 2);
            }
            length = Math.Sqrt(length);

            return length;
        }

        static public double[] NormVector(double[] vec)
        {
            double length;
            double[] normVec = new double[3];

            length = Math.Sqrt(Math.Pow(vec[0], 2) + Math.Pow(vec[1], 2) + Math.Pow(vec[2], 2));

            normVec[0] = vec[0] / length;
            normVec[1] = vec[1] / length;
            normVec[2] = vec[2] / length;

            return normVec;
        }


        #endregion
        //----------------------------------------------------------------------------------//


        //------------------------------- Matrix algebra -----------------------------------//
        #region

        /** Insert matrix2 into matrix1 at position specified by m1, m2 and n1, n2.
        *       
        */
        public static void InsertMatrix(Matrix matrix1, Matrix matrix2, int m1, int m2, int n1, int n2)
        {
            int m = m2 - m1;
            int n = n2 - n1;
            for (int i = 0; i <= m; i++)
            {
                for (int j = 0; j <= n; j++)
                    matrix1[m1 + i, n1 + j] = matrix2[i, j];
            }
        }

        /** Matrix multiplication
        *       
        */
        public static Matrix MatrixMultlipication(Matrix mat1, Matrix mat2)
        {
            Matrix resMat = new Matrix(mat1.RowCount, mat2.ColumnCount);
            resMat.Zero();
            double sum;

            for (int i = 0; i < mat1.RowCount; i++)
            {
                for (int j = 0; j < mat1.RowCount; j++)
                {
                    sum = 0;
                    for (int k = 0; k < mat1.ColumnCount; k++)
                    {
                        sum += mat1[i, k] * mat2[k, j];
                    }
                    resMat[i, j] = sum;
                }
            }
            return resMat;
        }


        #endregion
        //----------------------------------------------------------------------------------//


        //--------------------------------- Utilities --------------------------------------//
        #region

        /** Sort valus in a list in descending order.
        *
        */
        public static void Sort(ref List<double> values)
        {
            double temp;
            int nrSwapps = 1;

            while (nrSwapps > 0)
            {
                nrSwapps = 0;
                for (int i = 0; i < values.Count - 1; i++)
                {
                    if (values[i + 1] > values[i])
                    {
                        temp = values[i];
                        values[i] = values[i + 1];
                        values[i + 1] = temp;
                        nrSwapps++;
                    }
                }
            }
        }

        public static double SmallestValue(double val1, double val2)
        {
            if (val1 < val2)
                return val1;
            else
                return val2;
        }

        public static double LargestValue(double val1, double val2)
        {
            if (val1 > val2)
                return val1;
            else
                return val2;
        }

        public static double LargestAbsValue(double val1, double val2)
        {
            if (Math.Abs(val1) > Math.Abs(val2))
                return val1;
            else
                return val2;
        }

        static public double Distance(double[] x1, double[] x2)
        {
            return Math.Sqrt(Math.Pow((x1[0] - x2[0]), 2) + Math.Pow((x1[1] - x2[1]), 2) + Math.Pow((x1[2] - x2[2]), 2));
        }

        static public bool ContainsInteger(List<int> ints, int n)
        {
            foreach (int i in ints) 
                if (i == n)
                    return true;

            return false;
        }

        static public bool ContainsConstPt(List<ConstraintPoint> cps, Point3d pt)
        {
            foreach (ConstraintPoint cp in cps)
                if (cp.Point.DistanceTo(pt) < _tol)
                    return true;

            return false;
        }

        static public bool ContainsVertex(List<Vertex> vertices, Vertex vv)
        {
            foreach(Vertex v in vertices)
                if (v.Point.DistanceTo(vv.Point) < _tol)
                    return true;

            return false;
        }

        static public bool ContainsVertex(List<Vertex> vertices, Point3d pt, out int index)
        {
            index = -1;
            for (int i = 0; i < vertices.Count; i++)
            {
                if (vertices[i].Point.DistanceTo(pt) < _tol)
                {
                    index = i;
                    return true;
                }
            }
            return false;
        }

        static public bool ContainsEdge(List<Edge> edges, Edge e)
        {
            for (int i = 0; i < edges.Count; i++)
            {
                bool ss = edges[i].GetStart().Point.DistanceTo(e.GetStart().Point) < _tol;
                bool ee = edges[i].GetEnd().Point.DistanceTo(e.GetEnd().Point) < _tol;
                bool se = edges[i].GetStart().Point.DistanceTo(e.GetEnd().Point) < _tol;
                bool es = edges[i].GetEnd().Point.DistanceTo(e.GetStart().Point) < _tol;

                if (ss && ee || se && es)
                {
                    return true;
                }
            }
            return false;
        }

        static public bool ContainsEdge(List<Edge> edges, Edge e, out int index) 
        {
            index = -1;
            for (int i = 0; i < edges.Count; i++)
            {
                bool ss = edges[i].GetStart().Point.DistanceTo(e.GetStart().Point) < _tol;
                bool ee = edges[i].GetEnd().Point.DistanceTo(e.GetEnd().Point) < _tol;
                bool se = edges[i].GetStart().Point.DistanceTo(e.GetEnd().Point) < _tol;
                bool es = edges[i].GetEnd().Point.DistanceTo(e.GetStart().Point) < _tol;

                if (ss && ee || se && es)
                {
                    index = i;
                    return true;
                }
            }
            return false;   
        }

        static public bool ContainsEdge(List<Edge> edges, Vertex vS, Vertex vE, out int index)
        {
            index = -1;
            for (int i = 0; i < edges.Count; i++)
            {
                bool ss = edges[i].GetStart().Point.DistanceTo(vS.Point) < _tol;
                bool ee = edges[i].GetEnd().Point.DistanceTo(vE.Point) < _tol;
                bool se = edges[i].GetStart().Point.DistanceTo(vE.Point) < _tol;
                bool es = edges[i].GetEnd().Point.DistanceTo(vS.Point) < _tol;

                if (ss && ee || se && es)
                {
                    index = i;
                    return true;
                }
            }
            return false;
        }


        //static public bool ContainsSpline(List<Spline> splines, List<Edge> newSplineEdges) 
        //{
        //    List<bool> found = new List<bool>();

        //    foreach (Spline s in splines) 
        //    {
        //        List<Edge> se = s.GetEdges();

        //        foreach (Edge nse in newSplineEdges) 
        //            found.Add(Util.ContainsEdge(se, nse));

        //        if (found.Contains(false))
        //            return false;
        //        else
        //            return true;
        //    }
        //    return false;
        //}

        static public bool ContainsFace(List<Face> faces, List<Vertex> vertices)
        {
            int sumIdF = 0;
            int sumIdV = 0;
            List<int> fIds = new List<int>();
            List<int> vIds = new List<int>();

            foreach (Vertex v in vertices)
            {
                vIds.Add(v.ID);
                sumIdV += v.ID;
            }

            foreach (Face f in faces)
            {
                sumIdF = 0;
                fIds = new List<int>();

                //Check that both are triangle or quad.
                if (f.GetVertices().Count == vertices.Count)
                {
                    foreach (Vertex v in f.GetVertices())
                    {
                        fIds.Add(v.ID);
                        sumIdF += v.ID;

                        //Check that sum of ids match
                        if (sumIdF == sumIdV)
                        {
                            int isSameCounter = 0;

                            //Check how many of the ids that match.
                            foreach (int index in fIds)
                                if (vIds.Contains(index))
                                    isSameCounter++;

                            //All ids match, then return true.
                            if (isSameCounter == vertices.Count)
                                return true;
                        }
                    }
                }
            }

            return false;
        }

        public static double RadToDeg(double rad)
        {
            return (180 * rad / Math.PI);
        }

        public static double DegToRad(double deg)
        {
            return (deg * Math.PI / 180);
        }

        #endregion
        //----------------------------------------------------------------------------------//


        //---------------------------- Object specific stuff -------------------------------//
        #region


        public static int GetEdgePairWithSmallestAngles(List<Edge> edges, List<double> angles) 
        {
            double angle;
            double minAngle = 1000000;
            int index = -1;

            for (int i = 0; i < angles.Count; i++)
            {
                angle = angles[i];
                if (angle < minAngle)
                {
                    minAngle = angle;
                    index = i;
                }
            }

            return index;
        }

        /** Get the end of the element that is not coiciding with v. 
        *
        */
        static public Vertex GetOtherEnd(Edge e, Vertex v)
        {
            if (e.GetEnd().ID == v.ID)
                return e.GetStart();
            else
                return e.GetEnd();
        }

        public static bool CalcPlaneFromPoints(Vertex centerVt, List<Vertex> vts, ref Point3d centerPtProj, List<Point3d> projPts, ref Vector3d planeNormal) 
        {
            Plane plane = new Plane();
            List<Point3d> planePts = new List<Point3d>();
            planePts.Add(new Point3d(centerVt.Point));

            foreach (Vertex v in vts)
                planePts.Add(new Point3d(v.Point));

            if (Plane.FitPlaneToPoints(planePts, out plane) == PlaneFitResult.Success)
            {
                foreach (Vertex v in vts)
                    projPts.Add(plane.ClosestPoint(new Point3d(v.Point)));

                planeNormal = plane.Normal;
                centerPtProj = plane.ClosestPoint(new Point3d(centerVt.Point));
            }
            else
                return false;

            return true;
        }

        public static void ProjectPointsToPlane(List<Point3d> ptsIn, List<Point3d> ptsOut, Plane plane) 
        {
            foreach (Point3d p in ptsIn) 
            {
                ptsOut.Add(plane.ClosestPoint(p));
            } 
        }

        public static double CalcAntiClockwiseAngles(Vector3d vec1, Vector3d vec2, Vector3d normal) 
        {
            Vector3d crossProd = Vector3d.CrossProduct(vec1, vec2);
            double angle = Util.RadToDeg(Vector3d.VectorAngle(vec1,vec2));

            if(Util.DotProduct(crossProd, normal) > 0)
                return angle;
            else
                return 360 - angle;
        }

        public static bool CalcAnglesInPlane(Point3d cp, List<Point3d> pts, List<double> angles, Vector3d vNormal) 
        {
            Vector3d vecRef, vec, cpVec;
            vecRef = pts[0] - cp;

            for(int i = 1; i < pts.Count; i++)
            {
                vec = pts[i] - cp;
                cpVec = Vector3d.CrossProduct(vecRef, vec);

                if (Util.IsVectorTiny(cpVec, _tol))
                    angles.Add(180);
                else if (Util.IsSameDirection(vNormal, cpVec))
                    angles.Add(Util.RadToDeg(Vector3d.VectorAngle(vecRef, vec)));
                else
                    angles.Add(360 - Util.RadToDeg(Vector3d.VectorAngle(vecRef, vec))); 
            }
            return true;
        }

        public static bool SortEdgesByAngle(List<Edge> edges, List<double> angles) 
        {
            Edge e;
            double a;
            int swapCounter = 1;

            while (swapCounter > 0)
            {
                swapCounter = 0;
                for (int i = 0; i < angles.Count - 1; i++)
                {
                    if (angles[i + 1] < angles[i])
                    {
                        a = angles[i + 1];
                        angles[i + 1] = angles[i];
                        angles[i] = a;

                        e = edges[i + 2];
                        edges[i + 2] = edges[i + 1];
                        edges[i + 1] = e;

                        swapCounter++;
                    }
                }
            }
            return true;
        }

        /** Convert a number in a interval to a color between red, yelow and green
        *    
        */
        static public Color ConvertNumberToColor(double number, double realMin, double realMax, double showMin, double showMax) 
        {
            Color c = Color.FromArgb(125, 125, 125);
            double percentage;

            //HACK!!!!
            if (showMin == 0 && showMax == 100 && number >= 0)
            {
                percentage = number * 100;
            }
            else
            {
                double normNumb = NormaliseValue(number, realMin, realMax, 0, 100);
                percentage = normNumb;
            }

            c = GetBlendedColor(percentage);

            return c;
        }

        static private Color GetBlendedColor(double percentage)
        {
            double frac;

            if (percentage >= 0 && percentage <= 25)
            {
                //Blue fading to Cyan [0,x,255], where x is increasing from 0 to 255
                frac = percentage/ 25;
                return Color.FromArgb(0, (int)(frac * 255), 255);
            }
            else if(percentage <= 50)
            {
                //Cyan fading to Green [0,255,x], where x is decreasing from 255 to 0
                frac = 1 - Math.Abs(percentage - 25) / 25;
                return Color.FromArgb(0, 255, (int)(frac * 255));
            }
            else if(percentage <= 75)
            {
                //Green fading to Yellow [x,255,0], where x is increasing from 0 to 255
                frac = Math.Abs(percentage - 50) / 25;
                return Color.FromArgb((int)(frac * 255), 255, 0);
            }
            else if(percentage <= 100) 
            {
                //Yellow fading to red [255,x,0], where x is decreasing from 255 to 0
                frac = 1 - Math.Abs(percentage - 75) / 25;
                return Color.FromArgb(255, (int)(frac * 255), 0);
            }
            else if (percentage > 100)
            {
                return Color.FromArgb(255, 0, 0);
            }

            return Color.FromArgb(125,125,125);
        }

        static public bool ColorIsValid(double r, double g, double b)
        {
            if ((r > 255 || g > 255 || b > 255) || (r < 0 || g < 0 || b < 0))
                return false;

            return true;
        }

        static public double NormaliseValue(double value, double prevMin, double prevMax, double newMin, double newMax)
        {
            double oldRange = prevMax - prevMin;
            double newRange = newMax - newMin;
            double frac = (value - prevMin) / (oldRange);
            double newValue = newMin + frac * newRange;

            //To avoid round of errors.
            if(newValue > newMax)
                newValue = newMax;
            else if(newValue < newMin)
                newValue = newMin;

            return newValue;
        }

        static public List<double> NormaliseListOfValues(List<double> values, double newMin, double newMax) 
        {
            List<double> newValues = new List<double>(values.Count);
            double newRange = newMax - newMin;
            double maxValue = Max(values);
            double minValue = Min(values);
            double oldRange = maxValue - minValue;

            for (int i = 0; i < values.Count; i++)
            {
                double frac = (values[i] - minValue) / (oldRange);
                double newVal = newMin + frac * newRange;
                newValues.Add(newVal);
            }

            return newValues;
        }

        static public double Max(List<double> x)
        {
            double max = -10000000;
            for (int i = 0; i < x.Count; i++)
                if (x[i] > max)
                    max = x[i];
            return max;
        }

        static public double Min(List<double> x)
        {
            double min = 10000000;
            for (int i = 0; i < x.Count; i++)
                if (x[i] < min)
                    min = x[i];
            return min;
        }


        #endregion
        //----------------------------------------------------------------------------------//


        /** Checks if a string is numerical and if the value is within a certain extends
* 
*/
        static public bool IsValidNumericalInput(string text, double min, double max)
        {
            double dText;

            if (MyIsNumeric(text))
            {
                dText = double.Parse(text);

                if (dText >= min && dText <= max)
                    return true;
                else
                {
                    Rhino.UI.Dialogs.ShowMessageBox("Value must not be less than " + min + " or bigger than " + max, "Error message");
                    return false;
                }
            }
            else
            {
                Rhino.UI.Dialogs.ShowMessageBox("Invalid input", "Error message");
                return false;
            }
        }



        /** Checks so that a string contains only numeric characters and maximum one dot.
        *
        */
        static public bool MyIsNumeric(string str)
        {
            char c;
            char c2;
            bool result = false;
            bool isNeg = false;
            int nDots = 0;

            c = str[0];

            //To be an ok neg value the first char has to be '-' and the secound one (if there is an) har to be numeric.
            if (str.Length > 1)
            {
                c2 = str[1];
                if (c == '-' && Char.IsNumber(c2))
                {
                    isNeg = true;
                }
            }
            else { isNeg = false; }

            //If the fist char on the other hand is a '.' a zero should be added before the dot.
            if (c == '.')
            {
                str = "0" + str;
            }

            //Checks so that str contains numeric values and maximum one dot.
            for (int i = 0; i < str.Length; i++)
            {
                c = str[i];

                if (Char.IsNumber(c))
                {
                    result = true;
                }
                else if (c == '.' && nDots == 0)
                {
                    result = true;
                    nDots++;
                }
                else if (i == 0 && c == '-' && isNeg)
                {
                    result = true;
                    isNeg = false; //Should not be abel to have more than one '-' 
                }
                else
                {
                    result = false;
                    break;
                }
            }
            return result;
        }


    }
}
