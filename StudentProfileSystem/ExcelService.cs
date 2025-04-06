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

namespace StudentProfileSystem.Services
{
    /// <summary>
    /// Класс для работы с импортом данных в БД и экспортом в Exel
    /// </summary>
    public class ExcelService
    {
        private readonly ImcContext _context;

        /// <summary>
        /// Инициализирует новый экземпляр сервиса для работы с Excel
        /// </summary>
        /// <param name="context"></param>
        public ExcelService(ImcContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Экспорт данных из БД в Exel
        /// </summary>
        /// <param name="parentWindow"></param>
        /// <param name="students"></param>
        /// <returns></returns>
        public async Task ExportStudentsToExcel(Window parentWindow, IEnumerable<Student> students)
        {
            try
            {
                // Подтверждение экспорта
                var confirm = await ShowConfirmationDialog( parentWindow, "Подтверждение экспорта", "Вы уверены, что хотите экспортировать данные в Excel?");

                if (!confirm) return;

                var saveDialog = new SaveFileDialog
                {
                    Title = "Сохранить Excel-файл",
                    Filters = new List<FileDialogFilter> { new() { Name = "Excel Files", Extensions = { "xlsx" } } },
                    InitialFileName = $"Students_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                var filePath = await saveDialog.ShowAsync(parentWindow);
                if (string.IsNullOrEmpty(filePath)) return;

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Ученики");

                    // Заголовки
                    var headers = new[] 
                    { 
                        "Фамилия", 
                        "Имя", 
                        "Отчество", 
                        "Класс", 
                        "Школа", 
                        "Номер школы", 
                        "Район", 
                        "Город", 
                        "Тип ГИА", 
                        "Предмет ГИА", 
                        "Тип Олимпиады", 
                        "Предмет Олимпиады" 
                    };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cell(1, i + 1).Value = headers[i];
                        worksheet.Cell(1, i + 1).Style.Font.Bold = true;
                    }

                    // Данные
                    int row = 2;
                    foreach (var student in students)
                    {
                        worksheet.Cell(row, 1).Value = student.LastName;
                        worksheet.Cell(row, 2).Value = student.FirstName;
                        worksheet.Cell(row, 3).Value = student.Patronymic;
                        worksheet.Cell(row, 4).Value = student.Class?.ClassesNumber;
                        worksheet.Cell(row, 5).Value = student.School?.Name;
                        worksheet.Cell(row, 6).Value = student.School?.SchoolNumber;
                        worksheet.Cell(row, 7).Value = student.School?.District;
                        worksheet.Cell(row, 8).Value = student.School?.City;

                        var gia = student.StudentGiaResults.FirstOrDefault();
                        if (gia != null)
                        {
                            worksheet.Cell(row, 9).Value = gia.IdGiaSubjectsNavigation?.GiaType?.Name;
                            worksheet.Cell(row, 10).Value = gia.IdGiaSubjectsNavigation?.GiaSubjectsNavigation?.Name;
                        }

                        var olympiad = student.StudentOlympiadParticipations.FirstOrDefault();
                        if (olympiad != null)
                        {
                            worksheet.Cell(row, 11).Value = olympiad.IdOlympiadsNavigation?.OlympiadsNavigation?.Name;
                            worksheet.Cell(row, 12).Value = olympiad.IdOlympiadsNavigation?.OlympiadsItemsNavigation?.Name;
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
        /// Импорт данных в БД
        /// </summary>
        /// <param name="parentWindow"></param>
        /// <returns></returns>
        public async Task<bool> ImportStudentsFromExcel(Window parentWindow)
        {
            try
            {
                var confirm = await ShowConfirmationDialog(parentWindow, "Подтверждение импорта", 
                "Вы уверены, что хотите импортировать данные из Excel? Существующие данные не будут удалены.");

                if (!confirm) return false;

                var openDialog = new OpenFileDialog
                {
                    Title = "Выберите файл Excel",
                    Filters = new List<FileDialogFilter> { new() { Name = "Excel Files", Extensions = { "xlsx" } } },
                    AllowMultiple = false
                };

                var filePaths = await openDialog.ShowAsync(parentWindow);
                if (filePaths == null || filePaths.Length == 0) return false;

                using (var workbook = new XLWorkbook(filePaths[0]))
                {
                    var worksheet = workbook.Worksheet(1);
                    var rows = worksheet.RowsUsed().Skip(1);

                    var schoolsCache = new Dictionary<string, School>();
                    var classesCache = new Dictionary<string, Class>();
                    var itemsCache = new Dictionary<string, Item>();
                    var giaTypesCache = new Dictionary<string, GiaType>();
                    var olympiadTypesCache = new Dictionary<string, OlympiadsType>();

                    foreach (var row in rows)
                    {
                        // Обработка школы
                        var schoolName = row.Cell(5).GetString();
                        School school = null;
                        if (!string.IsNullOrEmpty(schoolName) && !schoolsCache.TryGetValue(schoolName, out school))
                        {
                            school = await GetOrCreateSchoolAsync(
                                name: schoolName,
                                number: row.Cell(6).GetString(),
                                district: row.Cell(7).GetString(),
                                city: row.Cell(8).GetString());
                            schoolsCache[schoolName] = school;
                        }

                        // Обработка класса
                        var className = row.Cell(4).GetString();
                        Class studentClass = null;
                        if (!string.IsNullOrEmpty(className) && !classesCache.TryGetValue(className, out studentClass))
                        {
                            studentClass = await GetOrCreateClassAsync(className);
                            classesCache[className] = studentClass;
                        }

                        // Добавление студента
                        var student = new Student
                        {
                            LastName = row.Cell(1).GetString(),
                            FirstName = row.Cell(2).GetString(),
                            Patronymic = row.Cell(3).GetString(),
                            ClassId = studentClass?.Id ?? 0,
                            SchoolId = school?.Id ?? 0
                        };
                        await AddStudentAsync(student);

                        // Обработка ГИА
                        var giaTypeName = row.Cell(9).GetString();
                        var giaSubjectName = row.Cell(10).GetString();
                        if (!string.IsNullOrEmpty(giaTypeName) && !string.IsNullOrEmpty(giaSubjectName))
                        {
                            var giaSubject = await GetOrCreateGiaSubjectAsync(giaTypeName, giaSubjectName);
                            await AddGiaResultAsync(student.Id, giaSubject.Id);
                        }

                        // Обработка олимпиад
                        var olympiadTypeName = row.Cell(11).GetString();
                        var olympiadSubjectName = row.Cell(12).GetString();
                        if (!string.IsNullOrEmpty(olympiadTypeName) && !string.IsNullOrEmpty(olympiadSubjectName))
                        {
                            var olympiad = await GetOrCreateOlympiadAsync(olympiadTypeName, olympiadSubjectName);
                            await AddOlympiadParticipationAsync(student.Id, olympiad.Id);
                        }
                    }
                }

                await ShowCustomMessage(parentWindow, "Импорт завершен", "Данные успешно импортированы из Excel!", Brushes.Green); return true;
            }
            catch (Exception ex)
            {
                await ShowCustomMessage(parentWindow, "Ошибка импорта", $"Произошла ошибка: {ex.Message}", Brushes.Red); return false;
            }
        }

        /// <summary>
        /// Диалоговое окно подтверждения
        /// </summary>
        /// <param name="parentWindow"></param>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task<bool> ShowConfirmationDialog(Window parentWindow, string title, string message)
        {
            var tcs = new TaskCompletionSource<bool>();

            var yesButton = CreateDialogButton("Да", Brushes.Green, tcs, true);
            var noButton = CreateDialogButton("Нет", Brushes.Red, tcs, false);

            var dialog = new Window
            {
                Title = title,
                Width = 550,
                Height = 250,
                MinHeight = 250,
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
                                FontSize = 16
                            },
                            new StackPanel
                            {
                                Orientation = Orientation.Horizontal,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                Children = { yesButton, noButton }
                            }
                        }
                    }
                }
                    }
                }
            };

            yesButton.Click += (s, e) => dialog.Close();
            noButton.Click += (s, e) => dialog.Close();

            dialog.Closed += (s, e) =>
            {
                if (!tcs.Task.IsCompleted)
                    tcs.SetResult(false);
            };

            await dialog.ShowDialog(parentWindow);
            return await tcs.Task;
        }

        /// <summary>
        /// Показывает информационное сообщение с кнопкой OK
        /// </summary>
        /// <param name="parentWindow">Родительское окно</param>
        /// <param name="title">Заголовок окна</param>
        /// <param name="message">Текст сообщения</param>
        /// <param name="borderBrush"></param>
        /// <returns></returns>
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
                Background = borderBrush,
                Margin = new Thickness(5)
            };

            var dialog = new Window
            {
                Title = title,
                Width = 550,
                Height = 250,
                MinHeight = 250,
                WindowState = WindowState.Normal,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                SizeToContent = SizeToContent.Manual,
                CanResize = false,
                Content = new Border
                {
                    BorderBrush = borderBrush,
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
                                Margin = new Thickness(0, 0, 0, 20)
                            },
                            okButton
                        }
                    }
                }
                    }
                }
            };

            okButton.Click += (s, e) =>
            {
                tcs.SetResult(true);
                dialog.Close();
            };

            dialog.Closed += (s, e) => tcs.TrySetResult(false);

            await dialog.ShowDialog(parentWindow);
            await tcs.Task;
        }

        /// <summary>
        /// Создает кнопки для диалогового окна
        /// </summary>
        /// <param name="text"></param>
        /// <param name="background"></param>
        /// <param name="tcs"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private Button CreateDialogButton(string text, IBrush background, TaskCompletionSource<bool> tcs, bool result)
        {
            var button = new Button
            {
                Content = new TextBlock
                {
                    Text = text,
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                },
                Width = 100,
                HorizontalAlignment = HorizontalAlignment.Center,
                Height = 30,
                Background = background,
                Margin = new Thickness(5)
            };

            button.Click += (s, e) => tcs.SetResult(result);
            return button;
        }

        /// <summary>
        /// Находит или создает новую школу в базе данных
        /// </summary>
        /// <param name="name"></param>
        /// <param name="number"></param>
        /// <param name="district"></param>
        /// <param name="city"></param>
        /// <returns></returns>
        private async Task<School> GetOrCreateSchoolAsync(string name, string number, string district, string city)
        {
            var school = await _context.Schools.FirstOrDefaultAsync(s => s.Name == name);
            if (school == null)
            {
                school = new School { Name = name, SchoolNumber = number, District = district, City = city };
                _context.Schools.Add(school);
                await _context.SaveChangesAsync();
            }
            return school;
        }

        /// <summary>
        /// Находит или создает новый класс в базе данных
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
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
        /// Добавляет нового студента в базу данных
        /// </summary>
        /// <param name="student"></param>
        /// <returns></returns>
        private async Task AddStudentAsync(Student student)
        {
            _context.Students.Add(student);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Находит или создает предмет ГИА в базе данных
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="subjectName"></param>
        /// <returns></returns>
        private async Task<GiaSubject> GetOrCreateGiaSubjectAsync(string typeName, string subjectName)
        {
            var giaType = await _context.GiaTypes.FirstOrDefaultAsync(t => t.Name == typeName);
            if (giaType == null)
            {
                giaType = new GiaType { Name = typeName };
                _context.GiaTypes.Add(giaType);
                await _context.SaveChangesAsync();
            }

            var item = await _context.Items.FirstOrDefaultAsync(i => i.Name == subjectName);
            if (item == null)
            {
                item = new Item { Name = subjectName };
                _context.Items.Add(item);
                await _context.SaveChangesAsync();
            }

            var giaSubject = await _context.GiaSubjects
                .FirstOrDefaultAsync(gs => gs.GiaTypeId == giaType.Id && gs.GiaSubjects == item.Id);

            if (giaSubject == null)
            {
                giaSubject = new GiaSubject { GiaTypeId = giaType.Id, GiaSubjects = item.Id };
                _context.GiaSubjects.Add(giaSubject);
                await _context.SaveChangesAsync();
            }

            return giaSubject;
        }

        /// <summary>
        /// Находит или создает олимпиаду в базе данных
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="subjectName"></param>
        /// <returns></returns>
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
                .FirstOrDefaultAsync(o => o.Olympiads == olympiadType.Id && o.OlympiadsItems == item.Id);

            if (olympiad == null)
            {
                olympiad = new Olympiad { Olympiads = olympiadType.Id, OlympiadsItems = item.Id };
                _context.Olympiads.Add(olympiad);
                await _context.SaveChangesAsync();
            }

            return olympiad;
        }

        /// <summary>
        /// Добавляет ГИА для студента
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="giaSubjectId"></param>
        /// <returns></returns>
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
        /// </summary>
        /// <param name="studentId"></param>
        /// <param name="olympiadId"></param>
        /// <returns></returns>
        private async Task AddOlympiadParticipationAsync(int studentId, int olympiadId)
        {
            _context.StudentOlympiadParticipations.Add(new StudentOlympiadParticipation
            {
                IdStudents = studentId,
                IdOlympiads = olympiadId
            });
            await _context.SaveChangesAsync();
        }
    }
}