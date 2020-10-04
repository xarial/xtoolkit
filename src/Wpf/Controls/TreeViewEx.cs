//*********************************************************************
//xToolkit
//Copyright(C) 2020 Xarial Pty Limited
//Product URL: https://xtoolkit.xarial.com
//License: https://xtoolkit.xarial.com/license/
//*********************************************************************

using System.Windows;
using System.Windows.Controls;

namespace Xarial.XToolkit.Wpf.Controls
{
    public class TreeViewEx : TreeView
    {
        public TreeViewEx()
            : base()
        {
            SelectedItemChanged += OnSelectedItemChanged;
        }

        private void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SetValue(SelectedItemProperty, e.NewValue);
        }

        public new object SelectedItem
        {
            get { return (object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public static readonly new DependencyProperty SelectedItemProperty
            = DependencyProperty.Register(nameof(SelectedItem), typeof(object),
                typeof(TreeViewEx), new FrameworkPropertyMetadata(
                    null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnSelectedItemPropertyChanged));

        private static void OnSelectedItemPropertyChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                SelectTreeViewItem(d as TreeView, e.NewValue);
            }
        }

        private static bool SelectTreeViewItem(ItemsControl parentContainer, object targetItem)
        {
            foreach (var childItem in parentContainer.Items)
            {
                var currentContainer = (TreeViewItem)parentContainer.ItemContainerGenerator.ContainerFromItem(childItem);
                
                if (currentContainer != null && childItem == targetItem)
                {
                    currentContainer.IsSelected = true;
                    currentContainer.BringIntoView();
                    return true;
                }
            }
            
            foreach (var childItem in parentContainer.Items)
            {
                var currentContainer = (TreeViewItem)parentContainer.ItemContainerGenerator.ContainerFromItem(childItem);

                if (currentContainer != null && currentContainer.Items.Count > 0)
                {
                    bool? origExpanded = null;

                    if (currentContainer.ItemContainerGenerator.Status != System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
                    {
                        origExpanded = currentContainer.IsExpanded;
                        currentContainer.IsExpanded = true;
                        currentContainer.UpdateLayout();
                    }
                    
                    if (!SelectTreeViewItem(currentContainer, targetItem))
                    {
                        if (origExpanded.HasValue)
                        {
                            currentContainer.IsExpanded = origExpanded.Value;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            
            return false;
        }
    }
}
