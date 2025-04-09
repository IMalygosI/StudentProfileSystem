using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using DocumentFormat.OpenXml.Spreadsheet;
using StudentProfileSystem.Models;

namespace StudentProfileSystem;

public partial class LoginWindow : Window
{
    List<School> schools = new List<School>();

    public LoginWindow()
    {
        InitializeComponent();

    }

    private void Button_Click_Entrance(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var selectedSchool = Helper.DateBase.Staff.FirstOrDefault(x => x.Password == password.Text && x.Name == Admin.Text);

        if (selectedSchool != null)
        {
            Window window = new MainWindow();
            window.Show();
            Close();
        }
    }

    private void Button_Click_Exit(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SchoolList window = new SchoolList();
        window.Show();
        Close();
    }
}