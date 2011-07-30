﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Inscribe.Common;
using Inscribe.ViewModels.PartBlocks.MainBlock.TimelineChild;

namespace Mystique.Views.PartBlocks.MainBlock.TimelineChild
{
    /// <summary>
    /// TimelineSelectedItem.xaml の相互作用ロジック
    /// </summary>
    public partial class TimelineSelectedItem : UserControl
    {
        public TimelineSelectedItem()
        {
            InitializeComponent();
        }

        bool captured = false;
        Point ip = new Point();
        MouseButtonEventArgs origEventArgs;
        private void ItemMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (captured)
                {
                    captured = false;
                }
                else
                {
                    origEventArgs = e;
                    captured = true;
                    ip = e.GetPosition(this);
                    e.Handled = true;
                }
            }
        }

        private void ItemMouseMove(object sender, MouseEventArgs e)
        {
            if (captured && e.GetPosition(this).DistanceDouble(ip) > 4)
            {
                origEventArgs.Handled = false;
                BodyText.EntryOnMouseDown(origEventArgs);
            }
        }

        private void ItemMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.GetPosition(this).DistanceDouble(ip) <= 4)
            {
                if (captured)
                {
                    e.Handled = true;
                    var tm = DataContext as TabDependentTweetViewModel;
                    if (tm != null)
                        tm.DeselectCommand.Execute();
                }
                captured = false;
            }
        }

        private void cCopy_Click(object sender, RoutedEventArgs e)
        {
            BodyText.Copy();
        }

        private void cSelectAll_Click(object sender, RoutedEventArgs e)
        {
            BodyText.SelectAll();
        }

        private void cSearchInKrile_Click(object sender, RoutedEventArgs e)
        {
            var tm = DataContext as TabDependentTweetViewModel;
            if (tm != null)
                tm.Parent.AddTopTimeline(new[] { new Inscribe.Filter.Filters.Text.FilterText(BodyText.Selection.Text) });
        }

        private void cGoogle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Browser.Start(
                    "http://www.google.co.jp/search?ie=UTF-8&q=" +
                    Dulcet.Util.HttpUtility.UrlEncode(BodyText.Selection.Text));
            }
            catch { }
        }
    }
}