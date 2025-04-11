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
using MsBox.Avalonia.ViewModels.Commands;

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
    /// ����������� ��� �������������� �����
    /// </summary>
    public AddAdnRedactOlympGia(School school) : this()
    {
        InitializeComponent();

        _School = school ?? new School();
        OkkoRedactAdd.DataContext = _School;

        BorderRedactSchool.IsVisible = true;

        Title = school.Id == 0 ? "���������� �����" : "�������������� �����";
    }

    /// <summary>
    /// ����������� ��� �������������� �������
    /// </summary>
    public AddAdnRedactOlympGia(Class _Class) : this()
    {
        InitializeComponent();

        _class = _Class ?? new Class();
        OkkoRedactAdd.DataContext = _class;

        BorderRedactClass.IsVisible = true;

        Title = _Class.Id == 0 ? "���������� ������" : "�������������� ������";
    }


    /// <summary>
    /// ����������� ��� ����� ���������
    /// </summary>
    /// <param name="olympiad"></param>
    public AddAdnRedactOlympGia(Olympiad olympiad) : this()
    {
        InitializeComponent();

        Title = "����� ���������";
        OlympAndGia1 = "����� ���������";

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
    /// ���������� �������� ��� ��� ���������
    /// </summary>
    /// <param name="OlympAndGia">��� �������� ("���", "���������" ��� "��������� ���")</param>
    public AddAdnRedactOlympGia(string OlympAndGia) : this()
    {
        InitializeComponent();
        OlympAndGia1 = OlympAndGia;

        if (OlympAndGia1 == "���")
        {
            _giaItem1 = new Item();
            _originalGiaItem = _giaItem1;
            OkkoRedactAdd.DataContext = _giaItem1;

            BorderOlympRedAdd.IsVisible = false;
            BorderOlympiad.IsVisible = false;
            BorderGiaRedAdd.IsVisible = true;

            Title = "���������� �������� ���";
        }
        else if (OlympAndGia1 == "���������")
        {
            _olympiadsType1 = new OlympiadsType();
            _originalOlympiad = _olympiadsType1;
            OkkoRedactAdd.DataContext = _olympiadsType1;

            BorderGiaRedAdd.IsVisible = false;
            BorderOlympiad.IsVisible = false;
            BorderOlympRedAdd.IsVisible = true;

            Title = "���������� ���������";
        }
        else if (OlympAndGia1 == "��������� ���")
        {
            _giaSubject = new GiaSubject();
            OkkoRedactAdd.DataContext = _giaSubject;

            BorderGiaRedAdd.IsVisible = false;
            BorderOlympRedAdd.IsVisible = false;
            BorderOlympiad.IsVisible = false;

            Title = "��������� ���";
        }
        else if (OlympAndGia1 == "��������� ��������")
        {
            _Olympiad = new Olympiad();
            OkkoRedactAdd.DataContext = _Olympiad;

            BorderGiaRedAdd.IsVisible = false;
            BorderOlympRedAdd.IsVisible = false;
            BorderOlympiad.IsVisible = true;

            Title = "��������� ��������";
        }
        LoadComboBoxData();
    }

    /// <summary>
    /// �������������� ��������
    /// </summary>
    /// <param name="olympiadsType">��� ���������</param>
    public AddAdnRedactOlympGia(OlympiadsType olympiadsType) : this()
    {
        InitializeComponent();
        OlympAndGia1 = "���������";

        _olympiadsType1 = olympiadsType;
        _originalOlympiad = olympiadsType;
        OkkoRedactAdd.DataContext = _olympiadsType1;

        BorderOlympRedAdd.IsVisible = true;
        Title = "�������������� ���������";
        LoadComboBoxData();
    }

    /// <summary>
    /// �������������� ���
    /// </summary>
    /// <param name="giaItem">������� ���</param>
    public AddAdnRedactOlympGia(Item giaItem) : this()
    {
        InitializeComponent();
        OlympAndGia1 = "���";

        _giaItem1 = giaItem;
        _originalGiaItem = giaItem;
        OkkoRedactAdd.DataContext = _giaItem1;

        BorderGiaRedAdd.IsVisible = true;
        Title = "�������������� ���";
        LoadComboBoxData();
    }

    /// <summary>
    /// �������� �����������
    /// </summary>
    public void LoadComboBoxData()
    {
        LoadComboBoxOlympiad();
    }

    /// <summary>
    /// �������� ������ � ComboBox ��� ��������
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
                tempTypes.Insert(0, new OlympiadsType() { Id = 0, Name = "��� ���������" });

                var tempItems = new List<Item>(items1);
                tempItems.Insert(0, new Item() { Id = 0, Name = "�������� ��������" });

                Box_Olympiad.ItemsSource = tempTypes.OrderByDescending(o => o.Id == 0);
                Box_Type_Olympiad.ItemsSource = tempItems.OrderByDescending(i => i.Id == 0);

                Box_Olympiad.SelectedIndex = 0;
                Box_Type_Olympiad.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"������ ��� �������� ComboBox: {ex.Message}");
        }
    }

    /// <summary>
    /// �������� ����
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Button_Click_Close(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }

    /// <summary>
    /// ���������� ���������
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
            else if (OlympAndGia1 == "���")
            {
                saveResult = await SaveGiaItem();
            }
            else if (OlympAndGia1 == "���������")
            {
                saveResult = await SaveOlympiadItem();
            }
            else if (OlympAndGia1 == "����� ���������" || OlympAndGia1 == "��������� ��������")
            {
                saveResult = await SaveOlympiad();
            }

            if (saveResult)
            {
                await ShowSuccessDialog("������ ������� ���������!");
                Close(true);
            }
        }
        catch (Exception ex)
        {
            await ShowErrorDialog($"������ ��� ����������: {ex.Message}");
        }
    }

    /// <summary>
    /// ���������� ��������� ���������� ���� � ���������� ��������� ����������
    /// </summary>
    /// <param name="owner"></param>
    /// <returns></returns>
    public new async Task<bool> ShowDialog(Window owner)
    {
        await base.ShowDialog(owner);
        return _isSaved;
    }

    /// <summary>
    /// �������� ���� � �����������
    /// </summary>
    /// <param name="result">��������� ��������</param>
    public void Close(bool result)
    {
        _isSaved = result;
        base.Close();
    }

    /// <summary>
    /// ��������� ��������� �������� ��� � ���� ������
    /// </summary>
    /// <returns>��������� �������� ����������</returns>
    private async Task<bool> SaveGiaItem()
    {
        if (string.IsNullOrWhiteSpace(_giaItem1.Name))
        {
            await ShowErrorDialog("�������� �������� �� ����� ���� ������");
            return false;
        }

        if (_giaItem1.Name.Length > 50)
        {
            await ShowErrorDialog("�������� �������� ������ ���� �� ������� 50 ��������");
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
            await ShowErrorDialog($"������ ���� ������: {dbEx.InnerException?.Message ?? dbEx.Message}");
            return false;
        }
        catch (Exception ex)
        {
            await ShowErrorDialog($"������ ��� ����������: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// ��������� ��������� ��������� (����� �������� � ���� ���������)
    /// </summary>
    /// <returns>��������� �������� ����������</returns>
    private async Task<bool> SaveOlympiad()
    {
        try
        {
            var selectedType = Box_Olympiad.SelectedItem as OlympiadsType;
            var selectedItem = Box_Type_Olympiad.SelectedItem as Item;

            // �������� ��� ������� �� ��������� (Id != 0)
            if (selectedType == null || selectedType.Id == 0 ||
                selectedItem == null || selectedItem.Id == 0)
            { await ShowErrorDialog("�������� ��� ��������� � ������� �� ������!"); return false; }

            // �������� ��� ������� �������� �������� (�� ���������)
            if (selectedType.Name == "��� ���������" ||
                selectedItem.Name == "�������� ��������")
            { await ShowErrorDialog("�������� ��� ��������� � ������� �� ������!"); return false; }

            _Olympiad.Olympiads = selectedType.Id;
            _Olympiad.OlympiadsItems = selectedItem.Id;

            // �������� �� ������������� ����� �� ����� (��� ������ ��� �����������)
            bool exists = Helper.DateBase.Olympiads
                .Any(o => o.Olympiads == selectedType.Id &&
                         o.OlympiadsItems == selectedItem.Id &&
                         o.Id != _Olympiad.Id); // ��������� ������� ������ ��� ��������������

            if (exists)
            {
                await ShowErrorDialog("����� ����� ��� ����������!");
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
            await ShowErrorDialog($"������ ��� ����������: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// ��������� ��������� ��������� � ���� ������
    /// </summary>
    /// <returns>��������� �������� ����������</returns>
    private async Task<bool> SaveOlympiadItem()
    {
        if (string.IsNullOrWhiteSpace(_olympiadsType1.Name))
        {
            await ShowErrorDialog("�������� ��������� �� ����� ���� ������");
            return false;
        }

        if (_olympiadsType1.Name.Length > 100)
        {
            await ShowErrorDialog("�������� ��������� ������ ���� �� ������� 100 ��������");
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
            await ShowErrorDialog($"������ ���� ������: {dbEx.InnerException?.Message ?? dbEx.Message}");
            return false;
        }
        catch (Exception ex)
        {
            await ShowErrorDialog($"������ ��� ����������: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// ���������� ���������� ���� � �������������� ����������
    /// </summary>
    /// <returns>��������� ������������� ������������</returns>
    private async Task<bool> ShowConfirmationDialog()
    {
        string message;

        if (BorderRedactSchool.IsVisible)
        {
            message = "�� �������, ��� ������ ��������� ��������� �����?";
        }
        else if (BorderRedactClass.IsVisible)
        {
            message = "�� �������, ��� ������ ��������� ��������� ������?";
        }
        else
        {
            message = OlympAndGia1 == "���"
                ? "�� �������, ��� ������ ��������� ��������� �������� ���?"
                : "�� �������, ��� ������ ��������� ��������� ���������?";
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

        var yesButton = CreateDialogButton("��", Brushes.Green, true);
        var noButton = CreateDialogButton("���", Brushes.Red, false);

        yesButton.Click += (s, e) => { result = true; dialog?.Close(); };
        noButton.Click += (s, e) => { result = false; dialog?.Close(); };

        dialog = new Window
        {
            Title = "������������� ����������",
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
    /// ���������� �������� ����, �������� ������������� ��������� � ���� ������
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
    /// ���������� ���������� ���� � ���������� �� ������
    /// </summary>
    /// <param name="message">��������� �� ������</param>
    /// <returns></returns>
    private async Task ShowErrorDialog(string message)
    {
        var dialog = new Window
        {
            Title = "������",
            Width = 400,
            Height = 200,
            SizeToContent = SizeToContent.WidthAndHeight, // �������������� ���������� �������
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            WindowState = WindowState.Normal, // �����������, ��� ���� �� � ������������� ������
            Content = new Border
            {
                BorderBrush = Brushes.Red,
                BorderThickness = new Thickness(2),
                Padding = new Thickness(15),
                Child = new StackPanel
                {
                    Spacing = 10,
                    Children =
                {
                    new TextBlock
                    {
                        Text = message,
                        TextWrapping = TextWrapping.Wrap,
                        HorizontalAlignment = HorizontalAlignment.Center
                    },
                    new Button
                    {
                        Content = "OK",
                        Width = 80,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Command = new RelayCommand(_ => (this.GetVisualRoot() as Window)?.Close())
                    }
                }
                }
            }
        };

        await dialog.ShowDialog(this);
    }

    /// <summary>
    /// ���������� ���������� ���� � ���������� �� �������� ����������
    /// </summary>
    /// <param name="message">��������� �� ������</param>
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
    /// ������� ������ ��� ���������� ����, ��
    /// </summary>
    /// <param name="text">����� ������</param>
    /// <param name="background">���� ����</param>
    /// <returns>��������� ������</returns>
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
    /// ��������� ��������� ����� � ���� ������
    /// </summary>
    /// <returns>��������� �������� ����������</returns>
    private async Task<bool> SaveSchool()
    {
        // �������� ������������ �����
        if (string.IsNullOrWhiteSpace(_School.Name))
        {
            await ShowErrorDialog("�������� ����� �� ����� ���� ������");
            return false;
        }

        if (string.IsNullOrWhiteSpace(_School.SchoolNumber))
        {
            await ShowErrorDialog("����� ����� �� ����� ���� ������");
            return false;
        }

        try
        {
            // ����������� �����
            string normalizedNumber = new string(_School.SchoolNumber.Where(char.IsDigit).ToArray());

            // ���������, ��� ����� ������������ � ������ �������� �����
            if (string.IsNullOrEmpty(normalizedNumber))
            {
                await ShowErrorDialog("����� ����� ������ ��������� �����");
                return false;
            }

            // ��������� ������������ �� ������
            bool duplicateExists = Helper.DateBase.Schools.AsEnumerable().Any(s => new string(s.SchoolNumber.Where(char.IsDigit)
                                                                         .ToArray()) == normalizedNumber && s.Id != _School.Id);

            if (duplicateExists)
            {
                await ShowErrorDialog($"����� � ������� '{normalizedNumber}' ��� ����������!");
                return false;
            }

            if (_School.Id == 0)
                Helper.DateBase.Schools.Add(_School);
            else
                Helper.DateBase.Schools.Update(_School);

            return await Helper.DateBase.SaveChangesAsync() > 0;
        }
        catch (DbUpdateException dbEx)
        {
            await ShowErrorDialog($"������ ���� ������: {dbEx.InnerException?.Message ?? dbEx.Message}");
            return false;
        }
        catch (Exception ex)
        {
            await ShowErrorDialog($"������ ��� ����������: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// ��������� ��������� ������
    /// </summary>
    /// <returns>��������� �������� ����������</returns>
    private async Task<bool> SaveClass()
    {
        // �������� ������������ �����
        if (string.IsNullOrWhiteSpace(_class.ClassesNumber))
        {
            await ShowErrorDialog("����� ������ �� ����� ���� ������");
            return false;
        }

        try
        {
            // �������� �� ������������ ������
            bool classExists = Helper.DateBase.Classes.Any(c => c.ClassesNumber == _class.ClassesNumber && c.Id != _class.Id);

            if (classExists)
            {
                await ShowErrorDialog("����� � ����� ������� ��� ����������!");
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
            await ShowErrorDialog($"������ ���� ������: {dbEx.InnerException?.Message ?? dbEx.Message}");
            return false;
        }
        catch (Exception ex)
        {
            await ShowErrorDialog($"������ ��� ���������� ������: {ex.Message}");
            return false;
        }
    }
}