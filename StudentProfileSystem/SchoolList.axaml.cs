using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Microsoft.EntityFrameworkCore;
using StudentProfileSystem.Models;
using Avalonia.Layout;
using StudentProfileSystem.Services;
using System.Threading.Tasks;
using Avalonia.Interactivity;

namespace StudentProfileSystem;

public partial class SchoolList : Window
{
    private readonly ExcelService _excelService;
    List<School> schoolList = new List<School>();

    public SchoolList()
    {
        InitializeComponent();
        _excelService = new ExcelService(Helper.DateBase);
        Filters();
    }

    private void Filters()
    {
        schoolList = Helper.DateBase.Schools.ToList();

        var Search = (SearchTextN.Text ?? "").ToLower().Split(' ');

        schoolList = schoolList.Where(s => Search.All(ter => s.Name.Contains(ter.ToLower()) ||
                                                         s.SchoolNumber.Contains(ter.ToLower()))).ToList();

        ListBox_School.ItemsSource = schoolList;
    }

    /// <summary>
    /// Вход в школу
    /// </summary>
    private void ListBox_DoubleTapped_Button_School(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        var selectedSchool = ListBox_School.SelectedItem as School;
        if (selectedSchool == null) return;

        var mainWindow = new MainWindow(selectedSchool.Id);
        mainWindow.Show();
        this.Close();
    }

    private void TextBox_TextChanged(object? sender, Avalonia.Controls.TextChangedEventArgs e) => Filters();

    /// <summary>
    /// Вход под админом
    /// </summary>
    private void Button_Click_Admin(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        LoginWindow loginWindow = new LoginWindow();
        loginWindow.Show();
        Close();
    }

    private async void Button_Click_Unload_data(object? sender, RoutedEventArgs e)
    {
        var students = Helper.DateBase.Students.ToList();
        await _excelService.ExportStudentsToExcel(this, students);
    }

    private async void Button_Click_Load_data(object? sender, RoutedEventArgs e)
    {
        if (await _excelService.ImportStudentsFromExcel(this))
        {
            Filters();
        }
    }

    private async void Button_Click_Redact_School(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var newSchool = new School();
        var addAdnRedactOlympGia = new AddAdnRedactOlympGia(newSchool);
        bool result = await addAdnRedactOlympGia.ShowDialog(this);
        if (result)
        {
            ListBox_School.ItemsSource = Helper.DateBase.Schools.ToList();
        }
    }
}