using System;
using System.Collections.Generic;
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

namespace WpfCC
{
    /// <summary>
    /// Выполните шаги 1a или 1b, а затем 2, чтобы использовать этот пользовательский элемент управления в файле XAML.
    ///
    /// Шаг 1a. Использование пользовательского элемента управления в файле XAML, существующем в текущем проекте.
    /// Добавьте атрибут XmlNamespace в корневой элемент файла разметки, где он 
    /// будет использоваться:
    ///
    ///     xmlns:MyNamespace="clr-namespace:WpfCC"
    ///
    ///
    /// Шаг 1б. Использование пользовательского элемента управления в файле XAML, существующем в другом проекте.
    /// Добавьте атрибут XmlNamespace в корневой элемент файла разметки, где он 
    /// будет использоваться:
    ///
    ///     xmlns:MyNamespace="clr-namespace:WpfCC;assembly=WpfCC"
    ///
    /// Потребуется также добавить ссылку из проекта, в котором находится файл XAML,
    /// на данный проект и пересобрать во избежание ошибок компиляции:
    ///
    ///     Щелкните правой кнопкой мыши нужный проект в обозревателе решений и выберите
    ///     "Добавить ссылку"->"Проекты"->[Поиск и выбор проекта]
    ///
    ///
    /// Шаг 2)
    /// Теперь можно использовать элемент управления в файле XAML.
    ///
    ///     <MyNamespace:CustomControl1/>
    ///
    /// </summary>
    public class HexagonCell : HexagonButton
    {
        static HexagonCell()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HexagonCell), new FrameworkPropertyMetadata(typeof(HexagonCell)));
        }


        /// <summary>
        /// Позиция центра ячейки.
        /// </summary>
        public Point Centre
        {
            get { return (Point)GetValue(CentreProperty); }
            set { SetValue(CentreProperty, value); }
        }

        /// <summary><see cref="DependencyProperty"/> для свойства <see cref="Centre"/>.</summary>
        public static readonly DependencyProperty CentreProperty =
            DependencyProperty.Register(
                nameof(Centre),
                typeof(Point),
                typeof(HexagonCell),
                new PropertyMetadata(new Point(), OnCentreChanged));

        private Point privateCentre;
        private static void OnCentreChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HexagonCell cell = (HexagonCell)d;
            cell.privateCentre = (Point)e.NewValue;
            cell.SetPositionAsync();
        }


        private Size arrangeBounds;
        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            this.arrangeBounds = arrangeBounds;
            SetPositionAsync();
            return base.ArrangeOverride(arrangeBounds);
        }

        private async void SetPositionAsync()
        {
            await Dispatcher.BeginInvoke(OnCentreAction, this, privateCentre, arrangeBounds);
        }

        private readonly Action<UIElement, Point, Size> OnCentreAction = (element, centre, size) =>
        {
            Canvas.SetLeft(element, centre.X - 0.5 * size.Width);
            Canvas.SetTop(element, centre.Y - 0.5 * size.Height);
        };

        /// <summary>
        /// Ячейка выделена.
        /// </summary>
        public bool IsHighlighted
        {
            get { return (bool)GetValue(IsHighlightedProperty); }
            set { SetValue(IsHighlightedProperty, value); }
        }

        /// <summary><see cref="DependencyProperty"/> для свойства <see cref="IsHighlighted"/>.</summary>
        public static readonly DependencyProperty IsHighlightedProperty =
            DependencyProperty.Register(nameof(IsHighlighted), typeof(bool), typeof(HexagonCell), new PropertyMetadata(false));



    }
}
