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

            // Загрузка данных
            LoadData();
        }

        /// <summary>
        /// Загрузка списка школ из базы данных
        /// </summary>
        private void LoadData()
        {
            // Получаем все школы из базы данных
            schoolList = Helper.DateBase.Schools.ToList();
            // Устанавливаем источник данных для ListBox
            ListBox_School.ItemsSource = schoolList;
        }

        /// <summary>
        /// Фильтрация школ по поисковому запросу
        /// </summary>
        private void Filters()
        {
            var searchText = (SearchTextN.Text ?? "").ToLower().Split(' ');

            schoolList = Helper.DateBase.Schools.Where(s => searchText.All(term => s.Name.ToLower().Contains(term) || s.SchoolNumber.ToLower().Contains(term))).ToList();

            ListBox_School.ItemsSource = schoolList;
        }

        /// <summary>
        /// Обработчик изменения текста в поле поиска
        /// </summary>
        private void TextBox_TextChanged(object? sender, Avalonia.Controls.TextChangedEventArgs e) => Filters();

        /// <summary>
        /// Обработчик двойного клика по школе (открытие школы)
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
        /// Вход в режим администратора
        /// </summary>
        private void Button_Click_Admin(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            new LoginWindow().Show();
            Close();
        }

        /// <summary>
        /// Выгрузка ВСЕХ данных студентов в Excel
        /// </summary>
        private async void Button_Click_Unloaddata(object? sender, RoutedEventArgs e)
        {
            try
            {
                // Получаем всех студентов с полной информацией
                var students = Helper.DateBase.Students
                    .Include(s => s.Class)  // Информация о классе
                    .Include(s => s.School) // Информация о школе
                    .Include(s => s.StudentGiaResults)  // Результаты ГИА
                        .ThenInclude(g => g.IdGiaSubjectsNavigation)
                        .ThenInclude(g => g.GiaSubjectsNavigation)
                    .Include(s => s.StudentOlympiadParticipations)  // Участие в олимпиадах
                        .ThenInclude(o => o.IdOlympiadsNavigation)
                        .ThenInclude(o => o.OlympiadsNavigation)
                    .Include(s => s.StudentOlympiadParticipations)
                        .ThenInclude(o => o.IdOlympiadsNavigation)
                        .ThenInclude(o => o.OlympiadsItemsNavigation)
                    .ToList();

                // Экспорт в Excel
                await _excelService.ExportStudentsToExcel(this, students);
            }
            catch (Exception ex)
            {
                await ShowCustomMessage("Ошибка", $"Ошибка экспорта: {ex.Message}", Brushes.Red);
            }
        }

        /// <summary>
        /// Редактирование школы
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
        /// Отображает информационное сообщение
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
        /// Добавление школы в ручную
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