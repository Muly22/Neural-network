using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System.Numerics;

namespace Prog
{
    internal enum TipeNeuron
    {
        Hiden, Output
    }
    internal class testSFML
    {
        public static void Main(string[] args)
        {
            var game = new Game(new VideoMode(1000, 1000), "Test");
        }
    }

    internal class Game : RenderWindow
    {
        private RenderWindow window;
        private uint Width;
        private uint Height;
        private int cellX;
        private int cellY;
        private int Max_val;
        private Population Population { get; set; }

        public static int nos = 200;
        public static int Mapsize = 50;
        private double TimeSpid = 1;
        public static int rov = 6;
        public Game(VideoMode mode, string title) : base(mode, title, Styles.Titlebar | Styles.Close)
        {
            window = this;
            Width = window.Size.X; Height = window.Size.Y;
            cellX = (int)(Width / Mapsize);
            cellY = (int)(Height / Mapsize);
            new Net(8 * rov + 1, 4);
            Population = new Population();
            Population.bft = new Type[3];
            RendDraw();
        }
        private void Iteration()
        {
            Population.iteration++;
            int[] fore_best = new int[Population.population.Length];
            bool end = true;
            do
            {

                for (int i = 0; i < Population.population.Length; i++)
                {
                    end = Population.population[i].Move();
                    fore_best[i] = Population.population[i].ball;
                }
                Draw(fore_best.Max());
            } while (end);
            int max_val = fore_best.Max();
            window.SetTitle($"best: {max_val}, iteration: {Population.iteration}");
            if (Max_val < max_val)
            {
                Max_val = max_val;
                Console.WriteLine($"New best!!! iteration: {Population.iteration} best: {max_val} ");
            }
            var Listfore_best = fore_best.ToList();
            int index = Listfore_best.IndexOf(max_val);
            Population.bft[0] = Population.population[index];
            max_val = Listfore_best.Max();
            index = Listfore_best.IndexOf(max_val);
            Population.bft[1] = Population.population[index];
            Population.Ress();
        }
        private void Draw(int max)
        {
            window.DispatchEvents();
            window.Clear();
            for (int i = 0; i < Population.population.Length; i++)
            {
                Vertex[] food = new Vertex[] { new Vertex(new Vector2f(0, 1), Color.Magenta),
                                               new Vertex(new Vector2f(cellX - 1, 1), Color.Magenta),
                                               new Vertex(new Vector2f(cellX - 1, cellY), Color.Magenta),
                                               new Vertex(new Vector2f(0, cellY), Color.Magenta) };

                Vertex[] cube = new Vertex[] { new Vertex(new Vector2f(0, 1), Color.White),
                                               new Vertex(new Vector2f(cellX - 1, 1), Color.White),
                                               new Vertex(new Vector2f(cellX - 1, cellY), Color.White),
                                               new Vertex(new Vector2f(0, cellY), Color.White) };
                Vector2f poscube = new Vector2f((Population.population[i].Playerpos.X + Mapsize / 2) * cellX, (Population.population[i].Playerpos.Y + Mapsize / 2) * cellY);
                Vector2f posfood = new Vector2f((Population.population[i].Foodpos.X + Mapsize / 2) * cellX, (Population.population[i].Foodpos.Y + Mapsize / 2) * cellY);
                for (int j = 0; j < cube.Length; j++)
                {
                    cube[j].Position += poscube;
                    food[j].Position += posfood;
                }
                window.Draw(cube, PrimitiveType.Quads);
                window.Draw(food, PrimitiveType.Quads);
            }
            Thread.Sleep((int)(100 / TimeSpid));
            window.Display();
        }
        private void RendDraw()
        {
            window.SetMouseCursorVisible(false);
            window.LostFocus += Window_LostFocus;
            window.GainedFocus += Window_GainedFocus;
            window.Closed += Window_Closed;
            window.KeyPressed += Window_KeyPressed;
            while (window.IsOpen)
            {
                Iteration();
            }
        }
        private void Window_KeyPressed(object? sender, KeyEventArgs e)
        {
            switch (e.Code)
            {
                case Keyboard.Key.Num1:
                    TimeSpid = 0.1;
                    break;
                case Keyboard.Key.Num2:
                    TimeSpid = 0.5;
                    break;
                case Keyboard.Key.Num3:
                    TimeSpid = 1;
                    break;
                case Keyboard.Key.Num4:
                    TimeSpid = 5;
                    break;
                case Keyboard.Key.Num5:
                    TimeSpid = 10;
                    break;
                case Keyboard.Key.Num6:
                    TimeSpid = 50;
                    break;
                case Keyboard.Key.Num7:
                    TimeSpid = 100;
                    break;
                case Keyboard.Key.Num8:
                    TimeSpid = 500;
                    break;
                case Keyboard.Key.Num9:
                    TimeSpid = 1000;
                    break;
            }

        }

        private void Window_GainedFocus(object? sender, EventArgs e)
        {
            window.SetActive(true);
            window.SetMouseCursorVisible(false);
        }

        private void Window_LostFocus(object? sender, EventArgs e)
        {
            window.SetActive(false);
            window.SetMouseCursorVisible(true);
        }

        private void Window_Closed(object? sender, EventArgs e)
        {
            window.Close();
        }

    }
    internal class Population
    {
        public Type[] population;
        public ulong iteration;
        public Type[] bft;
        private Random random;
        public Population()
        {
            bft = new Type[2];
            population = new Type[Game.nos];
            random = new Random();
            for (int i = 0; i < population.Length; i++)
            {
                population[i] = new Type();
            }
        }
        public void Ress()
        {
            for (int i = 0; i < population.Length; i++)
            {
                Type type1 = bft[0];
                Type type2 = bft[1];
                if (random.NextDouble() > 0.5)
                    type2 = bft[0];
                population[i] = new Type(bft[0].Mutation(type1.Weights, type2.Weights));
            }
        }
    }
    internal class Type
    {
        public int ball;
        private int movenumber;
        public Vector2 Playerpos;
        public Vector2 Foodpos;
        private int Mapsize;
        private Random random;
        private double[] Inputs;
        public double[][,] Weights;
        public Type()
        {
            random = new Random();
            Mapsize = Game.Mapsize;
            Playerpos = new Vector2(random.NextInt64(-Mapsize / 2, Mapsize / 2), random.NextInt64(-Mapsize / 2, Mapsize / 2));
            Inputs = new double[Net.InputL];
            Inputs[Inputs.Length - 1] = 1;
            Weights = new double[2][,];
            Weights[0] = new double[Net.HidenL, Net.InputL];
            Weights[1] = new double[Net.OutputL, Net.HidenL];
            for (int i = 0; i < Weights.Length; i++)
            {
                for (int j = 0; j < Weights[i].GetLength(0); j++)
                {
                    for (int m = 0; m < Weights[i].GetLength(1); m++)
                    {
                        if (random.NextDouble() < 0.5)
                            Weights[i][j, m] = random.NextDouble();
                        else
                            Weights[i][j, m] = random.NextDouble() * -1;
                    }
                }
            }
            GanewrateFood();
        }

        public Type(double[][,] weights)
        {
            random = new Random();
            Mapsize = Game.Mapsize;
            Playerpos = new Vector2(random.NextInt64(-Mapsize / 2, Mapsize / 2), random.NextInt64(-Mapsize / 2, Mapsize / 2));
            Inputs = new double[Net.InputL];
            Inputs[Inputs.Length - 1] = 1;
            Weights = weights;

            GanewrateFood();
        }
        public double[][,] Mutation(double[][,] weights1, double[][,] weights2)
        {
            var weightsnu = weights1;
            for (int i = 0; i < weightsnu.Length; i++)
            {
                for (int j = 0; j < Weights[i].GetLength(0); j++)
                {
                    for (int m = 0; m < Weights[i].GetLength(1); m++)
                    {
                        if (random.NextDouble() < 0.5)
                        {
                            weightsnu[i][j, m] = weights2[i][j, m];
                        }
                        else if (random.NextDouble() < 0.001)
                        {
                            if (random.NextDouble() < 0.5)
                                weightsnu[i][j, m] = random.NextDouble();
                            else
                                weightsnu[i][j, m] = random.NextDouble() * -1;
                        }
                    }
                }
            }
            return weightsnu;
        }
        private void GanewrateFood()
        {
            Foodpos = new Vector2(random.NextInt64(-Mapsize / 2, Mapsize / 2), random.NextInt64(-Mapsize / 2, Mapsize / 2));
        }
        private bool Eat()
        {
            if (Playerpos == Foodpos)
            {
                ball += 20;
                GanewrateFood();
                return true;
            }
            return false;
        }
        public bool Move()
        {
            movenumber++;
            if (movenumber > 100)
                return false;
            Vision();
            Vector2 pos = Playerpos;
            Playerpos += Net.Next(Inputs, Weights);
            if (Approximation(pos))
                ball++;
            else
                ball--;
            Eat();
            return true;
        }
        private bool Approximation(Vector2 pos)
        {
            Vector2 vector1 = Foodpos - pos;
            int distance1 = (int)vector1.X ^ 2 + (int)vector1.Y ^ 2;
            Vector2 vector2 = Foodpos - Playerpos;
            int distance2 = (int)vector2.X ^ 2 + (int)vector2.Y ^ 2;
            if (Playerpos.X > Mapsize / 2 || Playerpos.Y > Mapsize / 2 || Playerpos.X < -Mapsize / 2 || Playerpos.Y < -Mapsize / 2)
                ball -= 10;
            if (distance2 < distance1)
                return true;
            return false;
        }
        private void Vision()
        {
            for (int f = 1; f < Game.rov; f++)
            {
                for (int i = 0; i < 8; i++)
                {
                    Inputs[(f - 1) + i * Game.rov] = Comparison(i, f);
                }
            }
        }
        private int Comparison(int i, int f)
        {
            Vector2 inc;
            switch (i)
            {
                case 0:
                    inc = new Vector2(Playerpos.X, Playerpos.Y + f);
                    if (inc == Foodpos)
                        return 10 / f;
                    if (inc.X > Mapsize / 2 || inc.Y > Mapsize / 2 || inc.X < -Mapsize / 2 || inc.Y < -Mapsize / 2)
                        return 1 / f;
                    break;
                case 1:
                    inc = new Vector2(Playerpos.X + f, Playerpos.Y + f);
                    if (inc == Foodpos)
                        return 10 / f;
                    if (inc.X > Mapsize / 2 || inc.Y > Mapsize / 2 || inc.X < -Mapsize / 2 || inc.Y < -Mapsize / 2)
                        return 1 / f;
                    break;
                case 2:
                    inc = new Vector2(Playerpos.X + f, Playerpos.Y);
                    if (inc == Foodpos)
                        return 10 / f;
                    if (inc.X > Mapsize / 2 || inc.Y > Mapsize / 2 || inc.X < -Mapsize / 2 || inc.Y < -Mapsize / 2)
                        return 1 / f;
                    break;
                case 3:
                    inc = new Vector2(Playerpos.X + f, Playerpos.Y - f);
                    if (inc == Foodpos)
                        return 10 / f;
                    if (inc.X > Mapsize / 2 || inc.Y > Mapsize / 2 || inc.X < -Mapsize / 2 || inc.Y < -Mapsize / 2)
                        return 1 / f;
                    break;
                case 4:
                    inc = new Vector2(Playerpos.X, Playerpos.Y - f);
                    if (inc == Foodpos)
                        return 10 / f;
                    if (inc.X > Mapsize / 2 || inc.Y > Mapsize / 2 || inc.X < -Mapsize / 2 || inc.Y < -Mapsize / 2)
                        return 1 / f;
                    break;
                case 5:
                    inc = new Vector2(Playerpos.X - f, Playerpos.Y - f);
                    if (inc == Foodpos)
                        return 10 / f;
                    if (inc.X > Mapsize / 2 || inc.Y > Mapsize / 2 || inc.X < -Mapsize / 2 || inc.Y < -Mapsize / 2)
                        return 1 / f;
                    break;
                case 6:
                    inc = new Vector2(Playerpos.X - f, Playerpos.Y);
                    if (inc == Foodpos)
                        return 10 / f;
                    if (inc.X > Mapsize / 2 || inc.Y > Mapsize / 2 || inc.X < -Mapsize / 2 || inc.Y < -Mapsize / 2)
                        return 1 / f;
                    break;
                case 7:
                    inc = new Vector2(Playerpos.X - f, Playerpos.Y + f);
                    if (inc == Foodpos)
                        return 10 / f;
                    if (inc.X > Mapsize / 2 || inc.Y > Mapsize / 2 || inc.X < -Mapsize / 2 || inc.Y < -Mapsize / 2)
                        return 1 / f;
                    break;
            }
            return 0;
        }
    }
    internal class Net
    {
        public static int InputL;
        public static int HidenL;
        public static int OutputL;
        public Net(int inputL, int outputL)
        {
            InputL = inputL;
            HidenL = (int)(InputL * 0.75);
            OutputL = outputL;
        }
        internal static Vector2 Next(double[] inputs, double[][,] weights)
        {
            double[] Outputs1 = new double[HidenL];
            for (int i = 0; i < Outputs1.Length; i++)
            {
                Outputs1[i] = new Neuron(inputs, weights, TipeNeuron.Hiden, i).Output;
            }
            double[] Outputs2 = new double[OutputL];
            for (int i = 0; i < Outputs2.Length; i++)
            {
                Outputs2[i] = new Neuron(Outputs1, weights, TipeNeuron.Output, i).Output;
            }
            int vec = Outputs2.ToList().IndexOf(Outputs2.Max());
            switch (vec)
            {
                case 0:
                    return new Vector2(0, 1);
                case 1:
                    return new Vector2(1, 0);
                case 2:
                    return new Vector2(0, -1);
                case 3:
                    return new Vector2(-1, 0);
            }
            return new Vector2(0, 0);
        }
    }
    internal class Neuron
    {
        public double Output { get; private set; }
        private TipeNeuron tipe;
        private double[] inputs;
        private double[][,] weights;
        private int I;
        public Neuron(double[] _inputs, double[][,] _weights, TipeNeuron _tipe, int i)
        {
            tipe = _tipe;
            inputs = _inputs;
            weights = _weights;
            I = i;
            Output = Sum();
        }
        private double Sum()
        {
            double sum = 0;
            switch (tipe)
            {
                case TipeNeuron.Hiden:
                    for (int l = 0; l < inputs.Length; ++l)
                        sum += inputs[l] * weights[0][I, l];//линейные
                    return Math.Max(0, sum);
                case TipeNeuron.Output:
                    for (int l = 0; l < inputs.Length; ++l)
                        sum += inputs[l] * weights[1][I, l];//линейные
                    return sum;
            }
            return 0;
        }
    }
}