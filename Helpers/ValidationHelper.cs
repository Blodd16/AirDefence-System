using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Threading.Tasks;

namespace AirDefenseSimulation.Helpers
{
    public static class ValidationHelper
    {
        public static void ValidateAndCorrect(NumericUpDown control, double min, double max, string fieldName)
        {
            if (control == null)
            {
                ShowValidationMessage($"Control '{fieldName}' not found. Check x:Name in XAML.");
                return;
            }

            if (control.Value == null)
            {
                ShowValidationMessage($"Input field '{fieldName}' cannot be empty!\n\nPlease enter a value between {min} and {max}.");
                control.Value = (decimal)min;
                return;
            }

            var value = control.Value.Value;

            if (value < (decimal)min)
            {
                ShowValidationMessage($"Value for '{fieldName}' is too small!\n\nMinimum value is {min}. The value has been automatically corrected to {min}.");
                control.Value = (decimal)min;
                return;
            }

            if (value > (decimal)max)
            {
                ShowValidationMessage($"Value for '{fieldName}' is too large!\n\nMaximum value is {max}. The value has been automatically corrected to {max}.");
                control.Value = (decimal)max;
                return;
            }
        }

        public static bool ValidateAllFields(
            NumericUpDown fighters,
            NumericUpDown missiles,
            NumericUpDown drones,
            NumericUpDown population,
            NumericUpDown capacity,
            NumericUpDown detectionRadius,
            NumericUpDown interceptionRadius)
        {
            bool hasErrors = false;

            // Validiere alle Felder
            if (fighters.Value == null || fighters.Value < 0 || fighters.Value > 15)
            {
                ValidateAndCorrect(fighters, 0, 15, "Fighters");
                hasErrors = true;
            }

            if (missiles.Value == null || missiles.Value < 0 || missiles.Value > 20)
            {
                ValidateAndCorrect(missiles, 0, 20, "Missiles");
                hasErrors = true;
            }

            if (drones.Value == null || drones.Value < 0 || drones.Value > 15)
            {
                ValidateAndCorrect(drones, 0, 15, "Drones");
                hasErrors = true;
            }

            if (population.Value == null || population.Value < 10 || population.Value > 300)
            {
                ValidateAndCorrect(population, 10, 300, "Population");
                hasErrors = true;
            }

            if (capacity.Value == null || capacity.Value < 5 || capacity.Value > 50)
            {
                ValidateAndCorrect(capacity, 5, 50, "Defense Rocket Capacity");
                hasErrors = true;
            }

            if (detectionRadius.Value == null || detectionRadius.Value < 100 || detectionRadius.Value > 280)
            {
                ValidateAndCorrect(detectionRadius, 100, 280, "Detection Radius");
                hasErrors = true;
            }

            if (interceptionRadius.Value == null || interceptionRadius.Value < 50 || interceptionRadius.Value > 280)
            {
                ValidateAndCorrect(interceptionRadius, 50, 280, "Interception Radius");
                hasErrors = true;
            }

            return !hasErrors;
        }

        private static async void ShowValidationMessage(string message)
        {
            var window = new Window
            {
                Title = "Validation Error",
                Width = 450,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                Background = new SolidColorBrush(Color.Parse("#0f172a"))
            };

            var border = new Border
            {
                Background = new SolidColorBrush(Color.Parse("#1e293b")),
                CornerRadius = new Avalonia.CornerRadius(10),
                Margin = new Avalonia.Thickness(15),
                Padding = new Avalonia.Thickness(20)
            };

            var stackPanel = new StackPanel
            {
                Spacing = 15
            };

            // Icon und Titel
            var headerPanel = new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Spacing = 10,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
            };

            var iconText = new TextBlock
            {
                Text = "Error",
                FontSize = 32
            };

            var titleText = new TextBlock
            {
                Text = "Input Validation",
                FontSize = 20,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                Foreground = new SolidColorBrush(Color.Parse("#f59e0b")),
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
            };

            headerPanel.Children.Add(iconText);
            headerPanel.Children.Add(titleText);

            // Fehlermeldung
            var messageText = new TextBlock
            {
                Text = message,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.Parse("#e2e8f0")),
                TextAlignment = Avalonia.Media.TextAlignment.Center
            };

            // OK Button
            var okButton = new Button
            {
                Content = "OK",
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Padding = new Avalonia.Thickness(40, 10),
                Background = new SolidColorBrush(Color.Parse("#06b6d4")),
                Foreground = Brushes.White,
                FontWeight = Avalonia.Media.FontWeight.Bold,
                CornerRadius = new Avalonia.CornerRadius(6)
            };

            okButton.Click += (s, e) => window.Close();

            stackPanel.Children.Add(headerPanel);
            stackPanel.Children.Add(messageText);
            stackPanel.Children.Add(okButton);

            border.Child = stackPanel;
            window.Content = border;

            // Zeige Dialog
            try
            {
                var mainWindow = Avalonia.Application.Current?.ApplicationLifetime is
                    Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop
                    ? desktop.MainWindow
                    : null;

                if (mainWindow != null)
                {
                    await window.ShowDialog(mainWindow);
                }
            }
            catch
            {
                // Fallback falls kein Hauptfenster gefunden wird
            }
        }
    }
}
