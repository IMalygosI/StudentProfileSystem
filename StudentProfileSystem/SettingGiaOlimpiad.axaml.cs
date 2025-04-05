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

        if (OlympAndGia1 == "ГИА") 
        {
            Items_Button.IsVisible = true;
            Examen_Gia.IsVisible = true;
            AddGiaBut.IsVisible = true;
            BorderListGia.IsVisible = true;

            items = Helper.DateBase.Items.ToList();
            ListBox_Gia.ItemsSource = items;

        }
        else if (OlympAndGia1 == "Олимпиады")
        {
            AddOlympBut.IsVisible = true;
            BorderListOlymp.IsVisible = true;
            Olympiad_Button.IsVisible = true;
            Type_Olympiad.IsVisible = true;

            olympiadType = Helper.DateBase.OlympiadsTypes.ToList();
            ListBox_Olymp.ItemsSource = olympiadType;
        }
        else if (OlympAndGia1 == "Добавление ГИА")
        {
            AddOlympBut.IsVisible = false;
            BorderListOlymp.IsVisible = false;
            BorderListGia.IsVisible = false;
            Border_List_GiaSubject.IsVisible = true;

            giaSubjects1 = Helper.DateBase.GiaSubjects.ToList();
            ListBox_GiaSubject.ItemsSource = giaSubjects1;
        }
        else if (OlympAndGia1 == "Добавление Олимпиады")
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
    /// Выйти из меню настроек
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_Click_Close(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// Редактирование Предмета ГИА
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

            // Обновляем список
            items = Helper.DateBase.Items.ToList();
            ListBox_Gia.ItemsSource = items;
        }
        finally
        {
            this.Classes.Remove("blur-effect");
        }
    }

    /// <summary>
    /// Редактирование Олимпиады
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

            // Обновляем список
            olympiadType = Helper.DateBase.OlympiadsTypes.ToList();
            ListBox_Olymp.ItemsSource = olympiadType;
        }
        finally
        {
            this.Classes.Remove("blur-effect");
        }
    }

    /// <summary>
    /// Добавление нового Предмета для ГИА
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Click_Add_Gia(object? sender, RoutedEventArgs e)
    {
        this.Classes.Add("blur-effect");
        try
        {
            OlympAndGia1 = "ГИА";
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
    /// Добавление новой Олимпиады
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Click_Add_Olymp(object? sender, RoutedEventArgs e)
    {
        this.Classes.Add("blur-effect");
        try
        {
            OlympAndGia1 = "Олимпиады";
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
    /// Редактирование ГИА "Смена предмета и самого тип ГИА"
    /// </summary>
    /// <param name="sender">Источник события</param>
    /// <param name="e">Аргументы события</param>
    private async void ListBox_DoubleTapped_GiaSubject(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        var selected = ListBox_GiaSubject.SelectedItem as GiaSubject;
        if (selected == null) return;

        this.Classes.Add("blur-effect");
        try
        {
            var dialog = new AddAdnRedactOlympGia(selected);
            bool result = await dialog.ShowDialog(this);

            // Обновляем списки после редактирования
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
    /// Добавление новой связи в GiaSubject (предмет + тип ГИА)
    /// </summary>
    /// <param name="sender">Источник события</param>
    /// <param name="e">Аргументы события</param>
    private async void Button_Click_Add_GiaSubject(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.Classes.Add("blur-effect");
        try
        {
            OlympAndGia1 = "Настройка ГИА";
            var dialog = new AddAdnRedactOlympGia(OlympAndGia1);
            bool result = await dialog.ShowDialog(this);

            // Обновляем списки после добавления
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
    /// Удаление ГИА "Связи в таблице GiaSubject"
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void MenuItem_Click_Delete_GiaSubject(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var gia = ListBox_GiaSubject.SelectedItem as GiaSubject;
        if (gia == null) return;

        // Проверяем, есть ли связанные записи в StudentGiaResults
        bool hasRelations = Helper.DateBase.StudentGiaResults.Any(sgr => sgr.IdGiaSubjects == gia.Id);

        if (hasRelations)
        {
            await ShowCannotDeleteDialog("Этот ГИА удалить нельзя, так как он связан с результатами студентов.");
            return;
        }

        var confirm = await ShowDeleteConfirmationDialog($"Вы уверены, что хотите удалить ГИА?");
        if (!confirm) return;

        try
        {
            Helper.DateBase.GiaSubjects.Remove(gia);
            await Helper.DateBase.SaveChangesAsync();

            // Обновляем список
            giaSubjects1 = Helper.DateBase.GiaSubjects.Include(a => a.GiaSubjectsNavigation)
                                                      .Include(a => a.GiaType)
                                                      .ToList();
            ListBox_GiaSubject.ItemsSource = giaSubjects1;

            await ShowSuccessDialog("Связь ГИА успешно удалена");
        }
        catch (Exception ex)
        {
            await ShowErrorDialog($"Ошибка при удалении: {ex.Message}");
        }
    }








    /// <summary>
    /// Редактирование олимпиады "Смена предмета и типа олимпиады"
    /// </summary>
    /// <param name="sender">Источник события</param>
    /// <param name="e">Аргументы события</param>
    private async void ListBox_DoubleTapped_ListOlympiads(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        var selected = ListBox_Olympiad.SelectedItem as Olympiad;
        if (selected == null) return;

        this.Classes.Add("blur-effect");
        try
        {
            var dialog = new AddAdnRedactOlympGia(selected);
            bool result = await dialog.ShowDialog(this);

            // Обновляем списки после редактирования
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
    /// Добавление новой связи в Olympiad (предмет + тип олимпиады)
    /// </summary>
    /// <param name="sender">Источник события</param>
    /// <param name="e">Аргументы события</param>
    private async void Button_Click_Add_Olympiad(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        this.Classes.Add("blur-effect");
        try
        {
            OlympAndGia1 = "Настройка Олимпиад";
            var dialog = new AddAdnRedactOlympGia(OlympAndGia1);
            bool result = await dialog.ShowDialog(this);

            // Обновляем списки после добавления
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
    /// Удаление олимпиады "Связи в таблице Olympiad"
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void MenuItem_Click_Delete_Olympiad(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var olympiad = ListBox_Olympiad.SelectedItem as Olympiad;
        if (olympiad == null) return;

        // Проверяем, есть ли связанные записи в StudentOlympiadParticipations
        bool hasRelations = Helper.DateBase.StudentOlympiadParticipations.Any(sop => sop.IdOlympiads == olympiad.Id);

        if (hasRelations)
        {
            await ShowCannotDeleteDialog("Эту олимпиаду удалить нельзя, так как она связана с участием студентов.");
            return;
        }

        var confirm = await ShowDeleteConfirmationDialog($"Вы уверены, что хотите удалить олимпиаду?");
        if (!confirm) return;

        try
        {
            Helper.DateBase.Olympiads.Remove(olympiad);
            await Helper.DateBase.SaveChangesAsync();

            // Обновляем список
            olympiada1 = Helper.DateBase.Olympiads
                .Include(a => a.OlympiadsNavigation)
                .Include(a => a.OlympiadsItemsNavigation)
                .ToList();
            ListBox_Olympiad.ItemsSource = olympiada1;

            await ShowSuccessDialog("Связь олимпиады успешно удалена");
        }
        catch (Exception ex)
        {
            await ShowErrorDialog($"Ошибка при удалении: {ex.Message}");
        }
    }




    /// <summary>
    /// Показываем окно с Олимпиадами, связь с предметами
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
    /// Показываем Олимпиады
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
    /// Отображение предметов 
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
    /// Отображение Экзаменов ГИА.
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
    /// Удаление Предмета ГИА
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void MenuItem_Click_Delete_Gia(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var selectedItem = ListBox_Gia.SelectedItem as Item;
        if (selectedItem == null) return;

        // Проверяем связи в GiaSubject
        bool hasRelations = Helper.DateBase.GiaSubjects.Any(gs => gs.GiaSubjects == selectedItem.Id) || Helper.DateBase.Olympiads.Any(o => o.OlympiadsItems == selectedItem.Id);

        if (hasRelations)
        {
            await ShowCannotDeleteDialog("Предмет нельзя удалить, так как он связан с другими данными (ГИА или олимпиадами).");
            return;
        }

        var confirm = await ShowDeleteConfirmationDialog($"Вы уверены, что хотите удалить предмет '{selectedItem.Name}'?");
        if (!confirm) return;

        try
        {
            Helper.DateBase.Items.Remove(selectedItem);
            await Helper.DateBase.SaveChangesAsync();

            items = Helper.DateBase.Items.ToList();
            ListBox_Gia.ItemsSource = items;

            await ShowSuccessDialog("Предмет успешно удален");
        }
        catch (Exception ex)
        {
            await ShowErrorDialog($"Ошибка при удалении: {ex.Message}");
        }
    }

    /// <summary>
    /// Удаление Олимпиады
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void MenuItem_Click_Delete_Olymp(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var selectedOlympiad = ListBox_Olymp.SelectedItem as OlympiadsType;
        if (selectedOlympiad == null) return;

        // Проверяем связи в Olympiad
        bool hasRelations = Helper.DateBase.Olympiads.Any(o => o.Olympiads == selectedOlympiad.Id);

        if (hasRelations)
        {
            await ShowCannotDeleteDialog("Тип олимпиады нельзя удалить, так как он связан с олимпиадами.");
            return;
        }

        var confirm = await ShowDeleteConfirmationDialog($"Вы уверены, что хотите удалить тип олимпиады '{selectedOlympiad.Name}'?");
        if (!confirm) return;

        try
        {
            Helper.DateBase.OlympiadsTypes.Remove(selectedOlympiad);
            await Helper.DateBase.SaveChangesAsync();

            olympiadType = Helper.DateBase.OlympiadsTypes.ToList();
            ListBox_Olymp.ItemsSource = olympiadType;

            await ShowSuccessDialog("Тип олимпиады успешно удален");
        }
        catch (Exception ex)
        {
            await ShowErrorDialog($"Ошибка при удалении: {ex.Message}");
        }
    }

    /// <summary>
    /// Вывод диалога ошибки
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
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
    /// Вывод диалога невозможности удаления
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private async Task ShowCannotDeleteDialog(string message)
    {
        var dialog = new Window
        {
            Title = "Ошибка удаления",
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
    /// Вывод диалога подтверждения удаления
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
            Title = "Подтверждение удаления",
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
                                    CreateDialogButton("Да", Brushes.Red, true),
                                    CreateDialogButton("Нет", Brushes.Gray, false)
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
    /// Вывод диалога успешного выполнения операции
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
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
    /// Создает кнопку для диалоговых окон, ОК
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