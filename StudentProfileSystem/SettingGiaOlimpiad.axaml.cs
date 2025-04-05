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
            olympiadType = Helper.DateBase.OlympiadsTypes.ToList();
            ListBox_Olymp.ItemsSource = olympiadType;
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
    /// Отображение предметов
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_Click_Items_Button(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {

    }

    /// <summary>
    /// Отображение Экзаменов ГИА.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_Click_Examen_Gia(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {

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