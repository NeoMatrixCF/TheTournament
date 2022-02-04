using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xaml;

namespace WpfCC
{
    /// <summary>
    /// Класс Шестиугодьная кнопка.<br/>
    /// Предназначен для представления одной ячейки поля боя.
    /// </summary>
    public class HexagonButton : Button
    {
        static HexagonButton()
        {
            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/WpfCC;component/Themes/HexagonButtonDynamicKeys.xaml")
            }
            );
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HexagonButton), new FrameworkPropertyMetadata(typeof(HexagonButton)));
        }

        protected override void OnClick()
        {
            if (HexagonsPreviewClick.CanExecute(Content, this))
                HexagonsPreviewClick.Execute(Content, this);

            base.OnClick();

            if (HexagonsClick.CanExecute(Content, this))
                HexagonsClick.Execute(Content, this);
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            this.arrangeBounds = arrangeBounds;
            SetPointsAsync();
            return base.ArrangeOverride(arrangeBounds);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == BorderThicknessProperty)
            {
                borderThickness = (Thickness)e.NewValue;
                SetPointsAsync();
            }
        }

        private Size arrangeBounds;
        private Thickness borderThickness;
        private async void SetPointsAsync()
        {
            Size arrangeBounds = this.arrangeBounds;
            Thickness borderThickness = this.borderThickness;
            (PointCollection pointsC, PointCollection pointsB) =
            await Task.Run(() =>
            {
                Vector offset = 0.5 * new Vector(borderThickness.Left, borderThickness.Top);
                Vector border = 0.5 * new Vector(borderThickness.Left + borderThickness.Right, borderThickness.Top + borderThickness.Bottom);

                PointCollection pointsC = GetPoints(arrangeBounds, -border, offset);
                PointCollection pointsB = GetPoints(arrangeBounds, border, -offset);

                pointsC.Freeze();
                pointsB.Freeze();
                return (pointsC, pointsB);
            });

            ContentPoints = pointsC;
            BorderPoints = pointsB;

            static PointCollection GetPoints(Size size, Vector forSize, Vector offset)
            {
                if (size.Width + forSize.X > 0)
                    size.Width += forSize.X;
                else
                    size.Width = 0;

                if (size.Height + forSize.Y > 0)
                    size.Height += forSize.Y;
                else
                    size.Height = 0;

                return new(6)
                {
                    new Point(0.5 * size.Width, 0) + offset,
                    new Point(size.Width, 0.25 * size.Height) + offset,
                    new Point(size.Width, 0.75 * size.Height) + offset,
                    new Point(0.5 * size.Width, size.Height) + offset,
                    new Point(0, 0.75 * size.Height) + offset,
                    new Point(0, 0.25 * size.Height) + offset
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public PointCollection ContentPoints
        {
            get { return (PointCollection)GetValue(ContentPointsProperty); }
            private set { SetValue(ContentPointsPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey ContentPointsPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(ContentPoints), typeof(PointCollection), typeof(HexagonButton), new PropertyMetadata(null));
        /// <summary><see cref="DependencyProperty"/> для свойства <see cref="ContentPoints"/>.</summary>
        public static readonly DependencyProperty ContentPointsProperty = ContentPointsPropertyKey.DependencyProperty;




        /// <summary>
        /// Координаты точек вершин внешней области включая границу.
        /// </summary>
        public PointCollection BorderPoints
        {
            get { return (PointCollection)GetValue(BorderPointsProperty); }
            private set { SetValue(BorderPointsPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey BorderPointsPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(BorderPoints), typeof(PointCollection), typeof(HexagonButton), new PropertyMetadata(null));
        /// <summary><see cref="DependencyProperty"/> для свойства <see cref="BorderPoints"/>.</summary>
        public static readonly DependencyProperty BorderPointsProperty = BorderPointsPropertyKey.DependencyProperty;





        public static RoutedUICommand HexagonsClick { get; } = new RoutedUICommand("Click of Hexagon", nameof(HexagonsClick), typeof(HexagonButton));
        public static RoutedUICommand HexagonsPreviewClick { get; } = new RoutedUICommand("PreviewClick of Hexagon", nameof(HexagonsPreviewClick), typeof(HexagonButton));

        public static object FocusVisual { get; } = new object();
        public static object StaticBackground { get; } = new object();
        public static object StaticBorder { get; } = new object();
        public static object StaticForeground { get; } = new object();
        public static object MouseOverBackground { get; } = new object();
        public static object MouseOverBorder { get; } = new object();
        public static object PressedBackground { get; } = new object();
        public static object PressedBorder { get; } = new object();
        public static object DisabledBackground { get; } = new object();
        public static object DisabledBorder { get; } = new object();
        public static object DisabledForeground { get; } = new object();
    }

    public class SizeToThicknessConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double width = (double)values[0];
            double height = (double)values[1];
            double rate;
            if (values.Length < 3)
            {
                if (parameter is not double _rate)
                {
                    if (!double.TryParse(parameter?.ToString(), out _rate))
                        return DependencyProperty.UnsetValue;
                }
                rate = _rate;
            }
            else
            {
                rate = (double)values[2];
            }
            if (double.IsNaN(rate) || rate < 0)
                return DependencyProperty.UnsetValue;

            double w = width * rate;
            double h = height * rate;
            if (w < 1)
                w = 1;
            if (h < 1)
                h = 1;

            Thickness thickness = new(w, h, w, h);

            return thickness;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public static SizeToThicknessConverter Instance { get; } = new();


        /// <summary>Возвращает значение присоединённого свойства SizeToBorderThicknessRatio для <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="FrameworkElement"/> значение свойства которого будет возвращено.</param>
        /// <returns><see cref="double"/> значение свойства.</returns>
        public static double GetSizeToBorderThicknessRatio(FrameworkElement element)
        {
            return (double)element.GetValue(SizeToBorderThicknessRatioProperty);
        }

        /// <summary>Задаёт значение присоединённого свойства SizeToBorderThicknessRatio для <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="FrameworkElement"/> значение свойства которого будет возвращено.</param>
        /// <param name="value"><see cref="double"/> значение для свойства.</param>
        public static void SetSizeToBorderThicknessRatio(FrameworkElement element, double value)
        {
            element.SetValue(SizeToBorderThicknessRatioProperty, value);
        }

        /// <summary><see cref="DependencyProperty"/> для методов <see cref="GetSizeToBorderThicknessRatio(FrameworkElement)"/> и <see cref="SetSizeToBorderThicknessRatio(FrameworkElement, double)"/>.</summary>
        public static readonly DependencyProperty SizeToBorderThicknessRatioProperty =
            DependencyProperty.RegisterAttached(
                nameof(GetSizeToBorderThicknessRatio)[3..],
                typeof(double),
                typeof(SizeToThicknessConverter),
                new PropertyMetadata(double.NaN, RatioChanged));

        private static void RatioChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            switch (d)
            {
                case Control control:
                    SetBinding(control, Control.BorderThicknessProperty);
                    break;
                case Border border:
                    SetBinding(border, Border.BorderThicknessProperty);
                    break;
                default:
                    throw new ArgumentException("Только для Control и Border.", nameof(d));
            }

            void SetBinding(FrameworkElement element, DependencyProperty property)
            {
                if (double.IsNaN((double)e.NewValue) &&
                    element.ReadLocalValue(SizeToBorderThicknessRatioProperty) == DependencyProperty.UnsetValue)
                {
                    if (BindingOperations.GetBindingBase(element, property) == RateBinding)
                        BindingOperations.ClearBinding(element, property);
                    return;
                }
                else
                {
                    element.SetBinding(property, RateBinding);
                }
            }
        }

        private static readonly MultiBinding RateBinding;

        static SizeToThicknessConverter()
        {
            MultiBinding rateBinding = new();
            rateBinding.Converter = Instance;

            rateBinding.Bindings.Add(new Binding()
            {
                Path = new PropertyPath(FrameworkElement.ActualWidthProperty),
                RelativeSource = RelativeSource.Self
            });
            rateBinding.Bindings.Add(new Binding()
            {
                Path = new PropertyPath(FrameworkElement.ActualHeightProperty),
                RelativeSource = RelativeSource.Self
            });
            rateBinding.Bindings.Add(new Binding()
            {
                Path = new PropertyPath(SizeToBorderThicknessRatioProperty),
                RelativeSource = RelativeSource.Self
            });

            RateBinding = rateBinding;
        }

    }
}
