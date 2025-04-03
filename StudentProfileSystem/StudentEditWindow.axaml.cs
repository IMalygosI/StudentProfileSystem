using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using StudentProfileSystem.Models;

namespace StudentProfileSystem;

public partial class StudentEditWindow : Window
{
    Student Student1;

    /// <summary>
    /// Добавление
    /// </summary>
    public StudentEditWindow()
    {
        InitializeComponent();
        Student1 = new Student();

        Title = "Добавление";
    }

    /// <summary>
    /// Редактирование
    /// </summary>
    /// <param name="student"></param>
    public StudentEditWindow(Student student)
    {
        InitializeComponent();
        Student1 = student;

        Title = "Редактирование";

    }

    /// <summary>
    /// Закрытие окна редактирования/добавления Student
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_Click_Close(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }
}