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
        //List<Shape> shapes;
        ListShapes shapes;
        List<ShapePoint> points;

        class ListShapes
        {
            private List<Shape> data;
            private Graphics g;
            private Pen commonPen;
            private Pen selectedPen;
            private PictureBox picBoxRef;
            private ListBox listBoxRef;

			//public Shape selectedShape;
            //public int selectedIndex;

            public ListShapes(Graphics g, PictureBox pb, ListBox lb)
            {
                data = new List<Shape>();
                //selectedIndex = -1;
                commonPen = new Pen(Color.Blue);
                selectedPen = new Pen(Color.Red);
                this.g = g;
                picBoxRef = pb;
                listBoxRef = lb;
				//selectedShape = null;
            }

            public void Add(Shape s)
            {
				if (IsEmpty())
				{
					data.Add(s);
					//selectedIndex = 0;
					//selectedShape = data[selectedIndex];
				}
				else
					data.Add(s);
                listBoxRef.Items.Add(s.GetType().ToString());
                Draw();
            }

            public bool IsEmpty() 
            {
                return data.Count == 0;
            }

            /*public Shape GetNext()
            {
                if (IsEmpty()) return null;

                //selectedIndex = (selectedIndex + 1) % data.Count;
				//selectedShape = data[selectedIndex];

				Draw();
                //return data[selectedIndex];
                return data[listBoxRef.SelectedIndex];
            }*/

            public Shape GetSelectedShape()
            {
                return listBoxRef.SelectedIndex == -1 ? null : data[listBoxRef.SelectedIndex];
            }

            public List<ShapePoint> GetPointsList()
            {
                return data.Where(shape => shape is ShapePoint).Select(shape => (ShapePoint)shape).ToList();
            }

            public void Draw()
            {
                g.Clear(Color.White);
                for (int i = 0; i < data.Count; ++i)
                    if (i == listBoxRef.SelectedIndex)
                        data[i].draw(g, selectedPen);
                    else
                        data[i].draw(g, commonPen);
                picBoxRef.Invalidate();
            }
        }

        class ShapeBuilder
        {
            static private Shape data;
			static private Graphics g;
			static private PictureBox pb;
			static private Pen pen;

            static public bool isInProcess;
            static public bool isComplete;

			static public void Init(Graphics gr, PictureBox p) {
				g = gr;
				pb = p;
				pen = new Pen(Color.Green);
			}

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
				if (data is Polygon)
					for (int i = 0; i < data.location.cords.Count - 1; ++i)
						g.DrawLine(
							pen,
							data.location.cords[i],
							data.location.cords[i + 1]
						);
				pb.Invalidate();

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
            
            // смещение от Андрюхи
            public void translation(int dx, int dy)
            {
                double[,] trans_mat = new double[3, 3];
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                    {
                        if (i == j)
                            trans_mat[i, j] = 1;
                        else
                            trans_mat[i, j] = 0;
                    }
                trans_mat[2, 0] += dx;
                trans_mat[2, 1] += dy;

                for (int i = 0; i < location.cords.Count; i++)
                {
                    double prev_x = location.cords[i].X;
                    double prev_y = location.cords[i].Y;
                    double[] new_cords = mats_mult(new List<double> { prev_x, prev_y, 1 }, trans_mat);
                    location.cords[i] = new Point((int)new_cords[0], (int)new_cords[1]);
                }
            }

            // вращение от Андрюхи
            public void rotation(double phi, Point p)
            {
                double[,] rot_mat = new double[3, 3];
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        rot_mat[i, j] = 0;
                rot_mat[0, 0] = Math.Cos(phi * Math.PI / 180); // перевод градусов в радианы
                rot_mat[0, 1] = Math.Sin(phi * Math.PI / 180);
                rot_mat[1, 0] = -1 * rot_mat[0, 1];
                rot_mat[1, 1] = rot_mat[0, 0];
                rot_mat[2, 0] = -1 * p.X * rot_mat[0, 0] + p.Y * rot_mat[0, 1] + p.X;
                rot_mat[2, 1] = -1 * p.X * rot_mat[0, 1] - p.Y * rot_mat[0, 0] + p.Y;
                rot_mat[2, 2] = 1;

                for (int i = 0; i < location.cords.Count; i++)
                {
                    double prev_x = location.cords[i].X;
                    double prev_y = location.cords[i].Y;
                    double[] new_cords = mats_mult(new List<double> { prev_x, prev_y, 1 }, rot_mat);
                    location.cords[i] = new Point((int)new_cords[0], (int)new_cords[1]);
                }
            }

            // растяжение/сжатие от Андрюхи
            public void dilatation(double alpha, double beta, Point p)
            {
                double[,] dil_mat = new double[3, 3];
                for (int i = 0; i < 3; i++)
                    for (int j = 0; j < 3; j++)
                        dil_mat[i, j] = 0;
                dil_mat[0, 0] = alpha;
                dil_mat[1, 1] = beta;
                dil_mat[2, 0] = (1 - alpha) * p.X;
                dil_mat[2, 1] = (1 - beta) * p.Y;
                dil_mat[2, 2] = 1;

                for (int i = 0; i < location.cords.Count; i++)
                {
                    double prev_x = location.cords[i].X;
                    double prev_y = location.cords[i].Y;
                    double[] new_cords = mats_mult(new List<double> { prev_x, prev_y, 1 }, dil_mat);
                    location.cords[i] = new Point((int)new_cords[0], (int)new_cords[1]);
                }
            }

            public void scale(double alpha, double beta)
            {
                dilatation(alpha, beta, getShapeCenter());
            }

            public Point getShapeCenter()
            {
                int leftLimit;
                int rightLimit;
                int topLimit;
                int bottomLimit;
                leftLimit = rightLimit = location.cords[0].X;
                topLimit = bottomLimit = location.cords[0].Y;
                for (int i = 1; i < location.cords.Count; ++i)
                {
                    int x = location.cords[i].X;
                    int y = location.cords[i].Y;

                    if (x < leftLimit)
                        leftLimit = x;
                    else if (x > rightLimit)
                        rightLimit = x;

                    if (y < bottomLimit)
                        bottomLimit = y;
                    else if (y > topLimit)
                        topLimit = y;
                }

                return new Point((rightLimit + leftLimit) / 2, (topLimit + bottomLimit) / 2);
            }
        }
        
        // умножение строки на матрицу от Андрюхи
        static double[] mats_mult(List<double> prev_cords, double[,] aff_mat)
        {
            double[] res = new double[2];
            res[0] = 0;
            res[1] = 0;
           
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 2; j++)
                    res[j] += aff_mat[i, j] * prev_cords[i];

            return res;
        }

        // проверка, пересекаются ли отрезки TODO
		static bool if_segs_cross(Segment s1, Segment s2)
		{
			int p1x = s1.location.cords[0].X;
			int p1y = s1.location.cords[0].Y;
			int p2x = s1.location.cords[1].X;
			int p2y = s1.location.cords[1].Y;
			int p3x = s2.location.cords[0].X;
			int p3y = s2.location.cords[0].Y;
			int p4x = s2.location.cords[1].X;
			int p4y = s2.location.cords[1].Y;

			// считаем, что начальная точка находится левее конечной
			if (p2x < p1x)
			{
				int tmpx = p1x;
				int tmpy = p1y;
				p1x = p2x;
				p1y = p2y;
				p2x = tmpx;
				p2y = tmpy;
			}
			if (p4x < p3x)
			{
				int tmpx = p3x;
				int tmpy = p3y;
				p3x = p4x;
				p3y = p4y;
				p4x = tmpx;
				p4y = tmpy;
			}

			if (p2x < p3x)
				return false;

			//если оба отрезка вертикальные
			if ((p1x - p2x == 0) && (p3x - p4x == 0))
			{
				//если они лежат на одном X
				if (p1x == p3x)
					//проверим пересекаются ли они, т.е. есть ли у них общий Y
					//для этого возьмём отрицание от случая, когда они НЕ пересекаются
					if (!(Math.Max(p1y, p2y) < Math.Min(p3y, p4y) || Math.Min(p1y, p2y) > Math.Max(p3y, p4y)))	
						return true;
				return false;
			}

			return true;
		}

        // поиск точки пересечения рёбер от Андрюхи (TO-DO)
        static Point segments_crosspoint(Segment s1, Segment s2)
        {
            Point res = new Point();
            int p1x = s1.location.cords[0].X;
            int p1y = s1.location.cords[0].Y;
            int p2x = s1.location.cords[1].X;
            int p2y = s1.location.cords[1].Y;
            int p3x = s2.location.cords[0].X;
            int p3y = s2.location.cords[0].Y;
            int p4x = s2.location.cords[1].X;
            int p4y = s2.location.cords[1].Y;

			// считаем, что начальная точка находится левее конечной
			if (p2x < p1x)
			{
				int tmpx = p1x;
				int tmpy = p1y;
				p1x = p2x;
				p1y = p2y;
				p2x = tmpx;
				p2y = tmpy;
			}
			if (p4x < p3x)
			{
				int tmpx = p3x;
				int tmpy = p3y;
				p3x = p4x;
				p3y = p4y;
				p4x = tmpx;
				p4y = tmpy;
			}

			if (p2x < p3x)

			// 1-ый отрезок оси абсцисс
			if (p1x == p2x)
			{

			}

            double A1 = (p1y - p2y) / (p1x - p2x);
            double A2 = (p3y - p4y) / (p3x - p4x);
            double b1 = p1y - A1 * p1x;
            double b2 = p3y - A2 * p3x;
            res.X = (int)((b2 - b1) / (A1 - A2));
            res.Y = (int)(A2 * res.X + b2);

            return res;
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

        /*public void drawShapes()
        {
            foreach (Shape s in shapes)
                s.draw(g, bluePen);
            pictureBox1.Invalidate();
        }*/

        public void initComboBox() {
            comboBox1.Items.AddRange(new string[] {
                "Точка",
                "Отрезок",
                "Многоугольник"
            });

            comboBox1.SelectedIndex = 0;
        }

        private void initOptions(Shape s)
        {
            if (s == null)
            {
                groupBox1.Visible = false;
                groupBox2.Visible = false;
                groupBox3.Visible = false;
                return;
            }

            comboBox2.Items.Clear();
            groupBox1.Visible = true;
            groupBox2.Visible = true;
            groupBox3.Visible = true;
            textBox1.Text = textBox2.Text = textBox3.Text = textBox4.Text = textBox5.Text = "";

            points = new List<ShapePoint>();
            points = shapes.GetPointsList();
            for (int i = 0; i < points.Count; ++i)
                comboBox2.Items.Add("Точка " + i.ToString());
            comboBox2.SelectedIndex = -1;

        }

        private bool angleIsSet()
        {
            try
            {
                int.Parse(textBox3.Text);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool canRotate()
        {
            return angleIsSet() && comboBox2.SelectedIndex != -1;
        }

        private bool canTranslate()
        {
            try
            {
                int.Parse(textBox1.Text);
                int.Parse(textBox2.Text);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool canScale()
        {
            try
            {
                double.Parse(textBox4.Text);
                double.Parse(textBox5.Text);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Form1()
        {
            InitializeComponent();

            mainBitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            g = Graphics.FromImage(mainBitmap);
            g.Clear(Color.White);
            pictureBox1.Image = mainBitmap;
			ShapeBuilder.Init(g, pictureBox1);

            initComboBox();
            //shapes = new List<Shape>();
            shapes = new ListShapes(g, pictureBox1, listBox1);

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

            //shapes.Add(sp);
            //shapes.Add(seg);
            //shapes.Add(poly);
            //drawShapes();
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
            //drawShapes();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            shapes.Add(ShapeBuilder.GetShape());
            comboBox1.Enabled = true;
            button1.Enabled = true;
            button2.Enabled = false;
            button2.Visible = false;
            //drawShapes();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            shapes.Draw();
            initOptions(shapes.GetSelectedShape());
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            g.Clear(Color.White);
            shapes.Draw();
            g.FillEllipse(
                new SolidBrush(Color.Violet),
                points[comboBox2.SelectedIndex].location.cords[0].X - 5,
                points[comboBox2.SelectedIndex].location.cords[0].Y - 5,
                10,
                10
            );
            pictureBox1.Invalidate();
            button6.Enabled = canRotate();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            button6.Enabled = canRotate();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            shapes.GetSelectedShape().rotation(
                int.Parse(textBox3.Text),
                points[comboBox2.SelectedIndex].location.cords.First()
            );
            shapes.Draw();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button3.Enabled = canTranslate();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            shapes.GetSelectedShape().translation(
                int.Parse(textBox1.Text),
                int.Parse(textBox2.Text)
            );
            shapes.Draw();
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            button4.Enabled = canScale();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            shapes.GetSelectedShape().scale(
                double.Parse(textBox4.Text),
                double.Parse(textBox5.Text)
            );
            shapes.Draw();
        }
    }
}
