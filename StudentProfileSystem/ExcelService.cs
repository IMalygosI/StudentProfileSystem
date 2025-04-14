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
        /// Экспортирует список студентов в файл Excel из школы
        /// </summary>
        /// <param name="parentWindow">Родительское окно для отображения диалогов</param>
        /// <param name="students">Коллекция студентов для экспорта</param>
        public async Task ExportStudentsToExcel(Window parentWindow, IEnumerable<Student> students)
        {
            try
            {
                var studentIds = students.Select(s => s.Id).ToList();

                var studentsWithData = await _context.Students
                    .Where(s => studentIds.Contains(s.Id))
                    .Include(s => s.Class)
                    .Include(s => s.School)
                    .Include(s => s.StudentCertificateAndMedals)
                        .ThenInclude(m => m.CertificateAndMedalsFact)
                        .ThenInclude(m => m.CertificateAndMedals)
                    .Include(s => s.StudentCertificateAndMedals)
                        .ThenInclude(m => m.CertificateAndMedalsFact)
                        .ThenInclude(m => m.CertificateAndMedalsCheck)
                    .Include(s => s.StudentGiaResults)
                        .ThenInclude(g => g.IdGiaSubjectsNavigation)
                        .ThenInclude(g => g.GiaSubjectsNavigation)
                    .Include(s => s.StudentOlympiadParticipations)
                        .ThenInclude(o => o.IdOlympiadsNavigation)
                        .ThenInclude(o => o.OlympiadsNavigation)
                    .Include(s => s.StudentOlympiadParticipations)
                        .ThenInclude(o => o.IdOlympiadsNavigation)
                        .ThenInclude(o => o.OlympiadsItemsNavigation)
                    .ToListAsync();

                var confirm = await ShowConfirmationDialog(parentWindow, "Подтверждение экспорта", "Вы уверены, что хотите экспортировать данные в Excel?");

                if (!confirm) return;

                var saveDialog = new SaveFileDialog
                {
                    Title = "Сохранить Excel-файл",
                    Filters = new List<FileDialogFilter> { new() { Name = "Файлы Excel", Extensions = { "xlsx" } } },
                    InitialFileName = $"Экспорт_студентов_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                var filePath = await saveDialog.ShowAsync(parentWindow);
                if (string.IsNullOrEmpty(filePath)) return;

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Ученики");
                    // Формируем заголовки столбцов
                    var headers = new List<string> { "Фамилия", "Имя", "Отчество", "Класс", "Школа", "Номер школы", "Претендует на медаль", "Медаль фактическая", "ГИА" };

                    // Максимальное количество олимпиад
                    int maxOlympiads = studentsWithData.Max(s => s.StudentOlympiadParticipations?.Count ?? 0);

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
                    foreach (var student in studentsWithData)
                    {
                        // Основная информация
                        worksheet.Cell(row, 1).Value = student.LastName;
                        worksheet.Cell(row, 2).Value = student.FirstName;
                        worksheet.Cell(row, 3).Value = student.Patronymic;
                        worksheet.Cell(row, 4).Value = student.Class?.ClassesNumber;
                        worksheet.Cell(row, 5).Value = student.School?.Name;
                        worksheet.Cell(row, 6).Value = student.School?.SchoolNumber;

                        // Медали (новые столбцы 7-8)
                        var medalData = student.StudentCertificateAndMedals?.Select(m => (MedalName: m.CertificateAndMedalsFact?.CertificateAndMedals?.Name,
                                                                                          MedalStatus: m.CertificateAndMedalsFact?.CertificateAndMedalsCheck?.Name
                            )).Where(m => !string.IsNullOrEmpty(m.MedalName) && !string.IsNullOrEmpty(m.MedalStatus)).ToList();

                        if (medalData != null && medalData.Any())
                        {
                            worksheet.Cell(row, 7).Value = string.Join(", ", medalData.Select(m => m.MedalName));
                            worksheet.Cell(row, 8).Value = string.Join(", ", medalData.Select(m => m.MedalStatus));
                        }

                        // Предметы ГИА (теперь столбец 9)
                        var giaSubjects = student.StudentGiaResults?.Select(g => g.IdGiaSubjectsNavigation?.GiaSubjectsNavigation?.Name)
                                                                    .Where(name => !string.IsNullOrEmpty(name)).Distinct() ?? Enumerable.Empty<string>();
                        worksheet.Cell(row, 9).Value = string.Join(", ", giaSubjects);

                        // Обработка олимпиад (начиная со столбца 10)
                        var olympiadData = student.StudentOlympiadParticipations?.Where(o => o.IdOlympiadsNavigation != null)
                            .Select(o => (Type: o.IdOlympiadsNavigation.OlympiadsNavigation?.Name,Subject: o.IdOlympiadsNavigation.OlympiadsItemsNavigation?.Name
                            )).Where(o => !string.IsNullOrEmpty(o.Type) && !string.IsNullOrEmpty(o.Subject)).ToList() ?? new List<(string Type, string Subject)>();

                        // Записываем данные об олимпиадах
                        int col = 10;
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

                await ShowCustomMessage(parentWindow, "Экспорт завершен", "Данные успешно экспортированы в Excel!", Brushes.Green);
            }
            catch (Exception ex)
            {
                await ShowCustomMessage(parentWindow, "Ошибка экспорта", $"Произошла ошибка: {ex.Message}", Brushes.Red);
            }
        }

        /// <summary>
        /// Экспортирует список всех студентов в файл Excel из списка школ
        /// </summary>
        /// <param name="parentWindow">Родительское окно для отображения диалогов</param>
        public async Task ExportStudentsToExcel(Window parentWindow)
        {
            try
            {
                var confirm = await ShowConfirmationDialog(parentWindow, "Подтверждение экспорта", "Вы уверены, что хотите экспортировать данные всех студентов в Excel?");

                if (!confirm) return;

                // Загружаем всех студентов со всеми связанными данными
                var students = await _context.Students
                    .Include(s => s.Class)
                    .Include(s => s.School)
                    .Include(s => s.StudentCertificateAndMedals)
                        .ThenInclude(m => m.CertificateAndMedalsFact)
                        .ThenInclude(m => m.CertificateAndMedals)
                    .Include(s => s.StudentCertificateAndMedals)
                        .ThenInclude(m => m.CertificateAndMedalsFact)
                        .ThenInclude(m => m.CertificateAndMedalsCheck)
                    .Include(s => s.StudentGiaResults)
                        .ThenInclude(g => g.IdGiaSubjectsNavigation)
                        .ThenInclude(g => g.GiaSubjectsNavigation)
                    .Include(s => s.StudentOlympiadParticipations)
                        .ThenInclude(o => o.IdOlympiadsNavigation)
                        .ThenInclude(o => o.OlympiadsNavigation)
                    .Include(s => s.StudentOlympiadParticipations)
                        .ThenInclude(o => o.IdOlympiadsNavigation)
                        .ThenInclude(o => o.OlympiadsItemsNavigation)
                    .ToListAsync();

                var saveDialog = new SaveFileDialog
                {
                    Title = "Сохранить Excel-файл",
                    Filters = new List<FileDialogFilter> { new() { Name = "Файлы Excel", Extensions = { "xlsx" } } },
                    InitialFileName = $"Экспорт_всех_студентов_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                var filePath = await saveDialog.ShowAsync(parentWindow);
                if (string.IsNullOrEmpty(filePath)) return;

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Ученики");
                    // Формируем заголовки столбцов
                    var headers = new List<string> { "Фамилия", "Имя", "Отчество", "Класс", "Школа", "Номер школы", "Претендует на медаль", "Медаль фактическая", "ГИА"};

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

                        // Медали (столбцы 7-8)
                        var medalData = student.StudentCertificateAndMedals?.Select(m => (MedalName: m.CertificateAndMedalsFact?.CertificateAndMedals?.Name,
                                                                                          MedalStatus: m.CertificateAndMedalsFact?.CertificateAndMedalsCheck?.Name
                            )).Where(m => !string.IsNullOrEmpty(m.MedalName) && !string.IsNullOrEmpty(m.MedalStatus)).ToList() ?? new List<(string MedalName, string MedalStatus)>();

                        if (medalData.Any())
                        {
                            worksheet.Cell(row, 7).Value = string.Join(", ", medalData.Select(m => m.MedalName));
                            worksheet.Cell(row, 8).Value = string.Join(", ", medalData.Select(m => m.MedalStatus));
                        }

                        // Предметы ГИА (столбец 9)
                        var giaSubjects = student.StudentGiaResults?.Select(g => g.IdGiaSubjectsNavigation?.GiaSubjectsNavigation?.Name).Where(name => !string.IsNullOrEmpty(name)).Distinct() ?? Enumerable.Empty<string>();
                        worksheet.Cell(row, 9).Value = string.Join(", ", giaSubjects);

                        // Обработка олимпиад (начиная со столбца 10)
                        var olympiadData = student.StudentOlympiadParticipations?.Where(o => o.IdOlympiadsNavigation != null)
                              .Select(o => ( Type: o.IdOlympiadsNavigation.OlympiadsNavigation?.Name,Subject: o.IdOlympiadsNavigation.OlympiadsItemsNavigation?.Name
                            )).Where(o => !string.IsNullOrEmpty(o.Type) && !string.IsNullOrEmpty(o.Subject)).ToList() ?? new List<(string Type, string Subject)>();

                        // Записываем данные об олимпиадах
                        int col = 10;
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
                    "Данные всех студентов успешно экспортированы в Excel!", Brushes.Green);
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
                var confirm = await ShowConfirmationDialog(parentWindow, "Подтверждение импорта", "Вы уверены, что хотите импортировать данные из Excel?");
                if (!confirm) return false;

                var openDialog = new OpenFileDialog
                {
                    Title = "Выберите файл Excel", Filters = new List<FileDialogFilter> { new() { Name = "Excel Files", Extensions = { "xlsx" } } },
                    AllowMultiple = false
                };

                var filePaths = await openDialog.ShowAsync(parentWindow);
                if (filePaths == null || filePaths.Length == 0) return false;

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

                var validationResult = await ValidateExcelData(filePaths[0], parentWindow);
                if (!validationResult.IsValid)
                {
                    await ShowCustomMessage(parentWindow, "Ошибка валидации",
                        $"Обнаружены ошибки в данных:\n{string.Join("\n", validationResult.Errors)}",
                        Brushes.Red);
                    return false;
                }

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        using (var workbook = new XLWorkbook(filePaths[0]))
                        {
                            var worksheet = workbook.Worksheet(1);
                            var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

                            var importResults = new List<string>();
                            int addedCount = 0;
                            int updatedCount = 0;
                            int skippedCount = 0;

                            foreach (var row in rows)
                            {
                                try
                                {
                                    var lastName = row.Cell(1).GetString()?.Trim();
                                    var firstName = row.Cell(2).GetString()?.Trim();
                                    var patronymic = row.Cell(3).GetString()?.Trim();

                                    if (string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(firstName))
                                    {
                                        importResults.Add($"Строка {row.RowNumber()}: Не указаны ФИО");
                                        continue;
                                    }

                                    // Ищем существующего студента (точное совпадение по ФИО)
                                    var existingStudent = await _context.Students
                                        .Include(s => s.Class)
                                        .Include(s => s.School)
                                        .Include(s => s.StudentGiaResults)
                                            .ThenInclude(g => g.IdGiaSubjectsNavigation)
                                            .ThenInclude(g => g.GiaSubjectsNavigation)
                                        .Include(s => s.StudentOlympiadParticipations)
                                            .ThenInclude(o => o.IdOlympiadsNavigation)
                                            .ThenInclude(o => o.OlympiadsNavigation)
                                        .Include(s => s.StudentOlympiadParticipations)
                                            .ThenInclude(o => o.IdOlympiadsNavigation)
                                            .ThenInclude(o => o.OlympiadsItemsNavigation)
                                        .Include(s => s.StudentCertificateAndMedals)
                                            .ThenInclude(m => m.CertificateAndMedalsFact)
                                            .ThenInclude(m => m.CertificateAndMedals)
                                        .Include(s => s.StudentCertificateAndMedals)
                                            .ThenInclude(m => m.CertificateAndMedalsFact)
                                            .ThenInclude(m => m.CertificateAndMedalsCheck)
                                        .FirstOrDefaultAsync(s => s.LastName == lastName && s.FirstName == firstName &&s.Patronymic == patronymic);

                                    if (existingStudent != null)
                                    {
                                        // Полностью удаляем все связанные данные перед обновлением
                                        _context.StudentGiaResults.RemoveRange(existingStudent.StudentGiaResults);
                                        _context.StudentOlympiadParticipations.RemoveRange(existingStudent.StudentOlympiadParticipations);
                                        _context.StudentCertificateAndMedals.RemoveRange(existingStudent.StudentCertificateAndMedals);

                                        // Обновляем основные данные
                                        existingStudent.Class = await GetOrCreateClassAsync(row.Cell(4).GetString());
                                        existingStudent.School = await GetOrCreateSchoolAsync( row.Cell(5).GetString(),row.Cell(6).GetString(), "", "");

                                        // Обновляем медали (столбцы 7-8)
                                        await ProcessMedals(row, existingStudent.Id);

                                        // Обновляем ГИА (столбец 9)
                                        await ProcessGiaSubjects(row, existingStudent.Id);

                                        // Обновляем олимпиады (столбцы 10+)
                                        await ProcessOlympiads(row, existingStudent.Id);

                                        updatedCount++;
                                        importResults.Add($"Обновлен: {lastName} {firstName} {patronymic}");
                                    }
                                    else
                                    {
                                        // Создаем нового студента
                                        var newStudent = new Student
                                        {
                                            LastName = lastName,
                                            FirstName = firstName,
                                            Patronymic = patronymic,
                                            Class = await GetOrCreateClassAsync(row.Cell(4).GetString()),
                                            School = await GetOrCreateSchoolAsync( row.Cell(5).GetString(), row.Cell(6).GetString(),"", "")
                                        };

                                        _context.Students.Add(newStudent);
                                        await _context.SaveChangesAsync(); // Сохраняем, чтобы получить ID

                                        // Добавляем медали (столбцы 7-8)
                                        await ProcessMedals(row, newStudent.Id);

                                        // Добавляем ГИА (столбец 9)
                                        await ProcessGiaSubjects(row, newStudent.Id);

                                        // Добавляем олимпиады (столбцы 10+)
                                        await ProcessOlympiads(row, newStudent.Id);

                                        addedCount++;
                                        importResults.Add($"Добавлен: {lastName} {firstName} {patronymic}");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    importResults.Add($"Ошибка в строке {row.RowNumber()}: {ex.Message}");
                                }
                            }

                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();

                            // Формируем отчет
                            string resultMessage = $"Импорт завершен:\n" + $"Добавлено новых студентов: {addedCount}\n" + 
                                                   $"Обновлено существующих: {updatedCount}\n\n" + $"Детали:\n{string.Join("\n", importResults.Take(10))}";

                            if (importResults.Count > 10)
                                resultMessage += $"\n...и еще {importResults.Count - 10} записей";

                            await ShowCustomMessage(parentWindow, "Результат импорта",
                                resultMessage,
                                importResults.Any(r => r.StartsWith("Ошибка")) ? Brushes.Orange : Brushes.Green);
                        }
                        return true;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        await ShowCustomMessage(parentWindow, "Ошибка импорта",
                            $"Произошла ошибка: {ex.Message}", Brushes.Red);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                await ShowCustomMessage(parentWindow, "Ошибка импорта",
                    $"Произошла ошибка: {ex.Message}", Brushes.Red);
                return false;
            }
        }

        private async Task ProcessMedals(IXLRangeRow row, int studentId)
        {
            var medalNames = row.Cell(7).GetString()?.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(m => m.Trim()).Where(m => !string.IsNullOrEmpty(m)).ToList();
            var medalStatuses = row.Cell(8).GetString()?.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(m => m.Trim()).Where(m => !string.IsNullOrEmpty(m)).ToList();

            if (medalNames != null && medalStatuses != null && medalNames.Count == medalStatuses.Count)
            {
                for (int i = 0; i < medalNames.Count; i++)
                {
                    await AddOrUpdateMedalAsync(studentId, medalNames[i], medalStatuses[i]);
                }
            }
        }

        private async Task ProcessGiaSubjects(IXLRangeRow row, int studentId)
        {
            var giaSubjects = row.Cell(9).GetString()?.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();

            if (giaSubjects != null)
            {
                foreach (var subjectName in giaSubjects)
                {
                    var giaSubject = await GetOrCreateGiaSubjectAsync(subjectName);
                    await AddGiaResultAsync(studentId, giaSubject.Id);
                }
            }
        }

        private async Task ProcessOlympiads(IXLRangeRow row, int studentId)
        {
            int olympiadColumn = 10;
            while (olympiadColumn <= row.Worksheet.ColumnsUsed().Count())
            {
                var olympiadType = row.Cell(olympiadColumn).GetString()?.Trim();
                var olympiadSubject = row.Cell(olympiadColumn + 1).GetString()?.Trim();

                if (!string.IsNullOrEmpty(olympiadType) && !string.IsNullOrEmpty(olympiadSubject))
                {
                    var olympiad = await GetOrCreateOlympiadAsync(olympiadType, olympiadSubject);
                    await AddOlympiadParticipationAsync(studentId, olympiad.Id);
                }
                olympiadColumn += 2;
            }
        }

        /// <summary>
        /// Обновляет данные студентов из Excel (поиск по ФИО, обновление только олимпиад и ГИА)
        /// </summary>
        public async Task<bool> UpdateStudentsFromExcel(Window parentWindow)
        {
            try
            {
                var confirm = await ShowConfirmationDialog(parentWindow, "Подтверждение обновления", "Вы уверены, что хотите обновить данные студентов из Excel?");
                if (!confirm) return false;

                var openDialog = new OpenFileDialog
                {
                    Title = "Выберите файл Excel для обновления", Filters = new List<FileDialogFilter> { new() { Name = "Excel Files", Extensions = { "xlsx" } } },
                    AllowMultiple = false
                };

                var filePaths = await openDialog.ShowAsync(parentWindow);
                if (filePaths == null || filePaths.Length == 0) return false;

                string errorFilePath = string.Empty;
                int updatedCount = 0;
                int notFoundCount = 0;

                using (var workbook = new XLWorkbook(filePaths[0]))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

                    foreach (var row in rows)
                    {
                        var lastName = row.Cell(1).GetString();
                        var firstName = row.Cell(2).GetString();
                        var patronymic = row.Cell(3).GetString();

                        var student = await _context.Students.Include(s => s.StudentGiaResults).Include(s => s.StudentOlympiadParticipations)
                                                                                               .Include(s => s.StudentCertificateAndMedals).FirstOrDefaultAsync(s => s.LastName == lastName && 
                                                                                                                                                                     s.FirstName == firstName &&
                                                                                                                                                                     s.Patronymic == patronymic);

                        if (student != null)
                        {
                            // Удаляем старые записи ГИА, олимпиад и медалей
                            _context.StudentGiaResults.RemoveRange(student.StudentGiaResults);
                            _context.StudentOlympiadParticipations.RemoveRange(student.StudentOlympiadParticipations);
                            _context.StudentCertificateAndMedals.RemoveRange(student.StudentCertificateAndMedals);

                            // Обновляем медали (столбцы 7-8)
                            var medalNames = row.Cell(7).GetString()?.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(m => m.Trim()).Where(m => !string.IsNullOrEmpty(m)).ToList();
                            var medalStatuses = row.Cell(8).GetString()?.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(m => m.Trim()).Where(m => !string.IsNullOrEmpty(m)).ToList();

                            if (medalNames != null && medalStatuses != null &&
                                medalNames.Count == medalStatuses.Count)
                            {
                                for (int i = 0; i < medalNames.Count; i++)
                                {
                                    await AddOrUpdateMedalAsync(student.Id, medalNames[i], medalStatuses[i]);
                                }
                            }

                            // Обновляем предметы ГИА (столбец 9)
                            var giaSubjects = row.Cell(9).GetString()?.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

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

                            // Обновляем олимпиады (начиная со столбца 10)
                            int olympiadColumn = 10;
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

                            updatedCount++;
                        }
                        else
                        {
                            notFoundCount++;
                            row.Cell(worksheet.ColumnsUsed().Count() + 1).Value = "Ученик не найден";
                        }
                    }

                    if (notFoundCount > 0)
                    {
                        errorFilePath = Path.Combine( Path.GetDirectoryName(filePaths[0]), Path.GetFileNameWithoutExtension(filePaths[0]) + "_with_errors" + Path.GetExtension(filePaths[0]));

                        workbook.SaveAs(errorFilePath);
                    }

                    await _context.SaveChangesAsync();
                }

                string resultMessage = $"Обновлено студентов: {updatedCount}\nНе найдено: {notFoundCount}";
                if (notFoundCount > 0)
                {
                    resultMessage += $"\n\nФайл с пометками сохранен как:\n{Path.GetFileName(errorFilePath)}";
                }

                await ShowCustomMessage(parentWindow, "Результат обновления", resultMessage, notFoundCount > 0 ? Brushes.Orange : Brushes.Green);

                return true;
            }
            catch (Exception ex)
            {
                await ShowCustomMessage(parentWindow, "Ошибка обновления", $"Произошла ошибка: {ex.Message}", Brushes.Red);
                return false;
            }
        }

        /// <summary>
        /// Добавляет или обновляет медаль студента
        /// </summary>
        private async Task AddOrUpdateMedalAsync(int studentId, string medalName, string medalStatus)
        {
            // Находим или создаем медаль
            var medal = await _context.CertificateAndMedals.FirstOrDefaultAsync(m => m.Name == medalName);

            if (medal == null)
            {
                medal = new CertificateAndMedal { Name = medalName };
                _context.CertificateAndMedals.Add(medal);
                await _context.SaveChangesAsync();
            }

            // Находим или создаем статус медали
            var medalCheck = await _context.CertificateAndMedalsChecks.FirstOrDefaultAsync(c => c.Name == medalStatus);

            if (medalCheck == null)
            {
                medalCheck = new CertificateAndMedalsCheck { Name = medalStatus };
                _context.CertificateAndMedalsChecks.Add(medalCheck);
                await _context.SaveChangesAsync();
            }

            // Создаем факт медали
            var medalFact = await _context.CertificateAndMedalsFacts.FirstOrDefaultAsync(f => f.CertificateAndMedalsId == medal.Id && f.CertificateAndMedalsCheckId == medalCheck.Id);

            if (medalFact == null)
            {
                medalFact = new CertificateAndMedalsFact
                {
                    CertificateAndMedalsId = medal.Id,
                    CertificateAndMedalsCheckId = medalCheck.Id
                };
                _context.CertificateAndMedalsFacts.Add(medalFact);
                await _context.SaveChangesAsync();
            }

            // Удаляем старые медали студента
            var existingMedals = _context.StudentCertificateAndMedals.Where(m => m.StudentsId == studentId);
            _context.StudentCertificateAndMedals.RemoveRange(existingMedals);

            // Добавляем новую связь студента с медалью
            _context.StudentCertificateAndMedals.Add(new StudentCertificateAndMedal
            {
                StudentsId = studentId,
                CertificateAndMedalsFactId = medalFact.Id
            });

            await _context.SaveChangesAsync();
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

                        // Проверка медалей (столбцы 7-8)
                        var medalName = row.Cell(7).GetString();
                        var medalStatus = row.Cell(8).GetString();

                        if (!string.IsNullOrWhiteSpace(medalName) && string.IsNullOrWhiteSpace(medalStatus))
                        {
                            result.Errors.Add($"Строка {rowNum}: Не указан статус для медали");
                        }
                        else if (string.IsNullOrWhiteSpace(medalName) && !string.IsNullOrWhiteSpace(medalStatus))
                        {
                            result.Errors.Add($"Строка {rowNum}: Указан статус медали, но не указана медаль");
                        }

                        // Проверка предметов ГИА (столбец 9)
                        var giaSubjects = row.Cell(9).GetString()?.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

                        if (giaSubjects != null && giaSubjects.Any(string.IsNullOrWhiteSpace))
                        {
                            result.Errors.Add($"Строка {rowNum}: Обнаружены пустые значения в предметах ГИА");
                        }

                        // Проверка олимпиад (начиная со столбца 10)
                        int olympiadColumn = 10;
                        while (olympiadColumn < row.Worksheet.ColumnsUsed().Count())
                        {
                            var olympiadType = row.Cell(olympiadColumn).GetString();
                            var olympiadSubject = row.Cell(olympiadColumn + 1).GetString();

                            if (!string.IsNullOrWhiteSpace(olympiadType) && string.IsNullOrWhiteSpace(olympiadSubject))
                            {
                                result.Errors.Add($"Строка {rowNum}: Не указан предмет для олимпиады '{olympiadType}'");
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
        /// Отображает информационное сообщение
        /// </summary>
        private async Task ShowCustomMessage(Window parentWindow, string title, string message, IBrush borderBrush)
        {
            var tcs = new TaskCompletionSource<bool>();

            var okButton = new Button
            {
                Content = new TextBlock
                {
                    Text = "OK",
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                },
                Width = 100,
                HorizontalAlignment = HorizontalAlignment.Center,
                Height = 30,
                Background = borderBrush is SolidColorBrush brush && brush.Color == Brushes.Red.Color ? Brushes.Red : Brushes.Green,
                Margin = new Thickness(5)
            };

            var content = new StackPanel
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
                Margin = new Thickness(0, 0, 0, 20)
            }
        }
            };

            if (message.Count(c => c == '\n') > 5)
            {
                var scrollViewer = new ScrollViewer
                {
                    Height = 200,
                    Content = content,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                };

                content = new StackPanel
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Children =
            {
                scrollViewer,
                okButton
            }
                };
            }
            else
            {
                content.Children.Add(okButton);
            }

            var dialog = new Window
            {
                Title = title,
                Width = 550,
                Height = message.Count(c => c == '\n') > 5 ? 350 : 200,
                MinWidth = 400,
                MinHeight = 180,
                MaxWidth = 550,
                MaxHeight = 400,
                WindowState = WindowState.Normal,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                SizeToContent = SizeToContent.Manual,
                CanResize = true,
                Content = new Border
                {
                    BorderBrush = borderBrush,
                    BorderThickness = new Thickness(2),
                    Child = new Grid
                    {
                        Margin = new Thickness(15),
                        Children = { content }
                    }
                }
            };

            okButton.Click += (s, e) => { tcs.TrySetResult(true); dialog.Close(); };
            dialog.Closed += (sender, e) => tcs.TrySetResult(true);
            await dialog.ShowDialog(parentWindow);
            await tcs.Task;
        }

        /// <summary>
        /// Отображает диалоговое окно подтверждения
        /// </summary>
        private async Task<bool> ShowConfirmationDialog(Window parentWindow, string title, string message)
        {
            var tcs = new TaskCompletionSource<bool>();

            var yesButton = new Button
            {
                Content = new TextBlock
                {
                    Text = "Да",
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                },
                Width = 100,
                HorizontalAlignment = HorizontalAlignment.Center,
                Height = 30,
                Background = Brushes.Green,
                Margin = new Thickness(5)
            };

            var noButton = new Button
            {
                Content = new TextBlock
                {
                    Text = "Нет",
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                },
                Width = 100,
                HorizontalAlignment = HorizontalAlignment.Center,
                Height = 30,
                Background = Brushes.Red,
                Margin = new Thickness(5)
            };

            var dialog = new Window
            {
                Title = title,
                Width = 550,
                Height = 200,
                MinWidth = 400,
                MinHeight = 180,
                MaxWidth = 550,
                MaxHeight = 300,
                WindowState = WindowState.Normal,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                SizeToContent = SizeToContent.Manual,
                CanResize = false,
                Content = new Border
                {
                    BorderBrush = Brushes.Gray,
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
                                FontSize = 14
                            },
                            new StackPanel
                            {
                                Orientation = Orientation.Horizontal,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                Children =
                                {
                                    yesButton,
                                    noButton
                                }
                            }
                        }
                    }
                }
                    }
                }
            };

            yesButton.Click += (s, e) => { tcs.TrySetResult(true); dialog.Close(); };
            noButton.Click += (s, e) => { tcs.TrySetResult(false); dialog.Close(); };

            dialog.Closed += (sender, e) => tcs.TrySetResult(false);
            await dialog.ShowDialog(parentWindow);
            return await tcs.Task;
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

            var giaSubject = await _context.GiaSubjects.FirstOrDefaultAsync(gs => gs.GiaSubjectsNavigation.Name == subjectName);

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

            var olympiad = await _context.Olympiads.FirstOrDefaultAsync(o => o.OlympiadsNavigation.Name == typeName && o.OlympiadsItemsNavigation.Name == subjectName);

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