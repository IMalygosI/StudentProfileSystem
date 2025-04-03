using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;
using StudentProfileSystem.Models;

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
                                               student.School.SchoolNumber.ToLower().Contains(term));
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
        /// Удаление Student
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click_Delete(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {



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