using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Clipping
{
    public class CGPolygon
    {

        #region Static

        public static bool PolygonIsConvex(List<Point> Points)
        {
            // For each set of three adjacent points A, B, C,
            // find the cross product AB · BC. If the sign of
            // all the cross products is the same, the angles
            // are all positive or negative (depending on the
            // order in which we visit them) so the polygon
            // is convex.
            bool got_negative = false;
            bool got_positive = false;
            int num_points = Points.Count;
            int B, C;

            for (int A = 0; A < num_points; A++) {
                B = (A + 1) % num_points;
                C = (B + 1) % num_points;

                double cross_product = CGPolygon.CrossProductLength(Points[A].X, Points[A].Y, Points[B].X,
                    Points[B].Y, Points[C].X, Points[C].Y);

                if (cross_product < 0)
                    got_negative = true;
                else if (cross_product > 0)
                    got_positive = true;
                if (got_negative && got_positive)
                    return false;
            }

            // If we got this far, the polygon is convex.
            return true;
        }

        public static double CrossProductLength(double Ax, double Ay, double Bx, double By, double Cx, double Cy)
        {
            // Get the vectors' coordinates.
            double BAx = Ax - Bx;
            double BAy = Ay - By;
            double BCx = Cx - Bx;
            double BCy = Cy - By;

            // Calculate the Z coordinate of the cross product.
            return (BAx * BCy - BAy * BCx);
        }

        #endregion

        public List<Point> Points { get; protected set; }

        public CGPolygon(IEnumerable<Point> collection)
        {
            this.Points = new List<Point>(collection);
        }


        public bool IsConvex() {
            if (this.Points.Count >= 3) {
                for (int a = this.Points.Count - 2, b = this.Points.Count - 1, c = 0; c < this.Points.Count; a = b, b = c, ++c) {
                    if (!new CGLine(this.Points[a], this.Points[b]).OnLeft(this.Points[c]))
                        return false;
                }
            }
            return true;
        }


        public List<CGLine> Edges {
            get {
                List<CGLine> edges = new List<CGLine>();
                if (this.Points.Count >= 2) {
                    for (int a = this.Points.Count - 1, b = 0; b < this.Points.Count; a = b, ++b)
                        edges.Add(new CGLine(this.Points[a], this.Points[b]));
                }
                return edges;
            }
        }



        private CGLine ClipLine(CGLine line)
        {
            double tE = 0, tL = 1;
            var dir = line.Direction;
            Point nL;
            bool PE = false;

            foreach (var edge in this.Edges) {

                nL = edge.Normal;
                var t = line.IntersectionParameter(edge);

                double slope = nL.DotProduct(dir);
                PE = (slope < 0 ? true : false);

                //potentionally entering
                if (PE) {
                    //Choose for parameter tE the largest t ≤ 1 from the PE set
                    if (t > tE) 
                        tE = t;

                //potentionally leaving
                } else {
                    //Choose for parameter tL the smallest t ≥ 0 from the PL set
                    if (t < tL)
                        tL = t;
                }
            }

            //Line is discarded if tE > tL
            if (tE > tL)
                return null;

            //Clipped line is defined by [tE , tL]
            return line.Morph(tE, tL);
        }


        public List<CGLine> Clip(List<CGLine> lines)
        {
            if (!this.IsConvex())
                this.Points.Reverse();

            var clippedLines = new List<CGLine>();
            foreach (var line in lines) {
                CGLine clippedSegment = this.ClipLine(line);
                if (clippedSegment != null)
                    clippedLines.Add(clippedSegment);
            }

            return clippedLines;
        }
    }


    public static class PointExtensions
    {
        public static Point Add(this Point a, Point b)
        {
            return new Point(a.X + b.X, a.Y + b.Y);
        }

        public static Point Substract(this Point a, Point b)
        {
            return new Point(a.X - b.X, a.Y - b.Y);
        }

        public static Point Multiply(this Point a, double b)
        {
            return new Point(a.X * b, a.Y * b);
        }

        public static double DotProduct(this Point a, Point b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public static double CrossProduct(this Point a, Point b)
        {
            return a.X * b.Y - a.Y * b.X;
        }
    }
}
