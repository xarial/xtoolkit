//*********************************************************************
//xToolkit
//Copyright(C) 2026 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using Xarial.XToolkit.Wpf.Extensions;
using Xarial.XToolkit.Wpf.Themes;

namespace Xarial.XToolkit.Wpf.Controls
{
    [ContentProperty(nameof(Content))]
    public class PopupMenu : Control
    {
        public static Style DefaultToggleButtonStyle => typeof(PopupMenu).Assembly.LoadFromResources<Style>("Themes/PopupMenu.xaml", "ExpandToggleButtonStyle");
        public static Style DefaultPopupStyle => typeof(PopupMenu).Assembly.LoadFromResources<Style>("Themes/PopupMenu.xaml", "PopupStyle");

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

            StyleProperty.OverrideMetadata(typeof(PopupMenu),
                new FrameworkPropertyMetadata(StyleLoader.LoadStyle<PopupMenu>("PopupMenu.xaml")));
        }

        private Action<PopupMenu> m_Opened;
        private Action<PopupMenu> m_Closed;

        private Popup m_Popup;
        private ToggleButton m_Toggle;

        private Thumb m_ResizeGrip;
        private FrameworkElement m_PopupBorder;
        private Thumb m_MoveGrip;
        private Point m_MoveStartPoint;
        private double m_MoveStartHorizontalOffset;
        private double m_MoveStartVerticalOffset;

        public override void OnApplyTemplate()
        {
            m_Popup = (Popup)this.Template.FindName("PART_Popup", this);
            m_Toggle = (ToggleButton)this.Template.FindName("PART_Toggle", this);

            if (m_Opened != null)
            {
                m_Popup.Opened += OnOpened;
            }
            
            if (m_Closed != null)
            {
                m_Popup.Closed += OnClosed;
            }

            if (m_ResizeGrip != null)
            {
                m_ResizeGrip.DragDelta -= OnResizeGripDragDelta;
            }

            if (m_MoveGrip != null)
            {
                m_MoveGrip.DragStarted -= OnMoveGripDragStarted;
                m_MoveGrip.DragDelta -= OnMoveGripDragDelta;
            }

            m_MoveGrip = this.Template.FindName("PART_MoveGrip", this) as Thumb;

            if (m_MoveGrip != null)
            {
                m_MoveGrip.DragStarted += OnMoveGripDragStarted;
                m_MoveGrip.DragDelta += OnMoveGripDragDelta;
            }

            m_ResizeGrip = this.Template.FindName("PART_ResizeGrip", this) as Thumb;
            m_PopupBorder = this.Template.FindName("PART_PopupBorder", this) as FrameworkElement;

            if (m_ResizeGrip != null && m_PopupBorder != null)
            {
                m_ResizeGrip.DragDelta += OnResizeGripDragDelta;
            }

            m_Popup.Closed += OnPopupClosed;
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
            typeof(PopupMenu), new PropertyMetadata(DefaultToggleButtonStyle));

        public Style ToggleButtonStyle
        {
            get { return (Style)GetValue(ToggleButtonStyleProperty); }
            set { SetValue(ToggleButtonStyleProperty, value); }
        }

        public static readonly DependencyProperty PopupStyleProperty =
            DependencyProperty.Register(
            nameof(PopupStyle), typeof(Style),
            typeof(PopupMenu), new PropertyMetadata(DefaultPopupStyle));

        public Style PopupStyle
        {
            get { return (Style)GetValue(PopupStyleProperty); }
            set { SetValue(PopupStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="IsResizable"/> dependency property
        /// </summary>
        public static readonly DependencyProperty IsResizableProperty =
            DependencyProperty.Register(
            nameof(IsResizable), typeof(bool),
            typeof(PopupMenu), new PropertyMetadata(false));

        /// <summary>
        /// Indicates if the popup can be resized by dragging its bottom-right corner
        /// </summary>
        public bool IsResizable
        {
            get { return (bool)GetValue(IsResizableProperty); }
            set { SetValue(IsResizableProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="IsMovable"/> dependency property
        /// </summary>
        public static readonly DependencyProperty IsMovableProperty =
            DependencyProperty.Register(
            nameof(IsMovable), typeof(bool),
            typeof(PopupMenu), new PropertyMetadata(false));

        /// <summary>
        /// Indicates if the popup can be moved by dragging its top edge
        /// </summary>
        public bool IsMovable
        {
            get { return (bool)GetValue(IsMovableProperty); }
            set { SetValue(IsMovableProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="PopupWidth"/> dependency property
        /// </summary>
        public static readonly DependencyProperty PopupWidthProperty =
            DependencyProperty.Register(
            nameof(PopupWidth), typeof(double),
            typeof(PopupMenu), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// Width of the popup content area (NaN to size to content)
        /// </summary>
        public double PopupWidth
        {
            get { return (double)GetValue(PopupWidthProperty); }
            set { SetValue(PopupWidthProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="PopupHeight"/> dependency property
        /// </summary>
        public static readonly DependencyProperty PopupHeightProperty =
            DependencyProperty.Register(
            nameof(PopupHeight), typeof(double),
            typeof(PopupMenu), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// Height of the popup content area (NaN to size to content)
        /// </summary>
        public double PopupHeight
        {
            get { return (double)GetValue(PopupHeightProperty); }
            set { SetValue(PopupHeightProperty, value); }
        }

        private void OnOpened(object sender, EventArgs e)
        {
            m_Opened?.Invoke(this);
        }

        private void OnClosed(object sender, EventArgs e)
        {
            m_Closed?.Invoke(this);
        }

        private void OnResizeGripDragDelta(object sender, DragDeltaEventArgs e)
        {
            var margin = m_PopupBorder.Margin;

            var maxWidth = m_Popup.MaxWidth - margin.Left - margin.Right;
            var maxHeight = m_Popup.MaxHeight - margin.Top - margin.Bottom;

            var width = m_PopupBorder.ActualWidth + e.HorizontalChange;
            var height = m_PopupBorder.ActualHeight + e.VerticalChange;

            PopupWidth = Math.Min(Math.Max(width, m_ResizeGrip.ActualWidth), maxWidth);
            PopupHeight = Math.Min(Math.Max(height, m_ResizeGrip.ActualHeight), maxHeight);
        }

        private void OnMoveGripDragStarted(object sender, DragStartedEventArgs e)
        {
            m_MoveStartPoint = Mouse.GetPosition(m_Toggle);
            m_MoveStartHorizontalOffset = m_Popup.HorizontalOffset;
            m_MoveStartVerticalOffset = m_Popup.VerticalOffset;
        }

        private void OnMoveGripDragDelta(object sender, DragDeltaEventArgs e)
        {
            var offset = Mouse.GetPosition(m_Toggle) - m_MoveStartPoint;

            m_Popup.HorizontalOffset = m_MoveStartHorizontalOffset + offset.X;
            m_Popup.VerticalOffset = m_MoveStartVerticalOffset + offset.Y;
        }

        private void OnPopupClosed(object sender, EventArgs e)
        {
            m_Popup.HorizontalOffset = 0;
            m_Popup.VerticalOffset = 0;
        }
    }
}