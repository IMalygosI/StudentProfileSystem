using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using StudentProfileSystem.Models;
using Avalonia.Layout;

using Microsoft.EntityFrameworkCore;
using Avalonia.VisualTree;

namespace StudentProfileSystem;

public partial class SettingGiaOlimpiad : Window
{
    List<Item> items = new List<Item>();
    List<OlympiadsType> olympiadType = new List<OlympiadsType>();

    List<GiaSubject> giaSubjects1 = new List<GiaSubject>();
    List<Olympiad> olympiada1 = new List<Olympiad>();

    private string OlympAndGia1;

    public SettingGiaOlimpiad()
    {
        InitializeComponent();
    }

    public SettingGiaOlimpiad(string OlympAndGia)
    {
        InitializeComponent();
        
        OlympAndGia1 = OlympAndGia;
        Title = OlympAndGia1;

        if (OlympAndGia1 == "���") 
        {
            Items_Button.IsVisible = true;
            Examen_Gia.IsVisible = true;
            AddGiaBut.IsVisible = true;
            BorderListGia.IsVisible = true;

            items = Helper.DateBase.Items.ToList();
            ListBox_Gia.ItemsSource = items;

        }
        else if (OlympAndGia1 == "���������")
        {
            AddOlympBut.IsVisible = true;
            BorderListOlymp.IsVisible = true;
            Olympiad_Button.IsVisible = true;
            Type_Olympiad.IsVisible = true;

            olympiadType = Helper.DateBase.OlympiadsTypes.ToList();
            ListBox_Olymp.ItemsSource = olympiadType;
        }
        else if (OlympAndGia1 == "���������� ���")
        {
            AddOlympBut.IsVisible = false;
            BorderListOlymp.IsVisible = false;
            BorderListGia.IsVisible = false;
            Border_List_GiaSubject.IsVisible = true;

            giaSubjects1 = Helper.DateBase.GiaSubjects.ToList();
            ListBox_GiaSubject.ItemsSource = giaSubjects1;
        }
        else if (OlympAndGia1 == "���������� ���������")
        {
            AddOlympBut.IsVisible = false;
            BorderListOlymp.IsVisible = false;
            BorderListGia.IsVisible = false;
            Border_List_GiaSubject.IsVisible = false;

            Add_Olympiad.IsVisible = true;
            Border_List_Olympiad.IsVisible = true;


            giaSubjects1 = Helper.DateBase.GiaSubjects.ToList();
            ListBox_GiaSubject.ItemsSource = giaSubjects1;
        }
    }

    /// <summary>
    /// ����� �� ���� ��������
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_Click_Close(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// �������������� �������� ���
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ListBox_DoubleTapped_Redact_Gia(object? sender, TappedEventArgs e)
    {
        var selected = ListBox_Gia.SelectedItem as Item;
        if (selected == null) return;

        this.Classes.Add("blur-effect");
        try
        {
            var dialog = new AddAdnRedactOlympGia(selected);
            await dialog.ShowDialog(this);

            // ��������� ������
            items = Helper.DateBase.Items.ToList();
            ListBox_Gia.ItemsSource = items;
        }
        finally
        {
            this.Classes.Remove("blur-effect");
        }
    }

    /// <summary>
    /// �������������� ���������
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ListBox_DoubleTapped_Redact_Olymp(object? sender, TappedEventArgs e)
    {
        var selected = ListBox_Olymp.SelectedItem as OlympiadsType;
        if (selected == null) return;

        this.Classes.Add("blur-effect");
        try
        {
            var dialog = new AddAdnRedactOlympGia(selected);
            await dialog.ShowDialog(this);

            // ��������� ������
            olympiadType = Helper.DateBase.OlympiadsTypes.ToList();
            ListBox_Olymp.ItemsSource = olympiadType;
        }
        finally
        {
            this.Classes.Remove("blur-effect");
        }
    }

    /// <summary>
    /// ���������� ������ �������� ��� ���
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Click_Add_Gia(object? sender, RoutedEventArgs e)
    {
        this.Classes.Add("blur-effect");
        try
        {
            OlympAndGia1 = "���";
            var dialog = new AddAdnRedactOlympGia(OlympAndGia1);
            await dialog.ShowDialog(this);

            items = Helper.DateBase.Items.ToList();
            ListBox_Gia.ItemsSource = items;
        }
        finally
        {
            this.Classes.Remove("blur-effect");
        }
    }

    /// <summary>
    /// ���������� ����� ���������
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Click_Add_Olymp(object? sender, RoutedEventArgs e)
    {
        this.Classes.Add("blur-effect");
        try
        {
            OlympAndGia1 = "���������";
            var dialog = new AddAdnRedactOlympGia(OlympAndGia1);
            await dialog.ShowDialog<bool>(this);

            olympiadType = Helper.DateBase.OlympiadsTypes.ToList();
            ListBox_Olymp.ItemsSource = olympiadType;
        }
        finally
        {
            this.Classes.Remove("blur-effect");
        }
    }



    /// <summary>
    /// �������������� ��� "����� �������� � ������ ��� ���"
    /// </summary>
    /// <param name="sender">�������� �������</param>
    /// <param name="e">��������� �������</param>
    private async void ListBox_DoubleTapped_GiaSubject(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        var selected = ListBox_GiaSubject.SelectedItem as GiaSubject;
        if (selected == null) return;

        this.Classes.Add("blur-effect");
        try
        {
            var dialog = new AddAdnRedactOlympGia(selected);
            bool result = await dialog.ShowDialog(this);

            // ��������� ������ ����� ��������������
            if (result)
            {
                giaSubjects1 = Helper.DateBase.GiaSubjects
                    .Include(a => a.GiaSubjectsNavigation)
                    .Include(a => a.GiaType)
                    .ToList();
                ListBox_GiaSubject.ItemsSource = giaSubjects1;
            }
        }
        finally
        {
            this.Classes.Remove("blur-effect");
        }
    }

    /// <summary>
    /// ���������� ����� ����� � GiaSubject (������� + ��� ���)
    /// </summary>
    /// <param name="sender">�������� �������</param>
    /// <param name="e">��������� �������</param>
    private async void Button_Click_Add_GiaSubject(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.Classes.Add("blur-effect");
        try
        {
            OlympAndGia1 = "��������� ���";
            var dialog = new AddAdnRedactOlympGia(OlympAndGia1);
            bool result = await dialog.ShowDialog(this);

            // ��������� ������ ����� ����������
            if (result)
            {
                giaSubjects1 = Helper.DateBase.GiaSubjects
                    .Include(a => a.GiaSubjectsNavigation)
                    .Include(a => a.GiaType)
                    .ToList();
                ListBox_GiaSubject.ItemsSource = giaSubjects1;
            }
        }
        finally
        {
            this.Classes.Remove("blur-effect");
        }
    }

    /// <summary>
    /// �������� ��� "����� � ������� GiaSubject"
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void MenuItem_Click_Delete_GiaSubject(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var gia = ListBox_GiaSubject.SelectedItem as GiaSubject;
        if (gia == null) return;

        // ���������, ���� �� ��������� ������ � StudentGiaResults
        bool hasRelations = Helper.DateBase.StudentGiaResults.Any(sgr => sgr.IdGiaSubjects == gia.Id);

        if (hasRelations)
        {
            await ShowCannotDeleteDialog("���� ��� ������� ������, ��� ��� �� ������ � ������������ ���������.");
            return;
        }

        var confirm = await ShowDeleteConfirmationDialog($"�� �������, ��� ������ ������� ���?");
        if (!confirm) return;

        try
        {
            Helper.DateBase.GiaSubjects.Remove(gia);
            await Helper.DateBase.SaveChangesAsync();

            // ��������� ������
            giaSubjects1 = Helper.DateBase.GiaSubjects.Include(a => a.GiaSubjectsNavigation)
                                                      .Include(a => a.GiaType)
                                                      .ToList();
            ListBox_GiaSubject.ItemsSource = giaSubjects1;

            await ShowSuccessDialog("����� ��� ������� �������");
        }
        catch (Exception ex)
        {
            await ShowErrorDialog($"������ ��� ��������: {ex.Message}");
        }
    }








    /// <summary>
    /// �������������� ��������� "����� �������� � ���� ���������"
    /// </summary>
    /// <param name="sender">�������� �������</param>
    /// <param name="e">��������� �������</param>
    private async void ListBox_DoubleTapped_ListOlympiads(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        var selected = ListBox_Olympiad.SelectedItem as Olympiad;
        if (selected == null) return;

        this.Classes.Add("blur-effect");
        try
        {
            var dialog = new AddAdnRedactOlympGia(selected);
            bool result = await dialog.ShowDialog(this);

            // ��������� ������ ����� ��������������
            if (result)
            {
                olympiada1 = Helper.DateBase.Olympiads
                    .Include(a => a.OlympiadsNavigation)
                    .Include(a => a.OlympiadsItemsNavigation)
                    .ToList();
                ListBox_Olympiad.ItemsSource = olympiada1;
            }
        }
        finally
        {
            this.Classes.Remove("blur-effect");
        }
    }

    /// <summary>
    /// ���������� ����� ����� � Olympiad (������� + ��� ���������)
    /// </summary>
    /// <param name="sender">�������� �������</param>
    /// <param name="e">��������� �������</param>
    private async void Button_Click_Add_Olympiad(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.Classes.Add("blur-effect");
        try
        {
            OlympAndGia1 = "��������� ��������";
            var dialog = new AddAdnRedactOlympGia(OlympAndGia1);
            bool result = await dialog.ShowDialog(this);

            // ��������� ������ ����� ����������
            if (result)
            {
                olympiada1 = Helper.DateBase.Olympiads
                    .Include(a => a.OlympiadsNavigation)
                    .Include(a => a.OlympiadsItemsNavigation)
                    .ToList();
                ListBox_Olympiad.ItemsSource = olympiada1;
            }
        }
        finally
        {
            this.Classes.Remove("blur-effect");
        }
    }

    /// <summary>
    /// �������� ��������� "����� � ������� Olympiad"
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void MenuItem_Click_Delete_Olympiad(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var olympiad = ListBox_Olympiad.SelectedItem as Olympiad;
        if (olympiad == null) return;

        // ���������, ���� �� ��������� ������ � StudentOlympiadParticipations
        bool hasRelations = Helper.DateBase.StudentOlympiadParticipations.Any(sop => sop.IdOlympiads == olympiad.Id);

        if (hasRelations)
        {
            await ShowCannotDeleteDialog("��� ��������� ������� ������, ��� ��� ��� ������� � �������� ���������.");
            return;
        }

        var confirm = await ShowDeleteConfirmationDialog($"�� �������, ��� ������ ������� ���������?");
        if (!confirm) return;

        try
        {
            Helper.DateBase.Olympiads.Remove(olympiad);
            await Helper.DateBase.SaveChangesAsync();

            // ��������� ������
            olympiada1 = Helper.DateBase.Olympiads
                .Include(a => a.OlympiadsNavigation)
                .Include(a => a.OlympiadsItemsNavigation)
                .ToList();
            ListBox_Olympiad.ItemsSource = olympiada1;

            await ShowSuccessDialog("����� ��������� ������� �������");
        }
        catch (Exception ex)
        {
            await ShowErrorDialog($"������ ��� ��������: {ex.Message}");
        }
    }




    /// <summary>
    /// ���������� ���� � �����������, ����� � ����������
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_Click_Type_Olympiad(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        AddOlympBut.IsVisible = false;
        BorderListOlymp.IsVisible = false;
        Add_Olympiad.IsVisible = true;
        Border_List_Olympiad.IsVisible = true;

        olympiada1 = Helper.DateBase.Olympiads.Include(a => a.OlympiadsNavigation).Include(a => a.OlympiadsItemsNavigation).ToList();
        ListBox_Olympiad.ItemsSource = olympiada1;
    }

    /// <summary>
    /// ���������� ���������
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_Click_Olympiad_Button(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        AddOlympBut.IsVisible = true;
        BorderListOlymp.IsVisible = true;
        Add_Olympiad.IsVisible = false;
        Border_List_Olympiad.IsVisible = false;

        olympiadType = Helper.DateBase.OlympiadsTypes.ToList();
        ListBox_Olymp.ItemsSource = olympiadType;
    }

    /// <summary>
    /// ����������� ��������� 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_Click_Items_Button(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        AddGiaBut.IsVisible = true;
        BorderListGia.IsVisible = true;
        
        Add_GiaSubject.IsVisible = false;
        Border_List_GiaSubject.IsVisible = false;

        items = Helper.DateBase.Items.ToList();
        ListBox_Gia.ItemsSource = items;
    }

    /// <summary>
    /// ����������� ��������� ���.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_Click_Examen_Gia(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Add_GiaSubject.IsVisible = true;
        Border_List_GiaSubject.IsVisible = true;

        AddGiaBut.IsVisible = false;
        BorderListGia.IsVisible = false;

        giaSubjects1 = Helper.DateBase.GiaSubjects.Include(a => a.GiaSubjectsNavigation).Include(a => a.GiaType).ToList();
        ListBox_GiaSubject.ItemsSource = giaSubjects1;
    }

    /// <summary>
    /// �������� �������� ���
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void MenuItem_Click_Delete_Gia(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var selectedItem = ListBox_Gia.SelectedItem as Item;
        if (selectedItem == null) return;

        // ��������� ����� � GiaSubject
        bool hasRelations = Helper.DateBase.GiaSubjects.Any(gs => gs.GiaSubjects == selectedItem.Id) || Helper.DateBase.Olympiads.Any(o => o.OlympiadsItems == selectedItem.Id);

        if (hasRelations)
        {
            await ShowCannotDeleteDialog("������� ������ �������, ��� ��� �� ������ � ������� ������� (��� ��� �����������).");
            return;
        }

        var confirm = await ShowDeleteConfirmationDialog($"�� �������, ��� ������ ������� ������� '{selectedItem.Name}'?");
        if (!confirm) return;

        try
        {
            Helper.DateBase.Items.Remove(selectedItem);
            await Helper.DateBase.SaveChangesAsync();

            items = Helper.DateBase.Items.ToList();
            ListBox_Gia.ItemsSource = items;

            await ShowSuccessDialog("������� ������� ������");
        }
        catch (Exception ex)
        {
            await ShowErrorDialog($"������ ��� ��������: {ex.Message}");
        }
    }

    /// <summary>
    /// �������� ���������
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void MenuItem_Click_Delete_Olymp(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var selectedOlympiad = ListBox_Olymp.SelectedItem as OlympiadsType;
        if (selectedOlympiad == null) return;

        // ��������� ����� � Olympiad
        bool hasRelations = Helper.DateBase.Olympiads.Any(o => o.Olympiads == selectedOlympiad.Id);

        if (hasRelations)
        {
            await ShowCannotDeleteDialog("��� ��������� ������ �������, ��� ��� �� ������ � �����������.");
            return;
        }

        var confirm = await ShowDeleteConfirmationDialog($"�� �������, ��� ������ ������� ��� ��������� '{selectedOlympiad.Name}'?");
        if (!confirm) return;

        try
        {
            Helper.DateBase.OlympiadsTypes.Remove(selectedOlympiad);
            await Helper.DateBase.SaveChangesAsync();

            olympiadType = Helper.DateBase.OlympiadsTypes.ToList();
            ListBox_Olymp.ItemsSource = olympiadType;

            await ShowSuccessDialog("��� ��������� ������� ������");
        }
        catch (Exception ex)
        {
            await ShowErrorDialog($"������ ��� ��������: {ex.Message}");
        }
    }

    /// <summary>
    /// ����� ������� ������
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
    /// ����� ������� ������������� ��������
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private async Task ShowCannotDeleteDialog(string message)
    {
        var dialog = new Window
        {
            Title = "������ ��������",
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
    /// ����� ������� ������������� ��������
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private async Task<bool> ShowDeleteConfirmationDialog(string message)
    {
        bool result = false;
        Window dialog = null;

        Button CreateDialogButton(string text, IBrush background, bool dialogResult)
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
                result = dialogResult;
                dialog?.Close();
            };

            return button;
        }

        dialog = new Window
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
                            new DockPanel
                            {
                                HorizontalAlignment = HorizontalAlignment.Center,
                                LastChildFill = false,
                                Children =
                                {
                                    CreateDialogButton("��", Brushes.Red, true),
                                    CreateDialogButton("���", Brushes.Gray, false)
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
    /// ����� ������� ��������� ���������� ��������
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
    /// ������� ������ ��� ���������� ����, ��
    /// </summary>
    /// <param name="text"></param>
    /// <param name="background"></param>
    /// <returns></returns>
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

}