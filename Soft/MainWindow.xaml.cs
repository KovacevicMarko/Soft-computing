// -----------------------------------------------------------------------
// <copyright file="CommonFunction.cs" company="Njocko">
// Njocko reserves all copyrights.
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Soft
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Atributi
        /// <summary>
        /// Putanja do fajla za citanje.
        /// </summary>
        private readonly string _file;

        /// <summary>
        /// Dictionary koji sadrzi sve entitete.
        /// </summary>
        private Dictionary<int, List<String>> _entiteti;

        /// <summary>
        /// Dictionary koji sadrzi sve kardinalitete izmedju entiteta.
        /// </summary>
        private Dictionary<int, String> _kardinaliteti;

        /// <summary>
        /// Kljuc dictionary-a za 2 entiteta.
        /// </summary>
        private int _intEGenerator;

        /// <summary>
        /// Kljuc dictionary-a za kardinalitete izmedju 2 entiteta.
        /// </summary>
        private int _intKGenerator;

        /// <summary>
        /// Indikator da li je pritisnut klik misem nad entitetom.
        /// </summary>
        bool captured = false;
        
        /// <summary>
        /// Pomocne promenljive za pracenje pozicite entiteta.
        /// </summary>
        double x_shape, x_canvas, y_shape, y_canvas;

        /// <summary>
        /// Pomocna promenljiva za krajnju poziciju entiteta.
        /// </summary>
        UIElement source = null;

        /// <summary>
        /// Dictionary sa svim kreiranim entitetima i njihovim pozicijama.
        /// </summary>
        private Dictionary<int[], string> dinamicki;

        /// <summary>
        /// Dictionary sa svim poveznicima i njihovim poveznicima.
        /// </summary>
        private Dictionary<int[], string> dinamickiVeze;

        /// <summary>
        /// Dictionary sa svim linijama i Hashcodovima objekata koje spajaju.
        /// </summary>
        private Dictionary<Line,String> linije;

        /// <summary>
        /// String lista svih naziva poveznika.
        /// </summary>
        private List<String> modals;

        private int numRel;

        private Dictionary<int, String> linijeHash;

        #endregion Atributi
        #region Konstruktor
        public MainWindow()
        {
            InitializeComponent();
            _intEGenerator = 0;
            _intKGenerator = -1;
            numRel = 0;
            dinamicki = new Dictionary<int[], string>();
            dinamickiVeze = new Dictionary<int[], string>();
            _entiteti = new Dictionary<int, List<String>>();
            _kardinaliteti = new Dictionary<int, String>();
            linije = new Dictionary<Line, string>();
            linijeHash = new Dictionary<int, String>();
            modals = new List<string>();
            _file = String.Format("{0}", System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Output.txt"));
            this.Parsiraj(_file);
            this.Draw();
        }
        #endregion Konsutrktor
        #region Parser
        /// <summary>
        /// Metoda koja parsira ucitani text i izdvaja entitete i kardinalitete.
        /// </summary>
        /// <param name="file">Putanja do fajla za citanje.</param>
        private void Parsiraj(String file)
        {
            string text = System.IO.File.ReadAllText(String.Format(@"{0}", file));
            string[] stringSeparators = new string[] { "('.', '.')" };
            string[] separatorNNP = new string[] { ", 'NNP'" };
            string[] separatorNN = new string[] { ", 'NN'" };
            string[] separatorNNS = new string[] { ", 'NNS'" };
            string[] separatorCD = new string[] { ", 'CD'" };
            string[] separatorNONE = new string[] { ", '-NONE-'" };
            string[] separatorVBN = new string[] { ", 'VBN'" };
            string[] separatorVB = new string[] { ", 'VB'" };
            string[] temp = text.Split(stringSeparators, StringSplitOptions.None);
            string entitet = "";
            string kardinalitet = "";
            for(int i = 0; i < temp.Length; i++)
            {
                List<String> entiteti = new List<String>();
                string[] temp2 = temp[i].Split('(');
                int numCD = 0;
                bool added = false;
                for(int j = 0; j < temp2.Length; j++)
                {
                    if(temp2[j].Contains("NNP"))
                    {
                        entitet = CommonFunction.setUpper(temp2[j].Split(separatorNNP, StringSplitOptions.None)[0]);
                        entiteti.Add(entitet);
                    }else if(temp2[j].Contains("NNS"))
                    {
                        entitet = CommonFunction.setUpper(temp2[j].Split(separatorNNS, StringSplitOptions.None)[0]);
                        entiteti.Add(entitet);
                    }
                    else if(temp2[j].Contains("NN"))
                    {
                        entitet = CommonFunction.setUpper(temp2[j].Split(separatorNN, StringSplitOptions.None)[0]);
                        entiteti.Add(entitet);
                    }
                    else if(temp2[j].Contains("CD"))
                    {
                        string tempKard = temp2[j].Split(separatorCD, StringSplitOptions.None)[0];
                        kardinalitet += tempKard; 
                        numCD++;
                    }
                    else if(temp2[j].Contains("NONE"))
                    {
                        string tempKard = temp2[j].Split(separatorNONE, StringSplitOptions.None)[0];
                        kardinalitet += tempKard;
                        numCD++;
                    }

                    else if(temp2[j].Contains("VB"))
                    {
                        string[] glg = temp2[j].Split(separatorVB, StringSplitOptions.None);
                        
                        for(int k = 0; k < glg.Length; k++)
                        {
                            if(!added)
                            {
                                if((!glg[k].Equals("), ") && (!glg[k].Contains("'are'")) && (!glg[k].Contains("'is'"))))
                                {
                                    modals.Add(CommonFunction.reduceModals(glg[k]));
                                    Console.WriteLine("VB!!!" + CommonFunction.reduceModals(glg[k]) + "a recnija " + i);
                                    added = true;
                                }
                            }
                        }

                    }
                    if(numCD == 2)
                    {
                        numCD = 0;
                        _kardinaliteti.Add(intGenerator(false), kardinalitet);
                        kardinalitet = String.Empty;
                    }
                }

                _entiteti.Add(intGenerator(true), entiteti);
                
            }
        }

        #endregion Parser

        #region Draw

        /// <summary>
        /// Metoda koja vrsi iscrtavanje entiteta.
        /// </summary>
        private void Draw()
        {
            _entiteti = CommonFunction.Reduce(_entiteti);
            int j = 0;
            System.Random r = new System.Random();
            bool napravio = false;
            foreach(KeyValuePair<int, List<String>> ents in _entiteti)
            {
                j++;
                string rhombusName = "";
                string rhombusText = "";
                for(int i = 0; i < ents.Value.Count; i++)
                {
                    bool existEnt = false;
                    rhombusName +=ents.Value.ElementAt(i);
                    rhombusText += ents.Value.ElementAt(i);
                    rhombusName += "AAA";
                    foreach(KeyValuePair<int[], string> grids in dinamicki)
                    {
                        string tempGrid = grids.Value.Split(',')[0];
                        if(tempGrid.Equals(ents.Value.ElementAt(i)))
                        {
                            existEnt = true;
                        }
                    }
                    if(existEnt)
                    {
                        continue;   
                    }
                    var grid = new Grid();
                    grid.MouseLeftButtonDown += grid_MouseLeftButtonDown;
                    grid.MouseMove += grid_MouseMove;
                    grid.MouseLeftButtonUp += grid_MouseLeftButtonUp;
                    grid.Children.Add(new Rectangle()
                    {
                        Stroke = Brushes.CadetBlue,
                        Fill = Brushes.CadetBlue,
                        Width = 100,
                        Height = 60,
                        ToolTip = ents.Value.ElementAt(i),
                        Name = ents.Value.ElementAt(i)

                    });
                    grid.Children.Add(new TextBlock() { Text = ents.Value.ElementAt(i), FontSize = 20 });
                    
                    //int x = 75 *(j+2*i+1);
                    //int y = 75 * (2*i+1);
                    //int x = r.Next(0, 600);
                    //int y = r.Next(0, 200);
                    
                    int x = 30 * j ;
                    int y = 30 * j ;
                    if(napravio)
                    {
                        napravio = false;
                        x = 150 * j;
                        y = 150 * j;

                    }
                    Canvas.SetTop(grid, x);
                    Canvas.SetLeft(grid, y);
                    napravio = true;
                    LayoutRoot.Children.Add(grid);
                    int[] array = new int[2];
                    array[0] = x;
                    array[1] = y;
                    String temp = String.Format("{0},{1}", ents.Value.ElementAt(i), grid.GetHashCode());
                    dinamicki.Add(array, temp);
                     
                }
                if(j != _entiteti.Count)
                {
                    var rhombus = new Grid();
                    rhombus.MouseLeftButtonDown += grid_MouseLeftButtonDown;
                    rhombus.MouseMove += grid_MouseMove;
                    rhombus.MouseLeftButtonUp += grid_MouseLeftButtonUp;
                    Console.WriteLine("J je " + j);
                    string rhmName = modals.ElementAt(j-1);
                    //string rhmName = "aaa";
                    rhombus.Children.Add(new Ellipse
                    {
                        Stroke = Brushes.CadetBlue,
                        Fill = Brushes.CadetBlue,
                        Width = 50,
                        Height = 50,
                        ToolTip = rhombusText,
                        Name = rhombusName

                    });
                    rhombus.Children.Add(new TextBlock() { Text = rhmName, FontSize = 10 });
                    int x = 70 * j;
                    int y = 70 * j;
                    Canvas.SetTop(rhombus, x);
                    Canvas.SetLeft(rhombus, y);
                    LayoutRoot.Children.Add(rhombus);
                    int[] array = new int[2];
                    array[0] = x;
                    array[1] = y;
                    String temp = String.Format("{0}{1}", rhombusName, rhombus.GetHashCode());
                    dinamickiVeze.Add(array, temp);
                }
            }
            string[] stringSeparators = new string[] { "AAA" };
            foreach(KeyValuePair<int[], string> dinVeze in dinamickiVeze)
            {
                string[] veze = dinVeze.Value.Split(stringSeparators, StringSplitOptions.None);

                foreach(KeyValuePair<int[], string> ents in dinamicki)
                {
                    string temp = ents.Value.Split(',')[0];
                    if(veze[0].Equals(temp))
                    {
                        this.createLines(String.Format("{0},{1}", ents.Value, veze[2]),String.Format("{0} to {1}",veze[0],veze[1]), dinVeze.Key, ents.Key);
                    }
                    else if(veze[1].Equals(temp))
                    {
                        this.createLines(String.Format("{0},{1}", ents.Value, veze[2]), String.Format("{1} to {0}", veze[0], veze[1]), dinVeze.Key, ents.Key);
                    }

                }
            }
        }

        /// <summary>
        /// Metoda koja kreira linije izmedju entiteta i poveznika.
        /// </summary>
        /// <param name="first">X i Y kordinate poveznika.</param>
        /// <param name="second">X i Y kordinate entiteta.</param>
        private void createLines(string name, string toolTip, int[] first, int[] second)
        {
            string tempToolTip = "";
            foreach(KeyValuePair<int, string> kard in _kardinaliteti)
            {
                if(kard.Key.Equals(numRel))
                {
                    tempToolTip = CommonFunction.reduceCard(kard.Value);
                }
            }

            string tTip = String.Format("{0} -> {1}", toolTip, tempToolTip);
            string tempName = name.Split(',')[0];           
            Line line = new Line();
            line.MouseLeftButtonDown += line_MouseLeftButtonDown;
            line.Stroke = Brushes.Red;
            line.Fill = Brushes.Red;
            line.ToolTip = tTip;
            line.Name = tempName;
            line.X1 = first.ElementAt(0) + 30;
            line.X2 = second.ElementAt(0) +50;
            line.Y1 = first.ElementAt(1) + 25;
            line.Y2 = second.ElementAt(1) +30;
            LayoutRoot.Children.Add(line);
            string[] temp = name.Split(',');
            string lineTemp = String.Format("{0},{1}", temp[1], temp[2]);
            linije.Add(line, lineTemp);
            linijeHash.Add(line.GetHashCode(), tTip);
            numRel++;
        }

        #endregion Draw

        #region Events

        private void line_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            foreach(KeyValuePair<int, String> entry in linijeHash)
            {
                if(entry.Key.Equals(sender.GetHashCode()))
                {
                    MessageBox.Show(entry.Value, "Cardinalities");            
                }
            }
        }

        /// <summary>
        /// Event koji se aktivira klikom nad entitetom.
        /// </summary>
        /// <param name="sender">Objekat nad kojim se desio dogadjaj</param>
        /// <param name="e">Tip mouse eventa</param>
        private void grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            source = (UIElement)sender;
            Mouse.Capture(source);
            captured = true;
            x_shape = Canvas.GetLeft(source);
            x_canvas = e.GetPosition(LayoutRoot).X;
            y_shape = Canvas.GetTop(source);
            y_canvas = e.GetPosition(LayoutRoot).Y;

        }
        /// <summary>
        /// Event koji se aktivira pomeranje misa ukoliko je prethodno selektovan entitet.
        /// </summary>
        /// <param name="sender">Objekat nad kojim se desio dogadjaj</param>
        /// <param name="e">Tip mouse eventa</param>
        private void grid_MouseMove(object sender, MouseEventArgs e)
        {
            if(captured)
            {
                double x = e.GetPosition(LayoutRoot).X;
                double y = e.GetPosition(LayoutRoot).Y;
                x_shape += x - x_canvas;
                Canvas.SetLeft(source, x_shape);
                x_canvas = x;
                y_shape += y - y_canvas;
                Canvas.SetTop(source, y_shape);
                y_canvas = y;
                UIElement sourcez = null;
                sourcez = (UIElement)sender;
                
                
                foreach (KeyValuePair<Line, string> entry in linije)
                {
                    string[] temp = entry.Value.Split(',');
                    if(sender.GetHashCode().Equals(Int32.Parse(temp[0])))
                    {
                        double x1 = Canvas.GetLeft(sourcez) + 50;
                        double y1 = Canvas.GetTop(sourcez) + 30;
                        setPosition(entry.Key, x1, y1, true);
                    }
                    else if(sender.GetHashCode().Equals(Int32.Parse(temp[1])))
                    {
                        double x1 = Canvas.GetLeft(sourcez) + 25;
                        double y1 = Canvas.GetTop(sourcez) + 25;
                        setPosition(entry.Key, x1, y1, false);
                    }
                }


            }

        }
        /// <summary>
        /// Event koji se aktivira oslobadjanjem klika nad entitetom.
        /// </summary>
        /// <param name="sender">Objekat nad kojim se desio dogadjaj</param>
        /// <param name="e">Tip mouse eventa</param>
        private void grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            captured = false;
            double x = e.GetPosition(LayoutRoot).X;
            double y = e.GetPosition(LayoutRoot).Y;
        }

        /// <summary>
        /// Metoda koja sluzi za izmenu pozicije linije koja spaja entitet i poveznik.
        /// </summary>
        /// <param name="line">Linija kojoj treba podesiti poziciju.</param>
        /// <param name="x">x koordinata linije.</param>
        /// <param name="y">y koordinata linije.</param>
        /// <param name="isEntitet">Indikator da li je u pitanju entitet ili poveznik.</param>
        private void setPosition(Line line, double x, double y, bool isEntitet)
        {
            if(isEntitet)
            {
                line.X2 = x;
                line.Y2 = y;
            }
            else
            {
                line.X1 = x;
                line.Y1 = y;
            }
        }

        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var element = sender as UIElement;
            var position = e.GetPosition(element);
            var transform = element.RenderTransform as MatrixTransform;
            var matrix = transform.Matrix;
            var scale = e.Delta >= 0 ? 1.1 : (1.0 / 1.1); // choose appropriate scaling factor

            matrix.ScaleAtPrepend(scale, scale, position.X, position.Y);
            transform.Matrix = matrix;
        }

        #endregion Events

        #region Generator
        /// <summary>
        /// Metoda koja vraca jedinstveni kljuc dictionary-a za entitete i kardinalitete.
        /// </summary>
        /// <param name="entitet">Indikator da li je entitet ili kardinalitet</param>
        /// <returns>Kljuc dictionary-a</returns>
        private int intGenerator(bool entitet)
        {
            int intReturn = 0;
            if(entitet)
            {
                _intEGenerator++;
                intReturn = _intEGenerator;
            }
            else
            {
                _intKGenerator++;
                intReturn = _intKGenerator;
            }

            return intReturn;
        }
        #endregion Generator
    }
}
