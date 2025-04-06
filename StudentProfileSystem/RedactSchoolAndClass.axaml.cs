using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using MsBox.Avalonia.ViewModels.Commands;
using StudentProfileSystem.Models;

namespace StudentProfileSystem;

public partial class RedactSchoolAndClass : Window
{
    School _school;
    Class _class;

    /// <summary>
    /// ����������
    /// </summary>
    public RedactSchoolAndClass()
    {
        InitializeComponent();
        _school = new School();
        _class = new Class();
    }

    /// <summary>
    /// �������� ����
    /// </summary>
    /// <param name="school"></param>
    public RedactSchoolAndClass(School school)
    {
        InitializeComponent();
        _school = school;

        Button_Add_School.IsVisible = true;
        BorderRedactSchool.IsVisible = true;
        Button_Add_Class.IsVisible = false;

        ListBox_RedactSchool.ItemsSource = Helper.DateBase.Schools.ToList();
    }

    /// <summary>
    /// �������� �������
    /// </summary>
    /// <param name="_Class"></param>
    public RedactSchoolAndClass(Class _Class)
    {
        InitializeComponent();
        _class = _Class;

        Button_Add_School.IsVisible = false;
        Button_Add_Class.IsVisible = true;
        BorderRedactClass.IsVisible = true;

        ListBox_RedactClass.ItemsSource = Helper.DateBase.Classes.ToList();
    }

    /// <summary>
    /// �������� �����
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_Click_Add_School(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var newSchool = new School();

        var addAdnRedactOlympGia = new AddAdnRedactOlympGia(newSchool);
        bool result = await addAdnRedactOlympGia.ShowDialog(this);
        if (result)
        {
            ListBox_RedactSchool.ItemsSource = Helper.DateBase.Schools.ToList();
        }
    }

    /// <summary>
    /// �������������� �����
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ListBox_DoubleTapped_School(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        var school = ListBox_RedactSchool.SelectedItem as School;
        if (school == null) return;

        var addAdnRedactOlympGia = new AddAdnRedactOlympGia(school);
        bool result = await addAdnRedactOlympGia.ShowDialog(this);
        if (result)
        {
            ListBox_RedactSchool.ItemsSource = Helper.DateBase.Schools.ToList();
        }
    }

    /// <summary>
    /// �������������� ������
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ListBox_DoubleTapped_Class(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        var clas = ListBox_RedactClass.SelectedItem as Class;
        if (clas == null) return;

        var addAdnRedactOlympGia = new AddAdnRedactOlympGia(clas);
        bool result = await addAdnRedactOlympGia.ShowDialog(this);
        if (result)
        {
            ListBox_RedactClass.ItemsSource = Helper.DateBase.Classes.ToList();
        }
    }

    /// <summary>
    /// �������� �����
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_Click_Add_Class(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var newClass = new Class();

        var addAdnRedactOlympGia = new AddAdnRedactOlympGia(newClass);
        bool result = await addAdnRedactOlympGia.ShowDialog(this);
        if (result)
        {
            ListBox_RedactClass.ItemsSource = Helper.DateBase.Classes.ToList();
        }
    }

    /// <summary>
    /// ������� �������� ����/�������
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_Click_Close(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// �������� ������
    /// </summary>
    private async void MenuItem_Click_Delete_Class(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var selectedClass = ListBox_RedactClass.SelectedItem as Class;
        if (selectedClass == null) return;

        try
        {
            // �������� �� ������� ��������� � ���� ������
            bool hasStudents = Helper.DateBase.Students.Any(s => s.ClassId == selectedClass.Id);
            if (hasStudents)
            {
                await ShowErrorDialog("������ ������� �����, ��� ��� � ��� ���� ��������!");
                return;
            }

            var confirm = await ShowConfirmationDialog("�� �������, ��� ������ ������� ���� �����?");
            if (!confirm) return;

            Helper.DateBase.Classes.Remove(selectedClass);
            await Helper.DateBase.SaveChangesAsync();
            ListBox_RedactClass.ItemsSource = Helper.DateBase.Classes.ToList();

            await ShowSuccessDialog("����� ������� ������!");
        }
        catch (Exception ex)
        {
            await ShowErrorDialog($"������ ��� �������� ������: {ex.Message}");
        }
    }

    /// <summary>
    /// �������� �����
    /// </summary>
    private async void MenuItem_Click_Delete_School(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var selectedSchool = ListBox_RedactSchool.SelectedItem as School;
        if (selectedSchool == null) return;

        try
        {
            // �������� �� ������� ��������� � ���� �����
            bool hasStudents = Helper.DateBase.Students.Any(s => s.SchoolId == selectedSchool.Id);
            if (hasStudents)
            {
                await ShowErrorDialog("������ ������� �����, ��� ��� � ��� ���� ��������!");
                return;
            }

            var confirm = await ShowConfirmationDialog("�� �������, ��� ������ ������� ��� �����?");
            if (!confirm) return;

            Helper.DateBase.Schools.Remove(selectedSchool);
            await Helper.DateBase.SaveChangesAsync();
            ListBox_RedactSchool.ItemsSource = Helper.DateBase.Schools.ToList();

            await ShowSuccessDialog("����� ������� �������!");
        }
        catch (Exception ex)
        {
            await ShowErrorDialog($"������ ��� �������� �����: {ex.Message}");
        }
    }

    /// <summary>
    /// ���������� ���� �������������
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private async Task<bool> ShowConfirmationDialog(string message)
    {
        bool result = false;

        var dialog = new Window
        {
            Title = "������������� ��������",
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
                                    CreateDialogButton("��", Brushes.Green, () => { result = true; }),
                                    CreateDialogButton("���", Brushes.Red, () => { result = false; })
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
    /// ���������� ���� � ���������� �� ������
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
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
                            CreateDialogButton("OK", Brushes.Gray)
                        }
                    }
                }
                }
            }
        };

        await dialog.ShowDialog(this);
    }

    /// <summary>
    /// ���������� ���� � ���������� �� �������� ����������
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
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
                            CreateDialogButton("OK", Brushes.Green)
                        }
                    }
                }
                }
            }
        };

        await dialog.ShowDialog(this);
    }

    /// <summary>
    /// ������� ������ ��� ���������� ����
    /// </summary>
    /// <param name="text"></param>
    /// <param name="background"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    private Button CreateDialogButton(string text, IBrush background, Action? action = null)
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

        button.Click += (s, e) =>
        {
            action?.Invoke();
            (button.GetVisualRoot() as Window)?.Close();
        };

        return button;
    }

}