using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Clipping
{
    public class CGLine
    {
        public Point A { get; private set; }
        public Point B { get; private set; }

        //Normal vector
        public Point Normal { get { return new Point(B.Y - A.Y, A.X - B.X); } }
        public Point Direction { get { return new Point(B.X - A.X, B.Y - A.Y); } }


        public CGLine(Point a, Point b)
        {
            A = a;
            B = b;
        }

        public bool OnLeft(Point p)
        {
            var ab = new Point(B.X - A.X, B.Y - A.Y);
            var ap = new Point(p.X - A.X, p.Y - A.Y);
            return ab.CrossProduct(ap) >= 0;
        }

        public double IntersectionParameter(CGLine that)
        {
            var edge = that;

            var segmentToEdge = edge.A.Substract(this.A);
            var segmentDir = this.Direction;
            var edgeDir = edge.Direction;

            var t = edgeDir.CrossProduct(segmentToEdge) / edgeDir.CrossProduct(segmentDir);

            if (double.IsNaN(t))
                t = 0;

            return t;
        }

        public CGLine Morph(double tA, double tB)
        {
            var d = Direction;
            return new CGLine(A.Add(d.Multiply(tA)), A.Add(d.Multiply(tB)));
        }
    }
}
