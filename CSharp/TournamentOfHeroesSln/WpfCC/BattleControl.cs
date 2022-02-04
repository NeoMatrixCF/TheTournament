using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TournamentOfHeroes;

namespace WpfCC
{
    [StyleTypedProperty(Property = nameof(ItemContainerStyle), StyleTargetType = typeof(HexagonCell))]
    public class BattleControl : ItemsControl
    {
        static BattleControl()
        {
            // Ключ шаблона по умолчанию
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BattleControl), new FrameworkPropertyMetadata(typeof(BattleControl)));


            // Значение по умолчанию для свойства ItemsPanelProperty
            ItemsPanelTemplate template = new(new FrameworkElementFactory(typeof(Canvas)));
            template.Seal();
            ItemsPanelProperty.OverrideMetadata(typeof(BattleControl), new FrameworkPropertyMetadata(template));
        }
        public BattleControl()
        {
            centreBinding = new MultiBinding()
            {
                Converter = CellToCenterConverter.Instance
            };
            centreBinding.Bindings.Add(new Binding() { Path = new PropertyPath(CellSizeProperty), Source = this });
            centreBinding.Bindings.Add(new Binding() { Path = new PropertyPath(ContentControl.ContentProperty), RelativeSource = RelativeSource.Self });

            widthBinding = new Binding()
            {
                Path = new PropertyPath($"(0).{nameof(Size.Width)}", CellSizeProperty),
                Source = this
            };
            heightBinding = new Binding()
            {
                Path = new PropertyPath($"(0).{nameof(Size.Height)}", CellSizeProperty),
                Source = this
            };


            Cells = new(cells);

            LayoutUpdated += (_, _) => CellSize = new Size
            (
                ActualWidth  / 16,
                ActualHeight / 8.5
            );
        }




        // Ограничение источника
        protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);

            if (newValue != null && newValue is not BattleField)
                throw new ArgumentException("Только для BattleField.", nameof(newValue));
            cells.Clear();
        }

        // Задание UI элемента для контента
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new HexagonCell();
        }

        /// <summary>
        /// Размер ячейки.
        /// </summary>
        public Size CellSize
        {
            get { return (Size)GetValue(CellSizeProperty); }
            private set { SetValue(CellSizePropertyKey, value); }
        }

        private static readonly DependencyPropertyKey CellSizePropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(CellSize), typeof(Size), typeof(BattleControl), new PropertyMetadata(new Size()));
        /// <summary><see cref="DependencyProperty"/> для свойства <see cref="CellSize"/>.</summary>
        public static readonly DependencyProperty CellSizeProperty = CellSizePropertyKey.DependencyProperty;


        private readonly ObservableCollection<HexagonCell> cells = new();


        /// <summary>
        /// Коллекция UI ячеек поля боя.
        /// </summary>
        public BattleCellsCollection Cells
        {
            get { return (BattleCellsCollection)GetValue(CellsProperty); }
            private set { SetValue(CellsPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey CellsPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(Cells), typeof(BattleCellsCollection), typeof(BattleControl), new PropertyMetadata(null));
        /// <summary><see cref="DependencyProperty"/> для свойства <see cref="Cells"/>.</summary>
        public static readonly DependencyProperty CellsProperty = CellsPropertyKey.DependencyProperty;

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            HexagonCell? cell = (HexagonCell)element;
            Cell? itemCell = (Cell)item;

            _ = cell.SetBinding(WidthProperty, widthBinding);
            _ = cell.SetBinding(HeightProperty, heightBinding);
            _ = cell.SetBinding(HexagonCell.CentreProperty, centreBinding);
            int i;
            for (i = 0; i < cells.Count; i++)
            {
                var content = (Cell)cells[i].Content;
                if (content.Index == itemCell.Index)
                {
                    cells[i] = cell;
                    break;
                }
                else if (content.Index > itemCell.Index)
                {
                    cells.Insert(i, cell);
                    break;
                }
            }
            if (i >= cells.Count)
            {
                cells.Add(cell);
            }
        }

        private readonly MultiBinding centreBinding;
        private readonly Binding widthBinding;
        private readonly Binding heightBinding;
    }

    public class CellToCenterConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Size size = (Size)values[0];
            Cell cell = (Cell)values[1];

            double x = (cell.Column + 0.5) * size.Width;
            if (cell.Row % 2 == 0)
                x += 0.5 * size.Width;

            double y = (cell.Row * 0.75 + 0.5) * size.Height;

            return new Point(x, y);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static CellToCenterConverter Instance { get; } = new();
    }

    public class BattleCellsCollection : ReadOnlyObservableCollection<HexagonCell>
    {
        public BattleCellsCollection(ObservableCollection<HexagonCell> list) : base(list)
        {
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            base.OnCollectionChanged(args);
            OnPropertyChanged(Items2DArgs);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            base.OnPropertyChanged(args);
        }

        private static readonly PropertyChangedEventArgs Items2DArgs = new PropertyChangedEventArgs("Items[,]");
        public HexagonCell this[int row, int column] => this[BattleField.GetIndex(row, column)];
    }
}
