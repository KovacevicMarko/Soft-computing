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
            string[] separatorWBG = new string[] { ", 'WBG'" };
            string[] temp = text.Split(stringSeparators, StringSplitOptions.None);
            Console.WriteLine(temp[0]);
            string entitet = "";
            string kardinalitet = "";
            for(int i = 0; i < temp.Length; i++)
            {
                List<String> entiteti = new List<String>();
                string[] temp2 = temp[i].Split('(');
                int numCD = 0;
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
                        Console.WriteLine("KARD JE" + kardinalitet);
                        numCD++;
                    }
                    else if(temp2[j].Contains("WBG"))
                    {
                        string[] glg = temp2[j].Split(separatorWBG, StringSplitOptions.None);
                        for(int k = 0; k < glg.Length; k++)
                        {
                            if((!glg[k].Equals("are")) && (!glg[k].Equals("is")))
                            {
                                if(!modals.Contains(glg[k]))
                                {
                                    modals.Add(glg[k]);
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
                    System.Random r = new System.Random();
                    r.Next();
                    int x = 75 *(j+2*i+1);
                    int y = 75 * (2*i+1);
                    Canvas.SetTop(grid, x);
                    Canvas.SetLeft(grid, y);
                    LayoutRoot.Children.Add(grid);
                    int[] array = new int[2];
                    array[0] = x;
                    array[1] = y;
                    String temp = String.Format("{0},{1}", ents.Value.ElementAt(i), grid.GetHashCode());
                    Console.WriteLine(temp);
                    dinamicki.Add(array, temp);
                     
                }
                if(j != _entiteti.Count)
                {
                    var rhombus = new Grid();
                    rhombus.MouseLeftButtonDown += grid_MouseLeftButtonDown;
                    rhombus.MouseMove += grid_MouseMove;
                    rhombus.MouseLeftButtonUp += grid_MouseLeftButtonUp;
                    rhombus.Children.Add(new Ellipse
                    {
                        Stroke = Brushes.CadetBlue,
                        Fill = Brushes.CadetBlue,
                        Width = 50,
                        Height = 50,
                        ToolTip = rhombusText,
                        Name = rhombusName

                    });
                    rhombus.Children.Add(new TextBlock() { Text = rhombusText, FontSize = 10 });
                    int x = 200 * j;
                    int y = 200 * j;
                    Canvas.SetTop(rhombus, x);
                    Canvas.SetLeft(rhombus, y);
                    LayoutRoot.Children.Add(rhombus);
                    int[] array = new int[2];
                    array[0] = x;
                    array[1] = y;
                    String temp = String.Format("{0}{1}", rhombusName, rhombus.GetHashCode());
                    Console.WriteLine(temp);
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
                        this.createLines(String.Format("{0},{1}", ents.Value, veze[2]), dinVeze.Key, ents.Key);
                    }
                    else if(veze[1].Equals(temp))
                    {
                        this.createLines(String.Format("{0},{1}", ents.Value, veze[2]), dinVeze.Key, ents.Key);
                    }

                }
            }
        }

        /// <summary>
        /// Metoda koja kreira linije izmedju entiteta i poveznika.
        /// </summary>
        /// <param name="first">X i Y kordinate poveznika.</param>
        /// <param name="second">X i Y kordinate entiteta.</param>
        private void createLines(string name, int[] first, int[] second)
        {
            string tempToolTip = "";
            foreach(KeyValuePair<int, string> kard in _kardinaliteti)
            {
                if(kard.Key.Equals(numRel))
                {
                    tempToolTip = kard.Value;
                }
            }
            Console.WriteLine(
                "NAME je " + name,
                "Prvi je " + first.ElementAt(0) +
                "Drugi je " + second.ElementAt(0) +
                "Treci je " + first.ElementAt(1) +
                "Cetvrti je" + second.ElementAt(1));
            

            string tempName = name.Split(',')[0];

            Line line = new Line();
            line.Stroke = Brushes.Red;
            line.Fill = Brushes.Red;
            line.ToolTip = tempToolTip;
            line.Name = tempName;
            line.X1 = first.ElementAt(0) + 30;
            line.X2 = second.ElementAt(0) + 20;
            line.Y1 = first.ElementAt(1) + 25;
            line.Y2 = second.ElementAt(1) + 100;
            LayoutRoot.Children.Add(line);
            string[] temp = name.Split(',');
            string lineTemp = String.Format("{0},{1}", temp[1], temp[2]);
            linije.Add(line, lineTemp);
            numRel++;
        }

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

        #endregion Draw

        #region Events

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
