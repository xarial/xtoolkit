using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Xarial.XToolkit.Wpf.Behaviors
{
    public static class MouseBehavior
    {
        public static DependencyProperty DoubleClickCommandProperty =
            DependencyProperty.RegisterAttached("DoubleClickCommand",
                typeof(ICommand),
                typeof(MouseBehavior),
                new UIPropertyMetadata(OnDoubleClickCommandChanged));

        public static DependencyProperty DoubleClickCommandParameterProperty =
            DependencyProperty.RegisterAttached("DoubleClickCommandParameter",
                typeof(object),
                typeof(MouseBehavior),
                new UIPropertyMetadata(null));

        public static DependencyProperty ClickCommandProperty =
            DependencyProperty.RegisterAttached("ClickCommand",
                typeof(ICommand),
                typeof(MouseBehavior),
                new UIPropertyMetadata(OnClickCommandChanged));

        public static DependencyProperty ClickCommandParameterProperty =
            DependencyProperty.RegisterAttached("ClickCommandParameter",
                typeof(object),
                typeof(MouseBehavior),
                new UIPropertyMetadata(null));

        public static void SetDoubleClickCommand(DependencyObject target, ICommand value)
            => target.SetValue(DoubleClickCommandProperty, value);

        public static void SetDoubleClickCommandParameter(DependencyObject target, object value)
            => target.SetValue(DoubleClickCommandParameterProperty, value);

        public static object GetDoubleClickCommandParameter(DependencyObject target)
            => target.GetValue(DoubleClickCommandParameterProperty);

        public static void SetClickCommand(DependencyObject target, ICommand value)
            => target.SetValue(ClickCommandProperty, value);

        public static void SetClickCommandParameter(DependencyObject target, object value)
            => target.SetValue(ClickCommandParameterProperty, value);

        public static object GetClickCommandParameter(DependencyObject target)
            => target.GetValue(ClickCommandParameterProperty);

        private static void OnDoubleClickCommandChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            var uiElem = target as UIElement;

            if (uiElem != null)
            {
                if (e.OldValue != null)
                {
                    uiElem.MouseDown -= OnDoubleClickMouseUp;
                }

                if (e.NewValue != null)
                {
                    uiElem.MouseDown += OnDoubleClickMouseUp;
                }
            }
        }

        private static void OnDoubleClickMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                var uiElem = (UIElement)sender;

                var command = (ICommand)uiElem.GetValue(DoubleClickCommandProperty);
                var commandParameter = uiElem.GetValue(DoubleClickCommandParameterProperty);

                command.Execute(commandParameter);
            }
        }

        private static void OnClickCommandChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            var uiElem = target as UIElement;

            if (uiElem != null)
            {
                if (e.OldValue != null)
                {
                    uiElem.MouseDown -= OnClickMouseUp;
                }

                if (e.NewValue != null)
                {
                    uiElem.MouseDown += OnClickMouseUp;
                }
            }
        }

        private static void OnClickMouseUp(object sender, MouseButtonEventArgs e)
        {
            var uiElem = (UIElement)sender;

            var command = (ICommand)uiElem.GetValue(ClickCommandProperty);
            var commandParameter = uiElem.GetValue(ClickCommandParameterProperty);

            command.Execute(commandParameter);
        }
    }
}
