using life.Infrastructer;
using life.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace life.MainViewModel
{
    class MainViewModel : Notifier
    {

        //за 1 шаг
        #region step 1
        int born = 0;
        int dead = 0;
        int alive = 0;
        public int Born { get=>born; set { born = value;NotifyPropertyChanged(); } } 
        public int Dead { get=>dead; set { dead = value; NotifyPropertyChanged(); } } 
        public int Alive { get=>alive; set { alive = value; NotifyPropertyChanged(); } }
        #endregion
        //максимальное значение
        #region maximum
        int maxBorn = 0;
        int maxDead = 0;
        int maxAlive = 0;
        public int MaxBorn { get => maxBorn; set { maxBorn = value; NotifyPropertyChanged(); } }
        public int MaxDead { get => maxDead; set { maxDead = value; NotifyPropertyChanged(); } }
        public int MaxAlive { get => maxAlive; set { maxAlive = value; NotifyPropertyChanged(); } }
        #endregion
        int step = 0;
        public int Step { get => step; set { step = value; NotifyPropertyChanged(); } }
        //выполнение
        bool progress = true;

        #region properties
        Node selectedNode;
        public Node SelectedNode
        {
            get => selectedNode;
            set
            {
                selectedNode = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        #region collections
        // предыдущий шаг
        ObservableCollection<Node> fieldprev;
        // предпредыдущий шаг
        ObservableCollection<Node> fieldprevprev;
        ObservableCollection<Node> field = new ObservableCollection<Node>();
        public ObservableCollection<Node> Field
        {
            get => field;
            set
            {
                field = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        #region commands
        public ICommand CommandSetNode { get; set; }
        public ICommand CommandStart { get; set; }
        public ICommand CommandStep { get; set; }
        public ICommand CommandPause { get; set; }
        #endregion

        public MainViewModel()
        {
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    Field.Add(new Node() { X = x + 1, Y = y + 1 });
                }
            }
            InitializingCommands();
        }


        void InitializingCommands()
        {
            CommandSetNode = new RelayCommand(x =>
            {
                try
                {
                    var node = (Node)x;
                    if (node.isTaken == false)
                    {
                        node.isTaken = true;
                        node.ColorBrush = Brushes.LimeGreen;
                    }
                    else
                    {
                        node.isTaken = false;
                        node.ColorBrush = Brushes.Black;
                    }
                }
                catch { }
            }
            );
            CommandStart = new RelayCommand(async b => {
                progress = true;
                await StartGame();
                });
            CommandStep = new RelayCommand(async b => {
                await Start();
            });
            CommandPause = new RelayCommand(x => progress = false);

        }

        async Task StartGame()
        {
            while (progress)
            {
                await Task.Delay(400);
                progress = await Start();
            }
        }


        async Task<bool> Start()
        {
            if (progress == true)
            {
                bool change = CheckField();
                if (change == true)
                {
                    int born = 0;
                    int alive = 0;
                    int dead = 0;

                    foreach (Node node in Field)
                    {
                        node.Born = false;
                    }
                    for (int y = 1; y < 9; y++)
                        for (int x = 1; x < 10; x++)
                        {
                            var nodecheck = Field.First(node => node.X == x && node.Y == y);
                            if (nodecheck.isTaken != true)
                            {
                                nodecheck.isTaken = CheckForNeighbors(nodecheck, 1);
                                if (nodecheck.isTaken == true)
                                {
                                    nodecheck.Born = true;
                                    nodecheck.ColorBrush = Brushes.LimeGreen;
                                    born++;
                                }
                            }
                        }
                    for (int y = 1; y < 9; y++)
                        for (int x = 1; x < 10; x++)
                        {
                            var nodecheck = Field.First(node => node.X == x && node.Y == y);
                            if (nodecheck.isTaken == true && nodecheck.Born != true)
                            {
                                nodecheck.isTaken = CheckForNeighbors(nodecheck, 2);
                                if (nodecheck.isTaken == false)
                                {
                                    dead++;
                                    nodecheck.ColorBrush = Brushes.Black;
                                }
                                else
                                    alive++;
                            }
                        }

                    Step++;
                    Born = born;
                    Alive = alive;
                    Dead = dead;
                    if (MaxBorn < Born) MaxBorn = Born;
                    if (MaxAlive < Alive) MaxAlive = Alive;
                    if (MaxDead < Dead) MaxDead = Dead;
                    return true;
                }
                else
                {
                    MessageBox.Show($"Life freezed with no change at move# {Step}");
                    Step = 0;
                    return false;
                }
            }
            else
                return false;
        }

        bool CheckField()
        {
            int nonchangeNodeprev = 0;
            int nonchangeNodeprevprev = 0;
            //сохранение предпередыдущего поля
            if (fieldprev != null)
                foreach (var node in Field)
                    foreach (var nodeprev in fieldprev)
                        if (node.X == nodeprev.X && node.Y == nodeprev.Y)
                            if (node.isTaken == nodeprev.isTaken)
                                nonchangeNodeprev++;
            //сохранение предыдущего поля
            if (fieldprevprev != null)
                foreach (var node in Field)
                    foreach (var nodeprev in fieldprev)
                        if (node.X == nodeprev.X && node.Y == nodeprev.Y)
                            if (node.isTaken == nodeprev.isTaken)
                                nonchangeNodeprevprev++;

            if (nonchangeNodeprev == 72)
                return false;
            if (nonchangeNodeprevprev == 72)
                return false;
            CopyFields();
            return true;
        }

        void CopyFields()
        {
            if (fieldprev != null)
            {
                fieldprevprev = fieldprev;

            }
            fieldprev = new ObservableCollection<Node>();
            foreach (var node in Field)
                fieldprev.Add(new Node { X = node.X, Y = node.Y, isTaken = node.isTaken });
        }
        //1 born 2 alive
        bool CheckForNeighbors(Node node, int bornOrAlive)
        {
            // массив соседей
            Node[] nodes = new Node[8];
            //верхний левый угол
            if (node.X == 1 && node.Y == 1)
            {
                //x54
                //786
                //123
                nodes[0] = Field.First(x => x.X == 9 && x.Y == 1);
                nodes[1] = Field.First(x => x.X == node.X && x.Y == 8);
                nodes[2] = Field.First(x => x.X == node.X + 1 && x.Y == 8);
                nodes[3] = Field.First(x => x.X == 9 && x.Y == node.Y);
                nodes[4] = Field.First(x => x.X == node.X + 1 && x.Y == node.Y);
                nodes[5] = Field.First(x => x.X == 9 && x.Y == node.Y + 1);
                nodes[6] = Field.First(x => x.X == node.X && x.Y == node.Y + 1);
                nodes[7] = Field.First(x => x.X == node.X + 1 && x.Y == node.Y + 1);
            }
            //верхний правый угол
            if (node.X == 9 && node.Y == 1)
            {
                //54х
                //867
                //312
                nodes[0] = Field.First(x => x.X == node.X - 1 && x.Y == 8);
                nodes[1] = Field.First(x => x.X == node.X && x.Y == 8);
                nodes[2] = Field.First(x => x.X == 1 && x.Y == 8);
                nodes[3] = Field.First(x => x.X == node.X - 1 && x.Y == node.Y);
                nodes[4] = Field.First(x => x.X == 1 && x.Y == node.Y);
                nodes[5] = Field.First(x => x.X == node.X - 1 && x.Y == node.Y + 1);
                nodes[6] = Field.First(x => x.X == node.X && x.Y == node.Y + 1);
                nodes[7] = Field.First(x => x.X == 1 && x.Y == node.Y + 1);
            }
            //нижний правый угол
            if (node.X == 9 && node.Y == 8)
            {
                //867
                //312
                //54х
                nodes[0] = Field.First(x => x.X == node.X - 1 && x.Y == node.Y - 1);
                nodes[1] = Field.First(x => x.X == node.X && x.Y == node.Y - 1);
                nodes[2] = Field.First(x => x.X == 1 && x.Y == node.Y - 1);
                nodes[3] = Field.First(x => x.X == node.X - 1 && x.Y == node.Y);
                nodes[4] = Field.First(x => x.X == 1 && x.Y == node.Y);
                nodes[5] = Field.First(x => x.X == node.X - 1 && x.Y == 1);
                nodes[6] = Field.First(x => x.X == node.X && x.Y == 1);
                nodes[7] = Field.First(x => x.X == 1 && x.Y == 1);
            }
            //клетка левый угол
            if (node.X == 1 && node.Y == 8)
            {
                //786
                //231
                //x54
                nodes[0] = Field.First(x => x.X == 8 && x.Y == node.Y - 1);
                nodes[1] = Field.First(x => x.X == node.X && x.Y == node.Y - 1);
                nodes[2] = Field.First(x => x.X == node.X + 1 && x.Y == node.Y - 1);
                nodes[3] = Field.First(x => x.X == 9 && x.Y == node.Y);
                nodes[4] = Field.First(x => x.X == node.X + 1 && x.Y == node.Y);
                nodes[5] = Field.First(x => x.X == 9 && x.Y == 1);
                nodes[6] = Field.First(x => x.X == node.X && x.Y == 1);
                nodes[7] = Field.First(x => x.X == node.X + 1 && x.Y == 1);
            }
            //клетка в центре
            if (node.X > 1 && node.X < 9 && node.Y > 1 && node.Y < 8)
            {
                //123
                //4x5
                //678
                nodes[0] = Field.First(x => x.X == node.X - 1 && x.Y == node.Y - 1);
                nodes[1] = Field.First(x => x.X == node.X && x.Y == node.Y - 1);
                nodes[2] = Field.First(x => x.X == node.X + 1 && x.Y == node.Y - 1);
                nodes[3] = Field.First(x => x.X == node.X - 1 && x.Y == node.Y);
                nodes[4] = Field.First(x => x.X == node.X + 1 && x.Y == node.Y);
                nodes[5] = Field.First(x => x.X == node.X - 1 && x.Y == node.Y + 1);
                nodes[6] = Field.First(x => x.X == node.X && x.Y == node.Y + 1);
                nodes[7] = Field.First(x => x.X == node.X + 1 && x.Y == node.Y + 1);
            }
            //клетка вверху
            if (node.X > 1 && node.X < 9 && node.Y == 1)
            {
                //4x5
                //678
                //123
                nodes[0] = Field.First(x => x.X == node.X - 1 && x.Y == 8);
                nodes[1] = Field.First(x => x.X == node.X && x.Y == 8);
                nodes[2] = Field.First(x => x.X == node.X + 1 && x.Y == 8);
                nodes[3] = Field.First(x => x.X == node.X - 1 && x.Y == 1);
                nodes[4] = Field.First(x => x.X == node.X + 1 && x.Y == 1);
                nodes[5] = Field.First(x => x.X == node.X - 1 && x.Y == node.Y + 1);
                nodes[6] = Field.First(x => x.X == node.X && x.Y == node.Y + 1);
                nodes[7] = Field.First(x => x.X == node.X + 1 && x.Y == node.Y + 1);
            }
            // клетка внизу
            if (node.X > 1 && node.X < 9 && node.Y == 8)
            {
                //678
                //123
                //4x5
                nodes[0] = Field.First(x => x.X == node.X - 1 && x.Y == node.Y - 1);
                nodes[1] = Field.First(x => x.X == node.X && x.Y == node.Y - 1);
                nodes[2] = Field.First(x => x.X == node.X + 1 && x.Y == node.Y - 1);
                nodes[3] = Field.First(x => x.X == node.X - 1 && x.Y == node.Y);
                nodes[4] = Field.First(x => x.X == node.X + 1 && x.Y == node.Y);
                nodes[5] = Field.First(x => x.X == node.X - 1 && x.Y == 1);
                nodes[6] = Field.First(x => x.X == node.X && x.Y == 1);
                nodes[7] = Field.First(x => x.X == node.X + 1 && x.Y == 1);
            }
            //клетка справа
            if (node.X == 9 && node.Y > 1 && node.Y < 8)
            {
                //312
                //54x
                //867
                nodes[0] = Field.First(x => x.X == node.X - 1 && x.Y == node.Y - 1);
                nodes[1] = Field.First(x => x.X == node.X && x.Y == node.Y - 1);
                nodes[2] = Field.First(x => x.X == 1 && x.Y == node.Y - 1);
                nodes[3] = Field.First(x => x.X == node.X - 1 && x.Y == node.Y);
                nodes[4] = Field.First(x => x.X == 1 && x.Y == node.Y);
                nodes[5] = Field.First(x => x.X == node.X - 1 && x.Y == node.Y + 1);
                nodes[6] = Field.First(x => x.X == node.X && x.Y == node.Y + 1);
                nodes[7] = Field.First(x => x.X == 1 && x.Y == node.Y + 1);
            }
            //клетка слева
            if (node.X == 1 && node.Y > 1 && node.Y < 8)
            {
                //231
                //x54
                //786
                nodes[0] = Field.First(x => x.X == 9 && x.Y == node.Y - 1);
                nodes[1] = Field.First(x => x.X == node.X && x.Y == node.Y - 1);
                nodes[2] = Field.First(x => x.X == node.X + 1 && x.Y == node.Y - 1);
                nodes[3] = Field.First(x => x.X == 9 && x.Y == node.Y);
                nodes[4] = Field.First(x => x.X == node.X + 1 && x.Y == node.Y);
                nodes[5] = Field.First(x => x.X == 9 && x.Y == node.Y + 1);
                nodes[6] = Field.First(x => x.X == node.X && x.Y == node.Y + 1);
                nodes[7] = Field.First(x => x.X == node.X + 1 && x.Y == node.Y + 1);
            }
            int neighbourscounter = 0;
            if (bornOrAlive == 2)
            {
                for (int i = 0; i < nodes.Length; i++)
                {
                    if (nodes[i].isTaken == true)
                        neighbourscounter++;
                    //перенаселение
                    if (neighbourscounter > 3)
                        return false;
                }
                //одиночество
                if (neighbourscounter < 2)
                    return false;
                return true;
            }
            if (bornOrAlive == 1)
            {
                for (int i = 0; i < nodes.Length; i++)
                {
                    if (nodes[i].isTaken == true)
                        neighbourscounter++;
                }
                //рождение
                if (neighbourscounter == 3)
                    return true;
                return false;
            }
            return true;
        }
    }
}

