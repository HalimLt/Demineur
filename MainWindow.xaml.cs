using System;
using System.Data.Common;
using System.Linq;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MinesweeperWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int gridSize = 10; // taille de la grille
        private int nbMine = 10; // nombre de bombes
        private int nbCellOpen = 0; // nombre de cellules qui ont été vérifiées, ouvertes
        private int[,] matrix; // matrice conservant les valeurs de la grille (voir ci-dessous)
        private TextBox textBoxSize;
        private TextBox textBoxMine;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MyWindow_Loaded(object sender, RoutedEventArgs e)
        {
            backToLobby();
        }
        private void backToLobby()
        {
            GRDGame.Children.Clear();
            GRDGame.ColumnDefinitions.Clear();
            GRDGame.RowDefinitions.Clear();

            Label labelTitre = new Label();
            labelTitre.Content = "Créer votre partie :";
            labelTitre.Width = 300;
            labelTitre.Height = 50;
            labelTitre.Margin = new Thickness(0, 0, 273, 200);
            labelTitre.FontSize = 30;
            labelTitre.FontFamily = new FontFamily("Arial Rounded MT Bold");

            Label labelSize = new Label();
            labelSize.Content = "Taille du terrain :";
            labelSize.Width = 250;
            labelSize.Height = 40;
            labelSize.Margin = new Thickness(0, 0, 190, 90);
            labelSize.FontSize = 25;
            labelSize.FontFamily = new FontFamily("Arial Rounded MT Bold");

            Label labelMine = new Label();
            labelMine.Content = "Nombre de bombes :";
            labelMine.Width = 260;
            labelMine.Height = 40;
            labelMine.Margin = new Thickness(0, 0, 273, 0);
            labelMine.FontSize = 25;
            labelMine.FontFamily = new FontFamily("Arial Rounded MT Bold");
            labelMine.Background = new SolidColorBrush(Color.FromArgb(255,162, 209, 73));

            textBoxSize = new TextBox();
            textBoxSize.Width = 40;
            textBoxSize.Height = 30;
            textBoxSize.Margin = new Thickness(40, 0, 0, 85);
            textBoxSize.FontSize = 20;

            textBoxMine = new TextBox();
            textBoxMine.Width = 40;
            textBoxMine.Height = 30;
            textBoxMine.Margin = new Thickness(40, 5, 0, 0);
            textBoxMine.FontSize = 20;

            Button buttonStart = new Button();
            buttonStart.Content = "Lancer la partie";
            buttonStart.Click += ButtonStart_Click;
            buttonStart.Width = 175;
            buttonStart.Height = 40;
            buttonStart.Margin = new Thickness(0, 250, 0,0);
            buttonStart.FontSize = 20;
            buttonStart.FontFamily = new FontFamily("Arial Rounded MT Bold");
            GRDGame.Children.Add(labelTitre);
            GRDGame.Children.Add(buttonStart);
            GRDGame.Children.Add(labelMine);
            GRDGame.Children.Add(labelSize);
            GRDGame.Children.Add(textBoxSize);
            GRDGame.Children.Add(textBoxMine);
        }
        private void ButtonStart_Click(object sender, RoutedEventArgs e) {
            if (int.TryParse(textBoxSize.Text, out int gridSizeWanted) && int.TryParse(textBoxMine.Text, out int nbMinewanted) && gridSizeWanted>0 && nbMinewanted>0)
            {
                if (gridSizeWanted*gridSizeWanted <= nbMinewanted) 
                    MessageBox.Show("Le nombre de cases dans le terrain est inférieur au nombre de mines !");
                else
                    gridSize=gridSizeWanted;
                    nbMine = nbMinewanted;
                    createGame();
            }
            else
            {
                MessageBox.Show("Seul des chiffres doivent être entrés !");
            }
        }

        private void createGame()
        {
            matrix = new int[gridSize, gridSize];
            for (int i=0; i<gridSize; i++)
            {
                for (int j=0; j<gridSize; j++)
                {
                    matrix[i, j] = 0;
                }
            }
            int numberOfCases = gridSize * gridSize;
            nbCellOpen = 0;
            GRDGame.Children.Clear();
            GRDGame.ColumnDefinitions.Clear();
            GRDGame.RowDefinitions.Clear();
            for (int i = 0; i < gridSize; i++)
            {
                GRDGame.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                GRDGame.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            }

            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    Border b = new Border();
                    //b.BorderThickness = new Thickness(1);
                    //b.BorderBrush = new SolidColorBrush(Colors.LightBlue);
                    //b.Width = 50;
                    //b.Height = 50;  
                    b.SetValue(Grid.RowProperty, j);
                    b.SetValue(Grid.ColumnProperty, i);
                    GRDGame.Children.Add(b);


                    Label label = new Label();
                    label.FontSize = 20;
                    label.FontFamily = new FontFamily("Arial Rounded MT Bold");
                    label.HorizontalContentAlignment = HorizontalAlignment.Center;
                    label.VerticalContentAlignment = VerticalAlignment.Center;
                    Button button = new Button();
                    //button.Width = 50;
                    //button.Height = 50;
                    button.Click += Button_Click;
                    button.BorderThickness = new Thickness(0);
                    if((i%2==0 && j%2 == 0) || (i%2 != 0 && j%2 != 0))
                    {
                        button.Background = new SolidColorBrush(Color.FromArgb(255, 170, 215, 81));
                        label.Background = new SolidColorBrush(Color.FromArgb(255, 229, 194, 159));
                    }
                    else
                    {
                        button.Background = new SolidColorBrush(Color.FromArgb(255, 162, 209, 73));
                        label.Background = new SolidColorBrush(Color.FromArgb(255, 215, 184, 153));
                    }
                    Grid myGrid = new Grid();
                    myGrid.Children.Add(label);
                    myGrid.Children.Add(button);
                    b.Child = myGrid;
                }
            }
            generateBomb();
        }

        private void generateBomb()
        {
            Random rnd = new Random();
            for (int i=0 ; i < nbMine; i++)
            {
                int row = rnd.Next(0, gridSize);
                int column = rnd.Next(0, gridSize);
                while (matrix[row, column] == -1 )
                {
                    row = rnd.Next(0, gridSize);
                    column = rnd.Next(0, gridSize);
                }
                matrix[column, row] = -1;

                for (int j = Math.Max(0, column - 1); j <= Math.Min(gridSize - 1, column + 1); j++)
                {
                    for (int k = Math.Max(0, row - 1); k <= Math.Min(gridSize - 1, row + 1); k++)
                    {
                        if (matrix[j, k] != -1)
                            matrix[j, k]++;
                    }
                }
                /*
                
                if (column + 1 < gridSize) {
                    matrix[column + 1, row]++;
                    if (row + 1 < gridSize) {
                        matrix[column + 1, row + 1]++;
                    }
                    if (row - 1 >= 0){
                        matrix[column + 1, row - 1]++;
                    }
                }

               if (column - 1 >= 0){
                    matrix[column -1, row]++;
                    if (row + 1 < gridSize)
                    {
                        matrix[column - 1, row + 1] ++;
                    }
                    if (row - 1 >= 0)
                    {
                        matrix[column - 1, row - 1] ++;
                    }
                }
                if (row + 1 < gridSize)
                {
                    matrix[column, row + 1] ++;
                }
                if (row - 1 >= 0)
                {
                    matrix[column, row - 1] ++;
                }
                */


            }
        }

        public void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Border b = (Border)VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(button));
            int row = Grid.GetRow(b);
            int col = Grid.GetColumn(b);
            Boolean exe = verifieCellule(col, row);
        }
            private UIElement GetUIElementFromPosition(Grid g, int col, int row)
        {
            return g.Children.Cast<UIElement>().First(e => Grid.GetRow(e) == row && Grid.GetColumn(e) == col);
        }

        public Boolean verifieCellule(int column, int row)
        {
            
            UIElement GRDGameChild = GetUIElementFromPosition(GRDGame, column, row);
            Border border = (Border)GRDGameChild;
            Grid grid = (Grid) border.Child;
            Button button = (Button) grid.Children[1];
                            
            if (button.Visibility == Visibility.Visible)
            {
                button.Visibility = Visibility.Hidden;
                
                if (matrix[column, row]!=0)
                {
                    Label label = (Label)grid.Children[0];
                    label.Content = matrix[column, row].ToString();
                    if (matrix[column, row] == 1) label.Foreground = new SolidColorBrush(Color.FromArgb(255, 25, 118, 210));
                    if (matrix[column, row] == 2) label.Foreground = new SolidColorBrush(Color.FromArgb(255, 56, 142, 60));
                    if (matrix[column, row] == 3) label.Foreground = new SolidColorBrush(Color.FromArgb(255, 211, 47, 47));
                    if (matrix[column, row] == 4) label.Foreground = new SolidColorBrush(Color.FromArgb(255, 123, 31, 162));
                }
                if (matrix[column, row] == -1)
                {
                    gameLose();
                    return true;
                }
                else
                {
                    nbCellOpen++;
                    if (nbCellOpen == gridSize * gridSize - nbMine)
                    {
                        gameWin();
                        return true;
                    }
                    else if (matrix[column, row] == 0)
                    {
                        for (int i = Math.Max(0, column - 1); i <= Math.Min(gridSize-1, column + 1); i++)
                        {
                            for (int j = Math.Max(0, row - 1); j <= Math.Min(gridSize - 1, row + 1); j++)
                            {
                                Boolean resultat = verifieCellule(i, j);
                                if (resultat==true) return true;
                            }
                        }
                    }
                }
            }
            return false;

        }
        public void gameWin()
        {
            MessageBox.Show("Gagné !");
            backToLobby();
        }
        public void gameLose()
        {
            MessageBox.Show("Perdu");
            backToLobby();
        }


    }
}


/*
POUR i de Max(0, column-1) à Min(tailleGrille -1, column+1) {
POUR j de Max(0, row-1) à Min(tailleGrille -1, row+1){


                
*/