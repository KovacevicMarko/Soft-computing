// -----------------------------------------------------------------------
// <copyright file="CommonFunction.cs" company="Njocko">
// Njocko reserves all copyrights.
// </copyright>
// -----------------------------------------------------------------------

namespace Soft
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Staticka klasa koja sadrzi onsovne funkcije aplikacije.
    /// </summary>
    public static class CommonFunction
    {
        /// <summary>
        /// Metoda koja prima kao parametar enitet, parsira ga i vraca sa velikim pocetnim slovom.
        /// </summary>
        /// <param name="word">Entitet koji se parsira</param>
        /// <returns>Isparsiran entitet sa velikim pocetnim slovom</returns>
        public static String setUpper(string word)
        {
            string[] temp = word.Split('\'');
            return temp[1].First().ToString().ToUpper() + temp[1].Substring(1); 
        }
        /// <summary>
        /// Metoda koja vrsi redukovanje entiteta tako da se 2 puta ne pojavi isti entitet.
        /// </summary>
        /// <param name="dic">Dictionary koji treba da se redukuje</param>
        /// <returns>Redukovan dictionary entiteta</returns>
        public static Dictionary<int, List<String>> Reduce(Dictionary<int, List<String>> dic)
        {
            Dictionary<int, List<String>> temp = new Dictionary<int,List<string>>();

            foreach(KeyValuePair<int, List<String>> ents in dic)
            {
                List<String> tempList = new List<String>();
                try
                {
                    tempList.Add(ents.Value.ElementAt(0));
                    tempList.Add(ents.Value.ElementAt(2));
                }
                catch(ArgumentOutOfRangeException)
                {

                }
                temp.Add(ents.Key, tempList);
            }
            return temp; 
        }
    }
}
/*var rhombus = new Grid();
            //rhombus.MouseLeftButtonDown += grid_MouseLeftButtonDown;
            //rhombus.MouseMove += grid_MouseMove;
            //rhombus.MouseLeftButtonUp += grid_MouseLeftButtonUp;
            rhombus.Children.Add(new Line
            {
                Stroke = Brushes.Red,
                Fill = Brushes.Red,
                ToolTip = "",
                Name = "",
                X1 = first.ElementAt(0),
                X2 = second.ElementAt(0),
                Y1 = first.ElementAt(1),
                Y2 = second.ElementAt(1)

            });
            rhombus.Children.Add(new TextBlock() { Text = "dadaw", FontSize = 10 });
            int x = second.ElementAt(0);
            int y = second.ElementAt(1);
            Canvas.SetTop(rhombus, x);
            Canvas.SetLeft(rhombus, y);
            LayoutRoot.Children.Add(rhombus);*/
