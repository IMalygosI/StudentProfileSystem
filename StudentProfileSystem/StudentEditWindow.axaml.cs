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

namespace StudentProfileSystem;

public partial class StudentEditWindow : Window
{
    Student Student1;

    List<Class> classes = new List<Class>();
    List<School> schools = new List<School>();

    // ���� ��� ������ � ����������� � ���
    private List<Olympiad> olympiads = new List<Olympiad>();
    private List<GiaSubject> giaSubjects = new List<GiaSubject>();
    private List<StudentOlympiadParticipation> studentOlympiads = new List<StudentOlympiadParticipation>();
    private List<StudentGiaResult> studentGias = new List<StudentGiaResult>();

    /// <summary>
    /// ����������� ��� ���������� ������ ��������
    /// </summary>
    public StudentEditWindow()
    {
        InitializeComponent();

        Title = "����������";

        Student1 = new Student();
        OkkoRedactAndAdd.DataContext = Student1;

        LoadComboBoxData();
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

        LoadComboBoxData();
    }

    /// <summary>
    /// �������� ������ � ����������
    /// </summary>
    public void LoadComboBoxData()
    {
        LoadComboBoxClass();
        LoadComboBoxSchool();
        LoadComboBoxOlympiad();
        LoadComboBoxGia();

        // ��������� ������ ��� ������������� ��������
        if (Student1.Id != 0)
        {
            LoadStudentOlympiads();
            LoadStudentGias();
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
                ComboBox_School.ItemsSource = schools.OrderByDescending(z => z.Id == Student1.SchoolId);
                ComboBox_School.SelectedIndex = 0;
            }
            else
            {
                schools.Add(new School() { Name = "�����" });
                ComboBox_School.ItemsSource = schools.OrderByDescending(z => z.Name == "�����");
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
            giaSubjects = Helper.DateBase.GiaSubjects.Include(g => g.GiaSubjectsNavigation)
                                                     .Include(g => g.GiaType).ToList();

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
                                                           .Include(sgr => sgr.IdGiaSubjectsNavigation)
                                                               .ThenInclude(gs => gs.GiaType).ToList();

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
        if (string.IsNullOrWhiteSpace(Student1.LastName) ||
            string.IsNullOrWhiteSpace(Student1.FirstName))
        {
            ShowErrorDialog("������� � ��� ����������� ��� ����������!").Wait();
            return false;
        }

        // �������� �� ����� � ���
        if (ContainsNumbers(Student1.LastName) ||
            ContainsNumbers(Student1.FirstName) ||
            (Student1.Patronymic != null && ContainsNumbers(Student1.Patronymic)))
        {
            ShowErrorDialog("��� ����� ��������� ������ �����!").Wait();
            return false;
        }

        // �������� �����������
        var selectedSchool = ComboBox_School.SelectedItem as School;
        var selectedClass = ComboBox_Class.SelectedItem as Class;

        if (selectedSchool == null || selectedSchool.Name == "�����" ||
            selectedClass == null || selectedClass.ClassesNumber == "�����")
        {
            ShowErrorDialog("�������� ����� � ����� �� ������!").Wait();
            return false;
        }

        return true;
    }

    /// <summary>
    /// ���������� ������ ����� �������������� (������������ ������)
    /// </summary>
    private async void Button_Click_Save(object? sender, RoutedEventArgs e)
    {
        var confirm = await ShowConfirmationDialog("�� �������, ��� ������ ��������� ���������?");
        if (!confirm) return;

        using var transaction = await Helper.DateBase.Database.BeginTransactionAsync();
        try
        {
            var selectedSchool = ComboBox_School.SelectedItem as School;
            var selectedClass = ComboBox_Class.SelectedItem as Class;

            Student1.SchoolId = selectedSchool?.Id ?? 0;
            Student1.ClassId = selectedClass?.Id ?? 0;

            if (Student1.Id == 0)
            {
                Helper.DateBase.Students.Add(Student1);
                await Helper.DateBase.SaveChangesAsync();
            }
            else
            {
                Helper.DateBase.Students.Update(Student1);
            }

            // ������������ ��������� � ���
            await ProcessOlympiads();
            await ProcessGias();

            await Helper.DateBase.SaveChangesAsync();
            await transaction.CommitAsync();

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
            await ShowErrorDialog($"������: {ex.Message}");
        }
    }

    /// <summary>
    /// ��������� �������� ��������
    /// </summary>
    private async Task ProcessOlympiads()
    {
        var existingOlympiads = await Helper.DateBase.StudentOlympiadParticipations.Where(sop => sop.IdStudents == Student1.Id).ToListAsync();

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
                newPart.IdStudents = Student1.Id;
                Helper.DateBase.StudentOlympiadParticipations.Add(newPart);
            }
        }
    }

    /// <summary>
    /// ��������� ��� ��������
    /// </summary>
    private async Task ProcessGias()
    {
        var existingGias = await Helper.DateBase.StudentGiaResults.Where(sgr => sgr.IdStudents == Student1.Id).ToListAsync();

        // ������� �������������
        foreach (var existing in existingGias)
        {
            if (!studentGias.Any(sgr => sgr.IdGiaSubjects == existing.IdGiaSubjects))
            {
                Helper.DateBase.StudentGiaResults.Remove(existing);
            }
        }

        // ��������� �����
        foreach (var newGia in studentGias)
        {
            if (!existingGias.Any(eg => eg.IdGiaSubjects == newGia.IdGiaSubjects))
            {
                newGia.IdStudents = Student1.Id; 
                Helper.DateBase.StudentGiaResults.Add(newGia);
            }
        }
    }

    /// <summary>
    /// �������� �� ������� ���� � ������
    /// </summary>
    private bool ContainsNumbers(string input)
    {
        return input != null && input.Any(char.IsDigit);
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
                                new Grid
                                {
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    Children =
                                    {
                                        CreateDialogButton("OK", Brushes.Gray)
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

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
                                    TextWrapping = TextWrapping.Wrap,
                                    TextAlignment = TextAlignment.Center,
                                    FontSize = 14,
                                    Margin = new Thickness(0, 0, 0, 20)
                                },
                                new Grid
                                {
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    Children =
                                    {
                                        CreateDialogButton("OK", Brushes.Green)
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

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
    /// �������� �� ������������� ���������
    /// </summary>
    private bool HasUnsavedChanges()
    {
        if (Student1.Id == 0) return true;
        if (Helper.DateBase.Entry(Student1).State == EntityState.Modified) return true;

        var currentOlympiads = Helper.DateBase.StudentOlympiadParticipations.Where(sop => sop.IdStudents == Student1.Id).Select(sop => sop.IdOlympiads).ToList();

        var currentGias = Helper.DateBase.StudentGiaResults.Where(sgr => sgr.IdStudents == Student1.Id).Select(sgr => sgr.IdGiaSubjects).ToList();

        if (studentOlympiads.Count != currentOlympiads.Count ||
            studentGias.Count != currentGias.Count)
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
}