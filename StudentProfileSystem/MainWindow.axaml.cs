using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using StudentProfileSystem.Models;
using Avalonia.Media;
using MsBox.Avalonia.ViewModels.Commands;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Layout;
using DocumentFormat.OpenXml.Drawing;
using StudentProfileSystem.Services;


namespace StudentProfileSystem
{
    public partial class MainWindow : Window
    {
        private readonly ExcelService _excelService;

        List<Student> students = new List<Student>(); // Загрузка учеников
        List<Student> Filter_students = new List<Student>(); // фильрация учеников

        private readonly int _schoolId;

        public MainWindow(int schoolId)
        {
            InitializeComponent();

            _schoolId = schoolId;
            _excelService = new ExcelService(Helper.DateBase);

            var school = Helper.DateBase.Schools.FirstOrDefault(s => s.Id == _schoolId);
            School_Name_Id.IsVisible = true;
            SchoolNumber.IsVisible = true;
            School_Name_Id.Text = school?.Name;

            ComboBoxGia.SelectionChanged += ComboBoxGia_SelectionChanged;
            ComboBoxOlimpiad.SelectionChanged += ComboBoxOlimpiad_SelectionChanged;
            ComboBoxParallel.SelectionChanged += ComboBoxParallel_SelectionChanged;

            LoadInitialData();
        }

        public MainWindow()
        {
            InitializeComponent();

            School_Name_Id.IsVisible = false;
            School_Name.IsVisible = false;

            _excelService = new ExcelService(Helper.DateBase);

            ComboBoxGia.SelectionChanged += ComboBoxGia_SelectionChanged;
            ComboBoxOlimpiad.SelectionChanged += ComboBoxOlimpiad_SelectionChanged;
            ComboBoxParallel.SelectionChanged += ComboBoxParallel_SelectionChanged;

            LoadInitialData();
        }

        private void TextBox_TextChanged(object? sender, Avalonia.Controls.TextChangedEventArgs e) => Filters();
        private void ComboBoxGia_SelectionChanged(object? sender, SelectionChangedEventArgs e) => Filters();
        private void ComboBoxOlimpiad_SelectionChanged(object? sender, SelectionChangedEventArgs e) => Filters();
        private void ComboBoxGiaType_SelectionChanged(object? sender, SelectionChangedEventArgs e) => Filters();
        private void ComboBoxParallel_SelectionChanged(object? sender, SelectionChangedEventArgs e) => Filters();

        /// <summary>
        /// Загрузка данных {вызывать при обновлении, полное обновление данных}
        /// </summary>
        private void LoadInitialData()
        {
            LoadStudents();
            LoadComboBox();
        }

        /// <summary>
        /// Загрузка учащихся 
        /// </summary>
        public void LoadStudents()
        {
            if (_schoolId > 0) //  загружаем только студентов одной школы
            {
                students = Helper.DateBase.Students.Where(s => s.SchoolId == _schoolId)
                                                   .Include(z => z.Class)
                                                   .Include(z => z.School)
                                                   .Include(x => x.StudentGiaResults)
                                                       .ThenInclude(x2 => x2.IdGiaSubjectsNavigation)
                                                       .ThenInclude(x3 => x3.GiaSubjectsNavigation)
                                                  .Include(x => x.StudentGiaResults)
                                                       .ThenInclude(x2 => x2.IdGiaSubjectsNavigation)
                                                  .Include(a => a.StudentOlympiadParticipations)
                                                       .ThenInclude(a2 => a2.IdOlympiadsNavigation)
                                                       .ThenInclude(a3 => a3.OlympiadsNavigation)
                                                  .Include(a => a.StudentOlympiadParticipations)
                                                       .ThenInclude(a2 => a2.IdOlympiadsNavigation)
                                                       .ThenInclude(a3 => a3.OlympiadsItemsNavigation).ToList();
            }
            else // загружаем всех студентов
            {
                students = Helper.DateBase.Students.Include(z => z.Class)
                                                   .Include(z => z.School)
                                                   .Include(x => x.StudentGiaResults)
                                                       .ThenInclude(x2 => x2.IdGiaSubjectsNavigation)
                                                       .ThenInclude(x3 => x3.GiaSubjectsNavigation)
                                                  .Include(x => x.StudentGiaResults)
                                                       .ThenInclude(x2 => x2.IdGiaSubjectsNavigation)
                                                  .Include(a => a.StudentOlympiadParticipations)
                                                       .ThenInclude(a2 => a2.IdOlympiadsNavigation)
                                                       .ThenInclude(a3 => a3.OlympiadsNavigation)
                                                  .Include(a => a.StudentOlympiadParticipations)
                                                       .ThenInclude(a2 => a2.IdOlympiadsNavigation)
                                                       .ThenInclude(a3 => a3.OlympiadsItemsNavigation).ToList();
            }

            Filter_students = new List<Student>(students);
            Filters();
        }

        /// <summary>
        /// Загрузка данных в ComboBox данных из Olympiads и GiaSubjects
        /// </summary>
        public void LoadComboBox()
        {
            // Предметы
            var distinctGiaSubjects = Helper.DateBase.GiaSubjects.Include(x => x.GiaSubjectsNavigation)
                                                                    .GroupBy(x => x.GiaSubjectsNavigation.Name)
                                                                        .Select(g => g.First())
                                                                 .ToList();
            // Олимпиады
            var distinctOlympiads = Helper.DateBase.Olympiads.Include(a => a.OlympiadsNavigation)
                                                                .GroupBy(a => a.OlympiadsNavigation.Name)
                                                                    .Select(g => g.First())
                                                             .ToList();

            // Добавление заголовков в ComboBox
            distinctGiaSubjects.Insert(0, new GiaSubject
            {
                GiaSubjectsNavigation = new Item { Name = "ГИА" },
            });

            distinctOlympiads.Insert(0, new Olympiad
            {
                OlympiadsNavigation = new OlympiadsType { Name = "Олимпиады" },
            });

            // Загрузка данных в ComboBox
            ComboBoxGia.ItemsSource = distinctGiaSubjects;
            ComboBoxGia.SelectedIndex = 0;

            ComboBoxOlimpiad.ItemsSource = distinctOlympiads;
            ComboBoxOlimpiad.SelectedIndex = 0;
        }

        /// <summary>
        /// Фильтрация и сортировка, загрузка данных в ListBox
        /// </summary>
        private void Filters()
        {
            var filtered = new List<Student>(students);

            // Фильтрация по поисковику
            if (!string.IsNullOrWhiteSpace(SearchTextN.Text))
            {
                filtered = SearchStudents(filtered, SearchTextN.Text);
            }

            // Сортировка по ГИА
            if (ComboBoxGia.SelectedIndex > 0)
            {
                var selectedGia = (GiaSubject)ComboBoxGia.SelectedItem;
                filtered = FilterGiaSubject(filtered, selectedGia);
            }

            // Сортировка по олимпиадам
            if (ComboBoxOlimpiad.SelectedIndex > 0)
            {
                var selectedOlympiad = (Olympiad)ComboBoxOlimpiad.SelectedItem;
                filtered = FilterOlympiads(filtered, selectedOlympiad);
            }

            // Сортировка по параллелям (классам)
            if (ComboBoxParallel.SelectedIndex > 0)
            {
                var selectedClass = ComboBoxParallel.SelectedItem as ComboBoxItem;
                if (selectedClass != null)
                {
                    filtered = filtered.Where(s => s.Class?.ClassesNumber?.StartsWith(selectedClass.Content.ToString()) ?? false).ToList();
                }
            }

            Filter_students = filtered;
            ListBox_Student.ItemsSource = Filter_students;
        }

        /// <summary>
        /// Сортировка по "Предмету ГИА"
        /// </summary>
        /// <param name="students"></param>
        /// <param name="selectedGia"></param>
        /// <returns></returns>
        private List<Student> FilterGiaSubject(List<Student> students, GiaSubject selectedGia)
        {
            if (students == null) return new List<Student>();
            if (selectedGia?.GiaSubjectsNavigation == null) return students;

            return students.Where(s => s.StudentGiaResults?.Where(g => g?.IdGiaSubjectsNavigation?.GiaSubjectsNavigation != null)
                                                           .Any(g => g.IdGiaSubjectsNavigation.GiaSubjectsNavigation.Name == selectedGia.GiaSubjectsNavigation.Name) ?? false).ToList();
        }

        /// <summary>
        /// Филтрация по параметру "Олимпиада"
        /// </summary>
        /// <param name="students"></param>
        /// <param name="selectedOlympiad"></param>
        /// <returns></returns>
        private List<Student> FilterOlympiads(List<Student> students, Olympiad selectedOlympiad)
        {
            if (students == null) return new List<Student>();
            if (selectedOlympiad?.OlympiadsNavigation == null) return students;

            return students.Where(s => s.StudentOlympiadParticipations?.Where(p => p?.IdOlympiadsNavigation?.OlympiadsNavigation != null)
                                                                       .Any(p => p.IdOlympiadsNavigation.OlympiadsNavigation.Name == selectedOlympiad.OlympiadsNavigation.Name) ?? false).ToList();
        }

        /// <summary>
        /// Фильтрация студентов по поисковому запросу
        /// </summary>
        private List<Student> SearchStudents(List<Student> students, string searchText)
        {
            if (students == null || !students.Any()) return new List<Student>();
            if (string.IsNullOrWhiteSpace(searchText)) return students;

            var searchTerms = (searchText ?? "").ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return students.Where(student =>
            {
                if (student == null) return false;

                string fullName = $"{student.LastName} {student.FirstName} {student.Patronymic}".ToLower();

                return searchTerms.All(term => fullName.Contains(term) || student.Class.ClassesNumber.Contains(term));
            }).ToList();
        }

        /// <summary>
        /// Добавление нового Student
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click_AddStudents(object? sender, RoutedEventArgs e)
        {
            this.Classes.Add("blur-effect");

            var studentEditWindow = new StudentEditWindow();
            await studentEditWindow.ShowDialog(this);

            this.Classes.Remove("blur-effect");
            LoadStudents(); // ? Проверить!
        }

        /// <summary>
        /// Редактирование Student
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ListBox_DoubleTapped_Button_Redact(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.Classes.Add("blur-effect");

            try
            {
                var stud = ListBox_Student.SelectedItem as Student;
                if (stud == null) return;

                var studentEditWindow = new StudentEditWindow(stud);
                await studentEditWindow.ShowDialog(this);
                LoadStudents(); // ? Проверить!
            }
            finally
            {
                this.Classes.Remove("blur-effect");
            }
        }

        /// <summary>
        /// Удаления Stidents и данных что с ним связанны
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MenuItem_Click_Delete(object sender, RoutedEventArgs e)
        {
            var selectedStudents = ListBox_Student.SelectedItems.Cast<Student>().ToList();
            if (!selectedStudents.Any()) return;

            var message = selectedStudents.Count == 1
                ? $"Вы точно хотите удалить ученика: {selectedStudents[0].LastName} {selectedStudents[0].FirstName} {selectedStudents[0].Patronymic}?"
                : $"Вы точно хотите удалить {selectedStudents.Count} выбранных учеников?";

            var Yes_Button = new Button
            {
                Content = new TextBlock
                {
                    Text = "Да",
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                },
                Width = 100,
                Height = 30,
                Background = Brushes.Green,
                Margin = new Thickness(5)
            };

            var No_Button = new Button
            {
                Content = new TextBlock
                {
                    Text = "Нет",
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                },
                Width = 100,
                Height = 30,
                Background = Brushes.Red,
                Margin = new Thickness(5)
            };

            var DeleteConfirmationWindow = new Window
            {
                Title = "Подтверждение удаления",
                Width = 550,
                Height = 250,
                MinHeight = 250,
                WindowState = WindowState.Normal,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                SizeToContent = SizeToContent.Manual,
                CanResize = false,
                Content = new Border
                {
                    BorderBrush = Brushes.Red,
                    BorderThickness = new Thickness(2),
                    Child = new Grid
                    {
                        Margin = new Thickness(15),
                        Children =
                {
                    new StackPanel
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = message,
                                TextWrapping = TextWrapping.Wrap,
                                TextAlignment = TextAlignment.Center,
                                Margin = new Thickness(0, 0, 0, 20),
                                FontSize = 16
                            },
                            new DockPanel
                            {
                                HorizontalAlignment = HorizontalAlignment.Center,
                                LastChildFill = false,
                                Children =
                                {
                                    Yes_Button,
                                    No_Button
                                }
                            }
                        }
                    }
                }
                    }
                }
            };

            bool isConfirmed = false;

            Yes_Button.Click += async (s, args) =>
            {
                isConfirmed = true;
                DeleteConfirmationWindow.Close();
            };

            No_Button.Click += (s, args) =>
            {
                DeleteConfirmationWindow.Close();
            };

            await DeleteConfirmationWindow.ShowDialog(this);

            if (!isConfirmed) return;

            using (var transaction = Helper.DateBase.Database.BeginTransaction())
            {
                try
                {
                    foreach (var stud in selectedStudents)
                    {
                        // Удаление связанных записей ГИА
                        var giaResults = Helper.DateBase.StudentGiaResults
                            .Where(x => x.IdStudents == stud.Id);
                        Helper.DateBase.StudentGiaResults.RemoveRange(giaResults);

                        // Удаление связанных записей Олимпиады
                        var olympiads = Helper.DateBase.StudentOlympiadParticipations
                            .Where(x => x.IdStudents == stud.Id);
                        Helper.DateBase.StudentOlympiadParticipations.RemoveRange(olympiads);

                        // Удаление Students
                        Helper.DateBase.Students.Remove(stud);
                    }

                    await Helper.DateBase.SaveChangesAsync();
                    transaction.Commit();

                    LoadStudents();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    await ShowErrorDialog(ex.Message, this);
                }
            }
        }

        /// <summary>
        /// Вывод диалогового окна с сообщением об ошибке
        /// </summary>
        /// <param name="message"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        private async Task ShowErrorDialog(string message, Window owner)
        {
            var Ok_Button = new Button
            {
                Content = "OK",
                Width = 100,
                Height = 30,
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var ErrorWindowDialog = new Window
            {
                Title = "Ошибка",
                Width = 550,
                Height = 250,
                MinHeight = 250,
                WindowState = WindowState.Normal,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                SizeToContent = SizeToContent.Manual,
                CanResize = false,
                Content = new Border
                {
                    BorderBrush = Brushes.Red,
                    BorderThickness = new Thickness(2),
                    Child = new Grid
                    {
                        Margin = new Thickness(15),
                        Children =
                {
                    new StackPanel
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = message,
                                TextWrapping = TextWrapping.Wrap,
                                TextAlignment = TextAlignment.Center,
                                FontSize = 14,
                                Margin = new Thickness(0, 0, 0, 20)  // Отступ снизу
                            },
                            Ok_Button
                        }
                    }
                }
                    }
                }
            };

            Ok_Button.Click += (s, args) => ErrorWindowDialog.Close();
            await ErrorWindowDialog.ShowDialog(owner);
        }

        /// <summary>
        /// Меню с ГИА
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click_ComboBoxGia_Setting(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Classes.Add("blur-effect");

            try
            {
                string olymp = "ГИА";
                SettingGiaOlimpiad settingGiaOlimpiad = new SettingGiaOlimpiad(olymp);
                await settingGiaOlimpiad.ShowDialog(this);

                LoadInitialData();
            }
            finally
            {
                this.Classes.Remove("blur-effect");
                LoadStudents();
            }
        }

        /// <summary>
        /// Меню с Олимпиадами
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click_ComboBoxOlimpiad_Setting(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            this.Classes.Add("blur-effect");

            try
            {
                string olymp = "Олимпиады";
                SettingGiaOlimpiad settingGiaOlimpiad = new SettingGiaOlimpiad(olymp);
                await settingGiaOlimpiad.ShowDialog(this);
                LoadInitialData();
            }
            finally
            {
                this.Classes.Remove("blur-effect");
                LoadStudents();
            }
        }

        /// <summary>
        /// Выгрузка данных в формате Exel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click_Unload_data(object? sender, RoutedEventArgs e)
        {
            await _excelService.ExportStudentsToExcel(this, students);
        }

        /// <summary>
        /// Загрузка данных из Exel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click_Load_data(object? sender, RoutedEventArgs e)
        {
            if (await _excelService.ImportStudentsFromExcel(this))
            {
                LoadInitialData(); // Обновляем данные после успешного импорта
            }
        }

        /// <summary>
        /// Обновить данные
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_Upload_data(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
        }

        private void Button_Click_Exit(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            SchoolList window = new SchoolList();
            window.Show();
            Close();
        }

        /// <summary>
        /// Удаление школы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click_DeleteSchool(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (_schoolId <= 0) return;

            var school = Helper.DateBase.Schools.FirstOrDefault(s => s.Id == _schoolId);
            if (school == null) return;

            // Подтверждение удаления
            var message = $"Вы точно хотите удалить школу: {school.Name}?\n" +
                          "ВНИМАНИЕ: Это действие также удалит всех учеников этой школы " +
                          "и все связанные с ними данные (результаты ГИА, участия в олимпиадах и т.д.)";

            var Yes_Button = new Button
            {
                Content = new TextBlock
                {
                    Text = "Да",
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                },
                Width = 100,
                Height = 30,
                Background = Brushes.Green,
                Margin = new Thickness(5)
            };

            var No_Button = new Button
            {
                Content = new TextBlock
                {
                    Text = "Нет",
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                },
                Width = 100,
                Height = 30,
                Background = Brushes.Red,
                Margin = new Thickness(5)
            };

            var DeleteConfirmationWindow = new Window
            {
                Title = "Подтверждение удаления школы",
                Width = 550,
                Height = 250,
                MinHeight = 250,
                WindowState = WindowState.Normal,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                SizeToContent = SizeToContent.Manual,
                CanResize = false,
                Content = new Border
                {
                    BorderBrush = Brushes.Red,
                    BorderThickness = new Thickness(2),
                    Child = new Grid
                    {
                        Margin = new Thickness(15),
                        Children =
                {
                    new StackPanel
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Children =
                        {
                            new TextBlock
                            {
                                Text = message,
                                TextWrapping = TextWrapping.Wrap,
                                TextAlignment = TextAlignment.Center,
                                Margin = new Thickness(0, 0, 0, 20),
                                FontSize = 16
                            },
                            new DockPanel
                            {
                                HorizontalAlignment = HorizontalAlignment.Center,
                                LastChildFill = false,
                                Children =
                                {
                                    Yes_Button,
                                    No_Button
                                }
                            }
                        }
                    }
                }
                    }
                }
            };

            bool isConfirmed = false;

            Yes_Button.Click += async (s, args) =>
            {
                isConfirmed = true;
                DeleteConfirmationWindow.Close();
            };

            No_Button.Click += (s, args) =>
            {
                DeleteConfirmationWindow.Close();
            };

            await DeleteConfirmationWindow.ShowDialog(this);

            if (!isConfirmed) return;

            // Получаем всех студентов школы
            var schoolStudents = Helper.DateBase.Students
                .Where(s => s.SchoolId == _schoolId)
                .Include(s => s.StudentGiaResults)
                .Include(s => s.StudentOlympiadParticipations)
                .ToList();

            using (var transaction = Helper.DateBase.Database.BeginTransaction())
            {
                try
                {
                    // Удаляем все связанные данные студентов
                    foreach (var student in schoolStudents)
                    {
                        // Удаление результатов ГИА
                        if (student.StudentGiaResults != null && student.StudentGiaResults.Any())
                        {
                            Helper.DateBase.StudentGiaResults.RemoveRange(student.StudentGiaResults);
                        }

                        // Удаление участий в олимпиадах
                        if (student.StudentOlympiadParticipations != null && student.StudentOlympiadParticipations.Any())
                        {
                            Helper.DateBase.StudentOlympiadParticipations.RemoveRange(student.StudentOlympiadParticipations);
                        }
                    }

                    // Удаляем самих студентов
                    Helper.DateBase.Students.RemoveRange(schoolStudents);

                    // Удаляем школу
                    Helper.DateBase.Schools.Remove(school);

                    await Helper.DateBase.SaveChangesAsync();
                    transaction.Commit();

                    // Открываем окно со списком школ
                    SchoolList window = new SchoolList();
                    window.Show();
                    Close();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    await ShowErrorDialog($"Ошибка при удалении школы: {ex.Message}", this);
                }
            }
        }
    }
}