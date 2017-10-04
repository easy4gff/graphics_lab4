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

            public string ToString()
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

            public string ToString()
            {
                return location.ToString();
            }
        }

        // Точка
        class ShapePoint : ShapeLocation {
            public ShapePoint(Point p) : base(p){ }
        }

        // Отрезок
        class Segment : ShapeLocation
        {
            public Segment(Point p1, Point p2) : base(p1, p2) { }
        }

        // Многоугольник
        class Polygon : ShapeLocation
        {
            public Polygon(List<Point> points) : base(points) { }
        }

        public Form1()
        {
            InitializeComponent();
            Point p1 = new Point(27, 42);
            ShapePoint l = new ShapePoint(p1);
            
        }
    }
}
