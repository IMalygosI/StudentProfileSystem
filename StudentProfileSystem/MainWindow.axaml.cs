using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Microsoft.EntityFrameworkCore;
using StudentProfileSystem.Models;
using Avalonia.Media;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Layout;
using StudentProfileSystem.Services;
using MsBox.Avalonia.ViewModels.Commands;
using ClosedXML.Excel;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using Avalonia.VisualTree;
using System.IO;
using StudentProfileSystem.Context;

namespace StudentProfileSystem
{
    public partial class MainWindow : Window
    {
        private readonly ExcelService _excelService;
        private readonly int _schoolId;
        private readonly ImcContext _context = Helper.DateBase;

        // Списки студентов
        List<Student> students = new List<Student>(); // Полный список учеников
        List<Student> Filter_students = new List<Student>(); // Отфильтрованный список учеников

        public MainWindow(int schoolId)
        {
            InitializeComponent();
            _schoolId = schoolId;
            _excelService = new ExcelService(Helper.DateBase);

            // Настройка отображения информации о школе
            var school = Helper.DateBase.Schools.FirstOrDefault(s => s.Id == _schoolId);
            School_Name_Id.IsVisible = true;
            SchoolNumber.IsVisible = true;
            School_Name_Id.Text = school?.Name;

            // Подписка на события ComboBox
            ComboBoxGia.SelectionChanged += ComboBoxGia_SelectionChanged;
            ComboBoxOlimpiad.SelectionChanged += ComboBoxOlimpiad_SelectionChanged;
            ComboBoxParallel.SelectionChanged += ComboBoxParallel_SelectionChanged;

            LoadInitialData();
        }

        public MainWindow()
        {
            InitializeComponent();

            // Скрываем элементы школы для режима администратора
            School_Name_Id.IsVisible = false;
            School_Name.IsVisible = false;

            _excelService = new ExcelService(Helper.DateBase);

            // Подписка на события ComboBox
            ComboBoxGia.SelectionChanged += ComboBoxGia_SelectionChanged;
            ComboBoxOlimpiad.SelectionChanged += ComboBoxOlimpiad_SelectionChanged;
            ComboBoxParallel.SelectionChanged += ComboBoxParallel_SelectionChanged;

            LoadInitialData();
        }

        // Обработчики событий фильтрации
        private void TextBox_TextChanged(object? sender, Avalonia.Controls.TextChangedEventArgs e) => Filters();
        private void ComboBoxGia_SelectionChanged(object? sender, SelectionChangedEventArgs e) => Filters();
        private void ComboBoxOlimpiad_SelectionChanged(object? sender, SelectionChangedEventArgs e) => Filters();
        private void ComboBoxGiaType_SelectionChanged(object? sender, SelectionChangedEventArgs e) => Filters();
        private void ComboBoxParallel_SelectionChanged(object? sender, SelectionChangedEventArgs e) => Filters();

        /// <summary>
        /// Загрузка начальных данных (студентов и ComboBox)
        /// </summary>
        private void LoadInitialData()
        {
            LoadStudents();
            LoadComboBox();
        }

        /// <summary>
        /// Загрузка списка студентов из базы данных
        /// </summary>
        public void LoadStudents()
        {
            IQueryable<Student> query = Helper.DateBase.Students
                .Include(z => z.Class)
                .Include(z => z.School)
                .Include(x => x.StudentGiaResults)
                    .ThenInclude(x2 => x2.IdGiaSubjectsNavigation)
                    .ThenInclude(x3 => x3.GiaSubjectsNavigation)
                .Include(a => a.StudentOlympiadParticipations)
                    .ThenInclude(a2 => a2.IdOlympiadsNavigation)
                    .ThenInclude(a3 => a3.OlympiadsNavigation)
                .Include(a => a.StudentOlympiadParticipations)
                    .ThenInclude(a2 => a2.IdOlympiadsNavigation)
                    .ThenInclude(a3 => a3.OlympiadsItemsNavigation);

            // Фильтрация по школе, если указан schoolId
            if (_schoolId > 0)
            {
                query = query.Where(s => s.SchoolId == _schoolId);
            }

            students = query.ToList();
            Filter_students = new List<Student>(students);
            Filters();
        }

        /// <summary>
        /// Загрузка данных в ComboBox (ГИА, олимпиады, классы)
        /// </summary>
        public void LoadComboBox()
        {
            // Загрузка предметов ГИА
            var distinctGiaSubjects = Helper.DateBase.GiaSubjects
                .Include(x => x.GiaSubjectsNavigation)
                .GroupBy(x => x.GiaSubjectsNavigation.Name)
                .Select(g => g.First())
                .ToList();

            // Загрузка типов олимпиад
            var distinctOlympiads = Helper.DateBase.Olympiads
                .Include(a => a.OlympiadsNavigation)
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

            // Установка источников данных для ComboBox
            ComboBoxGia.ItemsSource = distinctGiaSubjects;
            ComboBoxGia.SelectedIndex = 0;

            ComboBoxOlimpiad.ItemsSource = distinctOlympiads;
            ComboBoxOlimpiad.SelectedIndex = 0;
        }

        /// <summary>
        /// Фильтрация и сортировка студентов с загрузкой в ListBox
        /// </summary>
        private void Filters()
        {
            var filtered = new List<Student>(students);

            // Фильтрация по поисковому запросу
            if (!string.IsNullOrWhiteSpace(SearchTextN.Text))
            {
                filtered = SearchStudents(filtered, SearchTextN.Text);
            }

            // Фильтрация по предмету ГИА
            if (ComboBoxGia.SelectedIndex > 0)
            {
                var selectedGia = (GiaSubject)ComboBoxGia.SelectedItem;
                filtered = FilterGiaSubject(filtered, selectedGia);
            }

            // Фильтрация по олимпиаде
            if (ComboBoxOlimpiad.SelectedIndex > 0)
            {
                var selectedOlympiad = (Olympiad)ComboBoxOlimpiad.SelectedItem;
                filtered = FilterOlympiads(filtered, selectedOlympiad);
            }

            // Фильтрация по классу (параллели)
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
        /// Фильтрация студентов по предмету ГИА
        /// </summary>
        private List<Student> FilterGiaSubject(List<Student> students, GiaSubject selectedGia)
        {
            if (students == null) return new List<Student>();
            if (selectedGia?.GiaSubjectsNavigation == null) return students;

            return students.Where(s => s.StudentGiaResults?
                .Any(g => g.IdGiaSubjectsNavigation?.GiaSubjectsNavigation?.Name == selectedGia.GiaSubjectsNavigation.Name) ?? false)
                .ToList();
        }

        /// <summary>
        /// Фильтрация студентов по олимпиаде
        /// </summary>
        private List<Student> FilterOlympiads(List<Student> students, Olympiad selectedOlympiad)
        {
            if (students == null) return new List<Student>();
            if (selectedOlympiad?.OlympiadsNavigation == null) return students;

            return students.Where(s => s.StudentOlympiadParticipations?
                .Any(p => p.IdOlympiadsNavigation?.OlympiadsNavigation?.Name == selectedOlympiad.OlympiadsNavigation.Name) ?? false)
                .ToList();
        }

        /// <summary>
        /// Поиск студентов по ФИО или классу
        /// </summary>
        private List<Student> SearchStudents(List<Student> students, string searchText)
        {
            if (students == null || !students.Any()) return new List<Student>();
            if (string.IsNullOrWhiteSpace(searchText)) return students;

            var searchTerms = searchText.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return students.Where(student =>
            {
                if (student == null) return false;

                string fullName = $"{student.LastName} {student.FirstName} {student.Patronymic}".ToLower();
                string className = student.Class?.ClassesNumber?.ToLower() ?? "";

                return searchTerms.All(term => fullName.Contains(term) || className.Contains(term));
            }).ToList();
        }

        /// <summary>
        /// Добавление нового студента
        /// </summary>
        private async void Button_Click_AddStudents(object? sender, RoutedEventArgs e)
        {
            this.Classes.Add("blur-effect");
            var studentEditWindow = new StudentEditWindow(_schoolId);
            await studentEditWindow.ShowDialog(this);
            this.Classes.Remove("blur-effect");
            LoadStudents();
        }

        /// <summary>
        /// Редактирование выбранного студента
        /// </summary>
        private async void ListBox_DoubleTapped_Button_Redact(object? sender, Avalonia.Input.TappedEventArgs e)
        {
            this.Classes.Add("blur-effect");
            try
            {
                var stud = ListBox_Student.SelectedItem as Student;
                if (stud == null) return;

                var studentEditWindow = new StudentEditWindow(stud);
                await studentEditWindow.ShowDialog(this);
                LoadStudents();
            }
            finally
            {
                this.Classes.Remove("blur-effect");
            }
        }

        /// <summary>
        /// Удаление выбранных студентов
        /// </summary>
        private async void MenuItem_Click_Delete(object sender, RoutedEventArgs e)
        {
            var selectedStudents = ListBox_Student.SelectedItems.Cast<Student>().ToList();
            if (!selectedStudents.Any()) return;

            var message = selectedStudents.Count == 1
                ? $"Вы точно хотите удалить ученика: {selectedStudents[0].LastName} {selectedStudents[0].FirstName} {selectedStudents[0].Patronymic}?\n" +
                  "ВНИМАНИЕ: Это действие также удалит:\n" +
                  "- Все результаты ГИА ученика\n" +
                  "- Все участия в олимпиадах\n" +
                  "- Всю историю классов ученика\n" +
                  "- Всю историю школ ученика"
                : $"Вы точно хотите удалить {selectedStudents.Count} выбранных учеников?\n" +
                  "ВНИМАНИЕ: Это действие также удалит все связанные данные (результаты ГИА, участия в олимпиадах, историю классов и школ)";

            var dialogResult = await ShowConfirmationDialog("Подтверждение удаления", message);
            if (!dialogResult) return;

            using (var transaction = Helper.DateBase.Database.BeginTransaction())
            {
                try
                {
                    foreach (var stud in selectedStudents)
                    {
                        // Явно загружаем связанные данные для каждого студента
                        var studentWithRelations = await Helper.DateBase.Students
                            .Include(s => s.StudentGiaResults)
                            .Include(s => s.StudentOlympiadParticipations)
                            .Include(s => s.StudentClassHistories)
                            .Include(s => s.StudentSchoolHistories)
                            .FirstOrDefaultAsync(s => s.Id == stud.Id);

                        if (studentWithRelations != null)
                        {
                            // Удаление связанных данных
                            Helper.DateBase.StudentGiaResults.RemoveRange(studentWithRelations.StudentGiaResults);
                            Helper.DateBase.StudentOlympiadParticipations.RemoveRange(studentWithRelations.StudentOlympiadParticipations);
                            Helper.DateBase.StudentClassHistories.RemoveRange(studentWithRelations.StudentClassHistories);
                            Helper.DateBase.StudentSchoolHistories.RemoveRange(studentWithRelations.StudentSchoolHistories);

                            // Удаление студента
                            Helper.DateBase.Students.Remove(studentWithRelations);
                        }
                    }

                    await Helper.DateBase.SaveChangesAsync();
                    transaction.Commit();
                    LoadStudents();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    await ShowErrorDialog($"Ошибка при удалении: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Открытие настроек ГИА
        /// </summary>
        private async void Button_Click_ComboBoxGia_Setting(object? sender, RoutedEventArgs e)
        {
            this.Classes.Add("blur-effect");
            try
            {
                var settingGiaOlimpiad = new SettingGiaOlimpiad("ГИА");
                await settingGiaOlimpiad.ShowDialog(this);
                LoadInitialData();
            }
            finally
            {
                this.Classes.Remove("blur-effect");
            }
        }

        /// <summary>
        /// Открытие настроек олимпиад
        /// </summary>
        private async void Button_Click_ComboBoxOlimpiad_Setting(object? sender, RoutedEventArgs e)
        {
            this.Classes.Add("blur-effect");
            try
            {
                var settingGiaOlimpiad = new SettingGiaOlimpiad("Олимпиады");
                await settingGiaOlimpiad.ShowDialog(this);
                LoadInitialData();
            }
            finally
            {
                this.Classes.Remove("blur-effect");
            }
        }

        /// <summary>
        /// Экспорт данных в Excel
        /// </summary>
        private async void Button_Click_Unload_data(object? sender, RoutedEventArgs e)
        {
            await _excelService.ExportStudentsToExcel(this, students);
        }

        /// <summary>
        /// Импорт данных из Excel
        /// </summary>
        private async void Button_Click_Load_data(object? sender, RoutedEventArgs e)
        {
            if (await _excelService.ImportStudentsFromExcel(this))
            {
                LoadInitialData();
            }
        }

        /// <summary>
        /// Обновление данных студентов из Excel (поиск по ФИО, обновление только олимпиад и ГИА)
        /// </summary>
        private async void Button_Click_Upload_data(object? sender, RoutedEventArgs e)
        {
            try
            {
                var openDialog = new OpenFileDialog
                {
                    Title = "Выберите файл Excel для обновления",
                    Filters = new List<FileDialogFilter> {
                        new() { Name = "Excel Files", Extensions = { "xlsx" } }
                    },
                    AllowMultiple = false
                };

                var filePaths = await openDialog.ShowAsync(this);
                if (filePaths == null || filePaths.Length == 0) return;

                string errorFilePath = string.Empty;
                int updatedCount = 0;
                int notFoundCount = 0;

                using (var workbook = new XLWorkbook(filePaths[0]))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Пропускаем заголовок

                    foreach (var row in rows)
                    {
                        var lastName = row.Cell(1).GetString();
                        var firstName = row.Cell(2).GetString();
                        var patronymic = row.Cell(3).GetString();

                        // Ищем студента по ФИО
                        var student = await _context.Students
                            .Include(s => s.StudentGiaResults)
                            .Include(s => s.StudentOlympiadParticipations)
                            .FirstOrDefaultAsync(s =>
                                s.LastName == lastName &&
                                s.FirstName == firstName &&
                                s.Patronymic == patronymic);

                        if (student != null)
                        {
                            // Удаляем старые записи ГИА и олимпиад
                            _context.StudentGiaResults.RemoveRange(student.StudentGiaResults);
                            _context.StudentOlympiadParticipations.RemoveRange(student.StudentOlympiadParticipations);

                            // Обновляем предметы ГИА
                            var giaSubjects = row.Cell(7).GetString()?
                                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

                            if (giaSubjects != null)
                            {
                                foreach (var subjectName in giaSubjects.Select(s => s.Trim()))
                                {
                                    if (!string.IsNullOrEmpty(subjectName))
                                    {
                                        var giaSubject = await GetOrCreateGiaSubjectAsync(subjectName);
                                        await AddGiaResultAsync(student.Id, giaSubject.Id);
                                    }
                                }
                            }

                            // Обновляем олимпиады
                            for (int i = 0; i < 3; i++)
                            {
                                var olympiadType = row.Cell(8 + i * 2).GetString();
                                var olympiadSubject = row.Cell(9 + i * 2).GetString();

                                if (!string.IsNullOrEmpty(olympiadType) && !string.IsNullOrEmpty(olympiadSubject))
                                {
                                    var olympiad = await GetOrCreateOlympiadAsync(olympiadType, olympiadSubject);
                                    await AddOlympiadParticipationAsync(student.Id, olympiad.Id);
                                }
                            }

                            updatedCount++;
                        }
                        else
                        {
                            notFoundCount++;
                            // Добавляем строку с ошибкой в файл
                            row.Cell(worksheet.ColumnsUsed().Count() + 1).Value = "Ученик не найден";
                        }
                    }

                    // Сохраняем файл с пометками о не найденных студентах
                    if (notFoundCount > 0)
                    {
                        errorFilePath = Path.Combine(
                            Path.GetDirectoryName(filePaths[0]),
                            Path.GetFileNameWithoutExtension(filePaths[0]) +
                            "_with_errors" +
                            Path.GetExtension(filePaths[0]));

                        workbook.SaveAs(errorFilePath);
                    }

                    await _context.SaveChangesAsync();
                }

                string resultMessage = $"Обновлено студентов: {updatedCount}\nНе найдено: {notFoundCount}";
                if (notFoundCount > 0)
                {
                    resultMessage += $"\n\nФайл с пометками сохранен как:\n{Path.GetFileName(errorFilePath)}";
                }

                await ShowCustomMessage("Результат обновления", resultMessage,
                    notFoundCount > 0 ? Brushes.Orange : Brushes.Green);

                LoadInitialData();
            }
            catch (Exception ex)
            {
                await ShowErrorDialog($"Произошла ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Выход из системы
        /// </summary>
        private void Button_Click_Exit(object? sender, RoutedEventArgs e)
        {
            new SchoolList().Show();
            Close();
        }

        /// <summary>
        /// Удаление школы
        /// </summary>
        private async void Button_Click_DeleteSchool(object? sender, RoutedEventArgs e)
        {
            if (_schoolId <= 0) return;

            var school = await Helper.DateBase.Schools
                .Include(s => s.Students)
                    .ThenInclude(st => st.StudentGiaResults)
                .Include(s => s.Students)
                    .ThenInclude(st => st.StudentOlympiadParticipations)
                .Include(s => s.Students)
                    .ThenInclude(st => st.StudentClassHistories)
                .Include(s => s.Students)
                    .ThenInclude(st => st.StudentSchoolHistories)
                .Include(s => s.StudentSchoolHistories)
                .FirstOrDefaultAsync(s => s.Id == _schoolId);

            if (school == null) return;

            var message = $"Вы точно хотите удалить школу: {school.Name}?\n" +
                         "ВНИМАНИЕ: Это действие также удалит:\n" +
                         "- Всех учеников этой школы\n" +
                         "- Все результаты ГИА учеников и все участия в олимпиадах\n" +
                         "- Всю историю классов учеников и всю историю школ учеников";

            var dialogResult = await ShowConfirmationDialog("Подтверждение удаления школы", message);
            if (!dialogResult) return;

            using (var transaction = await Helper.DateBase.Database.BeginTransactionAsync())
            {
                try
                {
                    // Удаляем все связанные данные учеников
                    foreach (var student in school.Students.ToList())
                    {
                        // Удаляем результаты ГИА
                        Helper.DateBase.StudentGiaResults.RemoveRange(student.StudentGiaResults);

                        // Удаляем участия в олимпиадах
                        Helper.DateBase.StudentOlympiadParticipations.RemoveRange(student.StudentOlympiadParticipations);

                        // Удаляем историю классов
                        Helper.DateBase.StudentClassHistories.RemoveRange(student.StudentClassHistories);

                        // Удаляем историю школ
                        Helper.DateBase.StudentSchoolHistories.RemoveRange(student.StudentSchoolHistories);

                        // Удаляем самого ученика
                        Helper.DateBase.Students.Remove(student);
                    }
                    // Удаляем историю школы
                    Helper.DateBase.StudentSchoolHistories.RemoveRange(school.StudentSchoolHistories);

                    // Удаляем саму школу
                    Helper.DateBase.Schools.Remove(school);

                    await Helper.DateBase.SaveChangesAsync();
                    await transaction.CommitAsync();

                    new SchoolList().Show();
                    Close();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    await ShowErrorDialog($"Ошибка при удалении школы: {ex.InnerException?.Message ?? ex.Message}");
                }
            }
        }

        private async Task<GiaSubject> GetOrCreateGiaSubjectAsync(string subjectName)
        {
            // Сначала ищем предмет
            var item = await _context.Items.FirstOrDefaultAsync(i => i.Name == subjectName);
            if (item == null)
            {
                item = new Item { Name = subjectName };
                _context.Items.Add(item);
                await _context.SaveChangesAsync();
            }

            // Затем ищем ГИА предмет
            var giaSubject = await _context.GiaSubjects
                .FirstOrDefaultAsync(gs => gs.GiaSubjectsNavigation.Name == subjectName);

            if (giaSubject == null)
            {
                giaSubject = new GiaSubject { GiaSubjects = item.Id };
                _context.GiaSubjects.Add(giaSubject);
                await _context.SaveChangesAsync();
            }

            return giaSubject;
        }

        private async Task<Olympiad> GetOrCreateOlympiadAsync(string typeName, string subjectName)
        {
            // Сначала ищем тип олимпиады
            var olympiadType = await _context.OlympiadsTypes.FirstOrDefaultAsync(t => t.Name == typeName);
            if (olympiadType == null)
            {
                olympiadType = new OlympiadsType { Name = typeName };
                _context.OlympiadsTypes.Add(olympiadType);
                await _context.SaveChangesAsync();
            }

            // Затем ищем предмет
            var item = await _context.Items.FirstOrDefaultAsync(i => i.Name == subjectName);
            if (item == null)
            {
                item = new Item { Name = subjectName };
                _context.Items.Add(item);
                await _context.SaveChangesAsync();
            }

            // Ищем олимпиаду
            var olympiad = await _context.Olympiads
                .FirstOrDefaultAsync(o => o.OlympiadsNavigation.Name == typeName &&
                                         o.OlympiadsItemsNavigation.Name == subjectName);

            if (olympiad == null)
            {
                olympiad = new Olympiad { Olympiads = olympiadType.Id, OlympiadsItems = item.Id };
                _context.Olympiads.Add(olympiad);
                await _context.SaveChangesAsync();
            }

            return olympiad;
        }

        private async Task AddGiaResultAsync(int studentId, int giaSubjectId)
        {
            _context.StudentGiaResults.Add(new StudentGiaResult
            {
                IdStudents = studentId,
                IdGiaSubjects = giaSubjectId
            });
            await _context.SaveChangesAsync();
        }

        private async Task AddOlympiadParticipationAsync(int studentId, int olympiadId)
        {
            _context.StudentOlympiadParticipations.Add(new StudentOlympiadParticipation
            {
                IdStudents = studentId,
                IdOlympiads = olympiadId
            });
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Показывает диалоговое окно с сообщением об ошибке
        /// </summary>
        private async Task ShowErrorDialog(string message)
        {
            var dialog = new Window
            {
                Title = "Ошибка",
                Width = 400,
                Height = 200,
                SizeToContent = SizeToContent.Manual,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new Border
                {
                    BorderBrush = Brushes.Red,
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
        /// Показывает информационное сообщение с настраиваемым цветом рамки
        /// </summary>
        private async Task ShowCustomMessage(string title, string message, IBrush borderBrush)
        {
            var dialog = new Window
            {
                Title = title,
                Width = 450,
                Height = 250,
                SizeToContent = SizeToContent.Manual,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                WindowState = WindowState.Normal, // Явно указываем нормальный режим
                CanResize = false,
                Content = new Border
                {
                    BorderBrush = borderBrush,
                    BorderThickness = new Thickness(2),
                    Padding = new Thickness(15),
                    Child = new StackPanel
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Children =
                {
                    new TextBlock
                    {
                        Text = message,
                        TextWrapping = TextWrapping.Wrap,
                        TextAlignment = TextAlignment.Center,
                        Margin = new Thickness(0, 0, 0, 15)
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
        /// Показывает диалоговое окно подтверждения
        /// </summary>
        private async Task<bool> ShowConfirmationDialog(string title, string message)
        {
            var result = false;

            // Создаем основное окно
            var dialog = new Window
            {
                Title = title,
                Width = 550,
                Height = 200,
                MinWidth = 400,
                MinHeight = 180,
                MaxWidth = 550,
                MaxHeight = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                SizeToContent = SizeToContent.Manual, // Отключаем авто-размер
                WindowState = WindowState.Normal, // Гарантируем нормальный режим (не полноэкранный)
                CanResize = false, // Запрещаем изменение размера
                Topmost = true // Делаем окно поверх других
            };

            // Создаем кнопки
            var yesButton = new Button
            {
                Content = "Да",
                Width = 80,
                Margin = new Thickness(0, 0, 10, 0),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var noButton = new Button
            {
                Content = "Нет",
                Width = 80,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Настраиваем содержимое окна
            dialog.Content = new Border
            {
                BorderBrush = Brushes.Red,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(20),
                Child = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Spacing = 20,
                    Children =
            {
                new TextBlock
                {
                    Text = message,
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center,
                    FontSize = 14
                },
                new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Spacing = 15,
                    Children = { yesButton, noButton }
                }
            }
                }
            };

            // Назначаем обработчики кнопок
            yesButton.Click += (s, e) => { result = true; dialog.Close(); };
            noButton.Click += (s, e) => { result = false; dialog.Close(); };

            // Показываем окно и ждем результат
            await dialog.ShowDialog(this);
            return result;
        }
    }
}