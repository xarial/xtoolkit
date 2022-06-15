using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Markup;
using Xarial.XToolkit.Wpf.Extensions;

namespace Xarial.XToolkit.Wpf.Controls
{
    [ContentProperty(nameof(Content))]
    public class PopupMenu : Control
    {
        public event Action<PopupMenu> Opened 
        {
            add 
            {
                if (m_Opened == null) 
                {
                    if (m_Popup != null)
                    {
                        m_Popup.Opened += OnOpened;
                    }
                }

                m_Opened += value;
            }
            remove 
            {
                m_Opened -= value;

                if (m_Opened == null)
                {
                    if (m_Popup != null)
                    {
                        m_Popup.Opened -= OnOpened;
                    }
                }
            }
        }

        public event Action<PopupMenu> Closed
        {
            add
            {
                if (m_Closed == null)
                {
                    if (m_Popup != null)
                    {
                        m_Popup.Closed += OnClosed;
                    }
                }

                m_Closed += value;
            }
            remove
            {
                m_Closed -= value;

                if (m_Closed == null)
                {
                    if (m_Popup != null)
                    {
                        m_Popup.Closed -= OnClosed;
                    }
                }
            }
        }

        static PopupMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PopupMenu),
                new FrameworkPropertyMetadata(typeof(PopupMenu)));
        }

        private Action<PopupMenu> m_Opened;
        private Action<PopupMenu> m_Closed;

        private ToggleButton m_Toggle;
        private Popup m_Popup;

        public override void OnApplyTemplate()
        {
            m_Toggle = (ToggleButton)this.Template.FindName("PART_Toggle", this);
            m_Popup = (Popup)this.Template.FindName("PART_Popup", this);

            var bindingIsChecked = new Binding(nameof(IsOpen));
            bindingIsChecked.Source = this;
            m_Toggle.SetBinding(ToggleButton.IsCheckedProperty, bindingIsChecked);

            var bindingIsOpen = new Binding(nameof(IsOpen));
            bindingIsOpen.Source = this;
            m_Popup.SetBinding(Popup.IsOpenProperty, bindingIsOpen);

            if (m_Opened != null)
            {
                m_Popup.Opened += OnOpened;
            }

            if (m_Closed != null)
            {
                m_Popup.Closed += OnClosed;
            }
        }

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(
            nameof(IsOpen), typeof(bool),
            typeof(PopupMenu), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(
                nameof(Content), typeof(FrameworkElement),
                typeof(PopupMenu));

        public FrameworkElement Content
        {
            get { return (FrameworkElement)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public static readonly DependencyProperty ToggleButtonStyleProperty =
            DependencyProperty.Register(
            nameof(ToggleButtonStyle), typeof(Style),
            typeof(PopupMenu),
            new PropertyMetadata(typeof(PopupMenu).Assembly.LoadFromResources<Style>("Themes/Generic.xaml", "ExpandToggleButtonStyle")));

        public Style ToggleButtonStyle
        {
            get { return (Style)GetValue(ToggleButtonStyleProperty); }
            set { SetValue(ToggleButtonStyleProperty, value); }
        }

        private void OnOpened(object sender, EventArgs e)
        {
            m_Opened?.Invoke(this);
        }

        private void OnClosed(object sender, EventArgs e)
        {
            m_Closed?.Invoke(this);
        }
    }
}
