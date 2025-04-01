using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Microsoft.EntityFrameworkCore;
using StudentProfileSystem.Models;

namespace StudentProfileSystem
{
    public partial class MainWindow : Window
    {
        List<Student> students = new List<Student>();
        public MainWindow()
        {
            InitializeComponent();

            DownloadAndUpdate();

        }

        /// <summary>
        /// Загрузка и обновление данных
        /// </summary>
        public void DownloadAndUpdate()
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

            var searchTerms = (SearchTextN.Text ?? "").ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            students = students.Where(student =>
            {
                // Объединяем LastName - Фамилию, FirstName - Имя, Patronymic - Отчество в единое поле "ФИО"
                string fullName = $"{student.LastName} {student.FirstName} {student.Patronymic}".ToLower();

                return searchTerms.All(term =>
                    fullName.Contains(term) ||
                    student.School.Name.ToLower().Contains(term) ||
                    student.School.SchoolNumber.ToLower().Contains(term));

            }).ToList();

            ListBox_Student.ItemsSource = students;
        }

        private void TextBox_TextChanged(object? sender, Avalonia.Controls.TextChangedEventArgs e) => DownloadAndUpdate();
    }
}