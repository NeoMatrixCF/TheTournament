using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfCC;

namespace TournamentOfHeroes
{
    /// <summary>
    /// Логика взаимодействия для FieldWindow.xaml
    /// </summary>
    public partial class FieldWindow : Window
    {
        public FieldWindow()
        {
            InitializeComponent();

            Field = (BattleField)DataContext ?? new BattleField();

            cells = control.Cells;
        }

        public BattleField Field { get; }

        private readonly BattleCellsCollection cells;
        private HexagonCell? start;
        private HexagonCell? end;
        private const int delay = 50;

        private bool isExecute = false;
        private async void OnHexagonsExecuteAsync(object sender, ExecutedRoutedEventArgs e)
        {
            isExecute = true;
            HexagonCell source = (HexagonCell)e.OriginalSource;
            if (start == null)
            {
                start = source;
                start.IsHighlighted = true;
            }
            else
            {
                end = source;

                ReadOnlyCollection<Cell>? path = ((Cell)start.Content).GetPath((Cell)end.Content);

                if (path != null)
                {
                    foreach (var cell in path)
                    {
                        cells[cell.Index].IsHighlighted = true;
                        await Task.Delay(delay);
                    }
                    foreach (var cell in path)
                    {
                        cells[cell.Index].IsHighlighted = false;
                        await Task.Delay(delay);
                    }
                }
                start.IsHighlighted = false;
                end.IsHighlighted = true;
                start = end;
            }
            isExecute = false;
        }

        private void OnCanHexagonsExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !isExecute;
        }
    }
}
