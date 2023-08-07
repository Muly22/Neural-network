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
            var game = new Game(new VideoMode(800, 600), "Test");
        }
    }

    internal class Game : RenderWindow
    {
        private RenderWindow window;
        private uint Width;
        private uint Height;
        private int cellX;
        private int cellY;
        private Population Population { get; set; }

        public static int nos = 32;
        public static int rov = 4;
        public static int Mapsize = 10;
        public Game(VideoMode mode, string title) : base(mode, title, Styles.Titlebar | Styles.Close)
        {
            window = this;
            Width = window.Size.X; Height = window.Size.Y;
            cellX = (int)(Width / Mapsize);
            cellY = (int)(Height / Mapsize);
            new Net(8 * rov + 1, 2);
            Population = new Population();
            Population.bft = new Type();
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
                    fore_best[i] = Population.population[i].FoodEaten;
                }
                Draw(fore_best.Max());
            } while (end);
            int max_val = fore_best.Max();
            if (Population.bft.FoodEaten < max_val)
                Population.bft = Population.population[fore_best.ToList().IndexOf(max_val)];
            Population.Ress();
        }
        private void Draw(int max)
        {
            window.DispatchEvents();
            window.Clear();
            Vertex[] linesX = new Vertex[Mapsize * 2];
            for (int x = 0; x < Mapsize * 2 - 1; x += 2)
            {
                linesX[x] = new Vertex(new Vector2f(cellX / 2 * x, 0), Color.White);
                linesX[x + 1] = new Vertex(new Vector2f(cellX / 2 * x, Height), Color.White);
            }
            window.Draw(linesX, PrimitiveType.Lines);
            Vertex[] linesY = new Vertex[Mapsize * 2];
            for (int y = 0; y < Mapsize * 2 - 1; y += 2)
            {
                linesY[y] = new Vertex(new Vector2f(0, cellY / 2 * y), Color.White);
                linesY[y + 1] = new Vertex(new Vector2f(Width, cellY / 2 * y), Color.White);
            }
            window.Draw(linesY, PrimitiveType.Lines);


            for (int i = 0; i < Population.population.Length; i++)
            {
                Vertex[] food = new Vertex[] { new Vertex(new Vector2f(0, 1), Color.Green),
                                               new Vertex(new Vector2f(cellX - 1, 1), Color.Green),
                                               new Vertex(new Vector2f(cellX - 1, cellY), Color.Green),
                                               new Vertex(new Vector2f(0, cellY), Color.Green) };

                Vertex[] cube = new Vertex[] { new Vertex(new Vector2f(0, 1), Color.Red),
                                               new Vertex(new Vector2f(cellX - 1, 1), Color.Red),
                                               new Vertex(new Vector2f(cellX - 1, cellY), Color.Red),
                                               new Vertex(new Vector2f(0, cellY), Color.Red) };
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
            window.SetTitle($"best: {max}");
            if (max > 1)
                Console.WriteLine($"iteration: {Population.iteration} best: {max} best: {Population.bft.Weights[0][0, 0]}");
            Thread.Sleep(10);
            window.Display();
        }
        private void RendDraw()
        {
            window.SetMouseCursorVisible(false);
            window.LostFocus += Window_LostFocus;
            window.GainedFocus += Window_GainedFocus;
            window.Closed += Window_Closed;
            while (window.IsOpen)
            {
                Iteration();
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
        public Type bft;

        public Population()
        {
            population = new Type[Game.nos];
            for (int i = 0; i < population.Length; i++)
            {
                population[i] = new Type();
            }
        }
        public void Ress()
        {
            for (int i = 0; i < population.Length; i++)
            {
                population[i] = new Type(bft.Mutation(bft.Weights, iteration));
            }
        }
    }
    internal class Type
    {
        public int FoodEaten;
        private int movenumber;
        public Vector2 Playerpos;
        public Vector2 Foodpos;
        private int Mapsize;
        private Random random;
        private int[] Inputs;
        public decimal[][,] Weights;
        public Type()
        {
            Mapsize = Game.Mapsize;
            Inputs = new int[Net.InputL];
            Inputs[Inputs.Length - 1] = 1;
            Weights = new decimal[2][,];
            Weights[0] = new decimal[Net.HidenL, Net.InputL];
            Weights[1] = new decimal[Net.OutputL, Net.HidenL];
            for (int i = 0; i < Weights.Length; i++)
            {
                for (int j = 0; j < Weights[i].GetLength(0); j++)
                {
                    for (int m = 0; m < Weights[i].GetLength(1); m++)
                    {
                        Weights[i][j, m] = 0.5m;
                    }
                }
            }
            random = new Random();
            GanewrateFood();
        }

        public Type(decimal[][,] weights)
        {
            Inputs = new int[Net.InputL];
            Inputs[Inputs.Length - 1] = 1;
            Mapsize = Game.Mapsize;
            Weights = weights;
            random = new Random();
            GanewrateFood();
        }
        public decimal[][,] Mutation(decimal[][,] weights, ulong I)
        {
            for (int i = 0; i < weights.Length; i++)
            {
                for (int j = 0; j < Weights[i].GetLength(0); j++)
                {
                    for (int m = 0; m < Weights[i].GetLength(1); m++)
                    {
                        weights[i][j, m] += random.Next(-1, 1) * 0.001m;
                    }
                }
            }
            return weights;
        }
        private void GanewrateFood()
        {
            Foodpos = new Vector2(random.NextInt64(-Mapsize / 2, Mapsize / 2), random.NextInt64(-Mapsize / 2, Mapsize / 2));
        }
        private bool Eat()
        {
            if (Playerpos == Foodpos)
            {
                FoodEaten++;
                GanewrateFood();
                return true;
            }
            return false;
        }
        public bool Move()
        {
            movenumber++;
            if (movenumber > 25)
                return false;
            Vision();
            Playerpos += Net.Next(Inputs, Weights);
            Eat();
            return true;
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
                        return -1;
                    if (inc.X > Mapsize / 2 || inc.Y > Mapsize / 2 || inc.X < -Mapsize / 2 || inc.Y < -Mapsize / 2)
                        return 1;
                    break;
                case 1:
                    inc = new Vector2(Playerpos.X + f, Playerpos.Y + f);
                    if (inc == Foodpos)
                        return -1;
                    if (inc.X > Mapsize / 2 || inc.Y > Mapsize / 2 || inc.X < -Mapsize / 2 || inc.Y < -Mapsize / 2)
                        return 1;
                    break;
                case 2:
                    inc = new Vector2(Playerpos.X + f, Playerpos.Y);
                    if (inc == Foodpos)
                        return -1;
                    if (inc.X > Mapsize / 2 || inc.Y > Mapsize / 2 || inc.X < -Mapsize / 2 || inc.Y < -Mapsize / 2)
                        return 1;
                    break;
                case 3:
                    inc = new Vector2(Playerpos.X + f, Playerpos.Y - f);
                    if (inc == Foodpos)
                        return -1;
                    if (inc.X > Mapsize / 2 || inc.Y > Mapsize / 2 || inc.X < -Mapsize / 2 || inc.Y < -Mapsize / 2)
                        return 1;
                    break;
                case 4:
                    inc = new Vector2(Playerpos.X, Playerpos.Y - f);
                    if (inc == Foodpos)
                        return -1;
                    if (inc.X > Mapsize / 2 || inc.Y > Mapsize / 2 || inc.X < -Mapsize / 2 || inc.Y < -Mapsize / 2)
                        return 1;
                    break;
                case 5:
                    inc = new Vector2(Playerpos.X - f, Playerpos.Y - f);
                    if (inc == Foodpos)
                        return -1;
                    if (inc.X > Mapsize / 2 || inc.Y > Mapsize / 2 || inc.X < -Mapsize / 2 || inc.Y < -Mapsize / 2)
                        return 1;
                    break;
                case 6:
                    inc = new Vector2(Playerpos.X - f, Playerpos.Y);
                    if (inc == Foodpos)
                        return -1;
                    if (inc.X > Mapsize / 2 || inc.Y > Mapsize / 2 || inc.X < -Mapsize / 2 || inc.Y < -Mapsize / 2)
                        return 1;
                    break;
                case 7:
                    inc = new Vector2(Playerpos.X - f, Playerpos.Y + f);
                    if (inc == Foodpos)
                        return -1;
                    if (inc.X > Mapsize / 2 || inc.Y > Mapsize / 2 || inc.X < -Mapsize / 2 || inc.Y < -Mapsize / 2)
                        return 1;
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
        internal static Vector2 Next(int[] inputs, decimal[][,] weights)
        {
            var Hiden = new Neuron[HidenL];
            decimal[] Outputs = new decimal[HidenL];
            for (int i = 0; i < Hiden.Length; i++)
            {
                Hiden[i] = new Neuron(inputs, weights, TipeNeuron.Hiden,i);
                Outputs[i]= Hiden[i].Output;
            }
            return new Vector2((float)Math.Round( new Neuron(Outputs, weights, TipeNeuron.Output, 0).Output), (float)Math.Round(new Neuron(Outputs, weights, TipeNeuron.Output, 1).Output));
        }
    }

    internal class Neuron
    {
        public decimal Output { get; private set; }
        private TipeNeuron tipe;
        private int[] inputs;
        private decimal[] outputs;
        private decimal[][,] weights;
        private int I;
        public Neuron(int[] _inputs, decimal[][,] _weights, TipeNeuron _tipe, int i)
        {
            tipe = _tipe;
            inputs = _inputs;
            weights = _weights;
            I = i;
            Output = Sum();
        }
        public Neuron(decimal[] _outputs, decimal[][,] _weights, TipeNeuron _tipe, int i)
        {
            tipe = _tipe;
            outputs = _outputs;
            weights = _weights;
            I = i;
            Output = Sum();
        }
        private decimal Sum()
        {
            decimal sum = 0;
            switch (tipe)
            {
                case TipeNeuron.Hiden:
                    for (int l = 0; l < inputs.Length; ++l)
                        sum += inputs[l] * weights[0][I, l];//линейные
                    return sum;
                case TipeNeuron.Output:
                    for (int l = 0; l < outputs.Length; ++l)
                        sum += outputs[l] * weights[1][I, l];//линейные
                    return (decimal)Math.Tanh((double)sum);
                default:
                    break;
            }
            return 0;
        }
    }
}