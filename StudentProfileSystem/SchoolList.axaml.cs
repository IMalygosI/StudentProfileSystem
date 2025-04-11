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
using System.Threading.Tasks;

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

            // �������� ������
            LoadData();
        }

        /// <summary>
        /// �������� ������ ���� �� ���� ������
        /// </summary>
        private void LoadData()
        {
            // �������� ��� ����� �� ���� ������
            schoolList = Helper.DateBase.Schools.ToList();
            // ������������� �������� ������ ��� ListBox
            ListBox_School.ItemsSource = schoolList;
        }

        /// <summary>
        /// ���������� ���� �� ���������� �������
        /// </summary>
        private void Filters()
        {
            var searchText = (SearchTextN.Text ?? "").ToLower().Split(' ');

            schoolList = Helper.DateBase.Schools.Where(s => searchText.All(term => s.Name.ToLower().Contains(term) || s.SchoolNumber.ToLower().Contains(term))).ToList();

            ListBox_School.ItemsSource = schoolList;
        }

        /// <summary>
        /// ���������� ��������� ������ � ���� ������
        /// </summary>
        private void TextBox_TextChanged(object? sender, Avalonia.Controls.TextChangedEventArgs e) => Filters();

        /// <summary>
        /// ���������� �������� ����� �� ����� (�������� �����)
        /// </summary>
        private void ListBox_DoubleTapped_Button_School(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            var selectedSchool = ListBox_School.SelectedItem as School;
            if (selectedSchool == null) return;

            var mainWindow = new MainWindow(selectedSchool.Id);
            mainWindow.Show();
            this.Close();
        }

        /// <summary>
        /// ���� � ����� ��������������
        /// </summary>
        private void Button_Click_Admin(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            new LoginWindow().Show();
            Close();
        }

        /// <summary>
        /// �������� ���� ������ ��������� � Excel
        /// </summary>
        private async void Button_Click_Unloaddata(object? sender, RoutedEventArgs e)
        {
            try
            {
                // �������� ���� ��������� � ������ �����������
                var students = Helper.DateBase.Students
                    .Include(s => s.Class)  // ���������� � ������
                    .Include(s => s.School) // ���������� � �����
                    .Include(s => s.StudentGiaResults)  // ���������� ���
                        .ThenInclude(g => g.IdGiaSubjectsNavigation)
                        .ThenInclude(g => g.GiaSubjectsNavigation)
                    .Include(s => s.StudentOlympiadParticipations)  // ������� � ����������
                        .ThenInclude(o => o.IdOlympiadsNavigation)
                        .ThenInclude(o => o.OlympiadsNavigation)
                    .Include(s => s.StudentOlympiadParticipations)
                        .ThenInclude(o => o.IdOlympiadsNavigation)
                        .ThenInclude(o => o.OlympiadsItemsNavigation)
                    .ToList();

                // ������� � Excel
                await _excelService.ExportStudentsToExcel(this, students);
            }
            catch (Exception ex)
            {
                await ShowCustomMessage("������", $"������ ��������: {ex.Message}", Brushes.Red);
            }
        }

        /// <summary>
        /// �������������� �����
        /// </summary>
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

        /// <summary>
        /// ���������� �������������� ���������
        /// </summary>
        private async Task ShowCustomMessage(string title, string message, IBrush borderBrush)
        {
            var dialog = new Window
            {
                Title = title,
                Width = 450,
                Height = 200,
                SizeToContent = SizeToContent.Manual,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new Border
                {
                    BorderBrush = borderBrush,
                    BorderThickness = new Thickness(2),
                    Padding = new Thickness(15),
                    Child = new StackPanel
                    {
                        Spacing = 10,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = message,
                                TextWrapping = TextWrapping.Wrap,
                                HorizontalAlignment = HorizontalAlignment.Center
                            },
                            new Button
                            {
                                Content = "OK",
                                Width = 80,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                Command = new RelayCommand(_ => (this.GetVisualRoot() as Window)?.Close())
                            }
                        }
                    }
                }
            };

            await dialog.ShowDialog(this);
        }

        /// <summary>
        /// ���������� ����� � ������
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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