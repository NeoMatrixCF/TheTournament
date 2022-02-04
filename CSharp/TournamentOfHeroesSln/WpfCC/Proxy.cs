using System;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

namespace Proxy
{
    /// <summary>Предоставляет прокси <see cref="DependencyObject"/> с одним свойством и 
    /// событем уведомляющем о его изменении.</summary>
    /// <typeparam name="T">Тип свойства <see cref="DataContext"/>.</typeparam>
    public class Proxy<T> : Freezable
    {
        /// <summary>Свойство для задания внешних привязок.</summary>
        public T DataContext
        {
            get => (T)GetValue(DataContextProperty);
            set => SetValue(DataContextProperty, value);
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataContextProperty =
            DependencyProperty.Register(nameof(DataContext), typeof(T), typeof(Proxy<T>), new PropertyMetadata(null));

        /// <summary>Привязка по умолсанию - пустой экземпляр <see cref="Binding()"/>.
        /// </summary>
        public static readonly Binding DefaultBinding = new();

        public Proxy()
            => SetValueBinding(DefaultBinding);

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            DataContextChanged?.Invoke(this, e);
        }

        /// <summary>Событие, возникающее при изменении значения любого <see cref="DependencyProperty"/>.</summary>
        public event EventHandler<DependencyPropertyChangedEventArgs>? DataContextChanged;

        /// <summary>Возвращает <see langword="true"/>, если значение свойства <see cref="DataContext"/> не задано.</summary>
        public bool IsUnsetValue => Equals(ReadLocalValue(DataContextProperty), DependencyProperty.UnsetValue);

        /// <summary>Очистка всех <see cref="DependencyProperty"/> этого <see cref="ProxyDO"/>.</summary>
        public void Reset()
        {
            LocalValueEnumerator locallySetProperties = GetLocalValueEnumerator();
            while (locallySetProperties.MoveNext())
            {
                DependencyProperty propertyToClear = locallySetProperties.Current.Property;
                if (!propertyToClear.ReadOnly)
                {
                    ClearValue(propertyToClear);
                }
            }

        }

        /// <summary><see langword="true"/> если свойству задана Привязка.</summary>
        public bool IsValueBinding => BindingOperations.GetBindingExpressionBase(this, DataContextProperty) != null;

        /// <summary><see langword="true"/> если свойству задана привязка
        /// и она в состоянии <see cref="BindingStatus.Active"/>.</summary>
        public bool IsActiveValueBinding
        {
            get
            {
                BindingExpressionBase exp = BindingOperations.GetBindingExpressionBase(this, DataContextProperty);
                if (exp == null)
                {
                    return false;
                }

                BindingStatus status = exp.Status;
                return status == BindingStatus.Active;
            }
        }

        public void SetValueBinding(BindingBase binding)
        {
            if (binding != null)
                BindingOperations.SetBinding(this, DataContextProperty, binding);
            else
                BindingOperations.ClearBinding(this, DataContextProperty);
        }

        protected override Freezable CreateInstanceCore()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>Предоставляет прокси <see cref="DependencyObject"/>
    /// с одним свойством типа <see cref="object"/> и 
    /// событем уведомляющем о его изменении.</summary>
    public class Proxy : Proxy<object>
    {

    }

    /// <summary>Предоставляет прокси <see cref="DependencyObject"/>
    /// с одним свойством типа <see cref="Brush"/> и 
    /// событем уведомляющем о его изменении.</summary>
    public class BrushProxy : Proxy<Brush>
    {

    }

    /// <summary>Возвращает экземпляр <see cref="Proxy{T}"/> для указанного типа
    /// и задаёт в нём указанныую привязку.</summary>
    [MarkupExtensionReturnType(typeof(Proxy<>))]
    public class ProxyExtension : MarkupExtension
    {
        public Type? Type { get; set; }

        public BindingBase? Binding { get; set; }

        public ProxyExtension(Type type)
        {
            Type = type;
        }

        public ProxyExtension()
        {
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            DependencyObject? proxy;
            DependencyProperty? property;
            Type typeT = Type ?? typeof(object);

            Type type = typeof(Proxy<>).MakeGenericType(typeT);
            proxy = (DependencyObject)Activator.CreateInstance(type);
            property = (DependencyProperty)proxy.GetType().GetField("ValueProperty", BindingFlags.Public | BindingFlags.Static).GetValue(null);

            if (Binding != null)
            {
                _ = BindingOperations.SetBinding(proxy, property, Binding);
            }

            return proxy;
        }
    }

}
