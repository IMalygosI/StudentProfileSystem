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


namespace StudentProfileSystem
{
    public partial class MainWindow : Window
    {
        List<Student> students = new List<Student>(); // Загрузка учеников
        List<Student> Filter_students = new List<Student>(); // фильрация учеников

        public MainWindow()
        {
            InitializeComponent();

            ComboBoxGia.SelectionChanged += ComboBoxGia_SelectionChanged;
            ComboBoxOlimpiad.SelectionChanged += ComboBoxOlimpiad_SelectionChanged;
            ComboBoxGiaType.SelectionChanged += ComboBoxGiaType_SelectionChanged;

            LoadInitialData();
        }

        private void TextBox_TextChanged(object? sender, Avalonia.Controls.TextChangedEventArgs e) => Filters();
        private void ComboBoxGia_SelectionChanged(object? sender, SelectionChangedEventArgs e) => Filters();
        private void ComboBoxOlimpiad_SelectionChanged(object? sender, SelectionChangedEventArgs e) => Filters();
        private void ComboBoxGiaType_SelectionChanged(object? sender, SelectionChangedEventArgs e) => Filters();

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
            students = Helper.DateBase.Students.Include(z => z.Class)
                                                .Include(z => z.School)
                                                .Include(x => x.StudentGiaResults)
                                                    .ThenInclude(x2 => x2.IdGiaSubjectsNavigation)
                                                    .ThenInclude(x3 => x3.GiaSubjectsNavigation)
                                                .Include(a => a.StudentOlympiadParticipations)
                                                    .ThenInclude(a2 => a2.IdOlympiadsNavigation)
                                                    .ThenInclude(a3 => a3.OlympiadsItemsNavigation)
                                                .ToList();

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
            // Тип ГИА
            var distinctGiaType = Helper.DateBase.GiaSubjects.Include(x => x.GiaType)
                                                                .GroupBy(x => x.GiaType.Name)
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

            distinctGiaType.Insert(0, new GiaSubject
            {
                GiaType = new GiaType { Name = "ОГЭ/ЕГЭ" },
            });

            // Загрузка данных в ComboBox
            ComboBoxGia.ItemsSource = distinctGiaSubjects;
            ComboBoxGia.SelectedIndex = 0;

            ComboBoxOlimpiad.ItemsSource = distinctOlympiads;
            ComboBoxOlimpiad.SelectedIndex = 0;

            ComboBoxGiaType.ItemsSource = distinctGiaType;
            ComboBoxGiaType.SelectedIndex = 0;
        }

        /// <summary>
        /// Фильтрация и сортировка, загрузка данных в ListBox
        /// </summary>
        private void Filters()
        {
            var filtered = new List<Student>(students);

            // Фильтрация по поисковику
            filtered = SearchStudents(filtered, SearchTextN.Text);

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

            // Сортировка по типу ГИА "ОГЭ/ЕГЭ"
            if (ComboBoxGiaType.SelectedIndex > 0)
            {
                var selectedType = (GiaSubject)ComboBoxGiaType.SelectedItem;
                filtered = FilterGiaType(filtered, selectedType);
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
        /// Фильрация по параметру Тип ГИА "ОГЭ/ЕГЭ"
        /// </summary>
        /// <param name="students"></param>
        /// <param name="selectedType"></param>
        /// <returns></returns>
        private List<Student> FilterGiaType(List<Student> students, GiaSubject selectedType)
        {
            if (students == null) return new List<Student>();
            if (selectedType?.GiaType == null) return students;

            return students.Where(s => s.StudentGiaResults?.Where(g => g?.IdGiaSubjectsNavigation?.GiaType != null)
                                                           .Any(g => g.IdGiaSubjectsNavigation.GiaType.Name == selectedType.GiaType.Name) ?? false).ToList();
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

                return searchTerms.All(term => fullName.Contains(term) ||
                                               student.School.Name.ToLower().Contains(term) ||
                                               student.School.SchoolNumber.ToLower().Contains(term) ||
                                               student.Class.ClassesNumber.Contains(term));
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
        /// Вывод диалогового окна с подтверждением Удаления Stidents
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MenuItem_Click_Delete(object sender, RoutedEventArgs e)
        {
            var stud = ListBox_Student.SelectedItem as Student;
            if (stud == null) return;

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

            // Создание окна
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
                                Text = $"Вы точно хотите удалить ученика: {stud.LastName} {stud.FirstName} {stud.Patronymic}?",
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

            // Флаг для отслеживания подтверждения удаления
            bool isConfirmed = false;

            Yes_Button.Click += async (s, args) =>
            {
                isConfirmed = true; // подтверждение удаления
                DeleteConfirmationWindow.Close();
            };

            No_Button.Click += (s, args) =>
            {
                DeleteConfirmationWindow.Close();
            };

            await DeleteConfirmationWindow.ShowDialog(this);

            // Если пользователь не подтвердил удаление выход
            if (!isConfirmed) return;

            using (var transaction = Helper.DateBase.Database.BeginTransaction())
            {
                try
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
        /// Выгрузка данных в формате Exel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_Upload_data(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
        }

        /// <summary>
        /// Загрузка данных из Exel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_Load_data(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
        }
    }
}