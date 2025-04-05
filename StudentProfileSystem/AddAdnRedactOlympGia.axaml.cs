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
    private Item _giaItem1;
    private GiaSubject _giaSubject;
    private OlympiadsType _olympiadsType1;
    private Item _originalGiaItem;
    private OlympiadsType _originalOlympiad;

    private bool _isSaved = false;
    private string OlympAndGia1;

    List<GiaType> giaTypes1 = new List<GiaType>();
    List<Item> items1 = new List<Item>();

    public AddAdnRedactOlympGia()
    {
        InitializeComponent();
        _giaItem1 = new Item();
        _olympiadsType1 = new OlympiadsType();
        _giaSubject = new GiaSubject();
        OkkoRedactAdd.DataContext = _giaItem1;
        LoadComboBoxGia();
    }

    /// <summary>
    /// ����������� ��� ����� ���
    /// </summary>
    /// <param name="giaSubject">������� ���</param>
    public AddAdnRedactOlympGia(GiaSubject giaSubject) : this()
    {
        InitializeComponent();
        Title = "����� ���";
        OlympAndGia1 = "����� ���";

        _giaSubject = giaSubject ?? new GiaSubject();
        OkkoRedactAdd.DataContext = _giaSubject;
        BorderGiaSubject.IsVisible = true;
        LoadComboBoxGia();

        if (_giaSubject.Id != 0)
        {
            Box_GiaSubject.SelectedItem = items1.FirstOrDefault(i => i.Id == _giaSubject.GiaSubjects);
            Box_Type_GiaSubject.SelectedItem = giaTypes1.FirstOrDefault(g => g.Id == _giaSubject.GiaTypeId);
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

            BorderGiaRedAdd.IsVisible = true;
            BorderGiaSubject.IsVisible = false;
            BorderOlympRedAdd.IsVisible = false;

            Title = "���������� �������� ���";
        }
        else if (OlympAndGia1 == "���������")
        {
            _olympiadsType1 = new OlympiadsType();
            _originalOlympiad = _olympiadsType1;
            OkkoRedactAdd.DataContext = _olympiadsType1;

            BorderGiaRedAdd.IsVisible = false;
            BorderGiaSubject.IsVisible = false;
            BorderOlympRedAdd.IsVisible = true;

            Title = "���������� ���������";
        }
        else if (OlympAndGia1 == "��������� ���")
        {
            _giaSubject = new GiaSubject();
            OkkoRedactAdd.DataContext = _giaSubject;

            BorderGiaRedAdd.IsVisible = false;
            BorderOlympRedAdd.IsVisible = false;
            BorderGiaSubject.IsVisible = true;

            Title = "��������� ���";
        }
        LoadComboBoxGia();
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
        LoadComboBoxGia();
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
        LoadComboBoxGia();
    }

    /// <summary>
    /// �������� ������ � ComboBox ���
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
                // ��������� ������ �������� ��� ���������
                var tempItems = new List<Item>(items1);
                tempItems.Insert(0, new Item() { Id = 0, Name = "�������� ��������" });

                var tempTypes = new List<GiaType>(giaTypes1);
                tempTypes.Insert(0, new GiaType() { Id = 0, Name = "��� ���" });

                Box_GiaSubject.ItemsSource = tempItems.OrderByDescending(g => g.Id == 0);
                Box_Type_GiaSubject.ItemsSource = tempTypes.OrderByDescending(g => g.Id == 0);

                Box_GiaSubject.SelectedIndex = 0;
                Box_Type_GiaSubject.SelectedIndex = 0;
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

            if (OlympAndGia1 == "���")
            {
                saveResult = await SaveGiaItem();
            }
            else if (OlympAndGia1 == "���������")
            {
                saveResult = await SaveOlympiadItem();
            }
            else if (OlympAndGia1 == "����� ���" || OlympAndGia1 == "��������� ���")
            {
                saveResult = await SaveGiaSubject();
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
    /// ��������� ��������� �������� ���
    /// </summary>
    /// <returns>��������� �������� ����������</returns>
    /// <summary>
    /// ��������� ��������� �������� ��� (����� �������� � ���� ���)
    /// </summary>
    /// <returns>��������� �������� ����������</returns>
    private async Task<bool> SaveGiaSubject()
    {
        try
        {
            var selectedItem = Box_GiaSubject.SelectedItem as Item;
            var selectedType = Box_Type_GiaSubject.SelectedItem as GiaType;

            // �������� ��� ������� �� ��������� (Id != 0)
            if (selectedItem == null || selectedItem.Id == 0 ||
                selectedType == null || selectedType.Id == 0)
            {
                await ShowErrorDialog("�������� ������� � ��� ��� �� ������!");
                return false;
            }

            // �������� ��� ������� �������� �������� (�� ���������)
            if (selectedItem.Name == "�������� ��������" || selectedType.Name == "��� ���")
            {
                await ShowErrorDialog("�������� ������� � ��� ��� �� ������!");
                return false;
            }

            _giaSubject.GiaSubjects = selectedItem.Id;
            _giaSubject.GiaTypeId = selectedType.Id;

            // �������� �� ������������� ����� �� ����� (��� ������ ��� �����������)
            bool exists = Helper.DateBase.GiaSubjects
                .Any(gs => gs.GiaSubjects == selectedItem.Id &&
                           gs.GiaTypeId == selectedType.Id &&
                           gs.Id != _giaSubject.Id); // ��������� ������� ������ ��� ��������������

            if (exists)
            {
                await ShowErrorDialog("����� ����� ��� ����������!");
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
        var message = OlympAndGia1 == "���"
            ? "�� �������, ��� ������ ��������� ��������� �������� ���?"
            : "�� �������, ��� ������ ��������� ��������� ���������?";

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
}