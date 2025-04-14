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


    // ���� ��� ������ � �������� ������� � ����
    private List<StudentSchoolHistory> _schoolHistory = new List<StudentSchoolHistory>();
    private List<StudentClassHistory> _classHistory = new List<StudentClassHistory>();

    // ���� ��� ������ � ����������� � ���
    private List<Olympiad> olympiads = new List<Olympiad>();
    private List<GiaSubject> giaSubjects = new List<GiaSubject>();
    private List<StudentOlympiadParticipation> studentOlympiads = new List<StudentOlympiadParticipation>();
    private List<StudentGiaResult> studentGias = new List<StudentGiaResult>();
    
    // ���� ��� ������ � ��������
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
    /// ����������� ��� ���������� ������ ��������
    /// </summary>
    public StudentEditWindow(int schoolId)
    {
        InitializeComponent();

        _schoolId = schoolId;
        Title = "����������";
        Student1 = new Student();

        // ���� �������� schoolId, ������������� ��� ��� ������ ��������
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
    /// ����������� ��� �������������� ������������� ��������
    /// </summary>
    /// <param name="student">������� ��� ��������������</param>
    public StudentEditWindow(Student student)
    {
        InitializeComponent();

        Title = "��������������";
        Student1 = student;
        OkkoRedactAndAdd.DataContext = Student1;

        ComboBox_Class.SelectionChanged += ComboBox_Class_SelectionChanged;
        ComboBox_EducationalInstitution.SelectionChanged += ComboBox_EducationalInstitution_SelectionChanged;
        ComboBox_Profile.SelectionChanged += ComboBox_Profile_SelectionChanged;
        ComboBox_Education.SelectionChanged += ComboBox_Education_SelectionChanged;

        LoadComboBoxData();
        UpdateFieldsVisibility();
    }

    /// ����� �������� ���� �����������
    public void LoadComboBoxData()
    {
        // ��������� ������� ���� � �������
        LoadSchoolHistory();
        LoadClassHistory();

        // ��������� �������� ����������
        LoadComboBoxClass();
        LoadComboBoxSchool();
        LoadComboBoxEducationalInstitution();
        LoadComboBoxEducation();
        LoadComboBoxOlympiad();
        LoadComboBoxGia();
        LoadComboBoxProfile();

        // ��������� ��������� �������
        LoadComboBoxMedals();

        // ��� ������������� �������� ��������� ��� ������
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

            // ����������/�������� ���� "�������" ������ ��� 10-11 �������
            bool isProfileVisible = isNumeric && (classNumber == 10 || classNumber == 11);
            ProfileLabel.IsVisible = isProfileVisible;
            ComboBox_Profile.IsVisible = isProfileVisible;
            EducationNameLabel.IsVisible = isProfileVisible;
            EducationNameTextBox.IsVisible = isProfileVisible;

            // ����������/�������� ���� "������� ���������" � "�����������" ��� 9 � 11 �������
            bool isEducationVisible = isNumeric && (classNumber == 9 || classNumber == 11);
            EducationalInstitutionLabel.IsVisible = isEducationVisible;
            ComboBox_EducationalInstitution.IsVisible = isEducationVisible;
            Name_Educational_Institution.IsVisible = isEducationVisible;
            NameEducationalInstitution.IsVisible = isEducationVisible;
            EducationLabel.IsVisible = isEducationVisible;
            ComboBox_Education.IsVisible = isEducationVisible;
            EducationNameLabel.IsVisible = isEducationVisible;
            EducationNameTextBox.IsVisible = isEducationVisible;

            // ���� ���� ����������� ������, ���������� ��������
            if (!isEducationVisible)
            {
                ComboBox_Education.SelectedIndex = 0;
                Student1.TypeEducation = null;
                Student1.NameProfile = null;
            }

            // ����������/�������� ������ ��� � ����������� �� ������ (������ ��� 9 � 11 �������)
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

        // ������� ������ �������� ���� ����
        _EducationalInstitution.RemoveAll(x => x.Id == 0);

        // ��������� ����� �������� � ������
        _EducationalInstitution.Insert(0, new EducationalInstitution()
        {
            Id = 0,
            Name = "������� ���������"
        });

        ComboBox_EducationalInstitution.ItemsSource = _EducationalInstitution;

        // ������������� ��������� �������� �� Student1 ��� ��������
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

        // ������� ������ �������� ���� ����
        _Profiles.RemoveAll(x => x.Id == 0);

        // ��������� ����� �������� � ������
        _Profiles.Insert(0, new Profile()
        {
            Id = 0,
            Name = "�������"
        });

        ComboBox_Profile.ItemsSource = _Profiles;

        // ������������� ��������� �������� �� Student1 ��� ��������
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

        // ������� ������ �������� ���� ����
        _Education.RemoveAll(x => x.Id == 0);

        // ��������� ����� �������� � ������
        _Education.Insert(0, new Education()
        {
            Id = 0,
            Name = "�����������"
        });

        ComboBox_Education.ItemsSource = _Education;

        // ������������� ��������� �������� �� Student1 ��� ��������
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
    /// ����� �������� ���������� �������
    /// </summary>
    private void LoadComboBoxMedals()
    {
        try
        {
            _allMedalFacts = Helper.DateBase.CertificateAndMedalsFacts.Include(m => m.CertificateAndMedals).Include(m => m.CertificateAndMedalsCheck).ToList();

            ComboBox_Medals.ItemsSource = _allMedalFacts;

            // ��� ������������� �������� ��������� ��� ������
            if (Student1.Id != 0)
            {
                LoadStudentMedals();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"������ ��� �������� �������: {ex.Message}");
        }
    }

    /// <summary>
    /// ����� �������� ������� ��������
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
            ShowErrorDialog($"������ �������� �������: {ex.Message}");
        }
    }


    /// <summary>
    /// ���������� ������ "�������� ������"
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_AddMedal_Click(object sender, RoutedEventArgs e)
    {
        // �������� ��������� ������ �� ��������
        var selectedMedalFact = ComboBox_Medals.SelectedItem as CertificateAndMedalsFact;
        if (selectedMedalFact == null) return;

        // ���������, ���� �� ��� ������ � ��������
        if (_studentMedals.Count > 0)
        {
            await ShowErrorDialog("� �������� ��� ���� ������. ������� ������� ������� ������, ����� �������� �����.");
            return;
        }

        // ��������� ������ � ������
        _studentMedals.Add(selectedMedalFact);
        UpdateMedalListBox();
    }

    /// <summary>
    /// ���������� ������ ������� ������
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_RemoveMedal_Click(object sender, RoutedEventArgs e)
    {
        // �������� ��������� ������ ��� ��������
        var selectedMedal = ListBox_Medals.SelectedItem as CertificateAndMedalsFact;
        if (selectedMedal == null) return;

        // ������� ������ �� ������
        _studentMedals.Remove(selectedMedal);
        UpdateMedalListBox();
    }


    /// <summary>
    /// ���������� ������ �������
    /// </summary>
    private void UpdateMedalListBox()
    {
        var temp = new List<CertificateAndMedalsFact>(_studentMedals);
        ListBox_Medals.ItemsSource = null;
        ListBox_Medals.ItemsSource = temp;
    }

    /// <summary>
    /// ����� ���������� ������� ��������
    /// </summary>
    /// <param name="studentId"></param>
    /// <returns></returns>
    private async Task ProcessMedals(int studentId)
    {
        try
        {
            // �������� ������� ������ �������� �� ��
            var existingMedals = await Helper.DateBase.StudentCertificateAndMedals.Where(m => m.StudentsId == studentId).ToListAsync();

            // ������� ������, ������� ��� � ����� ������
            var medalsToRemove = existingMedals.Where(em => !_studentMedals.Any(m => m.Id == em.CertificateAndMedalsFactId)).ToList();

            Helper.DateBase.StudentCertificateAndMedals.RemoveRange(medalsToRemove);

            // ��������� ����� ������
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
            throw new Exception($"������ ��� ���������� �������: {ex.Message}", ex);
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
    /// �������� ������ � ������� � ���������
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
                classes.Add(new Class() { ClassesNumber = "�����" });
                ComboBox_Class.ItemsSource = classes.OrderByDescending(z => z.ClassesNumber == "�����");
                ComboBox_Class.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"������ ��� �������� ComboBox: {ex.Message}");
        }
    }

    /// <summary>
    /// �������� ������ � ������ � ���������
    /// </summary>
    public void LoadComboBoxSchool()
    {
        try
        {
            schools = Helper.DateBase.Schools.ToList();

            if (Student1.Id != 0)
            {
                // ��� ������������� ��������
                ComboBox_School.ItemsSource = schools.OrderByDescending(z => z.Id == Student1.SchoolId);
                ComboBox_School.SelectedIndex = 0;
            }
            else
            {
                // ��� ������ ��������
                if (_schoolId != null)
                {
                    // ������������� ����� �� ���������
                    var school = schools.FirstOrDefault(s => s.Id == _schoolId);
                    if (school != null)
                    {
                        schools = schools.OrderByDescending(s => s.Id == _schoolId).ToList();
                        ComboBox_School.ItemsSource = schools;
                        ComboBox_School.SelectedIndex = 0;
                        return;
                    }
                }

                // ���� schoolId �� ������� ��� ����� �� ������� - ��������� ��������
                schools.Insert(0, new School() { Id = 0, Name = "�����" });
                ComboBox_School.ItemsSource = schools;
                ComboBox_School.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"������ ��� �������� ComboBox: {ex.Message}");
        }
    }

    /// <summary>
    /// �������� ������ �� ���������� � ���������
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
            Console.WriteLine($"������ ��� �������� ComboBox ��������: {ex.Message}");
        }
    }

    /// <summary>
    /// �������� ������ � ��� � ���������
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
            Console.WriteLine($"������ ��� �������� ComboBox ���: {ex.Message}");
        }
    }


    /// <summary>
    /// �������� �������� ��������
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
    /// �������� ��� ��������
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
    /// ���������� ��������� ��������
    /// </summary>
    private void Button_AddOlympiad_Click(object sender, RoutedEventArgs e)
    {
        var selectedOlympiad = ComboBox_Olympiad.SelectedItem as Olympiad;
        if (selectedOlympiad == null) return;

        // �������� �� ������������
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
    /// ���������� ������ ��������
    /// </summary>
    private void UpdateOlympiadListBox()
    {
        ListBox_Olympiads.ItemsSource = null;
        ListBox_Olympiads.ItemsSource = studentOlympiads.Select(sop => sop.IdOlympiadsNavigation).ToList();
    }

    /// <summary>
    /// �������� ��������� � ��������
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
    /// ���������� ��� ��������
    /// </summary>
    private void Button_AddGia_Click(object sender, RoutedEventArgs e)
    {
        var selectedGia = ComboBox_Gia.SelectedItem as GiaSubject;
        if (selectedGia == null) return;

        // �������� �� ������������
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
    /// ���������� ������ ���
    /// </summary>
    private void UpdateGiaListBox()
    {
        ListBox_Gias.ItemsSource = null;
        ListBox_Gias.ItemsSource = studentGias.Select(sgr => sgr.IdGiaSubjectsNavigation).ToList();
    }

    /// <summary>
    /// �������� ��� � ��������
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
    /// �������� ���������� ������ ��������
    /// </summary>
    private bool ValidateStudentData()
    {
        // �������� ���
        if (string.IsNullOrWhiteSpace(Student1.LastName))
        {
            ShowErrorDialog("������� ����������� ��� ����������!");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Student1.FirstName))
        {
            ShowErrorDialog("��� ����������� ��� ����������!");
            return false;
        }

        // �������� �� ����� � ����������� ������� � ���
        if (ContainsInvalidChars(Student1.LastName) ||
            ContainsInvalidChars(Student1.FirstName) ||
            (Student1.Patronymic != null && ContainsInvalidChars(Student1.Patronymic)))
        {
            ShowErrorDialog("��� ����� ��������� ������ ����� � �����!");
            return false;
        }

        // �������� �����������
        var selectedSchool = ComboBox_School.SelectedItem as School;
        var selectedClass = ComboBox_Class.SelectedItem as Class;

        if (selectedSchool == null || selectedSchool.Id == 0)
        {
            ShowErrorDialog("�������� ����� �� ������!");
            return false;
        }

        if (selectedClass == null || selectedClass.Id == 0)
        {
            ShowErrorDialog("�������� ����� �� ������!");
            return false;
        }

        return true;
    }



    private bool ContainsInvalidChars(string input)
    {
        return input != null && input.Any(c => !char.IsLetter(c) && c != '-' && c != ' ');
    }

    /// <summary>
    /// ���������� ������ ����� ��������������
    /// </summary>
    private async void Button_Click_Save(object? sender, RoutedEventArgs e)
    {
        if (!ValidateStudentData()) return;

        var confirm = await ShowConfirmationDialog("�� �������, ��� ������ ��������� ���������?");
        if (!confirm) return;

        // �������� ��������� ����� � �����
        var selectedSchool = ComboBox_School.SelectedItem as School;
        var selectedClass = ComboBox_Class.SelectedItem as Class;

        using (var transaction = await Helper.DateBase.Database.BeginTransactionAsync())
        {
            try
            {
                // ��������� �������� �������� ��������
                if (selectedSchool != null && selectedSchool.Id != 0)
                {
                    Student1.SchoolId = selectedSchool.Id;
                }

                if (selectedClass != null && selectedClass.Id != 0)
                {
                    Student1.ClassId = selectedClass.Id;
                }

                // ��������� �������� ���������� � ��������
                if (Student1.Id == 0)
                {
                    Helper.DateBase.Students.Add(Student1);
                }
                else
                {
                    // ��� ������������� ��������
                    var originalStudent = await Helper.DateBase.Students
                        .AsNoTracking()
                        .FirstOrDefaultAsync(s => s.Id == Student1.Id);

                    if (originalStudent != null)
                    {
                        // ��������� ��������� ����� � ������
                        bool schoolChanged = originalStudent.SchoolId != Student1.SchoolId;
                        bool classChanged = originalStudent.ClassId != Student1.ClassId;

                        // ��������� ������� ���������
                        var currentDate = DateOnly.FromDateTime(DateTime.Now);

                        if (schoolChanged)
                        {
                            Helper.DateBase.StudentSchoolHistories.Add(new StudentSchoolHistory
                            {
                                StudentId = Student1.Id,
                                SchoolId = originalStudent.SchoolId, // ��������� ������ ����� � �������
                                ChangeDate = currentDate
                            });
                        }

                        if (classChanged)
                        {
                            Helper.DateBase.StudentClassHistories.Add(new StudentClassHistory
                            {
                                StudentId = Student1.Id,
                                ClassId = originalStudent.ClassId, // ��������� ������ ����� � �������
                                ChangeDate = currentDate
                            });
                        }
                    }

                    Helper.DateBase.Students.Update(Student1);
                }

                // ��������� ��������� �������� ����� ���������� ������
                await Helper.DateBase.SaveChangesAsync();

                // ������������ ���������, ��� � ������
                await ProcessOlympiads(Student1.Id);
                await ProcessGias();
                await ProcessMedals(Student1.Id);

                // ��������� ����������
                await transaction.CommitAsync();

                // ���������� ��������� �� ������ � ��������� ����
                await ShowSuccessDialog("������ ������� ���������!");
                Close();
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync();
                string errorDetails = dbEx.InnerException?.Message ?? dbEx.Message;
                await ShowErrorDialog($"������ ���� ������: {errorDetails}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await ShowErrorDialog($"������ ��� ����������: {ex.Message}\n\nStackTrace: {ex.StackTrace}");
            }
        }
    }

    /// <summary>
    /// ���������� ��������� � ����������� ��������
    /// </summary>
    /// <param name="studentId"></param>
    /// <returns></returns>
    private async Task ProcessOlympiads(int studentId)
    {
        var existingOlympiads = await Helper.DateBase.StudentOlympiadParticipations.Where(sop => sop.IdStudents == studentId).ToListAsync();

        // ������� �������������
        foreach (var existing in existingOlympiads)
        {
            if (!studentOlympiads.Any(sop => sop.IdOlympiads == existing.IdOlympiads))
            {
                Helper.DateBase.StudentOlympiadParticipations.Remove(existing);
            }
        }

        // ��������� �����
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
    /// ���������� ��������� � ����������� ���
    /// </summary>
    /// <returns></returns>
    private async Task ProcessGias()
    {
        // �������� ������� ��� ��������
        var existingGias = await Helper.DateBase.StudentGiaResults.Where(sgr => sgr.IdStudents == Student1.Id).ToListAsync();

        // ������� ��, ������� ��� � ����� ������
        var giasToRemove = existingGias.Where(eg => !studentGias.Any(sg => sg.IdGiaSubjects == eg.IdGiaSubjects)).ToList();

        Helper.DateBase.StudentGiaResults.RemoveRange(giasToRemove);

        // ��������� �����
        var giasToAdd = studentGias.Where(sg => !existingGias.Any(eg => eg.IdGiaSubjects == sg.IdGiaSubjects)).Select(sg => new StudentGiaResult
            {
                IdStudents = Student1.Id,
                IdGiaSubjects = sg.IdGiaSubjects
            }).ToList();

        Helper.DateBase.StudentGiaResults.AddRange(giasToAdd);
        await Helper.DateBase.SaveChangesAsync();
    }

    /// <summary>
    /// ������ �������������
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

        var yesButton = CreateDialogButton("��", Brushes.Green, true);
        var noButton = CreateDialogButton("���", Brushes.Red, false);

        yesButton.Click += (s, e) => { result = true; dialog?.Close(); };
        noButton.Click += (s, e) => { result = false; dialog?.Close(); };

        dialog = new Window
        {
            Title = "�������������",
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
    /// ������ ������
    /// </summary>
    private async Task ShowErrorDialog(string message)
    {
        var dialog = new Window
        {
            Title = "������",
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
    /// ������ ������
    /// </summary>
    private async Task ShowSuccessDialog(string message)
    {
        var dialog = new Window
        {
            Title = "�����",
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
    /// �������� �� ������������� ���������
    /// </summary>
    private bool HasUnsavedChanges()
    {
        if (Student1.Id == 0) return true;
        if (Helper.DateBase.Entry(Student1).State == EntityState.Modified) return true;

        // �������� ��������
        var currentOlympiads = Helper.DateBase.StudentOlympiadParticipations.Where(sop => sop.IdStudents == Student1.Id).Select(sop => sop.IdOlympiads).ToList();

        // �������� ���
        var currentGias = Helper.DateBase.StudentGiaResults.Where(sgr => sgr.IdStudents == Student1.Id).Select(sgr => sgr.IdGiaSubjects).ToList();

        // �������� ������� �������
        var hasClassHistory = Helper.DateBase.StudentClassHistories
            .Any(h => h.StudentId == Student1.Id);

        // �������� ������� ����
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
    /// ��������� ���� ���� � ���
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
    /// �������� ���� � ��������������
    /// </summary>
    private async void Button_Click_Close(object? sender, RoutedEventArgs e)
    {
        if (HasUnsavedChanges())
        {
            var confirm = await ShowConfirmationDialog("� ��� ���� ������������� ���������. ������� ��� ����������?");
            if (!confirm) return;
        }
        Close();
    }

    /// <summary>
    /// �������������� ����
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
    /// �������������� �������
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
    /// ����� ������� ������� �������
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_Click_ClearHistoryClass(object? sender, RoutedEventArgs e)
    {
        if (Student1.Id == 0) return;

        var confirm = await ShowConfirmationDialog("�� �������, ��� ������ �������� ������� �������?");
        if (!confirm) return;

        try
        {
            var historyToDelete = Helper.DateBase.StudentClassHistories.Where(h => h.StudentId == Student1.Id).ToList();

            Helper.DateBase.StudentClassHistories.RemoveRange(historyToDelete);
            await Helper.DateBase.SaveChangesAsync();

            // ��������� ������������� ������� �� ����
            _classHistory = Helper.DateBase.StudentClassHistories.Where(h => h.StudentId == Student1.Id).Include(h => h.ClassNavigation).OrderByDescending(h => h.ChangeDate).ToList();

            // ��������� ��������
            ListBox_HistoryClass.ItemsSource = null;
            ListBox_HistoryClass.ItemsSource = _classHistory;

            await ShowSuccessDialog("������� ������� ������� �������!");
        }
        catch (Exception ex)
        {
            await ShowErrorDialog($"������ ��� ������� ������� �������: {ex.Message}");
        }
    }

    /// <summary>
    /// ���� ������� ������� ����
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_Click_ClearHistorySchool(object? sender, RoutedEventArgs e)
    {
        if (Student1.Id == 0) return;

        var confirm = await ShowConfirmationDialog("�� �������, ��� ������ �������� ������� ����?");
        if (!confirm) return;

        try
        {
            var historyToDelete = Helper.DateBase.StudentSchoolHistories.Where(h => h.StudentId == Student1.Id).ToList();

            Helper.DateBase.StudentSchoolHistories.RemoveRange(historyToDelete);
            await Helper.DateBase.SaveChangesAsync();

            // ��������� ������������� ������� �� ����
            _schoolHistory = Helper.DateBase.StudentSchoolHistories.Where(h => h.StudentId == Student1.Id).Include(h => h.SchoolNavigation).OrderByDescending(h => h.ChangeDate).ToList();

            // ��������� ��������
            ListBox_HistorySchool.ItemsSource = null;
            ListBox_HistorySchool.ItemsSource = _schoolHistory;

            await ShowSuccessDialog("������� ���� ������� �������!");
        }
        catch (Exception ex)
        {
            await ShowErrorDialog($"������ ��� ������� ������� ����: {ex.Message}");
        }
    }
}