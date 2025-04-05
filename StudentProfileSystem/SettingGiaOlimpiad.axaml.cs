using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using StudentProfileSystem.Models;

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
    /// Редактирование ГИА
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ListBox_DoubleTapped_Redact_Gia(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        var giared = ListBox_Gia.SelectedItem as Item;
        if (giared == null) return;

        this.Classes.Add("blur-effect");
        try
        {
            var dialog = new AddAdnRedactOlympGia(giared);
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
    /// Редактирование Олимпиад
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private async void ListBox_DoubleTapped_Redact_Olymp(object? sender, Avalonia.Input.TappedEventArgs e)
    {
        var giaOlympPredmet = ListBox_Olymp.SelectedItem as OlympiadsType;

        if (giaOlympPredmet == null) return;

        this.Classes.Add("blur-effect");
        try
        {
            var dialog = new AddAdnRedactOlympGia(giaOlympPredmet);
            await dialog.ShowDialog(this);

            items = Helper.DateBase.Items.ToList();
            ListBox_Gia.ItemsSource = items;
        }
        finally
        {
            this.Classes.Remove("blur-effect");
        }

    }


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


    private async void Click_Add_Olymp(object? sender, RoutedEventArgs e)
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






    private void MenuItem_Click_Delete_Gia(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {

    }
    private void MenuItem_Click_Delete_Olymp(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {

    }
}