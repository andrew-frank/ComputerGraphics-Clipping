using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Clipping
{
    public enum Mode
    {
        Polygon = 0,
        DrawLine,
    }


    public partial class MainWindow : Window
    {
        public List<Point> PolygonPoints { get; private set; }

        public Mode DrawingMode { get; protected set; }

        private List<Line> _lines = new List<Line>();
        public List<Line> Lines { get { return _lines; } }

        public int LinesCount { get; private set; }


        #region UI handles


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.PolygonPoints = new List<Point>();
        }


        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch (DrawingMode) {
                case Mode.Polygon:
                    Point p = e.GetPosition(mainContainer);
                    PolygonPoints.Add(p);
                    if (PolygonPoints.Count > 1)
                        this.PolygonRadioBtn.IsEnabled = true;

                    if (!CGPolygon.PolygonIsConvex(PolygonPoints)) {
                        MessageBox.Show("Polygon is not convex");
                        PolygonPoints.Remove(p);
                        break;
                    }

                    var circle = new Ellipse {
                        Stroke = Brushes.Black,
                        StrokeThickness = 2,
                    };

                    Canvas.SetLeft(circle, p.X - 2);
                    Canvas.SetTop(circle, p.Y - 2);
                    mainContainer.Children.Add(circle);
                    break;
                case Mode.DrawLine:
                    Point lP = e.GetPosition(mainContainer);
                    break;
            }
        }


        void mainContainer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Line l = new Line();
            l.X1 = e.GetPosition(mainContainer).X;
            l.Y1 = e.GetPosition(mainContainer).Y;
            this.Lines.Add(l);
            mainContainer.MouseMove += mainContainer_MouseMove;
            mainContainer.MouseLeftButtonUp += mainContainer_MouseLeftButtonUp;
        }


        void mainContainer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.Lines[this.LinesCount].X1 == this.Lines[this.LinesCount].X2 && this.Lines[this.LinesCount].Y1 == this.Lines[this.LinesCount].Y2)
                return;

            this.Lines[this.LinesCount].X2 = e.GetPosition(mainContainer).X;
            this.Lines[this.LinesCount].Y2 = e.GetPosition(mainContainer).Y;
            this.Lines[this.LinesCount].StrokeThickness = 1;
            mainContainer.Children.Remove(this.Lines[this.LinesCount]);
            mainContainer.Children.Add(this.Lines[this.LinesCount]);

            mainContainer.MouseLeftButtonUp -= mainContainer_MouseLeftButtonUp;
            mainContainer.MouseMove -= mainContainer_MouseMove;
            this.LinesCount++;
        }


        void mainContainer_MouseMove(object sender, MouseEventArgs e)
        {
            mainContainer.Children.Remove(this.Lines[this.LinesCount]);
            this.Lines[this.LinesCount].X2 = e.GetPosition(mainContainer).X;
            this.Lines[this.LinesCount].Y2 = e.GetPosition(mainContainer).Y;
            this.Lines[this.LinesCount].Stroke = Brushes.Gray;
            this.Lines[this.LinesCount].StrokeThickness = 0;
            mainContainer.Children.Add(this.Lines[this.LinesCount]);
        }



        private void PolygonRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            this.DrawingMode = Mode.Polygon;
        }


        private void LineRadioBtn_Checked(object sender, RoutedEventArgs e)
        {
            this.DrawingMode = Mode.DrawLine;
            mainContainer.MouseLeftButtonDown += mainContainer_MouseLeftButtonDown;
        }


        private void joinBtn_Click(object sender, RoutedEventArgs e)
        {
            SolidColorBrush blackBrush = new SolidColorBrush();
            blackBrush.Color = Colors.Black;

            Polygon polygon = new Polygon();
            polygon.Stroke = blackBrush;
            PointCollection pCollection = new PointCollection(PolygonPoints);
            polygon.Points = pCollection;

            mainContainer.Children.Add(polygon);
        }


        private void clipBtn_Click(object sender, RoutedEventArgs e)
        {
            CGPolygon polygon = new CGPolygon(PolygonPoints);
            List<CGLine> segments = new List<CGLine>();
            foreach (var l in this.Lines) {
                Point a = new Point(l.X1, l.Y1);
                Point b = new Point(l.X2, l.Y2);
                CGLine S = new CGLine(a, b);
                segments.Add(S);
            }

            List<Point> pList = new List<Point>();


            var x = polygon.Clip(segments);

            foreach (var item in x) {
                Line ln = new Line();
                ln.X1 = item.A.X;
                ln.Y1 = item.A.Y;

                ln.X2 = item.B.X;
                ln.Y2 = item.B.Y;
                ln.Stroke = Brushes.Pink;
                ln.StrokeThickness = 2;
                mainContainer.Children.Add(ln);
            }
        }

        private void clearBtn_Click(object sender, RoutedEventArgs e)
        {
            mainContainer.Children.RemoveRange(0, mainContainer.Children.Count);
            PolygonPoints.Clear();
        }

        #endregion


    }
}
