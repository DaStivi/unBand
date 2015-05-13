﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using unBand.BandHelpers;
using Xceed.Wpf.Toolkit;

namespace unBand.Controls
{
    /// <summary>
    ///     Interaction logic for BandColorSelector.xaml
    /// </summary>
    public partial class BandColorSelector : UserControl
    {
        public BandColorSelector()
        {
            InitializeComponent();
        }

        private void Color_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;

            (border.Child as ColorPicker).IsOpen = true;
        }

        private void Color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            Telemetry.TrackEvent(TelemetryCategory.Theme, Tag as string, e.NewValue);

            var band = DataContext as BandManager;

            var prop = band.Theme.GetType().GetProperty(Tag as string);

            prop.SetValue(band.Theme, new SolidColorBrush(e.NewValue));
        }

        private void ColorPicker_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // swallow this event so that it is not forwarded to our parent (who would reopen the picker if
            // the user selected the currently selected color)

            e.Handled = true;
        }
    }
}