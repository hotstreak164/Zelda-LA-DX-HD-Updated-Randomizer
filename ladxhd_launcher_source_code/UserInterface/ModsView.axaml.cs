using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using static LADXHD_Launcher.AdvancedSettings;

namespace LADXHD_Launcher;

public partial class ModsView : UserControl
{
    private MainWindow? _parent;

    public ModsView() { InitializeComponent(); }

    public ModsView(MainWindow parent)
    {
        InitializeComponent();
        _parent = parent;
    }

    private static decimal? GetOverride(Dictionary<string, decimal> overrides, string key)
    {
        // Exact match first
        if (overrides.TryGetValue(key, out decimal exact))
            return exact;

        // Partial suffix match
        foreach (var entry in overrides)
        {
            if (entry.Key.StartsWith("*") && key.EndsWith(entry.Key[1..]))
                return entry.Value;
        }

        return null;
    }

    public void LoadValues()
    {
        // Suppress the sound effects so the checkbox sound doesn't fire a bunch of times.
        XnbAudio.SuppressSound = true;

        ModsPanel.Children.Clear();

        foreach (var section in AdvancedSettings.Sections)
        {
            // Count total options
            int optionCount = 0;
            foreach (var g in section.Groups) optionCount += g.Options.Count;

            // Detect "lives-style" section: many options, single group, no sub-tooltips.
            bool twoColumn = false;

            // Section header: create a new combobox.
            var header = new TextBlock
            {
                Text       = section.Header,
                Foreground = Brushes.White,
                FontWeight = FontWeight.Bold,
                Margin     = new Thickness(2, 0, 0, 0)
            };
            // If a comment was set then set the tooltip.
            if (!string.IsNullOrEmpty(section.HeaderTooltip))
                ToolTip.SetTip(header, section.HeaderTooltip);

            int rowHeight = 36;
            int rows      = twoColumn
                ? (int)Math.Ceiling(optionCount / 2.0)
                : optionCount;
            int canvasH   = rows * rowHeight;

            var canvas = new Canvas { Height = canvasH };

            int col = 0;
            int row = 0;

            foreach (var group in section.Groups)
            {
                foreach (var option in group.Options)
                {
                    double x = twoColumn && col == 1 ? 232 : 0;
                    double y = row * rowHeight;

                    string tooltip = option.Tooltip;

                    // Checkbox: Option is boolean so present with a checkbox.
                    if (option.IsBool)
                    {
                        // Create a new checkbox.
                        var cb = new CheckBox
                        {
                            Content    = OptionLabels.Get(option.Key),
                            Foreground = Brushes.White,
                            IsChecked  = option.BoolValue
                        };
                        // Set a tooltip if comments were found.
                        if (!string.IsNullOrEmpty(tooltip))
                            ToolTip.SetTip(cb, tooltip);

                        string sHeader = section.Header;
                        string key     = option.Key;
                        cb.IsCheckedChanged += (s, e) =>
                            AdvancedSettings.UpdateValue(sHeader, key,
                                (cb.IsChecked == true).ToString().ToLower());

                        Canvas.SetLeft(cb, x);
                        Canvas.SetTop(cb, y);
                        canvas.Children.Add(cb);
                    }
                    // Numeric Up/Down: Option is a number so present with a numeric up/down.
                    else
                    {
                        // Width and offset of numeric up/downs.
                        const double nudWidth  = 140;
                        const double lblOffset = nudWidth + 6;

                        decimal? minOverride = GetOverride(AdvancedSettings.MinOverrides, option.Key);
                        decimal? maxOverride = GetOverride(AdvancedSettings.MaxOverrides, option.Key);

                        decimal minVal = minOverride ?? (group.AllowNegative ? decimal.MinValue : 0);
                        decimal maxVal = maxOverride ?? decimal.MaxValue;

                        // Apply the values to the numeric up/downs.
                        var nud = new NumericUpDown
                        {
                            Width        = nudWidth,
                            Minimum      = minVal,
                            Maximum      = maxVal,
                            Increment    = option.Increment,
                            Value        = (decimal)(option.IsFloat ? option.FloatValue : option.IntValue),
                            FormatString = option.FormatString
                        };
                        var lbl = new TextBlock
                        {
                            Text       = OptionLabels.Get(option.Key),
                            Foreground = Brushes.White
                        };
                        if (!string.IsNullOrEmpty(tooltip))
                        {
                            ToolTip.SetTip(nud, tooltip);
                            ToolTip.SetTip(lbl, tooltip);
                        }

                        string sHeader = section.Header;
                        string key     = option.Key;
                        nud.ValueChanged += (s, e) =>
                        {
                            string val = option.IsFloat
                                ? ((float)(nud.Value ?? 0)).ToString("F" + option.DecimalPlaces,
                                    System.Globalization.CultureInfo.InvariantCulture)
                                : ((int)(nud.Value ?? 0)).ToString();
                            AdvancedSettings.UpdateValue(sHeader, key, val);
                        };
                        Canvas.SetLeft(nud, x);
                        Canvas.SetTop(nud, y);
                        Canvas.SetLeft(lbl, x + lblOffset);
                        Canvas.SetTop(lbl, y + 7);
                        canvas.Children.Add(nud);
                        canvas.Children.Add(lbl);
                    }

                    if (twoColumn)
                    {
                        col++;
                        if (col > 1) { col = 0; row++; }
                    }
                    else
                    {
                        row++;
                    }
                }
            }

            var border = new Border
            {
                BorderBrush     = new SolidColorBrush(Color.Parse("#88FFFFFF")),
                BorderThickness = new Thickness(1),
                CornerRadius    = new CornerRadius(6),
                Padding         = new Thickness(10),
                Child           = canvas
            };

            var sectionPanel = new StackPanel { Spacing = 4 };
            sectionPanel.Children.Add(header);
            sectionPanel.Children.Add(border);
            ModsPanel.Children.Add(sectionPanel);
        }
        // Ok it's fine now.
        XnbAudio.SuppressSound = false;
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        _parent?.NavigateTo(_parent.HomeView);
        XnbAudio.PlayXnbSound(XnbAudio.SoundClose);
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        AdvancedSettings.Save(AppContext.BaseDirectory);
        _parent?.ShowSavedNotification();
        _parent?.NavigateTo(_parent.HomeView);
        XnbAudio.PlayXnbSound(XnbAudio.SoundXSave);
    }

    private void ExitButton_Click(object sender, RoutedEventArgs e)
    {
        _parent?.Close();
    }
}