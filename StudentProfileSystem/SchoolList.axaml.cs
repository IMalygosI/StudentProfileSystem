using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Microsoft.EntityFrameworkCore;
using StudentProfileSystem.Models;
using Avalonia.Layout;
using StudentProfileSystem.Services;
using System.Threading.Tasks;
using ClosedXML.Excel;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Avalonia.VisualTree;
using System.IO;
using StudentProfileSystem.Context;
using MsBox.Avalonia.ViewModels.Commands;

namespace StudentProfileSystem
{
    public partial class SchoolList : Window
    {
        private readonly ExcelService _excelService;
        private List<School> schoolList = new List<School>();
        private readonly ImcContext _context = Helper.DateBase;

        public SchoolList()
        {
            InitializeComponent();
            _excelService = new ExcelService(Helper.DateBase);
            LoadData();
        }

        private void LoadData()
        {
            schoolList = Helper.DateBase.Schools.ToList();
            ListBox_School.ItemsSource = schoolList;
        }

        private void Filters()
        {
            var searchText = (SearchTextN.Text ?? "").ToLower().Split(' ');
            schoolList = Helper.DateBase.Schools.Where(s => searchText.All(term => s.Name.ToLower().Contains(term) || s.SchoolNumber.ToLower().Contains(term))).ToList();
            ListBox_School.ItemsSource = schoolList;
        }

        private void TextBox_TextChanged(object? sender, Avalonia.Controls.TextChangedEventArgs e) => Filters();

        private void ListBox_DoubleTapped_Button_School(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            var selectedSchool = ListBox_School.SelectedItem as School;
            if (selectedSchool == null) return;

            var mainWindow = new MainWindow(selectedSchool.Id);
            mainWindow.Show();
            this.Close();
        }

        private void Button_Click_Admin(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            new LoginWindow().Show();
            Close();
        }

        private async void Button_Click_Unloaddata(object? sender, RoutedEventArgs e)
        {
            await _excelService.ExportStudentsToExcel(this);
        }

        private async void Button_Click_Redact_School(object? sender, RoutedEventArgs e)
        {
            var school = ListBox_School.SelectedItem as School;
            if (school == null) return;

            var addAdnRedactOlympGia = new AddAdnRedactOlympGia(school);
            bool result = await addAdnRedactOlympGia.ShowDialog(this);
            if (result)
            {
                ListBox_School.ItemsSource = Helper.DateBase.Schools.ToList();
            }
        }

        private async void Button_Click_Add_School(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
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
}