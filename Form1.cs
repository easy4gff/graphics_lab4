using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;

namespace lab4
{
    public partial class Form1 : Form
    {
        Graphics g;
        Bitmap   mainBitmap;
        Pen bluePen;
        List<Shape> shapes;

        class ShapeBuilder
        {
            static private Shape data;

            static public bool isInProcess;
            static public bool isComplete;

            static public void StartBuilding(int index)
            {
                Clear();
                switch (index) {
                    case 0:
                        data = new ShapePoint();
                        break;
                    case 1:
                        data = new Segment();
                        break;
                    default:
                        data = new Polygon();
                        break;
                }
                isInProcess = true;
            }

            static public bool AddPoint(Point p)
            {
                if (isComplete && !(data is Polygon)) return true;

                if (!isInProcess)
                    throw new DataException("Shape is not currently in building process!");

                data.location.cords.Add(p);
                int count = data.location.cords.Count;
                if (data is ShapePoint && count == 1 ||
                    data is Segment    && count == 2 ||
                    data is Polygon    && count >= 3)
                {
                    isComplete = true;
                    return true;
                }
                return false;
            }

            static public Shape GetShape()
            {
                if (!isComplete)
                    throw new DataException("Shape is not completed!");
                data.GetType();
                dynamic result = Convert.ChangeType(data, data.GetType());
                Clear();
                return result;
            }

            static private void Clear()
            {
                data = null;
                isInProcess = false;
                isComplete = false;
            }
        }

        // Координаты
        public class ShapeLocation
        {
            public List<Point> cords;

            public ShapeLocation() 
            {
                cords = new List<Point>();
            }

            public ShapeLocation(Point p) 
            {
                cords = new List<Point>();
                cords.Add(p);
            }

            public ShapeLocation(Point p1, Point p2) {
                cords = new List<Point>();
                cords.AddRange(new Point[]{p1, p2});
            }

            public ShapeLocation(List<Point> list)
            {
                cords = new List<Point>();
                cords.AddRange(list.ConvertAll(cord => new Point(cord.X, cord.Y)));
            }

            public ShapeLocation(ShapeLocation other) 
            {
                cords = new List<Point>();
                cords = other.cords.ConvertAll(cord => new Point(cord.X, cord.Y));
            }

            public override string ToString()
            {
                StringBuilder result = new StringBuilder();
                foreach (Point p in cords)
                {
                    result.Append("(" + p.X + "," + p.Y + ")  ");
                }
                result.Append("\n");
                return result.ToString();
            }
        }


        // Фигура
        class Shape
        {
            public ShapeLocation location;
            Graphics g;
            Pen p;

            public Shape()
            {
                location = new ShapeLocation();
            }

            public Shape(Point p)
            {
                location = new ShapeLocation(p);
            }

            public Shape(Point p1, Point p2)
            {
                location = new ShapeLocation(p1, p2);
            }

            public Shape(List<Point> list)
            {
                location = new ShapeLocation(list);
            }

            public Shape(ShapeLocation loc)
            {
                location = new ShapeLocation(loc);
            }

            public Shape(Shape sh)
            {
                location = new ShapeLocation(sh.location);
            }

            public override string ToString()
            {
                return location.ToString();
            }

            public virtual void draw(Graphics g, Pen pen) { }

        }

        // Точка
        class ShapePoint : Shape {
            public ShapePoint() {}

            public ShapePoint(Point p) : base(p) { }

            public override void draw(Graphics g, Pen pen)
            {
                g.DrawEllipse(pen, location.cords.First().X, location.cords.First().Y, 1, 1);
            }
        }

        // Отрезок
        class Segment : Shape
        {
            public Segment() { }

            public Segment(Point p1, Point p2) : base(p1, p2) { }

            public override void draw(Graphics g, Pen pen)
            {
                g.DrawLine(
                    pen,
                    location.cords.First().X,
                    location.cords.First().Y,
                    location.cords.Last().X,
                    location.cords.Last().Y
                );
            }
        }

        // Многоугольник
        class Polygon : Shape
        {
            public Polygon() { }

            public Polygon(List<Point> points) : base(points) { }

            public override void draw(Graphics g, Pen pen)
            {
                for (int i = 0; i < location.cords.Count - 1; ++i)
                    g.DrawLine(
                        pen,
                        location.cords[i].X,
                        location.cords[i].Y,
                        location.cords[i + 1].X,
                        location.cords[i + 1].Y
                    );
                g.DrawLine(
                        pen,
                        location.cords[0].X,
                        location.cords[0].Y,
                        location.cords[location.cords.Count - 1].X,
                        location.cords[location.cords.Count - 1].Y
                    );
            }
        }

        public void drawShapes()
        {
            foreach (Shape s in shapes)
                s.draw(g, bluePen);
            pictureBox1.Invalidate();
        }

        public void initComboBox() {
            comboBox1.Items.AddRange(new string[] {
                "Точка",
                "Отрезок",
                "Многоугольник"
            });

            comboBox1.SelectedIndex = 0;
        }

        public Form1()
        {
            InitializeComponent();

            mainBitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(mainBitmap);
            g.Clear(Color.White);
            pictureBox1.Image = mainBitmap;

            initComboBox();
            shapes = new List<Shape>();

            bluePen = new Pen(Color.Blue);
            

            Point p1 = new Point(0, 0);
            ShapePoint sp = new ShapePoint(p1);
            //sp.draw(g, bluePen);

            Segment seg = new Segment(new Point(100, 100), new Point(250, 250));
            //seg.draw(g, bluePen);

            Point p11 = new Point(0, 0);
            Point p2 = new Point(280, 120);
            Point p3 = new Point(105, 110);
            Point p4 = new Point(95, 84);
            Point p5 = new Point(49, 50);
            Point p6 = new Point(30, 20);
            List<Point> points = new List<Point>();
            points.AddRange(new Point[] { p11, p2, p3, p4, p5, p6 });
            Polygon poly = new Polygon(points);
            //poly.draw(g, bluePen);

            shapes.Add(sp);
            shapes.Add(seg);
            shapes.Add(poly);
            drawShapes();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 2)
            {
                button2.Visible = true;
            }
            ShapeBuilder.StartBuilding(comboBox1.SelectedIndex);
            comboBox1.Enabled = false;
            button1.Enabled = false;
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (!ShapeBuilder.isInProcess) return;

            bool add_result = ShapeBuilder.AddPoint(e.Location);
            if (add_result && comboBox1.SelectedIndex != 2)
            {
                switch (comboBox1.SelectedIndex)
                {
                    case 0:
                        shapes.Add((ShapePoint)ShapeBuilder.GetShape());
                        break;
                    case 1:
                        shapes.Add((Segment)ShapeBuilder.GetShape());
                        break;
                }
                comboBox1.Enabled = true;
                button1.Enabled = true;
            }
            else if (add_result)
            {
                button2.Enabled = true;
            }
            drawShapes();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            shapes.Add(ShapeBuilder.GetShape());
            comboBox1.Enabled = true;
            button1.Enabled = true;
            button2.Enabled = false;
            button2.Visible = false;
            drawShapes();
        }
    }
}
