using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using static languageTab.StaticData;

using static languageTab.Context.Helper;
using Avalonia.Media.Imaging;
using languageTab.Models;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using HarfBuzzSharp;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using languageTab.Context;
using System.Net.Mail;
using System.Threading.Tasks;

namespace languageTab;

public partial class AddWindow : Window
{
    private Client _ClientPlug = new Client(); //Объект для оображения ошибки в поле для ввода email 
    private string? _PictureFile = _RedClient != null ? _RedClient.Photo : null; //изображение, которое изначально имеет объект (если оно у него есть, иначе null)
    private string? _SelectedImage = null; //выбранное изображение

    private readonly FileDialogFilter fileFilter = new() //Фильтр для проводника
    {
        Extensions = new List<string>() {"jpg", "png"}, //доступные расширения, отображаемые в проводнике
        Name = "Файлы изображений" //пояснение
    };

    public AddWindow()
    {
        InitializeComponent();        
        tblock_header.Text = _RedClient == null ? "Добавить клиента" : " Редактирование клиента"; //В зависимости от наличия редактируемого объекта, интерфейс изменяется
        btn_confirm.Content = _RedClient == null ? "Добавить" : "Сохранить";
        btn_delete.IsVisible = _RedClient == null ? false : true;
        SelectedClientDataInsertion();
    }

    private void SelectedClientDataInsertion()
    {
        tbox_email.DataContext = _ClientPlug; //привязка контекста данных полю для ввода адреса эл. почты к полю Email (при неверном вводе почты выдает ошибку)
        if (_RedClient != null)
        {
            switch (_RedClient.Phone.Count()) //Отображение нужной маски для ввода телефона в зависимости от количества символов
            {
                case 16:
                    cbox_digitsCount.SelectedIndex = 0;
                    break;
                case 17:
                    cbox_digitsCount.SelectedIndex = 1;
                    break;
                case 15:
                    cbox_digitsCount.SelectedIndex = 2;
                    break;
            }
        }
        else
        {
            cbox_digitsCount.SelectedIndex = 0;
        }

        if (_RedClient != null) //если объект оедактируется
        {
            tblock_id.Text = $"ID: {_RedClient.IdClient}"; //Отображается ID
            tbox_firstname.Text = _RedClient.Firstname; //поля заполняются соответсвующими данными
            tbox_name.Text = _RedClient.Name;
            tbox_lastname.Text = _RedClient.Surname;

            tbox_phone.Text = _RedClient.Phone;
            tbox_email.Text = _RedClient.Email;

            tgswictch_gender.IsChecked = _RedClient.IdGender == 0 ? true : false; //отображение пола
            calendar_birthday.DisplayDate = _RedClient.DateOfBirth.ToDateTime(TimeOnly.MinValue); //На календре открывается страница с датой рождения и выбирается она же
            calendar_birthday.SelectedDate = _RedClient.DateOfBirth.ToDateTime(TimeOnly.MinValue);

            if (SameName(System.IO.Path.GetFileNameWithoutExtension(_RedClient.Photo)) != null) //проверяется наличие изображения в ассетах
            {
                image_clientPhoto.Source = new Bitmap($"Assets/{_RedClient.Photo}"); //устаанавливается на превью
                tblock_clientPhoto.Text = _SelectedImage = _RedClient.Photo; //передаются значения в текстблок-превью и поле для выбранного изображения
                tblock_clientPhoto.IsVisible = image_clientPhoto.IsVisible = btn_deleteImage.IsVisible = true; //превью и кнопка удаления картинки становится видимыми
            }
        }
    }

    private void ClientChangesApply(Client client) //Прменение изменений к клиенту
    {
        client.IdClient = _RedClient == null ? Helper.Database.Clients.OrderByDescending(x => x.IdClient).ToList().First().IdClient + 1 : _RedClient.IdClient;
        client.RegistrationDate = _RedClient == null ? new DateOnly(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day) : _RedClient.RegistrationDate;

        client.Firstname = tbox_firstname.Text;
        client.Name = tbox_name.Text;
        client.Surname = tbox_lastname.Text;
        client.Email = tbox_email.Text;
        client.Phone = tbox_phone.Text;
        client.IdGender = tgswictch_gender.IsChecked == true ? 0 : 1;
        client.DateOfBirth = calendar_birthday.SelectedDate != null ?
            new DateOnly(calendar_birthday.SelectedDate.Value.Year, calendar_birthday.SelectedDate.Value.Month, calendar_birthday.SelectedDate.Value.Day) : //Если не была выбрана иная дата, устанавливается сегодняшняя
            new DateOnly(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
        client.Photo = _SelectedImage;
    }


    private void ClientAddDelete(object? sender, Avalonia.Interactivity.RoutedEventArgs e) //Добавление и удаление клиента, выход из формы
    {
        try
        {
            var btn = (sender as Button)!;
            switch (btn.Name)
            {
                case "btn_cancel": //Отмена
                    if (_SelectedImage != null && _SelectedImage != _PictureFile) //Если поле для выбранного изображения не равно null и выбранное изображение не совпадает со значением из поля, хранящее изначальноне изображение товара
                        System.IO.File.Delete($"Assets/{_SelectedImage}"); //Файл изображения удаляется из папки ассетов

                    ShowMainWindow();
                    break;
                case "btn_confirm": //Добавление/сохранение изменений
                    if (tbox_firstname.Text != "" && tbox_name.Text != "" && IsEmailValid(tbox_email.Text) == true && tbox_phone.Text.Contains('_') == false &&
                        NamesCheck(tbox_name.Text) == false && NamesCheck(tbox_firstname.Text) == false && NamesCheck(tbox_lastname.Text) == false) 
                    { //Данные будут сохранены если в полях ФИО, телефона нет недопустимых символов, пройдена проверка на email
                        if (_RedClient != null) //если рдактирование
                        {
                            ClientChangesApply(_RedClient); //применение изменений
                            Helper.Database.SaveChanges(); //сохранение в БД
                            if (_SelectedImage != _PictureFile && _PictureFile != null) //Если было установлено новое изображение, 
                                System.IO.File.Delete($"Assets/{_PictureFile}"); //то старое удалится
                        }
                        else //Если создание нового
                        {
                            Client newClient = new Client(); //Создается новый объект
                            ClientChangesApply(newClient); //применяются изменения
                            Helper.Database.Clients.Add(newClient); //добавление в БД
                            Helper.Database.SaveChanges(); //сохранение изменений
                        }
                        ShowMainWindow(); //Переход к основному окну
                    }
                    break;
                case "btn_delete": //удаление
                    if (_SelectedImage != null) //Удаление выбранного и имеющегося изображений, если имеются
                        System.IO.File.Delete($"Assets/{_SelectedImage}");
                    if (_PictureFile != null)
                        System.IO.File.Delete($"Assets/{_PictureFile}");
                    Helper.Database.Clients.Remove(Helper.Database.Clients.Find(_RedClient.IdClient)); //В БД находит объект и удаляет
                    Helper.Database.SaveChanges(); //сохранение изменений
                    ShowMainWindow(); //Переход к основному окну
                    break;
            }
        }
        catch
        {
            if (_SelectedImage != null && _SelectedImage != _PictureFile) //если до исключения было выбрано новое изображение, оно удалится из ассетов. Если у объекта уже было изображение, оно останется на месте
                System.IO.File.Delete($"Assets/{_SelectedImage}");

            ShowMainWindow(); //Переход к основному окну
        }
    }

    private async void ImageSelection(object? sender, Avalonia.Interactivity.RoutedEventArgs e) //Выбор и удаление изображения
    {
        var btn = (sender as Button)!;
        switch (btn.Name)
        {
            case "btn_addImage": //добавление
                OpenFileDialog dialog = new(); //Открытие проводника
                dialog.Filters.Add(fileFilter); //Применение фильтра
                string[] result = await dialog.ShowAsync(this); //Выбор файла
                if (result == null || result.Length == 0 || new System.IO.FileInfo(result[0]).Length > 2000000)  
                    return;//Если закрыть проводник или размер файла будет превышать 2 МБ, то картинка не будет выбрана

                string imageName = System.IO.Path.GetFileName(result[0]); //получение имени файла
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

                if (_SelectedImage != null && _SelectedImage != _PictureFile) //Если до установки новой картинки была выбрана другая, и при этом выбранная картинка не значение из поля, хранящее изначальноне изображение товара
                    System.IO.File.Delete($"Assets/{_SelectedImage}"); //Удаление предыдущего изображения из ассетов
                _SelectedImage = imageName;

                break;
            case "btn_deleteImage": //удаление
                tblock_clientPhoto.IsVisible = image_clientPhoto.IsVisible = btn_deleteImage.IsVisible = false;

                if (_SelectedImage != _PictureFile) //Удаление произойдет только если удаляемое изображение не является значением из поля, хранящее изначальноне изображение объекта
                    System.IO.File.Delete($"Assets/{_SelectedImage}"); //Удаление выбранного изображения

                _SelectedImage = null;//очистка поля с выбранным изображением
                break;
        }
    }

    private string SameName(string filename) //Проверка уникальности имени файла
    {
        string[] withExtentions = Directory.GetFiles("Assets"); //Получение названий всех изображений из ассетов с расширениями
        List<string> withoutExtentions = []; //инициализация нового списка для названий файлов без расширений

        foreach (string file in withExtentions)
            withoutExtentions.Add(System.IO.Path.GetFileNameWithoutExtension(file)); //В новый список передаются названия файлов без расширений

        foreach (string file1 in withoutExtentions) //перебор каждого названия файла из списка названий
            if (file1 == filename) //если название одного из файлов идентично названию файла заданного в методе
            {
                return filename; //возвращает название файла
            }
        return null; //если такой файл не был найден, возвращает null
    }

    private bool IsEmailValid(string emailAddress) //проверка корректности email
    {
        try
        {
            MailAddress m = new MailAddress(emailAddress);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private void ComboBox_PhoneDigitsCount(object? sender, Avalonia.Controls.SelectionChangedEventArgs e) //смена элементов выпадающего списка
    {
        switch (cbox_digitsCount.SelectedIndex) //сменяются маски поля для ввода телефона
        {
            case 0:
                tbox_phone.Mask = "+7(000)000-00-00";
                break;
            case 1:
                tbox_phone.Mask = "+7(0000)000-00-00";
                break;
            case 2:
                tbox_phone.Mask = "+7(00)000-00-00";
                break;
        }
    }

    private bool NamesCheck(string name)// проверка на корректность ФИО
    {
        return new string[] 
        { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
          "`" , "~", "!", "@", "\"", "#", "№", "$", ";", "%",
          "^", ":" , "&", "?", "*", "(", ")", "_", "+", "=",
          "'", "|", "/", "<", ">"
        }.Any(name.Contains);
    }

    private void ShowMainWindow() //Переход к основному окну
    {
        _RedClient = null; //очистка поля для редактируемого объекта
        MainWindow mainWindow = new MainWindow();
        mainWindow.Show();
        this.Close();
    }

}