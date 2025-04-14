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
    
    // Поля для работы с медалями
    private List<CertificateAndMedalsFact> _allMedalFacts = new List<CertificateAndMedalsFact>();
    private List<CertificateAndMedalsFact> _studentMedals = new List<CertificateAndMedalsFact>();

    public StudentEditWindow()
    {
        InitializeComponent();

        Student1 = new Student();
        OkkoRedactAndAdd.DataContext = Student1;

        LoadComboBoxData();
        UpdateFieldsVisibility();
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
        ComboBox_EducationalInstitution.SelectionChanged += ComboBox_EducationalInstitution_SelectionChanged;
        ComboBox_Profile.SelectionChanged += ComboBox_Profile_SelectionChanged;
        ComboBox_Education.SelectionChanged += ComboBox_Education_SelectionChanged;

        LoadComboBoxData();
        UpdateFieldsVisibility();
    }

    /// Метод загрузки всех комбобоксов
    public void LoadComboBoxData()
    {
        // Загружаем историю школ и классов
        LoadSchoolHistory();
        LoadClassHistory();

        // Загружаем основные комбобоксы
        LoadComboBoxClass();
        LoadComboBoxSchool();
        LoadComboBoxEducationalInstitution();
        LoadComboBoxEducation();
        LoadComboBoxOlympiad();
        LoadComboBoxGia();
        LoadComboBoxProfile();

        // Загружаем комбобокс медалей
        LoadComboBoxMedals();

        // Для существующего студента загружаем его данные
        if (Student1.Id != 0)
        {
            LoadStudentOlympiads();
            LoadStudentGias();
            LoadStudentMedals();
        }
    }


    private void UpdateFieldsVisibility()
    {
        if (ComboBox_Class.SelectedItem is Class selectedClass)
        {
            var match = Regex.Match(selectedClass.ClassesNumber, @"^\d{1,2}");
            bool isNumeric = match.Success;
            int classNumber = isNumeric ? int.Parse(match.Value) : 0;

            // Показываем/скрываем поле "Профиль" только для 10-11 классов
            bool isProfileVisible = isNumeric && (classNumber == 10 || classNumber == 11);
            ProfileLabel.IsVisible = isProfileVisible;
            ComboBox_Profile.IsVisible = isProfileVisible;
            EducationNameLabel.IsVisible = isProfileVisible;
            EducationNameTextBox.IsVisible = isProfileVisible;

            // Показываем/скрываем поля "Учебное заведение" и "Образование" для 9 и 11 классов
            bool isEducationVisible = isNumeric && (classNumber == 9 || classNumber == 11);
            EducationalInstitutionLabel.IsVisible = isEducationVisible;
            ComboBox_EducationalInstitution.IsVisible = isEducationVisible;
            Name_Educational_Institution.IsVisible = isEducationVisible;
            NameEducationalInstitution.IsVisible = isEducationVisible;
            EducationLabel.IsVisible = isEducationVisible;
            ComboBox_Education.IsVisible = isEducationVisible;
            EducationNameLabel.IsVisible = isEducationVisible;
            EducationNameTextBox.IsVisible = isEducationVisible;

            // Если поля образования скрыты, сбрасываем значения
            if (!isEducationVisible)
            {
                ComboBox_Education.SelectedIndex = 0;
                Student1.TypeEducation = null;
                Student1.NameProfile = null;
            }

            // Показываем/скрываем раздел ГИА в зависимости от класса (только для 9 и 11 классов)
            bool shouldShowGiaSection = isNumeric && (classNumber == 9 || classNumber == 11);
            var giaSection = this.FindControl<Grid>("Gia_IsvisibleDan");
            if (giaSection != null)
            {
                giaSection.IsVisible = shouldShowGiaSection;
            }
        }
    }

    private void ComboBox_Profile_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ComboBox_Profile.SelectedItem is Profile selected)
        {
            bool isStub = selected.Id == 0;
            if (isStub)
            {
                Student1.ProfileId = null;
                Student1.NameProfile = null;
            }
            else
            {
                Student1.ProfileId = selected.Id;
            }
        }
    }

    public void LoadComboBoxEducationalInstitution()
    {
        _EducationalInstitution = Helper.DateBase.EducationalInstitutions.ToList();

        // Удаляем старую заглушку если есть
        _EducationalInstitution.RemoveAll(x => x.Id == 0);

        // Добавляем новую заглушку в начало
        _EducationalInstitution.Insert(0, new EducationalInstitution()
        {
            Id = 0,
            Name = "Учебное заведение"
        });

        ComboBox_EducationalInstitution.ItemsSource = _EducationalInstitution;

        // Устанавливаем выбранное значение из Student1 или заглушку
        if (Student1.EducationalInstitutionId.HasValue)
        {
            var selected = _EducationalInstitution.FirstOrDefault(x => x.Id == Student1.EducationalInstitutionId.Value);
            ComboBox_EducationalInstitution.SelectedItem = selected ?? _EducationalInstitution[0];
        }
        else
        {
            ComboBox_EducationalInstitution.SelectedIndex = 0;
        }
    }

    public void LoadComboBoxProfile()
    {
        _Profiles = Helper.DateBase.Profiles.ToList();

        // Удаляем старую заглушку если есть
        _Profiles.RemoveAll(x => x.Id == 0);

        // Добавляем новую заглушку в начало
        _Profiles.Insert(0, new Profile()
        {
            Id = 0,
            Name = "Профиль"
        });

        ComboBox_Profile.ItemsSource = _Profiles;

        // Устанавливаем выбранное значение из Student1 или заглушку
        if (Student1.ProfileId.HasValue)
        {
            var selected = _Profiles.FirstOrDefault(x => x.Id == Student1.ProfileId.Value);
            ComboBox_Profile.SelectedItem = selected ?? _Profiles[0];
        }
        else
        {
            ComboBox_Profile.SelectedIndex = 0;
        }
    }

    public void LoadComboBoxEducation()
    {
        _Education = Helper.DateBase.Educations.ToList();

        // Удаляем старую заглушку если есть
        _Education.RemoveAll(x => x.Id == 0);

        // Добавляем новую заглушку в начало
        _Education.Insert(0, new Education()
        {
            Id = 0,
            Name = "Образование"
        });

        ComboBox_Education.ItemsSource = _Education;

        // Устанавливаем выбранное значение из Student1 или заглушку
        if (Student1.TypeEducation.HasValue)
        {
            var selected = _Education.FirstOrDefault(x => x.Id == Student1.TypeEducation.Value);
            ComboBox_Education.SelectedItem = selected ?? _Education[0];
        }
        else
        {
            ComboBox_Education.SelectedIndex = 0;
        }
    }

    private void ComboBox_EducationalInstitution_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ComboBox_EducationalInstitution.SelectedItem is EducationalInstitution selected)
        {
            bool isStub = selected.Id == 0;
            NameEducationalInstitution.IsEnabled = !isStub;
            if (isStub)
            {
                Student1.EducationalInstitutionId = null;
                Student1.NameEducationalInstitution = null;
            }
            else
            {
                Student1.EducationalInstitutionId = selected.Id;
            }
        }
    }


    private void ComboBox_Education_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ComboBox_Education.SelectedItem is Education selected)
        {
            bool isStub = selected.Id == 0;
            if (isStub)
            {
                Student1.TypeEducation = null;
                Student1.NameProfile = null;
            }
            else
            {
                Student1.TypeEducation = selected.Id;
            }
        }
        UpdateFieldsVisibility();
    }




    /// <summary>
    /// Метод загрузки комбобокса медалей
    /// </summary>
    private void LoadComboBoxMedals()
    {
        try
        {
            _allMedalFacts = Helper.DateBase.CertificateAndMedalsFacts.Include(m => m.CertificateAndMedals).Include(m => m.CertificateAndMedalsCheck).ToList();

            ComboBox_Medals.ItemsSource = _allMedalFacts;

            // Для существующего студента загружаем его медали
            if (Student1.Id != 0)
            {
                LoadStudentMedals();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при загрузке медалей: {ex.Message}");
        }
    }

    /// <summary>
    /// Метод загрузки медалей студента
    /// </summary>
    private void LoadStudentMedals()
    {
        try
        {
            _studentMedals.Clear();

            if (Student1.Id != 0)
            {
                var medals = Helper.DateBase.StudentCertificateAndMedals.Where(m => m.StudentsId == Student1.Id).Include(m => m.CertificateAndMedalsFact).ThenInclude(m => m.CertificateAndMedals)
                                                                                                                .Include(m => m.CertificateAndMedalsFact).ThenInclude(m => m.CertificateAndMedalsCheck)
                                                                                                                    .Select(m => m.CertificateAndMedalsFact).ToList();

                _studentMedals.AddRange(medals);
            }

            ListBox_Medals.ItemsSource = _studentMedals.ToList();
        }
        catch (Exception ex)
        {
            ShowErrorDialog($"Ошибка загрузки медалей: {ex.Message}");
        }
    }


    /// <summary>
    /// Обработчик кнопки "Добавить медаль"
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_AddMedal_Click(object sender, RoutedEventArgs e)
    {
        // Получаем выбранную медаль со статусом
        var selectedMedalFact = ComboBox_Medals.SelectedItem as CertificateAndMedalsFact;
        if (selectedMedalFact == null) return;

        // Проверяем, есть ли уже медали у студента
        if (_studentMedals.Count > 0)
        {
            await ShowErrorDialog("У студента уже есть медаль. Сначала удалите текущую медаль, чтобы добавить новую.");
            return;
        }

        // Добавляем медаль в список
        _studentMedals.Add(selectedMedalFact);
        UpdateMedalListBox();
    }

    /// <summary>
    /// Обработчик кнопки Удалить медаль
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_RemoveMedal_Click(object sender, RoutedEventArgs e)
    {
        // Получаем выбранную медаль для удаления
        var selectedMedal = ListBox_Medals.SelectedItem as CertificateAndMedalsFact;
        if (selectedMedal == null) return;

        // Удаляем медаль из списка
        _studentMedals.Remove(selectedMedal);
        UpdateMedalListBox();
    }


    /// <summary>
    /// Обновление списка медалей
    /// </summary>
    private void UpdateMedalListBox()
    {
        var temp = new List<CertificateAndMedalsFact>(_studentMedals);
        ListBox_Medals.ItemsSource = null;
        ListBox_Medals.ItemsSource = temp;
    }

    /// <summary>
    /// Метод сохранения медалей студента
    /// </summary>
    /// <param name="studentId"></param>
    /// <returns></returns>
    private async Task ProcessMedals(int studentId)
    {
        try
        {
            // Получаем текущие медали студента из БД
            var existingMedals = await Helper.DateBase.StudentCertificateAndMedals.Where(m => m.StudentsId == studentId).ToListAsync();

            // Удаляем медали, которых нет в новом списке
            var medalsToRemove = existingMedals.Where(em => !_studentMedals.Any(m => m.Id == em.CertificateAndMedalsFactId)).ToList();

            Helper.DateBase.StudentCertificateAndMedals.RemoveRange(medalsToRemove);

            // Добавляем новые медали
            foreach (var newMedal in _studentMedals)
            {
                if (!existingMedals.Any(em => em.CertificateAndMedalsFactId == newMedal.Id))
                {
                    Helper.DateBase.StudentCertificateAndMedals.Add(new StudentCertificateAndMedal{ StudentsId = studentId, CertificateAndMedalsFactId = newMedal.Id });
                }
            }

            await Helper.DateBase.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Ошибка при сохранении медалей: {ex.Message}", ex);
        }
    }

    private void ComboBox_Class_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateFieldsVisibility();

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

        var confirm = await ShowConfirmationDialog("Вы уверены, что хотите сохранить изменения?");
        if (!confirm) return;

        // Получаем выбранные школу и класс
        var selectedSchool = ComboBox_School.SelectedItem as School;
        var selectedClass = ComboBox_Class.SelectedItem as Class;

        using (var transaction = await Helper.DateBase.Database.BeginTransactionAsync())
        {
            try
            {
                // Обновляем основные свойства студента
                if (selectedSchool != null && selectedSchool.Id != 0)
                {
                    Student1.SchoolId = selectedSchool.Id;
                }

                if (selectedClass != null && selectedClass.Id != 0)
                {
                    Student1.ClassId = selectedClass.Id;
                }

                // Сохраняем основную информацию о студенте
                if (Student1.Id == 0)
                {
                    Helper.DateBase.Students.Add(Student1);
                }
                else
                {
                    // Для существующего студента
                    var originalStudent = await Helper.DateBase.Students
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.Id == Student1.Id);

                    if (originalStudent != null)
                    {
                        // Проверяем изменения школы и класса
                        bool schoolChanged = originalStudent.SchoolId != Student1.SchoolId;
                        bool classChanged = originalStudent.ClassId != Student1.ClassId;

                        // Сохраняем историю изменений
                        var currentDate = DateOnly.FromDateTime(DateTime.Now);

                        if (schoolChanged)
                        {
                            Helper.DateBase.StudentSchoolHistories.Add(new StudentSchoolHistory
                            {
                                StudentId = Student1.Id,
                                SchoolId = originalStudent.SchoolId, // Сохраняем старую школу в историю
                                ChangeDate = currentDate
                            });
                        }

                        if (classChanged)
                        {
                            Helper.DateBase.StudentClassHistories.Add(new StudentClassHistory
                            {
                                StudentId = Student1.Id,
                                ClassId = originalStudent.ClassId, // Сохраняем старый класс в историю
                                ChangeDate = currentDate
                            });
                        }
                    }

                    Helper.DateBase.Students.Update(Student1);
                }

                // Сохраняем изменения студента перед обработкой связей
                await Helper.DateBase.SaveChangesAsync();

                // Обрабатываем олимпиады, ГИА и медали
                await ProcessOlympiads(Student1.Id);
                await ProcessGias();
                await ProcessMedals(Student1.Id);

                // Фиксируем транзакцию
                await transaction.CommitAsync();

                // Показываем сообщение об успехе и закрываем окно
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
                await ShowErrorDialog($"Ошибка при сохранении: {ex.Message}\n\nStackTrace: {ex.StackTrace}");
            }
        }
    }

    /// <summary>
    /// сохранение изменений в результатах Олимпиад
    /// </summary>
    /// <param name="studentId"></param>
    /// <returns></returns>
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

    /// <summary>
    /// сохранение изменений в результатах ГИА
    /// </summary>
    /// <returns></returns>
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
                                Content = new TextBlock
                                {
                                    Text = "OK",
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    VerticalAlignment = VerticalAlignment.Center
                                },
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
                                HorizontalAlignment = HorizontalAlignment.Center,
                                VerticalAlignment = VerticalAlignment.Center,
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
                                VerticalAlignment = VerticalAlignment.Center,
                                Height = 30,
                                Background = Brushes.Green,
                                Margin = new Thickness(5),
                                HorizontalContentAlignment = HorizontalAlignment.Center,
                                VerticalContentAlignment = VerticalAlignment.Center
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

    /// <summary>
    /// Метод очистки истории классов
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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

    /// <summary>
    /// Метд очистки истории школ
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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