using System;
using System.Collections;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TournamentOfHeroes;

namespace WpfCC
{
    // TODO: Добавить обязательный кавас в шаблоне.
    [TemplatePart(Name = CellsCanvasTemplateName, Type = typeof(Canvas))]

    public class BattleCanvas : Control
    {
        private const string CellsCanvasTemplateName = "PART_Canvas";
        private Canvas? cellsCanvas;

        static BattleCanvas()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BattleCanvas), new FrameworkPropertyMetadata(typeof(BattleCanvas)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            Canvas? cellsCanvas = GetTemplateChild(CellsCanvasTemplateName) as Canvas;

            if (cellsCanvas != this.cellsCanvas)
            {
                if (this.cellsCanvas != null)
                {
                    this.cellsCanvas.LayoutUpdated -= new EventHandler(OnCellsCanvasLayoutUpdated);
                    cellsCanvas.Children.Clear();

                }

                editableTextBoxSite.TextChanged += new TextChangedEventHandler(OnEditableTextBoxTextChanged);
                editableTextBoxSite.SelectionChanged += new RoutedEventHandler(OnEditableTextBoxSelectionChanged);
                editableTextBoxSite.PreviewTextInput += new TextCompositionEventHandler(OnEditableTextBoxPreviewTextInput);
                editableTextBoxSite.GotKeyboardFocus += new KeyboardFocusChangedEventHandler(OnEditableTextBoxGotKeyboardFocus);

                this.editableTextBoxSite = editableTextBoxSite;
            }

            Popup dropDownPopup = GetTemplateChild(PopupTemplateName) as Popup
                ?? throw new InvalidOperationException($"В шаблоне обязательно должен быть {nameof(Popup)} с именем \"{PopupTemplateName}\"");

            if (dropDownPopup != this.dropDownPopup)
            {
                this.dropDownPopup.RemoveHandler(PreviewMouseLeftButtonUpEvent, OnIsClickOverHandler);
                dropDownPopup.AddHandler(PreviewMouseLeftButtonUpEvent, OnIsClickOverHandler, true);
                this.dropDownPopup = dropDownPopup;
            }
        }



        /// <summary>
        /// Модель поля боя.
        /// </summary>
        public BattleField Battle
        {
            get { return (BattleField)GetValue(BattleProperty); }
            set { SetValue(BattleProperty, value); }
        }

        /// <summary><see cref="DependencyProperty"/> для свойства <see cref="Battle"/>.</summary>
        public static readonly DependencyProperty BattleProperty =
            DependencyProperty.Register(
                nameof(Battle),
                typeof(BattleField),
                typeof(BattleCanvas),
                new PropertyMetadata(null, OnBattleChanged));

        private static void OnBattleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BattleCanvas battleCanvas = (BattleCanvas)d;
            BattleField field = (BattleField)e.NewValue;
            if (field == null)
            {
                battleCanvas.ClearValue(CellsProperty);
            }
            else
            {
                battleCanvas.Cells = new BattleCellsCollection (field);
            }

        }



        /// <summary>
        /// Список ячеек поля поля боя.Пересрздаются при замене <see cref="Battle"/>.
        /// </summary>
        // TODO: Заменить на кастомный тип с двойной индексацией.
        public BattleCellsCollection Cells
        {
            get { return (BattleCellsCollection)GetValue(CellsProperty); }
            private set { SetValue(CellsPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey CellsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(Cells),
                typeof(BattleCellsCollection),
                typeof(BattleCanvas),
                new PropertyMetadata(BattleCellsCollection.Empty), OnCellsChanged));
        /// <summary><see cref="DependencyProperty"/> для свойства <see cref="Cells"/>.</summary>
        public static readonly DependencyProperty CellsProperty = CellsPropertyKey.DependencyProperty;

        private static bool OnCellsChanged(object value)
        {
            // TODO: Запись шаблона в ячейки и вызов медода задающего размер и позмцию.
        }

        private ReadOnlyCollection<HexagonCell>? privateCells;
        private static void OnCellsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BattleCanvas battle = (BattleCanvas)d;
            battle.privateCells = (ReadOnlyCollection<HexagonCell>?)e.NewValue;
        }


        /// <summary>
        /// Шаблон Данных ячейки поля боя.
        /// </summary>
        public DataTemplate CellTemplate
        {
            get { return (DataTemplate)GetValue(CellTemplateProperty); }
            set { SetValue(CellTemplateProperty, value); }
        }

        /// <summary><see cref="DependencyProperty"/> для свойства <see cref="CellTemplate"/>.</summary>
        public static readonly DependencyProperty CellTemplateProperty =
            DependencyProperty.Register(
                nameof(CellTemplate),
                typeof(DataTemplate),
                typeof(BattleCanvas),
                new PropertyMetadata(null, OnCellTemplateChanged));

        private DataTemplate? privateCellTemplate;
        private static void OnCellTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BattleCanvas battle = (BattleCanvas)d;
            DataTemplate? template = (DataTemplate?)e.NewValue;
            battle.privateCellTemplate = template;
            foreach (var cell in battle.Cells)
            {
                cell.ContentTemplate = template;
            }
        }
    }

    public class BattleCellsCollection : IReadOnlyList<HexagonCell>
    {
        private readonly HexagonCell[] cells;

        private BattleCellsCollection(HexagonCell[] cells)
        {
            this.cells = cells;
        }
        public BattleCellsCollection(BattleField field)
           : this (field.Select(cell => new HexagonCell() { Content = cell}).ToArray())
        { }

        public HexagonCell this[int index] => ((IReadOnlyList<HexagonCell>)cells)[index];
        public HexagonCell this[int row, int column] => ((IReadOnlyList<HexagonCell>)cells)[BattleField.GetIndex(row, column)];

        public int Count => ((IReadOnlyCollection<HexagonCell>)cells).Count;

        public IEnumerator<HexagonCell> GetEnumerator()
        {
            return ((IEnumerable<HexagonCell>)cells).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return cells.GetEnumerator();
        }

        public static BattleCellsCollection Empty { get; }
            = new BattleCellsCollection(Array.Empty<HexagonCell>());
    }
}
