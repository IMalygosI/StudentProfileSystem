using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Layout;
using ClosedXML.Excel;
using StudentProfileSystem.Models;
using StudentProfileSystem.Context;
using Microsoft.EntityFrameworkCore;
using Avalonia.VisualTree;
using Avalonia.Controls.Primitives;
using MsBox.Avalonia.ViewModels.Commands;

namespace StudentProfileSystem.Services
{
    /// <summary>
    /// Сервис для экспорта и импорта данных студентов в формате Excel
    /// </summary>
    public class ExcelService
    {
        private readonly ImcContext _context;

        /// <summary>
        /// Инициализирует новый экземпляр сервиса для работы с Excel
        /// </summary>
        /// <param name="context">Контекст базы данных</param>
        public ExcelService(ImcContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Экспортирует список студентов в файл Excel
        /// </summary>
        /// <param name="parentWindow">Родительское окно для отображения диалогов</param>
        /// <param name="students">Коллекция студентов для экспорта</param>
        public async Task ExportStudentsToExcel(Window parentWindow, IEnumerable<Student> students)
        {
            try
            {
                var confirm = await ShowConfirmationDialog(parentWindow,
                    "Подтверждение экспорта",
                    "Вы уверены, что хотите экспортировать данные в Excel?");

                if (!confirm) return;

                var saveDialog = new SaveFileDialog
                {
                    Title = "Сохранить Excel-файл",
                    Filters = new List<FileDialogFilter> {
                new() { Name = "Файлы Excel", Extensions = { "xlsx" } }
            },
                    InitialFileName = $"Экспорт_студентов_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                var filePath = await saveDialog.ShowAsync(parentWindow);
                if (string.IsNullOrEmpty(filePath)) return;

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Ученики");
                    // Формируем заголовки столбцов
                    var headers = new List<string> { "Фамилия", "Имя", "Отчество", "Класс", "Школа", "Номер школы", "ГИА" };
                    // Максимальное количество олимпиад
                    int maxOlympiads = students.Max(s => s.StudentOlympiadParticipations?.Count ?? 0);

                    // Добавляем колонки для олимпиад
                    for (int i = 0; i < maxOlympiads; i++)
                    {
                        headers.Add($"Олимпиада {i + 1}");
                        headers.Add($"Предметы олимпиады {i + 1}");
                    }

                    // Заполняем заголовки
                    for (int i = 0; i < headers.Count; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = headers[i];
                        worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                    }

                    // Заполняем данные студентов
                    int row = 2;
                    foreach (var student in students)
                    {
                        // Основная информация
                        worksheet.Cell(row, 1).Value = student.LastName;
                        worksheet.Cell(row, 2).Value = student.FirstName;
                        worksheet.Cell(row, 3).Value = student.Patronymic;
                        worksheet.Cell(row, 4).Value = student.Class?.ClassesNumber;
                        worksheet.Cell(row, 5).Value = student.School?.Name;
                        worksheet.Cell(row, 6).Value = student.School?.SchoolNumber;

                        // Предметы ГИА
                        var giaSubjects = student.StudentGiaResults?
                            .Select(g => g.IdGiaSubjectsNavigation?.GiaSubjectsNavigation?.Name)
                            .Where(name => !string.IsNullOrEmpty(name))
                            .Distinct() ?? Enumerable.Empty<string>();

                        worksheet.Cell(row, 7).Value = string.Join(", ", giaSubjects);

                        // Обработка олимпиад
                        var olympiadData = student.StudentOlympiadParticipations?
                            .Where(o => o.IdOlympiadsNavigation != null)
                            .Select(o => (
                                Type: o.IdOlympiadsNavigation.OlympiadsNavigation?.Name,
                                Subject: o.IdOlympiadsNavigation.OlympiadsItemsNavigation?.Name
                            ))
                            .Where(o => !string.IsNullOrEmpty(o.Type) && !string.IsNullOrEmpty(o.Subject))
                            .ToList() ?? new List<(string Type, string Subject)>();

                        // Записываем данные об олимпиадах
                        int col = 8;
                        foreach (var olympiad in olympiadData)
                        {
                            worksheet.Cell(row, col++).Value = olympiad.Type;
                            worksheet.Cell(row, col++).Value = olympiad.Subject;
                        }

                        row++;
                    }

                    worksheet.Columns().AdjustToContents();
                    workbook.SaveAs(filePath);
                }

                await ShowCustomMessage(parentWindow, "Экспорт завершен",
                    "Данные успешно экспортированы в Excel!", Brushes.Green);
            }
            catch (Exception ex)
            {
                await ShowCustomMessage(parentWindow, "Ошибка экспорта",
                    $"Произошла ошибка: {ex.Message}", Brushes.Red);
            }
        }




        /// <summary>
        /// Импортирует данные студентов из файла Excel
        /// </summary>
        /// <param name="parentWindow">Родительское окно для отображения диалогов</param>
        /// <returns>True, если импорт выполнен успешно, иначе False</returns>
        public async Task<bool> ImportStudentsFromExcel(Window parentWindow)
        {
            try
            {
                // Запрос подтверждения операции
                var confirm = await ShowConfirmationDialog(parentWindow,
                    "Подтверждение импорта",
                    "Вы уверены, что хотите импортировать данные из Excel?");
                if (!confirm) return false;

                // Настройка диалога открытия файла
                var openDialog = new OpenFileDialog
                {
                    Title = "Выберите файл Excel",
                    Filters = new List<FileDialogFilter> {
                        new() { Name = "Excel Files", Extensions = { "xlsx" } }
                    },
                    AllowMultiple = false
                };

                var filePaths = await openDialog.ShowAsync(parentWindow);
                if (filePaths == null || filePaths.Length == 0) return false;

                // Проверка доступности файла
                try
                {
                    using (var fileStream = new FileStream(filePaths[0],
                           FileMode.Open, FileAccess.Read, FileShare.None))
                    {
                        fileStream.Close();
                    }
                }
                catch (IOException)
                {
                    await ShowCustomMessage(parentWindow, "Ошибка",
                        "Необходимо закрыть импортируемый файл Excel", Brushes.Red);
                    return false;
                }

                // Валидация данных
                var validationResult = await ValidateExcelData(filePaths[0], parentWindow);
                if (!validationResult.IsValid)
                {
                    await ShowCustomMessage(parentWindow, "Ошибка валидации",
                        $"Обнаружены ошибки в данных:\n{string.Join("\n", validationResult.Errors)}",
                        Brushes.Red);
                    return false;
                }

                // Импорт данных
                using (var workbook = new XLWorkbook(filePaths[0]))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

                    foreach (var row in rows)
                    {
                        var lastName = row.Cell(1).GetString();
                        var firstName = row.Cell(2).GetString();
                        var patronymic = row.Cell(3).GetString();

                        // Поиск существующего студента
                        var existingStudent = await _context.Students
                            .Include(s => s.StudentGiaResults)
                            .Include(s => s.StudentOlympiadParticipations)
                            .FirstOrDefaultAsync(s => s.LastName == lastName &&
                                                     s.FirstName == firstName &&
                                                     s.Patronymic == patronymic);

                        if (existingStudent != null)
                        {
                            await UpdateStudentData(existingStudent, row);
                        }
                        else
                        {
                            await CreateNewStudent(row);
                        }
                    }
                }

                await ShowCustomMessage(parentWindow, "Импорт завершен",
                    "Данные успешно импортированы из Excel!", Brushes.Green);
                return true;
            }
            catch (Exception ex)
            {
                await ShowCustomMessage(parentWindow, "Ошибка импорта",
                    $"Произошла ошибка: {ex.Message}", Brushes.Red);
                return false;
            }
        }

        /// <summary>
        /// Обновляет данные существующего студента
        /// </summary>
        /// <param name="student">Объект студента для обновления</param>
        /// <param name="row">Строка данных из Excel</param>
        private async Task UpdateStudentData(Student student, IXLRangeRow row)
        {
            // Обновление основной информации
            var className = row.Cell(4).GetString();
            if (!string.IsNullOrEmpty(className))
            {
                student.Class = await GetOrCreateClassAsync(className);
            }

            var schoolName = row.Cell(5).GetString();
            var schoolNumber = row.Cell(6).GetString();
            if (!string.IsNullOrEmpty(schoolName))
            {
                student.School = await GetOrCreateSchoolAsync(schoolName, schoolNumber, "", "");
            }

            // Удаление старых данных
            _context.StudentGiaResults.RemoveRange(student.StudentGiaResults);
            _context.StudentOlympiadParticipations.RemoveRange(student.StudentOlympiadParticipations);

            // Добавление предметов ГИА
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

            // Добавление олимпиад
            int olympiadColumn = 8;
            while (olympiadColumn <= row.Worksheet.ColumnsUsed().Count())
            {
                var olympiadType = row.Cell(olympiadColumn).GetString();
                var olympiadSubject = row.Cell(olympiadColumn + 1).GetString();

                if (!string.IsNullOrEmpty(olympiadType) && !string.IsNullOrEmpty(olympiadSubject))
                {
                    var olympiad = await GetOrCreateOlympiadAsync(olympiadType, olympiadSubject);
                    await AddOlympiadParticipationAsync(student.Id, olympiad.Id);
                }
                olympiadColumn += 2;
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Создает нового студента на основе данных из Excel
        /// </summary>
        /// <param name="row">Строка данных из Excel</param>
        private async Task CreateNewStudent(IXLRangeRow row)
        {
            // Создание школы
            var schoolName = row.Cell(5).GetString();
            var schoolNumber = row.Cell(6).GetString();
            var school = await GetOrCreateSchoolAsync(schoolName, schoolNumber, "", "");

            // Создание класса
            var className = row.Cell(4).GetString();
            var studentClass = await GetOrCreateClassAsync(className);

            // Создание студента
            var student = new Student
            {
                LastName = row.Cell(1).GetString(),
                FirstName = row.Cell(2).GetString(),
                Patronymic = row.Cell(3).GetString(),
                ClassId = studentClass?.Id ?? 0,
                SchoolId = school?.Id ?? 0
            };
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            // Добавление предметов ГИА
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

            // Добавление олимпиад
            int olympiadColumn = 8;
            while (olympiadColumn <= row.Worksheet.ColumnsUsed().Count())
            {
                var olympiadType = row.Cell(olympiadColumn).GetString();
                var olympiadSubject = row.Cell(olympiadColumn + 1).GetString();

                if (!string.IsNullOrEmpty(olympiadType) && !string.IsNullOrEmpty(olympiadSubject))
                {
                    var olympiad = await GetOrCreateOlympiadAsync(olympiadType, olympiadSubject);
                    await AddOlympiadParticipationAsync(student.Id, olympiad.Id);
                }
                olympiadColumn += 2;
            }
        }

        /// <summary>
        /// Проверяет данные в файле Excel перед импортом
        /// </summary>
        /// <param name="filePath">Путь к файлу Excel</param>
        /// <param name="parentWindow">Родительское окно для отображения ошибок</param>
        /// <returns>Результат проверки</returns>
        private async Task<ValidationResult> ValidateExcelData(string filePath, Window parentWindow)
        {
            var result = new ValidationResult { IsValid = true, Errors = new List<string>() };

            try
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RowsUsed().Skip(1);

                    int rowNum = 2;
                    foreach (var row in rows)
                    {
                        // Проверка обязательных полей
                        if (string.IsNullOrWhiteSpace(row.Cell(1).GetString()))
                            result.Errors.Add($"Строка {rowNum}: Не указана фамилия");

                        if (string.IsNullOrWhiteSpace(row.Cell(2).GetString()))
                            result.Errors.Add($"Строка {rowNum}: Не указано имя");

                        // Проверка предметов ГИА
                        var giaSubjects = row.Cell(7).GetString()?
                            .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

                        if (giaSubjects != null && giaSubjects.Any(string.IsNullOrWhiteSpace))
                        {
                            result.Errors.Add($"Строка {rowNum}: Обнаружены пустые значения в предметах ГИА");
                        }

                        // Проверка олимпиад
                        int olympiadColumn = 8;
                        while (olympiadColumn < row.Worksheet.ColumnsUsed().Count())
                        {
                            var olympiadType = row.Cell(olympiadColumn).GetString();
                            var olympiadSubject = row.Cell(olympiadColumn + 1).GetString();

                            if ((!string.IsNullOrWhiteSpace(olympiadType) && string.IsNullOrWhiteSpace(olympiadSubject)) ||
                                (string.IsNullOrWhiteSpace(olympiadType) && !string.IsNullOrWhiteSpace(olympiadSubject)))
                            {
                                result.Errors.Add($"Строка {rowNum}: Неполные данные олимпиады (колонки {olympiadColumn}-{olympiadColumn + 1})");
                            }
                            olympiadColumn += 2;
                        }

                        rowNum++;
                    }
                }

                if (result.Errors.Any())
                {
                    result.IsValid = false;
                }
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Errors.Add($"Ошибка при проверке файла: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Отображает диалоговое окно подтверждения
        /// </summary>
        private async Task<bool> ShowConfirmationDialog(Window parentWindow, string title, string message)
        {
            var tcs = new TaskCompletionSource<bool>();
            var result = false;

            // Сначала создаем элементы управления
            var yesButton = new Button
            {
                Content = "Да",
                Width = 80,
                Background = Brushes.Green
            };

            var noButton = new Button
            {
                Content = "Нет",
                Width = 80,
                Background = Brushes.Red
            };

            // Затем создаем окно
            var dialog = new Window
            {
                Title = title,
                Width = 450,
                Height = 200,
                SizeToContent = SizeToContent.Manual,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new Border
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(2),
                    Padding = new Thickness(15),
                    Child = new StackPanel
                    {
                        Spacing = 15,
                        Children =
                {
                    new TextBlock
                    {
                        Text = message,
                        TextWrapping = TextWrapping.Wrap,
                        HorizontalAlignment = HorizontalAlignment.Center
                    },
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Spacing = 10,
                        Children = { yesButton, noButton }
                    }
                }
                    }
                }
            };

            // Назначаем команды после создания всех элементов
            yesButton.Command = new RelayCommand(_ =>
            {
                result = true;
                dialog.Close();
            });

            noButton.Command = new RelayCommand(_ =>
            {
                result = false;
                dialog.Close();
            });

            dialog.Closed += (sender, e) =>
            {
                tcs.TrySetResult(result);
            };

            await dialog.ShowDialog(parentWindow);
            return await tcs.Task;
        }

        /// <summary>
        /// Отображает информационное сообщение
        /// </summary>
        private async Task ShowCustomMessage(Window parentWindow, string title, string message, IBrush borderBrush)
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
                                Command = new RelayCommand(_ => (parentWindow.GetVisualRoot() as Window)?.Close())
                            }
                        }
                    }
                }
            };

            await dialog.ShowDialog(parentWindow);
        }

        /// <summary>
        /// Находит или создает школу
        /// </summary>
        private async Task<School> GetOrCreateSchoolAsync(string name, string number, string district, string city)
        {
            var school = await _context.Schools.FirstOrDefaultAsync(s => s.Name == name && s.SchoolNumber == number);
            if (school == null)
            {
                school = new School { Name = name, SchoolNumber = number };
                _context.Schools.Add(school);
                await _context.SaveChangesAsync();
            }
            return school;
        }

        /// <summary>
        /// Находит или создает класс
        /// </summary>
        private async Task<Class> GetOrCreateClassAsync(string name)
        {
            var classEntity = await _context.Classes.FirstOrDefaultAsync(c => c.ClassesNumber == name);
            if (classEntity == null)
            {
                classEntity = new Class { ClassesNumber = name };
                _context.Classes.Add(classEntity);
                await _context.SaveChangesAsync();
            }
            return classEntity;
        }

        /// <summary>
        /// Находит или создает предмет ГИА
        /// </summary>
        private async Task<GiaSubject> GetOrCreateGiaSubjectAsync(string subjectName)
        {
            var item = await _context.Items.FirstOrDefaultAsync(i => i.Name == subjectName);
            if (item == null)
            {
                item = new Item { Name = subjectName };
                _context.Items.Add(item);
                await _context.SaveChangesAsync();
            }

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

        /// <summary>
        /// Находит или создает олимпиаду
        /// </summary>
        private async Task<Olympiad> GetOrCreateOlympiadAsync(string typeName, string subjectName)
        {
            var olympiadType = await _context.OlympiadsTypes.FirstOrDefaultAsync(t => t.Name == typeName);
            if (olympiadType == null)
            {
                olympiadType = new OlympiadsType { Name = typeName };
                _context.OlympiadsTypes.Add(olympiadType);
                await _context.SaveChangesAsync();
            }

            var item = await _context.Items.FirstOrDefaultAsync(i => i.Name == subjectName);
            if (item == null)
            {
                item = new Item { Name = subjectName };
                _context.Items.Add(item);
                await _context.SaveChangesAsync();
            }

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

        /// <summary>
        /// Добавляет результат ГИА для студента
        /// </summary>
        private async Task AddGiaResultAsync(int studentId, int giaSubjectId)
        {
            _context.StudentGiaResults.Add(new StudentGiaResult
            {
                IdStudents = studentId,
                IdGiaSubjects = giaSubjectId
            });
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Добавляет участие студента в олимпиаде
        /// </summary>ё
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
        /// Результат валидации Excel файла
        /// </summary>
        private class ValidationResult
        {
            public bool IsValid { get; set; }
            public List<string> Errors { get; set; } = new List<string>();
        }
    }
}
