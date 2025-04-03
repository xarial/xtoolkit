//*********************************************************************
//xToolkit
//Copyright(C) 2025 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
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
using Xarial.XToolkit.Wpf.Extensions;
using Xarial.XToolkit.Wpf.Themes;

namespace Xarial.XToolkit.Wpf.Controls
{
    [ContentProperty(nameof(Content))]
    public class ProgressPanel : Control
    {
        static ProgressPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ProgressPanel),
                new FrameworkPropertyMetadata(typeof(ProgressPanel)));

            StyleProperty.OverrideMetadata(typeof(ProgressPanel),
                new FrameworkPropertyMetadata(StyleLoader.LoadStyle<ProgressPanel>("ProgressPanel.xaml")));
        }

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(
            nameof(Content), typeof(FrameworkElement),
            typeof(ProgressPanel));

        public FrameworkElement Content
        {
            get { return (FrameworkElement)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        public static readonly DependencyProperty IsWorkInProgressProperty =
            DependencyProperty.Register(
            nameof(IsWorkInProgress), typeof(bool),
            typeof(ProgressPanel));

        public bool IsWorkInProgress
        {
            get { return (bool)GetValue(IsWorkInProgressProperty); }
            set { SetValue(IsWorkInProgressProperty, value); }
        }

        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register(
            nameof(Progress), typeof(double?),
            typeof(ProgressPanel));

        public double? Progress
        {
            get { return (double?)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(
            nameof(Message), typeof(string),
            typeof(ProgressPanel));

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public static readonly DependencyProperty ProgressBarStyleProperty =
            DependencyProperty.Register(
            nameof(ProgressBarStyle), typeof(Style),
            typeof(ProgressPanel),
            new PropertyMetadata(typeof(ProgressPanel).Assembly.LoadFromResources<Style>("Themes/ProgressPanel.xaml", "ProgressBarDefaultStyle")));

        public Style ProgressBarStyle
        {
            get { return (Style)GetValue(ProgressBarStyleProperty); }
            set { SetValue(ProgressBarStyleProperty, value); }
        }
        
        public static readonly DependencyProperty MessageTextBlockStyleProperty =
            DependencyProperty.Register(
            nameof(MessageTextBlockStyle), typeof(Style),
            typeof(ProgressPanel),
            new PropertyMetadata(typeof(ProgressPanel).Assembly.LoadFromResources<Style>("Themes/ProgressPanel.xaml", "MessageTextBlockDefaultStyle")));

        public Style MessageTextBlockStyle
        {
            get { return (Style)GetValue(MessageTextBlockStyleProperty); }
            set { SetValue(MessageTextBlockStyleProperty, value); }
        }


        public static readonly DependencyProperty CancelButtonTemplateProperty =
            DependencyProperty.Register(
            nameof(CancelButtonTemplate), typeof(DataTemplate),
            typeof(ProgressPanel),
            new PropertyMetadata(typeof(ProgressPanel).Assembly.LoadFromResources<DataTemplate>("Themes/ProgressPanel.xaml", "CancelButtonDefaultTemplate")));

        public DataTemplate CancelButtonTemplate
        {
            get { return (DataTemplate)GetValue(CancelButtonTemplateProperty); }
            set { SetValue(CancelButtonTemplateProperty, value); }
        }

        public static readonly DependencyProperty CancelCommandProperty =
            DependencyProperty.Register(
            nameof(CancelCommand), typeof(ICommand),
            typeof(ProgressPanel));

        public ICommand CancelCommand
        {
            get { return (ICommand)GetValue(CancelCommandProperty); }
            set { SetValue(CancelCommandProperty, value); }
        }
    }
}
