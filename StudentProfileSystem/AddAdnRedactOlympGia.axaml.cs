using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Microsoft.EntityFrameworkCore;
using StudentProfileSystem.Models;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace StudentProfileSystem;

public partial class AddAdnRedactOlympGia : Window
{
    private Item _giaItem1;
    private OlympiadsType _olympiadsType1;
    private string OlympAndGia1;
    private Item _originalGiaItem;
    private OlympiadsType _originalOlympiad;

    public AddAdnRedactOlympGia()
    {
        InitializeComponent();
        _giaItem1 = new Item();
        _olympiadsType1 = new OlympiadsType();
        OkkoRedactAdd.DataContext = _giaItem1;
    }

    public AddAdnRedactOlympGia(string OlympAndGia) : this()
    {
        OlympAndGia1 = OlympAndGia;

        if (OlympAndGia1 == "ГИА")
        {
            _giaItem1 = new Item();
            _originalGiaItem = _giaItem1; // Инициализируем оригинальный объект
            OkkoRedactAdd.DataContext = _giaItem1;
            BorderGiaRedAdd.IsVisible = true;
            Title = "Добавление предмета ГИА";
        }
        else if (OlympAndGia1 == "Олимпиады")
        {
            _olympiadsType1 = new OlympiadsType();
            _originalOlympiad = _olympiadsType1; // Инициализируем оригинальный объект
            OkkoRedactAdd.DataContext = _olympiadsType1;
            BorderOlympRedAdd.IsVisible = true;
            Title = "Добавление Олимпиады";
        }
    }

    public AddAdnRedactOlympGia(OlympiadsType olympiadsType) : this()
    {
        _olympiadsType1 = new OlympiadsType
        {
            Id = olympiadsType.Id,
            Name = olympiadsType.Name
            // Копируем остальные свойства
        };
        _originalOlympiad = olympiadsType;
        OkkoRedactAdd.DataContext = _olympiadsType1;
        BorderOlympRedAdd.IsVisible = true;
        Title = "Редактирование Олимпиады";
    }

    public AddAdnRedactOlympGia(Item giaItem) : this()
    {
        _giaItem1 = new Item
        {
            Id = giaItem.Id,
            Name = giaItem.Name
            // Копируем остальные свойства
        };
        _originalGiaItem = giaItem;
        OkkoRedactAdd.DataContext = _giaItem1;
        BorderGiaRedAdd.IsVisible = true;
        Title = "Редактирование ГИА";
    }

    private void Button_Click_Close(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void Button_Click_Save(object? sender, RoutedEventArgs e)
    {
        var confirm = await ShowConfirmationDialog();
        if (!confirm) return;

        try
        {
            bool saveResult = false;

            if (OlympAndGia1 == "ГИА")
            {
                saveResult = await SaveGiaItem();
            }
            else if (OlympAndGia1 == "Олимпиады")
            {
                saveResult = await SaveOlympiadItem();
            }

            if (saveResult)
            {
                await ShowSuccessDialog("Данные успешно сохранены!");
                Close();
            }
        }
        catch (Exception ex)
        {
            await ShowErrorDialog($"Критическая ошибка: {ex.Message}");
        }
    }

    private async Task<bool> ShowConfirmationDialog()
    {
        var message = OlympAndGia1 == "ГИА"
            ? "Вы уверены, что хотите сохранить изменения предмета ГИА?"
            : "Вы уверены, что хотите сохранить изменения олимпиады?";

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
                                new DockPanel
                                {
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    LastChildFill = false,
                                    Children =
                                    {
                                        CreateDialogButton("Да", Brushes.Green, true),
                                        CreateDialogButton("Нет", Brushes.Red, false)
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

    private async Task<bool> SaveGiaItem()
    {
        if (string.IsNullOrWhiteSpace(_giaItem1.Name))
        {
            await ShowErrorDialog("Название предмета не может быть пустым");
            return false;
        }

        if (_giaItem1.Name.Length > 50)
        {
            await ShowErrorDialog("Название предмета должно быть не длиннее 50 символов");
            return false;
        }

        try
        {
            if (_originalGiaItem.Id == 0)
            {
                var newItem = new Item
                {
                    Name = _giaItem1.Name
                };
                Helper.DateBase.Items.Add(newItem);
            }
            else
            {
                _originalGiaItem.Name = _giaItem1.Name;
                Helper.DateBase.Items.Update(_originalGiaItem);
            }

            int changes = await Helper.DateBase.SaveChangesAsync();
            return changes > 0;
        }
        catch (DbUpdateException dbEx)
        {
            await ShowErrorDialog($"Ошибка базы данных: {dbEx.InnerException?.Message ?? dbEx.Message}");
            return false;
        }
        catch (Exception ex)
        {
            await ShowErrorDialog($"Ошибка при сохранении: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> SaveOlympiadItem()
    {
        if (string.IsNullOrWhiteSpace(_olympiadsType1.Name))
        {
            await ShowErrorDialog("Название олимпиады не может быть пустым");
            return false;
        }

        if (_olympiadsType1.Name.Length > 100)
        {
            await ShowErrorDialog("Название олимпиады должно быть не длиннее 100 символов");
            return false;
        }

        try
        {
            if (_originalOlympiad.Id == 0)
            {
                var newOlympiad = new OlympiadsType
                {
                    Name = _olympiadsType1.Name
                };
                Helper.DateBase.OlympiadsTypes.Add(newOlympiad);
            }
            else
            {
                _originalOlympiad.Name = _olympiadsType1.Name;
                Helper.DateBase.OlympiadsTypes.Update(_originalOlympiad);
            }

            int changes = await Helper.DateBase.SaveChangesAsync();
            return changes > 0;
        }
        catch (DbUpdateException dbEx)
        {
            await ShowErrorDialog($"Ошибка базы данных: {dbEx.InnerException?.Message ?? dbEx.Message}");
            return false;
        }
        catch (Exception ex)
        {
            await ShowErrorDialog($"Ошибка при сохранении: {ex.Message}");
            return false;
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        // Отменяем изменения если пользователь закрыл форму без сохранения
        if (_originalGiaItem != null && _originalGiaItem.Id != 0)
            Helper.DateBase.Entry(_originalGiaItem).State = EntityState.Unchanged;

        if (_originalOlympiad != null && _originalOlympiad.Id != 0)
            Helper.DateBase.Entry(_originalOlympiad).State = EntityState.Unchanged;

        base.OnClosing(e);
    }

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
            Height = 30,
            Background = background,
            Margin = new Thickness(5)
        };

        button.Click += (s, e) => (button.GetVisualRoot() as Window)?.Close();
        return button;
    }
}