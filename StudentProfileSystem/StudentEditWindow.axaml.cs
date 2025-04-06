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

    // Новые поля для работы с олимпиадами и ГИА
    private List<Olympiad> olympiads = new List<Olympiad>();
    private List<GiaSubject> giaSubjects = new List<GiaSubject>();
    private List<StudentOlympiadParticipation> studentOlympiads = new List<StudentOlympiadParticipation>();
    private List<StudentGiaResult> studentGias = new List<StudentGiaResult>();

    /// <summary>
    /// Добавление
    /// </summary>
    public StudentEditWindow()
    {
        InitializeComponent();

        Title = "Добавление";

        Student1 = new Student();
        OkkoRedactAndAdd.DataContext = Student1;

        LoadComboBoxData();
    }

    /// <summary>
    /// Редактирование student
    /// </summary>
    /// <param name="student"></param>
    public StudentEditWindow(Student student)
    {
        InitializeComponent();

        Title = "Редактирование";

        Student1 = student;
        OkkoRedactAndAdd.DataContext = Student1;

        LoadComboBoxData();
    }

    /// <summary>
    /// Загрузка данных в комбобоксы
    /// </summary>
    public void LoadComboBoxData()
    {
        LoadComboBoxClass();
        LoadComboBoxSchool();
        LoadComboBoxOlympiad();
        LoadComboBoxGia();
        LoadStudentOlympiads();
        LoadStudentGias();
    }

    /// <summary>
    /// ComboBox с данными классов
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
    /// ComboBox с данными школ
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
                schools.Add(new School() { Name = "Школа" });
                ComboBox_School.ItemsSource = schools.OrderByDescending(z => z.Name == "Школа");
                ComboBox_School.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при загрузке ComboBox: {ex.Message}");
        }
    }

    /// <summary>
    /// ComboBox с данными олимпиад
    /// </summary>
    private void LoadComboBoxOlympiad()
    {
        try
        {
            olympiads = Helper.DateBase.Olympiads
                .Include(o => o.OlympiadsNavigation)
                .Include(o => o.OlympiadsItemsNavigation)
                .ToList();

            ComboBox_Olympiad.ItemsSource = olympiads;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при загрузке ComboBox олимпиад: {ex.Message}");
        }
    }

    /// <summary>
    /// ComboBox с данными ГИА
    /// </summary>
    private void LoadComboBoxGia()
    {
        try
        {
            giaSubjects = Helper.DateBase.GiaSubjects
                .Include(g => g.GiaSubjectsNavigation)
                .Include(g => g.GiaType)
                .ToList();

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
            studentOlympiads = Helper.DateBase.StudentOlympiadParticipations
                .Where(sop => sop.IdStudents == Student1.Id)
                .Include(sop => sop.IdOlympiadsNavigation)
                .ThenInclude(o => o.OlympiadsNavigation)
                .Include(sop => sop.IdOlympiadsNavigation)
                .ThenInclude(o => o.OlympiadsItemsNavigation)
                .ToList();

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
            studentGias = Helper.DateBase.StudentGiaResults
                .Where(sgr => sgr.IdStudents == Student1.Id)
                .Include(sgr => sgr.IdGiaSubjectsNavigation)
                .ThenInclude(gs => gs.GiaSubjectsNavigation)
                .Include(sgr => sgr.IdGiaSubjectsNavigation)
                .ThenInclude(gs => gs.GiaType)
                .ToList();

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

        if (!studentOlympiads.Any(sop => sop.IdOlympiads == selectedOlympiad.Id))
        {
            var participation = new StudentOlympiadParticipation
            {
                IdStudents = Student1.Id,
                IdOlympiads = selectedOlympiad.Id,
                IdOlympiadsNavigation = selectedOlympiad // Добавляем навигационное свойство
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

        if (!studentGias.Any(sgr => sgr.IdGiaSubjects == selectedGia.Id))
        {
            var result = new StudentGiaResult
            {
                IdStudents = Student1.Id,
                IdGiaSubjects = selectedGia.Id,
                IdGiaSubjectsNavigation = selectedGia // Добавляем навигационное свойство
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
    /// Сохранение данных после редактирования
    /// </summary>
    private async void Button_Click_Save(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Валидация текстовых полей
        if (string.IsNullOrWhiteSpace(Student1.LastName) ||
            string.IsNullOrWhiteSpace(Student1.FirstName))
        {
            await ShowErrorDialog("Фамилия и имя обязательны для заполнения!");
            return;
        }

        // Проверка на цифры в текстовых полях
        if (ContainsNumbers(Student1.LastName) ||
            ContainsNumbers(Student1.FirstName) ||
            (Student1.Patronymic != null && ContainsNumbers(Student1.Patronymic)))
        {
            await ShowErrorDialog("ФИО может содержать только буквы!");
            return;
        }

        // Валидация комбобоксов
        var selectedSchool = ComboBox_School.SelectedItem as School;
        var selectedClass = ComboBox_Class.SelectedItem as Class;

        if (selectedSchool == null || selectedSchool.Name == "Школа" ||
            selectedClass == null || selectedClass.ClassesNumber == "Класс")
        {
            await ShowErrorDialog("Выберите школу и класс из списка!");
            return;
        }

        // Подтверждение сохранения
        var confirm = await ShowConfirmationDialog();
        if (!confirm) return;

        try
        {
            // Присваиваем выбранные значения
            Student1.SchoolId = selectedSchool.Id;
            Student1.ClassId = selectedClass.Id;

            // Сохранение в БД
            if (Student1.Id == 0)
            {
                Helper.DateBase.Students.Add(Student1);
                await Helper.DateBase.SaveChangesAsync(); // Сохраняем, чтобы получить Id
            }
            else
            {
                Helper.DateBase.Students.Update(Student1);
            }

            // Получаем текущие записи из БД
            var existingOlympiads = Helper.DateBase.StudentOlympiadParticipations
                .Where(sop => sop.IdStudents == Student1.Id)
                .ToList();

            var existingGias = Helper.DateBase.StudentGiaResults
                .Where(sgr => sgr.IdStudents == Student1.Id)
                .ToList();

            // Удаляем записи, которые есть в БД, но нет в текущем списке
            foreach (var existing in existingOlympiads)
            {
                if (!studentOlympiads.Any(sop => sop.IdOlympiads == existing.IdOlympiads))
                {
                    Helper.DateBase.StudentOlympiadParticipations.Remove(existing);
                }
            }

            foreach (var existing in existingGias)
            {
                if (!studentGias.Any(sgr => sgr.IdGiaSubjects == existing.IdGiaSubjects))
                {
                    Helper.DateBase.StudentGiaResults.Remove(existing);
                }
            }

            // Добавляем новые записи
            foreach (var newParticipation in studentOlympiads)
            {
                if (!existingOlympiads.Any(eo => eo.IdOlympiads == newParticipation.IdOlympiads))
                {
                    Helper.DateBase.StudentOlympiadParticipations.Add(newParticipation);
                }
            }

            foreach (var newResult in studentGias)
            {
                if (!existingGias.Any(eg => eg.IdGiaSubjects == newResult.IdGiaSubjects))
                {
                    Helper.DateBase.StudentGiaResults.Add(newResult);
                }
            }

            await Helper.DateBase.SaveChangesAsync();
            await ShowSuccessDialog("Данные студента успешно сохранены!");
            Close();
        }
        catch (Exception ex)
        {
            await ShowErrorDialog($"Ошибка при сохранении: {ex.Message}");
        }
    }

    // Проверка на наличие цифр в строке
    private bool ContainsNumbers(string input)
    {
        return input != null && input.Any(char.IsDigit);
    }

    // Диалог подтверждения
    private async Task<bool> ShowConfirmationDialog()
    {
        var message = "Вы уверены, что хотите сохранить изменения студента?";
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
            Title = "Подтверждение сохранения",
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

    // Диалог ошибки
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

    // Диалог успеха
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
    /// Закрытие окна редактирования/добавления Student
    /// </summary>
    private void Button_Click_Close(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }
}