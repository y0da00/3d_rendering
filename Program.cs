using System;
using Raylib_cs;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Reflection.Metadata;

using System.Numerics;
using System.Globalization;
namespace _3d_rendering;



class Program
{
    static int WIDTH = 800;
    static int HEIGHT = 800;
    static double DISTANCE = 5;
    static Position light;
    static double angle = 0;
    static List<Vertex> vertices = LoadModel("moon.txt");
    struct Position
    {
        public double x;
        public double y;
        public double z;
        public Position(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
    struct Vertex
    {
        public Position[] positions;
        public Raylib_cs.Color color;
        public Vertex(Position a, Position b, Position c, Raylib_cs.Color color)
        {
            this.positions = new Position[3] { a, b, c };
            this.color = color;
        }
    }
    static double[] Translate(Position pos)
    {
        double z = pos.z + DISTANCE;;
        double x = ((pos.x/z+1)/2)*WIDTH;
        double y = ((pos.y/z+1)/2)*HEIGHT;
        return new double[2] {x,y};
    }

    static List<Vertex> LoadModel(string path)
    {
        double[] n = File.ReadAllText(path).Split(new char[0], StringSplitOptions.RemoveEmptyEntries).Select(s => double.Parse(s)).ToArray();

        int count = (int)n[0];
        List<Vertex> result = new List<Vertex>();

        for (int t = 0; t < count; t++)
        {
            int k = 1 + t * 9; 
            Position a = new(n[k],     n[k + 1] - 1.5, n[k + 2]);
            Position b = new(n[k + 3], n[k + 4] - 1.5, n[k + 5]);
            Position c = new(n[k + 6], n[k + 7] - 1.5, n[k + 8]);

            
            float hue = (float)((a.y + 1.5) / 3.2 * 300);
            result.Add(new Vertex(a, b, c, Raylib_cs.Color.White));
        }
        
        return result;
    }
    static void Display(double[] point)
    {
        
        int size_index = 2;
        Raylib.DrawCircle((int)point[0], (int)point[1], size_index, Raylib_cs.Color.White);
    }
    static void DisplayVetices()
    {
        for(int i = 0; i < vertices.Count; i++)
        {
            List<Vector2> trig = new List<Vector2>(){};
            List<Position> nodes = new List<Position>(){};
            for(int j = 0; j < 3; j++)
            {
                Position node = Transform(vertices[i].positions[j]);
                nodes.Add(node);
                double[] start = Translate(node);
                trig.Add(new Vector2((float)start[0], (float)start[1]));
               
                //double[] end = Translate(Transform(vertices[i].positions[(j + 1) % 3]));
                //Display(start);
                //Raylib.DrawLine((int)start[0], (int)start[1], (int)end[0], (int)end[1], Raylib_cs.Color.Red);
                
            }
            Position normalised = Normalise(Norm(Substract(nodes[1] , nodes[0]),Substract(nodes[2], nodes[0])));
            double angle = DotProduct(normalised, light);
            Color lit = Color.FromHSV(25f, 0.3f, (float)(0.9 * (0.1 + 0.9 * Math.Max(0, angle))));
            Raylib.DrawTriangle(trig[0],trig[1],trig[2],lit);
            Raylib.DrawTriangle(trig[0], trig[1], trig[2],lit);
           
        }
    }
    static Position MultiplyMatrix(double[,] m, Position p)
    {
        return new Position(
            m[0, 0] * p.x + m[0, 1] * p.y + m[0, 2] * p.z,
            m[1, 0] * p.x + m[1, 1] * p.y + m[1, 2] * p.z,
            m[2, 0] * p.x + m[2, 1] * p.y + m[2, 2] * p.z
        );
    }

    static Position RotateX(Position p, double angle)
    {
        double c = Math.Cos(angle), s = Math.Sin(angle);
        double[,] m =
        {
            { 1, 0,  0 },
            { 0, c, -s },
            { 0, s,  c }
        };
        return MultiplyMatrix(m, p);
    }

    static Position RotateY(Position p, double angle)
    {
        double c = Math.Cos(angle), s = Math.Sin(angle);
        double[,] m =
        {
            {  c, 0, s },
            {  0, 1, 0 },
            { -s, 0, c }
        };
        return MultiplyMatrix(m, p);
    }

    static Position RotateZ(Position p, double angle)
    {
        double c = Math.Cos(angle), s = Math.Sin(angle);
        double[,] m =
        {
            { c, -s, 0 },
            { s,  c, 0 },
            { 0,  0, 1 }
        };
        return MultiplyMatrix(m, p);
    }
    static Position Transform(Position p)
    {
        p = RotateY(p, angle);
        p = RotateX(p, Math.PI  );
        return p;
    }   
    static Position Normalise(Position vector)
    {
        double len = Math.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
        return new Position(vector.x / len, vector.y / len, vector.z / len);
    }
    static Position Substract(Position v1, Position v2)
    {
        return new Position(v1.x-v2.x,v1.y-v2.y,v1.z-v2.z);
    }
    static Position Norm(Position v1, Position v2)
    {
        return new Position(v1.y * v2.z - v1.z * v2.y, v1.z * v2.x - v1.x * v2.z, v1.x * v2.y - v1.y * v2.x);
    }
    static double DotProduct(Position v1, Position v2)
    {
        return v1.x * v2.x + v1.y * v2.y + v1.z * v2.z;
    }

    static void Main(string[] args)
    {
        light = Normalise(new Position(-0.5, 0.7, -0.6));
        Position test = new Position(0,0,0);
        Raylib.InitWindow(WIDTH,HEIGHT, "3d");
        Raylib.SetTargetFPS(60);
        while(!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
           
            //DisplayPoints(false);
            vertices = vertices.OrderByDescending(v => v.positions.Average(p => Transform(p).z)).ToList();
            angle += Raylib.GetFrameTime();
            Raylib.ClearBackground( Raylib_cs.Color.Black);
            DisplayVetices();
           // Raylib.DrawFPS(0,0);
            Raylib.EndDrawing();
            
        }
        Raylib.CloseWindow();
    }
}