using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using DocumentFormat.OpenXml.Drawing.Charts;
using StudentProfileSystem.Models;
using Avalonia.Layout;
using Avalonia.Input;
using Thickness = Avalonia.Thickness;
using Orientation = Avalonia.Layout.Orientation;
using Avalonia.VisualTree;
using Microsoft.EntityFrameworkCore;
using Avalonia.Interactivity;
using DocumentFormat.OpenXml.Spreadsheet;
using Border = Avalonia.Controls.Border;
using System.Text.RegularExpressions;

namespace StudentProfileSystem;

public partial class StudentEditWindow : Window
{
    Student Student1;
    private readonly int _schoolId;

    List<Class> classes = new List<Class>();
    List<School> schools = new List<School>();

    List<Education> _Education = new List<Education>();
    List<EducationalInstitution> _EducationalInstitution = new List<EducationalInstitution>();
    List<Profile> _Profiles = new List<Profile>();

    // Поля для работы с Историей классов и школ
    private List<StudentSchoolHistory> _schoolHistory = new List<StudentSchoolHistory>();
    private List<StudentClassHistory> _classHistory = new List<StudentClassHistory>();

    // Поля для работы с олимпиадами и ГИА
    private List<Olympiad> olympiads = new List<Olympiad>();
    private List<GiaSubject> giaSubjects = new List<GiaSubject>();
    private List<StudentOlympiadParticipation> studentOlympiads = new List<StudentOlympiadParticipation>();
    private List<StudentGiaResult> studentGias = new List<StudentGiaResult>();

    public StudentEditWindow()
    {
        InitializeComponent();

        Student1 = new Student();
        OkkoRedactAndAdd.DataContext = Student1;

        LoadComboBoxData();
    }

    /// <summary>
    /// Конструктор для добавления нового студента
    /// </summary>
    public StudentEditWindow(int schoolId)
    {
        InitializeComponent();

        _schoolId = schoolId;
        Title = "Добавление";
        Student1 = new Student();

        // Если передали schoolId, устанавливаем его для нового студента
        if (_schoolId != null)
        {
            Student1.SchoolId = _schoolId;
        }

        OkkoRedactAndAdd.DataContext = Student1;
        ComboBox_Class.SelectionChanged += ComboBox_Class_SelectionChanged;

        LoadComboBoxData();
        UpdateFieldsVisibility();
    }

    /// <summary>
    /// Конструктор для редактирования существующего студента
    /// </summary>
    /// <param name="student">Студент для редактирования</param>
    public StudentEditWindow(Student student)
    {
        InitializeComponent();

        Title = "Редактирование";

        Student1 = student;
        OkkoRedactAndAdd.DataContext = Student1;
        ComboBox_Class.SelectionChanged += ComboBox_Class_SelectionChanged;

        LoadComboBoxData();
        UpdateFieldsVisibility();
    }

    private void UpdateFieldsVisibility()
    {
        if (ComboBox_Class.SelectedItem is Class selectedClass)
        {
            // Используем регулярное выражение для извлечения числовой части класса
            var match = Regex.Match(selectedClass.ClassesNumber, @"^\d{1,2}");
            bool isNumeric = match.Success;
            int classNumber = isNumeric ? int.Parse(match.Value) : 0;

            // Показываем/скрываем поле "Профиль" только для 10-11 классов
            bool isProfileVisible = isNumeric && (classNumber == 10 || classNumber == 11);
            ProfileLabel.IsVisible = isProfileVisible;
            ComboBox_Profile.IsVisible = isProfileVisible;

            // Если профиль скрыт, сбрасываем значение
            if (!isProfileVisible)
            {
                ComboBox_Profile.SelectedIndex = 0;
                Student1.ProfileId = null;
            }

            // Показываем/скрываем поля "Учебное заведение" и "Образование" для 9 и 11 классов
            bool isEducationVisible = isNumeric && (classNumber == 9 || classNumber == 11);
            EducationalInstitutionLabel.IsVisible = isEducationVisible;
            ComboBox_EducationalInstitution.IsVisible = isEducationVisible;
            EducationLabel.IsVisible = isEducationVisible;
            ComboBox_Education.IsVisible = isEducationVisible;

            // Если поля образования скрыты, сбрасываем значения
            if (!isEducationVisible)
            {
                ComboBox_EducationalInstitution.SelectedIndex = 0;
                ComboBox_Education.SelectedIndex = 0;
                Student1.EducationalInstitutionId = null;
                Student1.TypeEducation = null;
            }

            // Показываем/скрываем раздел ГИА в зависимости от класса и наличия записей
            bool shouldShowGiaSection = (isNumeric && (classNumber == 9 || classNumber == 11)) ||
                                        (Student1.Id != 0 && studentGias.Any());

            // Находим родительский Border, содержащий раздел ГИА
            var giaBorder = this.FindControl<Border>("GiaBorder");
            if (giaBorder != null)
            {
                giaBorder.IsVisible = shouldShowGiaSection;
            }
        }
    }

    private void ComboBox_Class_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateFieldsVisibility();
    }

    /// <summary>
    /// Загрузка данных в комбобоксы
    /// </summary>
    public void LoadComboBoxData()
    {
        LoadSchoolHistory();
        LoadClassHistory();
        LoadComboBoxClass();
        LoadComboBoxSchool();
        LoadComboBoxEducationalInstitution();
        LoadComboBoxEducation();
        LoadComboBoxOlympiad();
        LoadComboBoxGia();
        LoadComboBoxProfile();

        // Загружаем только для существующего студента
        if (Student1.Id != 0)
        {
            LoadStudentOlympiads();
            LoadStudentGias();
        }
    }

    private void LoadSchoolHistory()
    {
        if (Student1.Id != 0)
        {
            _schoolHistory = Helper.DateBase.StudentSchoolHistories.Where(h => h.StudentId == Student1.Id).Include(h => h.SchoolNavigation).OrderByDescending(h => h.ChangeDate).ToList();
            ListBox_HistorySchool.ItemsSource = _schoolHistory;
        }
        else
        {
            _schoolHistory.Clear();
            ListBox_HistorySchool.ItemsSource = _schoolHistory;
        }
    }

    private void LoadClassHistory()
    {
        if (Student1.Id != 0)
        {
            _classHistory = Helper.DateBase.StudentClassHistories.Where(h => h.StudentId == Student1.Id).Include(h => h.ClassNavigation).OrderByDescending(h => h.ChangeDate).ToList();
            ListBox_HistoryClass.ItemsSource = _classHistory;
        }
        else
        {
            _classHistory.Clear();
            ListBox_HistoryClass.ItemsSource = _classHistory;
        }
    }

    /// <summary>
    /// Загрузка профилей
    /// </summary>
    public void LoadComboBoxProfile()
    {
        _Profiles = Helper.DateBase.Profiles.ToList();

        if (Student1.ProfileId == null)
        {
            _Profiles.Insert(0, new Profile()
            {
                Id = 0,
                Name = "Профиль"
            });
        }

        ComboBox_Profile.ItemsSource = _Profiles.OrderByDescending(z => Student1.ProfileId != null ? z.Id == Student1.ProfileId : z.Name == "Профиль");
        ComboBox_Profile.SelectedIndex = 0;
    }

    public void LoadComboBoxEducationalInstitution()
    {
        _EducationalInstitution = Helper.DateBase.EducationalInstitutions.ToList();

        if (Student1.EducationalInstitutionId == null)
        {
            _EducationalInstitution.Insert(0, new EducationalInstitution()
            {
                Id = 0,
                Name = "Учебное заведение"
            });
        }

        ComboBox_EducationalInstitution.ItemsSource = _EducationalInstitution.OrderByDescending(z => Student1.EducationalInstitutionId != null ? z.Id == Student1.EducationalInstitutionId : z.Name == "Учебное заведение");
        ComboBox_EducationalInstitution.SelectedIndex = 0;
    }

    public void LoadComboBoxEducation()
    {
        _Education = Helper.DateBase.Educations.ToList();

        if (Student1.TypeEducation == null)
        {
            _Education.Insert(0, new Education()
            {
                Id = 0,
                Name = "Образование"
            });
        }

        ComboBox_Education.ItemsSource = _Education.OrderByDescending(z => Student1.TypeEducation != null ? z.Id == Student1.TypeEducation : z.Name == "Образование");
        ComboBox_Education.SelectedIndex = 0;
    }

    /// <summary>
    /// Загрузка данных о классах в комбобокс
    /// </summary>
    public void LoadComboBoxClass()
    {
        try
        {
            classes = Helper.DateBase.Classes.ToList();

            if (Student1.Id != 0)
            {
                ComboBox_Class.ItemsSource = classes.OrderByDescending(z => z.Id == Student1.ClassId);
                ComboBox_Class.SelectedIndex = 0;
            }
            else
            {
                classes.Add(new Class() { ClassesNumber = "Класс" });
                ComboBox_Class.ItemsSource = classes.OrderByDescending(z => z.ClassesNumber == "Класс");
                ComboBox_Class.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при загрузке ComboBox: {ex.Message}");
        }
    }

    /// <summary>
    /// Загрузка данных о школах в комбобокс
    /// </summary>
    public void LoadComboBoxSchool()
    {
        try
        {
            schools = Helper.DateBase.Schools.ToList();

            if (Student1.Id != 0)
            {
                // Для существующего студента
                ComboBox_School.ItemsSource = schools.OrderByDescending(z => z.Id == Student1.SchoolId);
                ComboBox_School.SelectedIndex = 0;
            }
            else
            {
                // Для нового студента
                if (_schoolId != null)
                {
                    // Устанавливаем школу из параметра
                    var school = schools.FirstOrDefault(s => s.Id == _schoolId);
                    if (school != null)
                    {
                        schools = schools.OrderByDescending(s => s.Id == _schoolId).ToList();
                        ComboBox_School.ItemsSource = schools;
                        ComboBox_School.SelectedIndex = 0;
                        return;
                    }
                }

                // Если schoolId не передан или школа не найдена - добавляем заглушку
                schools.Insert(0, new School() { Id = 0, Name = "Школа" });
                ComboBox_School.ItemsSource = schools;
                ComboBox_School.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при загрузке ComboBox: {ex.Message}");
        }
    }

    /// <summary>
    /// Загрузка данных об олимпиадах в комбобокс
    /// </summary>
    private void LoadComboBoxOlympiad()
    {
        try
        {
            olympiads = Helper.DateBase.Olympiads.Include(o => o.OlympiadsNavigation)
                                                 .Include(o => o.OlympiadsItemsNavigation).ToList();

            ComboBox_Olympiad.ItemsSource = olympiads;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при загрузке ComboBox олимпиад: {ex.Message}");
        }
    }

    /// <summary>
    /// Загрузка данных о ГИА в комбобокс
    /// </summary>
    private void LoadComboBoxGia()
    {
        try
        {
            giaSubjects = Helper.DateBase.GiaSubjects.Include(g => g.GiaSubjectsNavigation).ToList();

            ComboBox_Gia.ItemsSource = giaSubjects;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при загрузке ComboBox ГИА: {ex.Message}");
        }
    }

    /// <summary>
    /// Загрузка олимпиад студента
    /// </summary>
    private void LoadStudentOlympiads()
    {
        if (Student1.Id != 0)
        {
            studentOlympiads = Helper.DateBase.StudentOlympiadParticipations.Where(sop => sop.IdStudents == Student1.Id)
                                                                            .Include(sop => sop.IdOlympiadsNavigation)
                                                                                .ThenInclude(o => o.OlympiadsNavigation)
                                                                            .Include(sop => sop.IdOlympiadsNavigation)
                                                                                .ThenInclude(o => o.OlympiadsItemsNavigation).ToList();

            ListBox_Olympiads.ItemsSource = studentOlympiads.Select(sop => sop.IdOlympiadsNavigation).ToList();
        }
    }

    /// <summary>
    /// Загрузка ГИА студента
    /// </summary>
    private void LoadStudentGias()
    {
        if (Student1.Id != 0)
        {
            studentGias = Helper.DateBase.StudentGiaResults.Where(sgr => sgr.IdStudents == Student1.Id)
                                                           .Include(sgr => sgr.IdGiaSubjectsNavigation)
                                                               .ThenInclude(gs => gs.GiaSubjectsNavigation)
                                                           .Include(sgr => sgr.IdGiaSubjectsNavigation).ToList();

            ListBox_Gias.ItemsSource = studentGias.Select(sgr => sgr.IdGiaSubjectsNavigation).ToList();
        }
    }

    /// <summary>
    /// Добавление олимпиады студенту
    /// </summary>
    private void Button_AddOlympiad_Click(object sender, RoutedEventArgs e)
    {
        var selectedOlympiad = ComboBox_Olympiad.SelectedItem as Olympiad;
        if (selectedOlympiad == null) return;

        // Проверка на дублирование
        if (!studentOlympiads.Any(sop => sop.IdOlympiads == selectedOlympiad.Id))
        {
            var participation = new StudentOlympiadParticipation
            {
                IdStudents = Student1.Id,
                IdOlympiads = selectedOlympiad.Id,
                IdOlympiadsNavigation = selectedOlympiad
            };

            studentOlympiads.Add(participation);
            UpdateOlympiadListBox();
        }
    }

    /// <summary>
    /// Обновление списка олимпиад
    /// </summary>
    private void UpdateOlympiadListBox()
    {
        ListBox_Olympiads.ItemsSource = null;
        ListBox_Olympiads.ItemsSource = studentOlympiads.Select(sop => sop.IdOlympiadsNavigation).ToList();
    }

    /// <summary>
    /// Удаление олимпиады у студента
    /// </summary>
    private void Button_RemoveOlympiad_Click(object sender, RoutedEventArgs e)
    {
        var selectedOlympiad = ListBox_Olympiads.SelectedItem as Olympiad;
        if (selectedOlympiad == null) return;

        var participation = studentOlympiads.FirstOrDefault(sop => sop.IdOlympiads == selectedOlympiad.Id);
        if (participation != null)
        {
            studentOlympiads.Remove(participation);
            UpdateOlympiadListBox();
        }
    }

    /// <summary>
    /// Добавление ГИА студенту
    /// </summary>
    private void Button_AddGia_Click(object sender, RoutedEventArgs e)
    {
        var selectedGia = ComboBox_Gia.SelectedItem as GiaSubject;
        if (selectedGia == null) return;

        // Проверка на дублирование
        if (!studentGias.Any(sgr => sgr.IdGiaSubjects == selectedGia.Id))
        {
            var result = new StudentGiaResult
            {
                IdStudents = Student1.Id,
                IdGiaSubjects = selectedGia.Id,
                IdGiaSubjectsNavigation = selectedGia
            };

            studentGias.Add(result);
            UpdateGiaListBox();
        }
    }

    /// <summary>
    /// Обновление списка ГИА
    /// </summary>
    private void UpdateGiaListBox()
    {
        ListBox_Gias.ItemsSource = null;
        ListBox_Gias.ItemsSource = studentGias.Select(sgr => sgr.IdGiaSubjectsNavigation).ToList();
    }

    /// <summary>
    /// Удаление ГИА у студента
    /// </summary>
    private void Button_RemoveGia_Click(object sender, RoutedEventArgs e)
    {
        var selectedGia = ListBox_Gias.SelectedItem as GiaSubject;
        if (selectedGia == null) return;

        var result = studentGias.FirstOrDefault(sgr => sgr.IdGiaSubjects == selectedGia.Id);
        if (result != null)
        {
            studentGias.Remove(result);
            UpdateGiaListBox();
        }
    }

    /// <summary>
    /// Проверка валидности данных студента
    /// </summary>
    private bool ValidateStudentData()
    {
        // Проверка ФИО
        if (string.IsNullOrWhiteSpace(Student1.LastName))
        {
            ShowErrorDialog("Фамилия обязательна для заполнения!");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Student1.FirstName))
        {
            ShowErrorDialog("Имя обязательно для заполнения!");
            return false;
        }

        // Проверка на цифры и специальные символы в ФИО
        if (ContainsInvalidChars(Student1.LastName) ||
            ContainsInvalidChars(Student1.FirstName) ||
            (Student1.Patronymic != null && ContainsInvalidChars(Student1.Patronymic)))
        {
            ShowErrorDialog("ФИО может содержать только буквы и дефис!");
            return false;
        }

        // Проверка комбобоксов
        var selectedSchool = ComboBox_School.SelectedItem as School;
        var selectedClass = ComboBox_Class.SelectedItem as Class;

        if (selectedSchool == null || selectedSchool.Id == 0)
        {
            ShowErrorDialog("Выберите школу из списка!");
            return false;
        }

        if (selectedClass == null || selectedClass.Id == 0)
        {
            ShowErrorDialog("Выберите класс из списка!");
            return false;
        }

        return true;
    }

    private bool ContainsInvalidChars(string input)
    {
        return input != null && input.Any(c => !char.IsLetter(c) && c != '-' && c != ' ');
    }

    /// <summary>
    /// Сохранение данных после редактирования
    /// </summary>
    private async void Button_Click_Save(object? sender, RoutedEventArgs e)
    {
        if (!ValidateStudentData()) return;

        // Обновляем значения профиля, образования и учебного заведения в зависимости от видимости полей
        if (ComboBox_Profile.IsVisible && ComboBox_Profile.SelectedItem is Profile selectedProfile)
        {
            Student1.ProfileId = selectedProfile.Id == 0 ? null : selectedProfile.Id;
        }
        else
        {
            Student1.ProfileId = null;
        }

        if (ComboBox_EducationalInstitution.IsVisible && ComboBox_EducationalInstitution.SelectedItem is EducationalInstitution selectedInstitution)
        {
            Student1.EducationalInstitutionId = selectedInstitution.Id == 0 ? null : selectedInstitution.Id;
        }
        else
        {
            Student1.EducationalInstitutionId = null;
        }

        if (ComboBox_Education.IsVisible && ComboBox_Education.SelectedItem is Education selectedEducation)
        {
            Student1.TypeEducation = selectedEducation.Id == 0 ? null : selectedEducation.Id;
        }
        else
        {
            Student1.TypeEducation = null;
        }

        var confirm = await ShowConfirmationDialog("Вы уверены, что хотите сохранить изменения?");
        if (!confirm) return;

        var selectedSchool = ComboBox_School.SelectedItem as School;
        var selectedClass = ComboBox_Class.SelectedItem as Class;

        bool schoolChanged = false;
        bool classChanged = false;

        if (Student1.Id != 0)
        {
            var originalStudent = Helper.DateBase.Students.AsNoTracking().FirstOrDefault(s => s.Id == Student1.Id);

            if (originalStudent != null)
            {
                schoolChanged = originalStudent.SchoolId != selectedSchool?.Id;
                classChanged = originalStudent.ClassId != selectedClass?.Id;
            }
        }
        else
        {
            schoolChanged = selectedSchool != null;
            classChanged = selectedClass != null;
        }

        using (var transaction = await Helper.DateBase.Database.BeginTransactionAsync())
        {
            try
            {
                // Устанавливаем SchoolId и ClassId
                if (selectedSchool != null)
                    Student1.SchoolId = selectedSchool.Id;

                if (selectedClass != null)
                    Student1.ClassId = selectedClass.Id;

                // Сохраняем основную информацию о студенте
                if (Student1.Id == 0)
                {
                    Helper.DateBase.Students.Add(Student1);
                    await Helper.DateBase.SaveChangesAsync();
                }
                else
                {
                    Helper.DateBase.Students.Update(Student1);
                    await Helper.DateBase.SaveChangesAsync();
                }

                var currentDate = DateOnly.FromDateTime(DateTime.Now);

                // Сохраняем историю изменений школы
                if (schoolChanged && selectedSchool != null)
                {
                    Helper.DateBase.StudentSchoolHistories.Add(new StudentSchoolHistory
                    {
                        StudentId = Student1.Id,
                        SchoolId = selectedSchool.Id,
                        ChangeDate = currentDate
                    });
                }

                // Сохраняем историю изменений класса
                if (classChanged && selectedClass != null)
                {
                    Helper.DateBase.StudentClassHistories.Add(new StudentClassHistory
                    {
                        StudentId = Student1.Id,
                        ClassId = selectedClass.Id,
                        ChangeDate = currentDate
                    });
                }

                await Helper.DateBase.SaveChangesAsync();

                // Обрабатываем олимпиады и ГИА
                await ProcessOlympiads(Student1.Id);
                await ProcessGias();

                await transaction.CommitAsync();
                await ShowSuccessDialog("Данные успешно сохранены!");
                Close();
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                string errorDetails = dbEx.InnerException?.Message ?? dbEx.Message;
                await ShowErrorDialog($"Ошибка базы данных: {errorDetails}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await ShowErrorDialog($"Ошибка: {ex.Message}");
            }
        }
    }

    private async Task ProcessOlympiads(int studentId)
    {
        var existingOlympiads = await Helper.DateBase.StudentOlympiadParticipations.Where(sop => sop.IdStudents == studentId).ToListAsync();

        // Удаляем отсутствующие
        foreach (var existing in existingOlympiads)
        {
            if (!studentOlympiads.Any(sop => sop.IdOlympiads == existing.IdOlympiads))
            {
                Helper.DateBase.StudentOlympiadParticipations.Remove(existing);
            }
        }

        // Добавляем новые
        foreach (var newPart in studentOlympiads)
        {
            if (!existingOlympiads.Any(eo => eo.IdOlympiads == newPart.IdOlympiads))
            {
                newPart.IdStudents = studentId;
                Helper.DateBase.StudentOlympiadParticipations.Add(newPart);
            }
        }

        await Helper.DateBase.SaveChangesAsync();
    }

    private async Task ProcessGias()
    {
        // Получаем текущие ГИА студента
        var existingGias = await Helper.DateBase.StudentGiaResults.Where(sgr => sgr.IdStudents == Student1.Id).ToListAsync();

        // Удаляем те, которых нет в новом списке
        var giasToRemove = existingGias.Where(eg => !studentGias.Any(sg => sg.IdGiaSubjects == eg.IdGiaSubjects)).ToList();

        Helper.DateBase.StudentGiaResults.RemoveRange(giasToRemove);

        // Добавляем новые
        var giasToAdd = studentGias.Where(sg => !existingGias.Any(eg => eg.IdGiaSubjects == sg.IdGiaSubjects)).Select(sg => new StudentGiaResult
            {
                IdStudents = Student1.Id,
                IdGiaSubjects = sg.IdGiaSubjects
            }).ToList();

        Helper.DateBase.StudentGiaResults.AddRange(giasToAdd);
        await Helper.DateBase.SaveChangesAsync();
    }

    /// <summary>
    /// Диалог подтверждения
    /// </summary>
    private async Task<bool> ShowConfirmationDialog(string message)
    {
        bool result = false;
        Window dialog = null;

        Button CreateDialogButton(string text, IBrush background, bool dialogResult)
        {
            return new Button
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
        }

        var yesButton = CreateDialogButton("Да", Brushes.Green, true);
        var noButton = CreateDialogButton("Нет", Brushes.Red, false);

        yesButton.Click += (s, e) => { result = true; dialog?.Close(); };
        noButton.Click += (s, e) => { result = false; dialog?.Close(); };

        dialog = new Window
        {
            Title = "Подтверждение",
            Width = 550,
            Height = 250,
            MinHeight = 250,
            WindowState = WindowState.Normal,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            SizeToContent = SizeToContent.Manual,
            CanResize = false,
            Content = new Avalonia.Controls.Border
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

        await dialog.ShowDialog(this);
        return result;
    }

    /// <summary>
    /// Диалог ошибки
    /// </summary>
    private async Task ShowErrorDialog(string message)
    {
        var dialog = new Window
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
                                Margin = new Thickness(0, 0, 0, 20)
                            },
                            new Button
                            {
                                Content = "OK",
                                Width = 100,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                Height = 30,
                                Background = Brushes.Gray,
                                Margin = new Thickness(5)
                            }
                        }
                    }
                }
                }
            }
        };

        var border = (Border)dialog.Content;
        var grid = (Grid)border.Child;
        var stackPanel = (StackPanel)grid.Children[0];
        var button = (Button)stackPanel.Children[1];
        button.Click += (s, e) => dialog.Close();

        await dialog.ShowDialog(this);
    }

    /// <summary>
    /// Диалог успеха
    /// </summary>
    private async Task ShowSuccessDialog(string message)
    {
        var dialog = new Window
        {
            Title = "Успех",
            Width = 550,
            Height = 250,
            MinHeight = 250,
            WindowState = WindowState.Normal,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            SizeToContent = SizeToContent.Manual,
            CanResize = false,
            Content = new Border
            {
                BorderBrush = Brushes.Green,
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
                            new Button
                            {
                                Content = "OK",
                                Width = 100,
                                HorizontalAlignment = HorizontalAlignment.Center,
                                Height = 30,
                                Background = Brushes.Green,
                                Margin = new Thickness(5)
                            }
                        }
                    }
                }
                }
            }
        };

        var border = (Border)dialog.Content;
        var grid = (Grid)border.Child;
        var stackPanel = (StackPanel)grid.Children[0];
        var button = (Button)stackPanel.Children[1];
        button.Click += (s, e) => dialog.Close();

        await dialog.ShowDialog(this);
    }

    private Button CreateDialogButton(string text, IBrush background)
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

        button.Click += (s, e) => (button.GetVisualRoot() as Window)?.Close();
        return button;
    }

    /// <summary>
    /// Проверка на несохраненные изменения
    /// </summary>
    private bool HasUnsavedChanges()
    {
        if (Student1.Id == 0) return true;
        if (Helper.DateBase.Entry(Student1).State == EntityState.Modified) return true;

        // Проверка олимпиад
        var currentOlympiads = Helper.DateBase.StudentOlympiadParticipations.Where(sop => sop.IdStudents == Student1.Id).Select(sop => sop.IdOlympiads).ToList();

        // Проверка ГИА
        var currentGias = Helper.DateBase.StudentGiaResults.Where(sgr => sgr.IdStudents == Student1.Id).Select(sgr => sgr.IdGiaSubjects).ToList();

        // Проверка истории классов
        var hasClassHistory = Helper.DateBase.StudentClassHistories
            .Any(h => h.StudentId == Student1.Id);

        // Проверка истории школ
        var hasSchoolHistory = Helper.DateBase.StudentSchoolHistories.Any(h => h.StudentId == Student1.Id);

        if (studentOlympiads.Count != currentOlympiads.Count ||
            studentGias.Count != currentGias.Count ||
            (hasClassHistory && _classHistory.Count == 0) ||
            (hasSchoolHistory && _schoolHistory.Count == 0))
            return true;

        if (studentOlympiads.Any(sop => !currentOlympiads.Contains(sop.IdOlympiads)))
            return true;

        if (studentGias.Any(sgr => !currentGias.Contains(sgr.IdGiaSubjects)))
            return true;

        return false;
    }

    /// <summary>
    /// Блокирует ввод цифр в ФИО
    /// </summary>
    private void TextBox_TextInput(object? sender, TextInputEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            if (!char.IsLetter(e.Text[0]) && e.Text[0] != ' ' && e.Text[0] != '-')
            {
                e.Handled = true;
            }
        }
    }

    /// <summary>
    /// Закрытие окна с подтверждением
    /// </summary>
    private async void Button_Click_Close(object? sender, RoutedEventArgs e)
    {
        if (HasUnsavedChanges())
        {
            var confirm = await ShowConfirmationDialog("У вас есть несохраненные изменения. Закрыть без сохранения?");
            if (!confirm) return;
        }
        Close();
    }

    /// <summary>
    /// Редактирование школ
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_Click_Redact_School(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        School school = new School();
        RedactSchoolAndClass redactSchoolAndClass = new RedactSchoolAndClass(school);
        redactSchoolAndClass.Closed += (s, args) => LoadComboBoxData();
        redactSchoolAndClass.ShowDialog(this);
    }

    /// <summary>
    /// Редактирование классов
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_Click_Redact_Class(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Class classs = new Class();
        RedactSchoolAndClass redactSchoolAndClass = new RedactSchoolAndClass(classs);
        redactSchoolAndClass.Closed += (s, args) => LoadComboBoxData();
        redactSchoolAndClass.ShowDialog(this);
    }

    private async void Button_Click_ClearHistoryClass(object? sender, RoutedEventArgs e)
    {
        if (Student1.Id == 0) return;

        var confirm = await ShowConfirmationDialog("Вы уверены, что хотите очистить историю классов?");
        if (!confirm) return;

        try
        {
            var historyToDelete = Helper.DateBase.StudentClassHistories.Where(h => h.StudentId == Student1.Id).ToList();

            Helper.DateBase.StudentClassHistories.RemoveRange(historyToDelete);
            await Helper.DateBase.SaveChangesAsync();

            // Полностью перезагружаем историю из базы
            _classHistory = Helper.DateBase.StudentClassHistories.Where(h => h.StudentId == Student1.Id).Include(h => h.ClassNavigation).OrderByDescending(h => h.ChangeDate).ToList();

            // Обновляем привязку
            ListBox_HistoryClass.ItemsSource = null;
            ListBox_HistoryClass.ItemsSource = _classHistory;

            await ShowSuccessDialog("История классов успешно очищена!");
        }
        catch (Exception ex)
        {
            await ShowErrorDialog($"Ошибка при очистке истории классов: {ex.Message}");
        }
    }

    private async void Button_Click_ClearHistorySchool(object? sender, RoutedEventArgs e)
    {
        if (Student1.Id == 0) return;

        var confirm = await ShowConfirmationDialog("Вы уверены, что хотите очистить историю школ?");
        if (!confirm) return;

        try
        {
            var historyToDelete = Helper.DateBase.StudentSchoolHistories.Where(h => h.StudentId == Student1.Id).ToList();

            Helper.DateBase.StudentSchoolHistories.RemoveRange(historyToDelete);
            await Helper.DateBase.SaveChangesAsync();

            // Полностью перезагружаем историю из базы
            _schoolHistory = Helper.DateBase.StudentSchoolHistories.Where(h => h.StudentId == Student1.Id).Include(h => h.SchoolNavigation).OrderByDescending(h => h.ChangeDate).ToList();

            // Обновляем привязку
            ListBox_HistorySchool.ItemsSource = null;
            ListBox_HistorySchool.ItemsSource = _schoolHistory;

            await ShowSuccessDialog("История школ успешно очищена!");
        }
        catch (Exception ex)
        {
            await ShowErrorDialog($"Ошибка при очистке истории школ: {ex.Message}");
        }
    }
}