using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiAPR_2
{
    public partial class Form1 : Form
    {
        private const int MIN_POINTS_COUNT = 10;
        private const int MAX_POINTS_COUNT = 100000;
        private const int MIN_CLASS_COUNT = 2;
        private const int MAX_CLASS_COUNT = 128;
        private const int POINTS_WIDTH = 1;
        private const int KERNELS_WIDTH = 10;
        private const int INTEND = 5;    //Border width
        private const int MINAVERAGESQUAREDISTANCE = 5000;                                                                                                                                                                                                                                                                                                                          private const int PRECISION1 = 500;    //Точность 1
        private const int MAX_RGB_COMPONENT_VALUE = 256;                                                                                                                                                                                                                                                                                                                            private const int PRECISION2 = 100;    //Точность 2

        private struct point
        {
            public int x;
            public int y;
            public int Class;
        };

        private struct ClassDistance
        {
            public int Class;
            public double distance;
        };

        private bool IsMaximin;
        private int points_count, class_count;
        private Color[] Colors = new Color[MAX_CLASS_COUNT];
        private point[] Points = new point[MAX_POINTS_COUNT];
        private point[] OldKernels = new point[MAX_CLASS_COUNT];
        private point[] NewKernels = new point[MAX_CLASS_COUNT];
        private point[,] PointsInClasses = new point[MAX_CLASS_COUNT, MAX_POINTS_COUNT];
        private int[] CountPointsInClasses = new int[MAX_CLASS_COUNT];
        private double sumPairDistance = 0;
        private int pairCount = 0;
        private Graphics graphics;
        private BufferedGraphicsContext bufferedGraphicsContext;
        private BufferedGraphics bufferedGraphics;

        public Form1()
        {
            Random rand = new Random();

            InitializeComponent();
            Colors[0] = Color.White;
            Colors[1] = Color.Yellow;
            Colors[2] = Color.Red;
            Colors[3] = Color.Lime;
            Colors[4] = Color.Blue;
            Colors[5] = Color.Aqua;
            Colors[6] = Color.Fuchsia;
            Colors[7] = Color.Orange;
            Colors[8] = Color.Green;
            Colors[9] = Color.Gray;
            for (int i = 10; i < MAX_CLASS_COUNT; i++)
                Colors[i] = newRGBcolor(rand.Next(MAX_RGB_COMPONENT_VALUE),
                                        rand.Next(MAX_RGB_COMPONENT_VALUE),
                                        rand.Next(MAX_RGB_COMPONENT_VALUE));
            graphics = pictureBox1.CreateGraphics();
            bufferedGraphicsContext = new BufferedGraphicsContext();
            bufferedGraphics = bufferedGraphicsContext.Allocate(graphics,
                new Rectangle(0, 0, pictureBox1.Width, pictureBox1.Height));
            ClearForm();
        }

        private Color newRGBcolor(int r, int g, int b)
        {
            Color RGBcolor = new Color();

            RGBcolor = Color.FromArgb(r, g, b);

            return RGBcolor;
        }

        private void ClearForm()
        {
            ClearImage();
            IsMaximin = true;
            textBox1.Text = "100000";
            textBox2.Text = "0";
            points_count = int.Parse(textBox1.Text);
            class_count = int.Parse(textBox2.Text);

            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = false;
            button1.Focus();
        }

        private void ClearImage()
        {
            bufferedGraphics.Graphics.Clear(pictureBox1.BackColor);
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar < '0' || e.KeyChar > '9') && e.KeyChar != 8)
                e.KeyChar = '\0';
        }

        private double EvklidDistance(point a, point b)
        {
            return Math.Sqrt(Math.Pow((a.x - b.x), 2) + Math.Pow((a.y - b.y), 2));
        }

        private int min(int a, int b)
        {
            if (a < b)
                return a;
            else
                return b;
        }

        private void PointsСlassification()
        {
            for (int i = 0; i < class_count; i++)
            {
                OldKernels[i] = NewKernels[i];
                CountPointsInClasses[i] = 0;
            }

            for (int i = 0; i < points_count; i++)   
            {
                ClassDistance[] distances = new ClassDistance[class_count];
                ClassDistance min;

                for (int j = 0; j < class_count; j++)
                {
                    distances[j].Class = j;
                    distances[j].distance = EvklidDistance(Points[i], NewKernels[j]);
                }

                min.Class = distances[0].Class;
                min.distance = distances[0].distance;
                for (int k = 1; k < class_count; k++)
                    if (distances[k].distance < min.distance)
                    {
                        min.Class = distances[k].Class;
                        min.distance = distances[k].distance;
                    }
                Points[i].Class = min.Class;
                CountPointsInClasses[Points[i].Class]++;
            }
        }
        
        private void PointsDistributionIntoClasses()
        {
            for (int i = 0; i < class_count; i++)   
            {
                int TempCountPoints = 0;

                for (int j = 0; j < points_count; j++)
                    if (Points[j].Class == i)
                    {
                        PointsInClasses[i, TempCountPoints] = Points[j];
                        TempCountPoints++;
                    }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (IsMaximin)
            {
                if (class_count == 0)
                {
                    Random rand = new Random();

                    points_count = int.Parse(textBox1.Text);
                    if (points_count < MIN_POINTS_COUNT)
                        points_count = MIN_POINTS_COUNT;
                    if (points_count > MAX_POINTS_COUNT)
                        points_count = MAX_POINTS_COUNT;
                    textBox1.Text = points_count.ToString();

                    for (int i = 0; i < points_count; i++)
                    {
                        Points[i].x = INTEND + rand.Next(pictureBox1.Width - 2 * INTEND);
                        Points[i].y = INTEND + rand.Next(pictureBox1.Height - 2 * INTEND);
                        Points[i].Class = 0;
                        bufferedGraphics.Graphics.FillRectangle(new SolidBrush(Colors[Points[i].Class]),
                            Points[i].x, Points[i].y, POINTS_WIDTH, POINTS_WIDTH);
                    }

                    OldKernels[class_count] = NewKernels[class_count] = Points[rand.Next(points_count)];
                    OldKernels[class_count].Class = NewKernels[class_count].Class = class_count;
                    bufferedGraphics.Graphics.FillEllipse(
                        new SolidBrush(Colors[NewKernels[class_count].Class]),
                        NewKernels[class_count].x - KERNELS_WIDTH/2, 
                        NewKernels[class_count].y - KERNELS_WIDTH/2,
                        KERNELS_WIDTH, KERNELS_WIDTH);
                }

                if (class_count > 0)
                {
                    double[] maxDistances = new double[class_count];
                    int[] newKernelIndexes = new int[class_count];
                    double maxDistance = 0;
                    int newKernelIndex = 0;

                    for (int i = 0; i < class_count; i++)
                    {
                        double maxDistanceInClass = 0;
                        int newKernelIndexInClass = 0;

                        for (int j = 0; j < points_count; j++)
                            if (Points[j].Class == i)
                                if (EvklidDistance(Points[j], NewKernels[i]) > maxDistanceInClass)
                                {
                                    maxDistanceInClass = EvklidDistance(Points[j], NewKernels[i]);
                                    newKernelIndexInClass = j;
                                }

                        maxDistances[i] = maxDistanceInClass;
                        newKernelIndexes[i] = newKernelIndexInClass;
                    }

                    for (int i = 0; i < class_count; i++)
                        if (maxDistances[i] > maxDistance)
                        {
                            maxDistance = maxDistances[i];
                            newKernelIndex = newKernelIndexes[i];
                        }

                    for (int i = 0; i < class_count; i++)
                    {
                        sumPairDistance += EvklidDistance(NewKernels[i], NewKernels[class_count]);
                        pairCount++;
                    }

                    if (maxDistance > sumPairDistance / (pairCount * 2))
                    {
                        OldKernels[class_count] = NewKernels[class_count] = Points[newKernelIndex];
                        OldKernels[class_count].Class = NewKernels[class_count].Class = class_count;
                        bufferedGraphics.Graphics.FillEllipse(new SolidBrush
                            (Colors[NewKernels[class_count].Class]),
                            NewKernels[class_count].x - KERNELS_WIDTH/2,
                            NewKernels[class_count].y - KERNELS_WIDTH/2, 
                            KERNELS_WIDTH, KERNELS_WIDTH);
                    }
                    else
                        IsMaximin = false;
                }

                if (IsMaximin)
                {
                    class_count++;
                    textBox2.Text = class_count.ToString();
                    PointsСlassification();
                    PointsDistributionIntoClasses();
                    ClearImage();   //Draw points with temp kernels
                    for (int i = 0; i < points_count; i++)
                        bufferedGraphics.Graphics.FillRectangle(new SolidBrush(Colors[Points[i].Class]),
                            Points[i].x, Points[i].y, POINTS_WIDTH, POINTS_WIDTH);
                    for (int i = 0; i < class_count; i++)
                        bufferedGraphics.Graphics.FillEllipse(new SolidBrush(Colors[NewKernels[i].Class]),
                            NewKernels[i].x - KERNELS_WIDTH/2, NewKernels[i].y - KERNELS_WIDTH/2, 
                            KERNELS_WIDTH, KERNELS_WIDTH);
                    bufferedGraphics.Render();
                }
                else
                {
                    string caption = "Алгоритм Максимина закончен", message;

                    button1.Enabled = false;
                    button3.Enabled = true;
                    button3.Focus();
                    message = caption + ". Количество классов - " + class_count.ToString();
                    MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ClearForm();
            bufferedGraphics.Render();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int step = 0;
            DateTime start_time = DateTime.Now;
            bool is_class_change = true;
            string caption = "Найдены наилучшие ядра", message;

            while (is_class_change)
            {
                step++;
                is_class_change = false;

                PointsСlassification();
                PointsDistributionIntoClasses();

                for (int i = 0; i < class_count; i++)   //Search best kernels
                {
                    double MinAverageSquareDistance = MINAVERAGESQUAREDISTANCE;
                    int number = -1;

                    for (int j = 0; j < min(CountPointsInClasses[i], PRECISION1); j++)
                    {
                        double SumSquareDistance = 0, TempAverageSquareDistances;

                        for (int k = 0; k < min(CountPointsInClasses[i], PRECISION2); k++)
                            SumSquareDistance += Math.Pow(EvklidDistance(PointsInClasses[i, j],
                                                                         PointsInClasses[i, k]), 2);
                        TempAverageSquareDistances = Math.Sqrt(SumSquareDistance/CountPointsInClasses[i]);
                        if (TempAverageSquareDistances < MinAverageSquareDistance)   //Best kernel was found
                        {
                            MinAverageSquareDistance = TempAverageSquareDistances;
                            number = j;
                        }
                    }
                    if (number != -1)
                        NewKernels[i] = PointsInClasses[i, number];
                }

                for (int i = 0; i < class_count; i++)
                    if ((OldKernels[i].x != NewKernels[i].x) || (OldKernels[i].y != NewKernels[i].y))
                        is_class_change = true;

                ClearImage();   //Draw points with new kernels
                for (int i = 0; i < points_count; i++)
                    bufferedGraphics.Graphics.FillRectangle(new SolidBrush(Colors[Points[i].Class]),
                        Points[i].x, Points[i].y, POINTS_WIDTH, POINTS_WIDTH);
                for (int i = 0; i < class_count; i++)
                    bufferedGraphics.Graphics.FillEllipse(new SolidBrush(Colors[NewKernels[i].Class]),
                        NewKernels[i].x - KERNELS_WIDTH/2, NewKernels[i].y - KERNELS_WIDTH/2, 
                        KERNELS_WIDTH, KERNELS_WIDTH);
                bufferedGraphics.Render();
            }

            DateTime end_time = DateTime.Now;

            message = caption + " за " + (end_time - start_time).Minutes.ToString() + " мин " +
                                         (end_time - start_time).Seconds.ToString() + " с " +
                                         (end_time - start_time).Milliseconds.ToString() + " мс.";
            caption += '!';
            MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);

            button3.Enabled = false;
        }
    }
}
