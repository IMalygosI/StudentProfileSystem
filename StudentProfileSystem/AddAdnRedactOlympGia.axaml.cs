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
using System.Collections.Generic;

namespace StudentProfileSystem;

public partial class AddAdnRedactOlympGia : Window
{
    List<GiaType> giaTypes1 = new List<GiaType>();
    List<Item> items1 = new List<Item>();
    List<OlympiadsType> _OlympiadsTypes = new List<OlympiadsType>();

    private OlympiadsType _originalOlympiad;
    private OlympiadsType _olympiadsType1;
    private Item _giaItem1;
    private Item _originalGiaItem;
    private GiaSubject _giaSubject;
    private Olympiad _Olympiad;
    private Class _class;
    private School _School;

    private bool _isSaved = false;
    private string OlympAndGia1;

    public AddAdnRedactOlympGia()
    {
        InitializeComponent();

        _giaItem1 = new Item();
        _olympiadsType1 = new OlympiadsType();
        _giaSubject = new GiaSubject();
        _Olympiad = new Olympiad();

        OkkoRedactAdd.DataContext = _giaItem1;

        LoadComboBoxData();
    }

    /// <summary>
    /// Конструктор для редактирования школы
    /// </summary>
    public AddAdnRedactOlympGia(School school) : this()
    {
        InitializeComponent();

        _School = school ?? new School();
        OkkoRedactAdd.DataContext = _School;

        BorderRedactSchool.IsVisible = true;

        Title = school.Id == 0 ? "Добавление школы" : "Редактирование школы";
    }

    /// <summary>
    /// Конструктор для редактирования Классов
    /// </summary>
    public AddAdnRedactOlympGia(Class _Class) : this()
    {
        InitializeComponent();

        _class = _Class ?? new Class();
        OkkoRedactAdd.DataContext = _class;

        BorderRedactClass.IsVisible = true;

        Title = _Class.Id == 0 ? "Добавление класса" : "Редактирование класса";
    }

    /// <summary>
    /// Конструктор для смены ГИА
    /// </summary>
    /// <param name="giaSubject">Предмет ГИА</param>
    public AddAdnRedactOlympGia(GiaSubject giaSubject) : this()
    {
        InitializeComponent();

        Title = "Смена ГИА";
        OlympAndGia1 = "Смена ГИА";

        _giaSubject = giaSubject ?? new GiaSubject();
        OkkoRedactAdd.DataContext = _giaSubject;
        BorderGiaSubject.IsVisible = true;

        LoadComboBoxData();

        if (_giaSubject.Id != 0)
        {
            Box_GiaSubject.SelectedItem = items1.FirstOrDefault(i => i.Id == _giaSubject.GiaSubjects);
            Box_Type_GiaSubject.SelectedItem = giaTypes1.FirstOrDefault(g => g.Id == _giaSubject.GiaTypeId);
        }
    }

    /// <summary>
    /// Конструктор для смены Олимпиады
    /// </summary>
    /// <param name="olympiad"></param>
    public AddAdnRedactOlympGia(Olympiad olympiad) : this()
    {
        InitializeComponent();

        Title = "Смена олимпиады";
        OlympAndGia1 = "Смена олимпиады";

        _Olympiad = olympiad ?? new Olympiad();
        OkkoRedactAdd.DataContext = _Olympiad;
        BorderOlympiad.IsVisible = true;

        LoadComboBoxData();

        if (_Olympiad.Id != 0)
        {
            Box_Olympiad.SelectedItem = _OlympiadsTypes.FirstOrDefault(o => o.Id == _Olympiad.Olympiads);
            Box_Type_Olympiad.SelectedItem = items1.FirstOrDefault(i => i.Id == _Olympiad.OlympiadsItems);
        }
    }

    /// <summary>
    /// Добавление предмета ГИА или Олимпиады
    /// </summary>
    /// <param name="OlympAndGia">Тип операции ("ГИА", "Олимпиады" или "Настройка ГИА")</param>
    public AddAdnRedactOlympGia(string OlympAndGia) : this()
    {
        InitializeComponent();
        OlympAndGia1 = OlympAndGia;

        if (OlympAndGia1 == "ГИА")
        {
            _giaItem1 = new Item();
            _originalGiaItem = _giaItem1;
            OkkoRedactAdd.DataContext = _giaItem1;

            BorderGiaSubject.IsVisible = false;
            BorderOlympRedAdd.IsVisible = false;
            BorderOlympiad.IsVisible = false;
            BorderGiaRedAdd.IsVisible = true;

            Title = "Добавление предмета ГИА";
        }
        else if (OlympAndGia1 == "Олимпиады")
        {
            _olympiadsType1 = new OlympiadsType();
            _originalOlympiad = _olympiadsType1;
            OkkoRedactAdd.DataContext = _olympiadsType1;

            BorderGiaRedAdd.IsVisible = false;
            BorderGiaSubject.IsVisible = false;
            BorderOlympiad.IsVisible = false;
            BorderOlympRedAdd.IsVisible = true;

            Title = "Добавление Олимпиады";
        }
        else if (OlympAndGia1 == "Настройка ГИА")
        {
            _giaSubject = new GiaSubject();
            OkkoRedactAdd.DataContext = _giaSubject;

            BorderGiaRedAdd.IsVisible = false;
            BorderOlympRedAdd.IsVisible = false;
            BorderOlympiad.IsVisible = false;
            BorderGiaSubject.IsVisible = true;

            Title = "Настройка ГИА";
        }
        else if (OlympAndGia1 == "Настройка Олимпиад")
        {
            _Olympiad = new Olympiad();
            OkkoRedactAdd.DataContext = _Olympiad;

            BorderGiaRedAdd.IsVisible = false;
            BorderOlympRedAdd.IsVisible = false;
            BorderGiaSubject.IsVisible = false;
            BorderOlympiad.IsVisible = true;

            Title = "Настройка Олимпиад";
        }
        LoadComboBoxData();
    }

    /// <summary>
    /// Редактирование Олимпиад
    /// </summary>
    /// <param name="olympiadsType">Тип олимпиады</param>
    public AddAdnRedactOlympGia(OlympiadsType olympiadsType) : this()
    {
        InitializeComponent();
        OlympAndGia1 = "Олимпиады";

        _olympiadsType1 = olympiadsType;
        _originalOlympiad = olympiadsType;
        OkkoRedactAdd.DataContext = _olympiadsType1;

        BorderOlympRedAdd.IsVisible = true;
        Title = "Редактирование Олимпиады";
        LoadComboBoxData();
    }

    /// <summary>
    /// Редактирование ГИА
    /// </summary>
    /// <param name="giaItem">Предмет ГИА</param>
    public AddAdnRedactOlympGia(Item giaItem) : this()
    {
        InitializeComponent();
        OlympAndGia1 = "ГИА";

        _giaItem1 = giaItem;
        _originalGiaItem = giaItem;
        OkkoRedactAdd.DataContext = _giaItem1;

        BorderGiaRedAdd.IsVisible = true;
        Title = "Редактирование ГИА";
        LoadComboBoxData();
    }

    /// <summary>
    /// Загрузка комбобоксов
    /// </summary>
    public void LoadComboBoxData()
    {
        LoadComboBoxGia();
        LoadComboBoxOlympiad();
    }

    /// <summary>
    /// Загрузка данных в ComboBox для олимпиад
    /// </summary>
    public void LoadComboBoxOlympiad()
    {
        try
        {
            items1 = Helper.DateBase.Items.ToList();
            _OlympiadsTypes = Helper.DateBase.OlympiadsTypes.ToList();

            if (_Olympiad.Id != 0)
            {
                Box_Olympiad.ItemsSource = _OlympiadsTypes.OrderBy(o => o.Name);
                Box_Type_Olympiad.ItemsSource = items1.OrderBy(i => i.Name);

                Box_Olympiad.SelectedItem = _OlympiadsTypes.FirstOrDefault(o => o.Id == _Olympiad.Olympiads);
                Box_Type_Olympiad.SelectedItem = items1.FirstOrDefault(i => i.Id == _Olympiad.OlympiadsItems);
            }
            else
            {
                var tempTypes = new List<OlympiadsType>(_OlympiadsTypes);
                tempTypes.Insert(0, new OlympiadsType() { Id = 0, Name = "Тип олимпиады" });

                var tempItems = new List<Item>(items1);
                tempItems.Insert(0, new Item() { Id = 0, Name = "Название предмета" });

                Box_Olympiad.ItemsSource = tempTypes.OrderByDescending(o => o.Id == 0);
                Box_Type_Olympiad.ItemsSource = tempItems.OrderByDescending(i => i.Id == 0);

                Box_Olympiad.SelectedIndex = 0;
                Box_Type_Olympiad.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при загрузке ComboBox: {ex.Message}");
        }
    }

    /// <summary>
    /// Загрузка данных в ComboBox ГИА
    /// </summary>
    public void LoadComboBoxGia()
    {
        try
        {
            items1 = Helper.DateBase.Items.ToList();
            giaTypes1 = Helper.DateBase.GiaTypes.ToList();

            if (_giaSubject.Id != 0)
            {
                Box_GiaSubject.ItemsSource = items1.OrderBy(g => g.Name);
                Box_Type_GiaSubject.ItemsSource = giaTypes1.OrderBy(g => g.Name);

                Box_GiaSubject.SelectedItem = items1.FirstOrDefault(i => i.Id == _giaSubject.GiaSubjects);
                Box_Type_GiaSubject.SelectedItem = giaTypes1.FirstOrDefault(g => g.Id == _giaSubject.GiaTypeId);
            }
            else
            {
                var tempItems = new List<Item>(items1);
                tempItems.Insert(0, new Item() { Id = 0, Name = "Название предмета" });

                var tempTypes = new List<GiaType>(giaTypes1);
                tempTypes.Insert(0, new GiaType() { Id = 0, Name = "Тип ГИА" });

                Box_GiaSubject.ItemsSource = tempItems.OrderByDescending(g => g.Id == 0);
                Box_Type_GiaSubject.ItemsSource = tempTypes.OrderByDescending(g => g.Id == 0);

                Box_GiaSubject.SelectedIndex = 0;
                Box_Type_GiaSubject.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при загрузке ComboBox: {ex.Message}");
        }
    }

    /// <summary>
    /// Закрытие окна
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_Click_Close(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    /// <summary>
    /// Сохранение изменений
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void Button_Click_Save(object? sender, RoutedEventArgs e)
    {
        var confirm = await ShowConfirmationDialog();
        if (!confirm) return;

        try
        {
            bool saveResult = false;

            if (BorderRedactSchool.IsVisible)
            {
                saveResult = await SaveSchool();
            }
            else if (BorderRedactClass.IsVisible)
            {
                saveResult = await SaveClass();
            }
            else if (OlympAndGia1 == "ГИА")
            {
                saveResult = await SaveGiaItem();
            }
            else if (OlympAndGia1 == "Олимпиады")
            {
                saveResult = await SaveOlympiadItem();
            }
            else if (OlympAndGia1 == "Смена ГИА" || OlympAndGia1 == "Настройка ГИА")
            {
                saveResult = await SaveGiaSubject();
            }
            else if (OlympAndGia1 == "Смена олимпиады" || OlympAndGia1 == "Настройка Олимпиад")
            {
                saveResult = await SaveOlympiad();
            }

            if (saveResult)
            {
                await ShowSuccessDialog("Данные успешно сохранены!");
                Close(true);
            }
        }
        catch (Exception ex)
        {
            await ShowErrorDialog($"Ошибка при сохранении: {ex.Message}");
        }
    }

    /// <summary>
    /// Показывает модальное диалоговое окно и возвращает результат сохранения
    /// </summary>
    /// <param name="owner"></param>
    /// <returns></returns>
    public new async Task<bool> ShowDialog(Window owner)
    {
        await base.ShowDialog(owner);
        return _isSaved;
    }

    /// <summary>
    /// Закрытие окна с результатом
    /// </summary>
    /// <param name="result">Результат операции</param>
    public void Close(bool result)
    {
        _isSaved = result;
        base.Close();
    }

    /// <summary>
    /// Сохраняет изменения Предмета ГИА в базу данных
    /// </summary>
    /// <returns>Результат операции сохранения</returns>
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
                Helper.DateBase.Items.Add(new Item { Name = _giaItem1.Name });
            }
            else
            {
                _originalGiaItem.Name = _giaItem1.Name;
                Helper.DateBase.Items.Update(_originalGiaItem);
            }

            return await Helper.DateBase.SaveChangesAsync() > 0;
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

    /// <summary>
    /// Сохраняет настройки олимпиады (связь предмета и типа олимпиады)
    /// </summary>
    /// <returns>Результат операции сохранения</returns>
    private async Task<bool> SaveOlympiad()
    {
        try
        {
            var selectedType = Box_Olympiad.SelectedItem as OlympiadsType;
            var selectedItem = Box_Type_Olympiad.SelectedItem as Item;

            // Проверка что выбраны не подсказки (Id != 0)
            if (selectedType == null || selectedType.Id == 0 ||
                selectedItem == null || selectedItem.Id == 0)
            { await ShowErrorDialog("Выберите тип олимпиады и предмет из списка!"); return false; }

            // Проверка что выбраны реальные элементы (не подсказки)
            if (selectedType.Name == "Тип олимпиады" ||
                selectedItem.Name == "Название предмета")
            { await ShowErrorDialog("Выберите тип олимпиады и предмет из списка!"); return false; }

            _Olympiad.Olympiads = selectedType.Id;
            _Olympiad.OlympiadsItems = selectedItem.Id;

            // Проверка на существование такой же связи (для нового или измененного)
            bool exists = Helper.DateBase.Olympiads
                .Any(o => o.Olympiads == selectedType.Id &&
                         o.OlympiadsItems == selectedItem.Id &&
                         o.Id != _Olympiad.Id); // Исключаем текущую запись при редактировании

            if (exists)
            {
                await ShowErrorDialog("Такая связь уже существует!");
                return false;
            }

            if (_Olympiad.Id == 0)
            {
                Helper.DateBase.Olympiads.Add(_Olympiad);
            }
            else
            {
                Helper.DateBase.Olympiads.Update(_Olympiad);
            }

            return await Helper.DateBase.SaveChangesAsync() > 0;
        }
        catch (Exception ex)
        {
            await ShowErrorDialog($"Ошибка при сохранении: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Сохраняет настройки предмета ГИА (связь предмета и типа ГИА)
    /// </summary>
    /// <returns>Результат операции сохранения</returns>
    private async Task<bool> SaveGiaSubject()
    {
        try
        {
            var selectedItem = Box_GiaSubject.SelectedItem as Item;
            var selectedType = Box_Type_GiaSubject.SelectedItem as GiaType;

            // Проверка что выбраны не нулевые значения (Id != 0)
            if (selectedItem == null || selectedItem.Id == 0 ||
                selectedType == null || selectedType.Id == 0)
            {
                await ShowErrorDialog("Выберите предмет и тип ГИА из списка!");
                return false;
            }

            // Проверка что выбраны реальные элементы (не подсказки)
            if (selectedItem.Name == "Название предмета" || selectedType.Name == "Тип ГИА")
            {
                await ShowErrorDialog("Выберите предмет и тип ГИА из списка!");
                return false;
            }

            _giaSubject.GiaSubjects = selectedItem.Id;
            _giaSubject.GiaTypeId = selectedType.Id;

            // Проверка на существование такой же связи (для нового или измененного)
            bool exists = Helper.DateBase.GiaSubjects
                .Any(gs => gs.GiaSubjects == selectedItem.Id &&
                           gs.GiaTypeId == selectedType.Id &&
                           gs.Id != _giaSubject.Id); // Исключаем текущую запись при редактировании

            if (exists)
            {
                await ShowErrorDialog("Такая связь уже существует!");
                return false;
            }

            if (_giaSubject.Id == 0)
            {
                Helper.DateBase.GiaSubjects.Add(_giaSubject);
            }
            else
            {
                Helper.DateBase.GiaSubjects.Update(_giaSubject);
            }

            return await Helper.DateBase.SaveChangesAsync() > 0;
        }
        catch (Exception ex)
        {
            await ShowErrorDialog($"Ошибка при сохранении: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Сохраняет изменения олимпиады в базу данных
    /// </summary>
    /// <returns>Результат операции сохранения</returns>
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
                Helper.DateBase.OlympiadsTypes.Add(new OlympiadsType { Name = _olympiadsType1.Name });
            }
            else
            {
                _originalOlympiad.Name = _olympiadsType1.Name;
                Helper.DateBase.OlympiadsTypes.Update(_originalOlympiad);
            }

            return await Helper.DateBase.SaveChangesAsync() > 0;
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

    /// <summary>
    /// Показывает диалоговое окно с подтверждением сохранения
    /// </summary>
    /// <returns>Результат подтверждения пользователя</returns>
    private async Task<bool> ShowConfirmationDialog()
    {
        string message;

        if (BorderRedactSchool.IsVisible)
        {
            message = "Вы уверены, что хотите сохранить изменения школы?";
        }
        else if (BorderRedactClass.IsVisible)
        {
            message = "Вы уверены, что хотите сохранить изменения класса?";
        }
        else
        {
            message = OlympAndGia1 == "ГИА"
                ? "Вы уверены, что хотите сохранить изменения предмета ГИА?"
                : "Вы уверены, что хотите сохранить изменения олимпиады?";
        }

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

    /// <summary>
    /// Обработчик закрытия окна, отменяет несохраненные изменения в базе данных
    /// </summary>
    /// <param name="e"></param>
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (!_isSaved)
        {
            if (_originalGiaItem != null && _originalGiaItem.Id != 0)
                Helper.DateBase.Entry(_originalGiaItem).State = EntityState.Unchanged;

            if (_originalOlympiad != null && _originalOlympiad.Id != 0)
                Helper.DateBase.Entry(_originalOlympiad).State = EntityState.Unchanged;
        }

        base.OnClosing(e);
    }

    /// <summary>
    /// Показывает диалоговое окно с сообщением об ошибке
    /// </summary>
    /// <param name="message">Сообщение об ошибке</param>
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
    /// Показывает диалоговое окно с сообщением об успешном выполнении
    /// </summary>
    /// <param name="message">Сообщение об успехе</param>
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

    /// <summary>
    /// Создает кнопку для диалоговых окон, ОК
    /// </summary>
    /// <param name="text">Текст кнопки</param>
    /// <param name="background">Цвет фона</param>
    /// <returns>Созданная кнопка</returns>
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
    /// Сохраняет изменения школы в базу данных
    /// </summary>
    /// <returns>Результат операции сохранения</returns>
    private async Task<bool> SaveSchool()
    {
        // Проверка обязательных полей
        if (string.IsNullOrWhiteSpace(_School.Name))
        {
            await ShowErrorDialog("Название школы не может быть пустым");
            return false;
        }

        if (string.IsNullOrWhiteSpace(_School.SchoolNumber))
        {
            await ShowErrorDialog("Номер школы не может быть пустым");
            return false;
        }

        if (string.IsNullOrWhiteSpace(_School.District))
        {
            await ShowErrorDialog("Район не может быть пустым");
            return false;
        }

        if (string.IsNullOrWhiteSpace(_School.City))
        {
            await ShowErrorDialog("Город не может быть пустым");
            return false;
        }

        try
        {
            // Извлекаем только цифры из номера 
            string numberDigits = Helper.ExtractDigits(_School.SchoolNumber);

            // Проверяем дублирование по цифрам
            bool exists = Helper.DateBase.Schools.AsEnumerable().Any(s => Helper.ExtractDigits(s.SchoolNumber) == numberDigits && s.Id != _School.Id);

            if (exists)
            {
                await ShowErrorDialog("Школа с таким номером уже существует!");
                return false;
            }

            if (_School.Id == 0)
                Helper.DateBase.Schools.Add(_School);
            else
                Helper.DateBase.Schools.Update(_School);

            return await Helper.DateBase.SaveChangesAsync() > 0;
        }
        catch (Exception ex)
        {
            await ShowErrorDialog($"Ошибка: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Сохраняет изменения класса
    /// </summary>
    /// <returns>Результат операции сохранения</returns>
    private async Task<bool> SaveClass()
    {
        // Проверка обязательных полей
        if (string.IsNullOrWhiteSpace(_class.ClassesNumber))
        {
            await ShowErrorDialog("Номер класса не может быть пустым");
            return false;
        }

        try
        {
            // Проверка на дублирование класса
            bool classExists = Helper.DateBase.Classes.Any(c => c.ClassesNumber == _class.ClassesNumber && c.Id != _class.Id);

            if (classExists)
            {
                await ShowErrorDialog("Класс с таким номером уже существует!");
                return false;
            }

            if (_class.Id == 0)
            {
                Helper.DateBase.Classes.Add(_class);
            }
            else
            {
                Helper.DateBase.Classes.Update(_class);
            }

            return await Helper.DateBase.SaveChangesAsync() > 0;
        }
        catch (DbUpdateException dbEx)
        {
            await ShowErrorDialog($"Ошибка базы данных: {dbEx.InnerException?.Message ?? dbEx.Message}");
            return false;
        }
        catch (Exception ex)
        {
            await ShowErrorDialog($"Ошибка при сохранении класса: {ex.Message}");
            return false;
        }
    }
}