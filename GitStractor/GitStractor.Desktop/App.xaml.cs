using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Telerik.Windows.Controls;

namespace GitStractor.Desktop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            FluentPalette.LoadPreset(FluentPalette.ColorVariation.Dark);
            StyleManager.ApplicationTheme = new FluentTheme();

            MainWindow tabbedWindow = new();
            tabbedWindow.Header = "GitStractor by Matt Eland";
            tabbedWindow.Width = 1020;
            tabbedWindow.Height = 600;
            tabbedWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            tabbedWindow.Show();
        }
    }
}
