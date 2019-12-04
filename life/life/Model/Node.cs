using life.Infrastructer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace life.Model
{
    class Node: Notifier
    {
        int neighborsMin = 2;
        int neighborsMax = 3;
        int neighborsBornNew = 3;
        bool Changed = false;
        private Brush BlackBrush = Brushes.Black;
        private Brush LimeBrush = Brushes.LimeGreen;
        public int X { get; set; }
        public int Y { get; set; }
        Brush colorBrush;
        public Brush ColorBrush
        { get=> colorBrush;
            set
            {
                colorBrush = value;
                NotifyPropertyChanged();
            }
        }
        //занята
        public bool isTaken { get; set; } = false;
        public bool StayAlive { get; set; } = false;
        //родилась на этом ходе
        public bool Born { get; set; } = false;
        public Node()
        {
            ColorBrush = Brushes.Blue;
        }
    }
}
