using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using languageTab.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using static languageTab.StaticData;

namespace languageTab;

public partial class AddWindow : Window
{
    private string? _PictureFile = _RedClient != null ? _RedClient.Photo : null;
    private string? _SelectedImage = null; //выбранное изображение

    private readonly FileDialogFilter fileFilter = new() //Фильтр для проводника
    {
        Extensions = new List<string>() { "png", "jpg", "jpeg" }, //доступные расширения, отображаемые в проводнике
        Name = "Файлы изображений" //пояснение
    };

    public AddWindow()
    {
        InitializeComponent();

        Client client = new Client();
        client.Email = "eduefda@ijh.com";
        tbox_email.DataContext = client;
        client.Phone = "8(464)376-98-22";
        tbox_phone.DataContext = client;
    }


    private void EmailInsertion(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        //tblock_emailValidation.IsVisible = tbox_email.Text != "" ? !IsValidEmail(tbox_email.Text) : false;

    }

    private bool IsValidEmail(string email)
    {
        var trimmedEmail = email.Trim();

        if (trimmedEmail.EndsWith("."))
        {
            return false; // suggested by @TK-421
        }
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == trimmedEmail;
        }
        catch
        {
            return false;
        }
    }

    private void ClientAddDelete(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var btn = (sender as Button)!;
        switch (btn.Name)
        {
            case "btn_cancel":
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
                break;
            case "btn_confirm":
                break;
        }
    }

    private async void ImageSelection(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var btn = (sender as Button)!;
        switch (btn.Name)
        {
            case "btn_addImage":
                OpenFileDialog dialog = new(); //Открытие проводника
                dialog.Filters.Add(fileFilter); //Применение фильтра
                string[] result = await dialog.ShowAsync(this); //Выбор файла
                if (result == null || result.Length == 0)
                    return;//Если закрыть проводник то картинка не будет выбрана

                string imageName = Path.GetFileName(result[0]); //получение имени файла
                string[] extention = imageName.Split('.'); //Название файла делится на название и расширение
                string temp = extention[0]; //В изменяемой переменной хранится название файла. Оно будет меняться в процессе
                int i = 1; //Счетчик
                while (SameName(temp) != null) //Пока метод для проверки уникальности файла возвращает название файла
                {
                    temp = extention[0] + $"{i}"; //Новое имя файла
                    i++;
                }
                imageName = temp + '.' + extention[1]; //Новое имя файла с расширением

                System.IO.File.Copy(result[0], $"Assets/{imageName}", true); //Копирование файла в папку ассетов

                tblock_clientPhoto.Text = imageName;
                image_clientPhoto.Source = new Bitmap($"Assets/{imageName}");
                tblock_clientPhoto.IsVisible = image_clientPhoto.IsVisible = btn_deleteImage.IsVisible = true;
                break;
            case "btn_deleteImage":
                tblock_clientPhoto.IsVisible = image_clientPhoto.IsVisible = btn_deleteImage.IsVisible = false;
                _SelectedImage = null;//очистка поля с выбранным изображением

                if (tblock_clientPhoto.Text != _PictureFile) //Удаление произойдет только если удаляемое изображение не является значением из поля, хранящее изначальноне изображение товара
                    System.IO.File.Delete($"Assets/{tblock_clientPhoto.Text}"); //Удаление файла по названию из превью-текстблока

                break;
        }
    }

    private string SameName(string filename) //Проверка уникальности имени файла
    {
        string[] withExtentions = Directory.GetFiles("Assets"); //Получение названий всех изображений из ассетов с расширениями
        List<string> withoutExtentions = []; //инициализация нового списка для названий файлов без расширений

        foreach (string file in withExtentions)
            withoutExtentions.Add(Path.GetFileNameWithoutExtension(file)); //В новый список передаются названия файлов без расширений

        foreach (string file1 in withoutExtentions) //перебор каждого названия файла из списка названий
            if (file1 == filename) //если название одного из файлов идентично названию файла заданного в методе
            {
                return filename; //возвращает название файла
            }
        return null; //если такой файл не был найден, возвращает null
    }
}