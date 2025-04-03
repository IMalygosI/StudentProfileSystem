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
        List<Student> students = new List<Student>(); // �������� ��������
        List<Student> Filter_students = new List<Student>(); // ��������� ��������

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
        /// �������� ������ {�������� ��� ����������, ������ ���������� ������}
        /// </summary>
        private void LoadInitialData()
        {
            LoadStudents();
            LoadComboBox();
        }

        /// <summary>
        /// �������� �������� 
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
        /// �������� ������ � ComboBox ������ �� Olympiads � GiaSubjects
        /// </summary>
        public void LoadComboBox()
        {
            // ��������
            var distinctGiaSubjects = Helper.DateBase.GiaSubjects.Include(x => x.GiaSubjectsNavigation)
                                                                    .GroupBy(x => x.GiaSubjectsNavigation.Name)
                                                                        .Select(g => g.First())
                                                                 .ToList();
            // ���������
            var distinctOlympiads = Helper.DateBase.Olympiads.Include(a => a.OlympiadsNavigation)
                                                                .GroupBy(a => a.OlympiadsNavigation.Name)
                                                                    .Select(g => g.First())
                                                             .ToList();
            // ��� ���
            var distinctGiaType = Helper.DateBase.GiaSubjects.Include(x => x.GiaType)
                                                                .GroupBy(x => x.GiaType.Name)
                                                                    .Select(g => g.First())
                                                             .ToList();

            // ���������� ���������� � ComboBox
            distinctGiaSubjects.Insert(0, new GiaSubject
            {
                GiaSubjectsNavigation = new Item { Name = "���" },
            });

            distinctOlympiads.Insert(0, new Olympiad
            {
                OlympiadsNavigation = new OlympiadsType { Name = "���������" },
            });

            distinctGiaType.Insert(0, new GiaSubject
            {
                GiaType = new GiaType { Name = "���/���" },
            });

            // �������� ������ � ComboBox
            ComboBoxGia.ItemsSource = distinctGiaSubjects;
            ComboBoxGia.SelectedIndex = 0;

            ComboBoxOlimpiad.ItemsSource = distinctOlympiads;
            ComboBoxOlimpiad.SelectedIndex = 0;

            ComboBoxGiaType.ItemsSource = distinctGiaType;
            ComboBoxGiaType.SelectedIndex = 0;
        }

        /// <summary>
        /// ���������� � ����������, �������� ������ � ListBox
        /// </summary>
        private void Filters()
        {
            var filtered = new List<Student>(students);

            // ���������� �� ����������
            filtered = SearchStudents(filtered, SearchTextN.Text);

            // ���������� �� ���
            if (ComboBoxGia.SelectedIndex > 0)
            {
                var selectedGia = (GiaSubject)ComboBoxGia.SelectedItem;
                filtered = FilterGiaSubject(filtered, selectedGia);
            }

            // ���������� �� ����������
            if (ComboBoxOlimpiad.SelectedIndex > 0)
            {
                var selectedOlympiad = (Olympiad)ComboBoxOlimpiad.SelectedItem;
                filtered = FilterOlympiads(filtered, selectedOlympiad);
            }

            // ���������� �� ���� ��� "���/���"
            if (ComboBoxGiaType.SelectedIndex > 0)
            {
                var selectedType = (GiaSubject)ComboBoxGiaType.SelectedItem;
                filtered = FilterGiaType(filtered, selectedType);
            }

            Filter_students = filtered;
            ListBox_Student.ItemsSource = Filter_students;
        }

        /// <summary>
        /// ���������� �� "�������� ���"
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
        /// ��������� �� ��������� "���������"
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
        /// ��������� �� ��������� ��� ��� "���/���"
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
        /// ���������� ��������� �� ���������� �������
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
        /// ���������� ������ Student
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Button_Click_AddStudents(object? sender, RoutedEventArgs e)
        {
            this.Classes.Add("blur-effect");

            var studentEditWindow = new StudentEditWindow();
            await studentEditWindow.ShowDialog(this);

            this.Classes.Remove("blur-effect");
            LoadStudents(); // ? ���������!
        }

        /// <summary>
        /// �������������� Student
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
                LoadStudents(); // ? ���������!
            }
            finally
            {
                this.Classes.Remove("blur-effect");
            }
        }

        /// <summary>
        /// �������� Student
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click_Delete(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {



        }

        /// <summary>
        /// �������� ������ � ������� Exel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_Upload_data(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
        }

        /// <summary>
        /// �������� ������ �� Exel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_Load_data(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
        }
    }
}