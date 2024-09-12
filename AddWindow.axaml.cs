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
    private Client _ClientPlug = new Client();
    private string? _PictureFile = _RedClient != null ? _RedClient.Photo : null;
    private string? _SelectedImage = null; //��������� �����������

    private readonly FileDialogFilter fileFilter = new() //������ ��� ����������
    {
        Extensions = new List<string>() {"jpg", "png"}, //��������� ����������, ������������ � ����������
        Name = "����� �����������" //���������
    };

    public AddWindow()
    {
        InitializeComponent();        
        tblock_header.Text = _RedClient == null ? "�������� �������" : " �������������� �������";
        btn_confirm.Content = _RedClient == null ? "��������" : "���������";
        btn_delete.IsVisible = _RedClient == null ? false : true;
        SelectedClientDataInsertion();
    }

    private void SelectedClientDataInsertion()
    {
        tbox_email.DataContext = _ClientPlug;
        if (_RedClient != null)
        {
            switch (_RedClient.Phone.Count())
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

        if (_RedClient != null)
        {
            tblock_id.Text = $"ID: {_RedClient.IdClient}";
            tbox_firstname.Text = _RedClient.Firstname;
            tbox_name.Text = _RedClient.Name;
            tbox_lastname.Text = _RedClient.Surname;

            tbox_phone.Text = _RedClient.Phone;
            tbox_email.Text = _RedClient.Email;

            tgswictch_gender.IsChecked = _RedClient.IdGender == 0 ? true : false;
            calendar_birthday.DisplayDate = _RedClient.DateOfBirth.ToDateTime(TimeOnly.MinValue);
            calendar_birthday.SelectedDate = _RedClient.DateOfBirth.ToDateTime(TimeOnly.MinValue);



            if (SameName(System.IO.Path.GetFileNameWithoutExtension(_RedClient.Photo)) != null)
            {
                image_clientPhoto.Source = new Bitmap($"Assets/{_RedClient.Photo}");
                tblock_clientPhoto.Text = _SelectedImage = _RedClient.Photo;
                tblock_clientPhoto.IsVisible = image_clientPhoto.IsVisible = btn_deleteImage.IsVisible = true;
            }
        }
    }

    private void ClientChangesApply(Client client)
    {
        if (client != null)
        {
            client.IdClient = _RedClient == null ? Helper.Database.Clients.OrderByDescending(x => x.IdClient).ToList().First().IdClient + 1 : _RedClient.IdClient;
            client.RegistrationDate = _RedClient == null ? new DateOnly(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day) : _RedClient.RegistrationDate ;
            
            client.Firstname = tbox_firstname.Text;
            client.Name = tbox_name.Text;
            client.Surname = tbox_lastname.Text;
            client.Email = tbox_email.Text;
            client.Phone = tbox_phone.Text;
            client.IdGender = tgswictch_gender.IsChecked == true  ? 0 : 1;
            client.DateOfBirth = calendar_birthday.SelectedDate != null ? 
                new DateOnly(calendar_birthday.SelectedDate.Value.Year, calendar_birthday.SelectedDate.Value.Month, calendar_birthday.SelectedDate.Value.Day) : 
                new DateOnly(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);
            client.Photo = _SelectedImage;
        }
    }


    private void ClientAddDelete(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        try
        {
            var btn = (sender as Button)!;
            switch (btn.Name)
            {
                case "btn_cancel":
                    if (_SelectedImage != null && _SelectedImage != _PictureFile) //���� ���� ��� ���������� ����������� �� ����� null � ��������� ����������� �� ��������� �� ��������� �� ����, �������� ������������ ����������� ������
                        System.IO.File.Delete($"Assets/{_SelectedImage}"); //���� ����������� ��������� �� ����� �������

                    ShowMainWindow();
                    break;
                case "btn_confirm":
                    if (tbox_firstname.Text != "" && tbox_name.Text != "" && IsEmailValid(tbox_email.Text) == true && tbox_phone.Text.Contains('_') == false &&
                        NamesCheck(tbox_name.Text) == false && NamesCheck(tbox_firstname.Text) == false && NamesCheck(tbox_lastname.Text) == false)
                    {
                        if (_RedClient != null)
                        {
                            ClientChangesApply(_RedClient);
                            Helper.Database.SaveChanges();


                            if (_SelectedImage != _PictureFile && _PictureFile != null) //���� ���� ����������� ����� �����������, 
                                System.IO.File.Delete($"Assets/{_PictureFile}");
                        }
                        else
                        {
                            Client newClient = new Client();
                            ClientChangesApply(newClient);
                            Helper.Database.Clients.Add(newClient);
                            Helper.Database.SaveChanges();
                        }
                        ShowMainWindow();
                    }
                    
                    break;
                case "btn_delete":
                    Helper.Database.Clients.Remove(Helper.Database.Clients.Find(_RedClient.IdClient));
                    Helper.Database.SaveChanges();
                    ShowMainWindow();
                    break;
            }
        }
        catch
        {
            if (_SelectedImage != null && _SelectedImage != _PictureFile) //���� �� ���������� ���� ������� ������ �����������, ��� �������� �� �������. ���� � ����� ��� ���� �����������, ��� ��������� �� �����
                System.IO.File.Delete($"Assets/{_SelectedImage}");

            ShowMainWindow();
        }
    }

    private async void ImageSelection(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var btn = (sender as Button)!;
        switch (btn.Name)
        {
            case "btn_addImage":
                OpenFileDialog dialog = new(); //�������� ����������
                dialog.Filters.Add(fileFilter); //���������� �������
                string[] result = await dialog.ShowAsync(this); //����� �����
                if (result == null || result.Length == 0 || new System.IO.FileInfo(result[0]).Length > 2000000)  
                    return;//���� ������� ��������� ��� ������ ����� ����� ��������� 2 ��, �� �������� �� ����� �������

                string imageName = System.IO.Path.GetFileName(result[0]); //��������� ����� �����
                string[] extention = imageName.Split('.'); //�������� ����� ������� �� �������� � ����������
                string temp = extention[0]; //� ���������� ���������� �������� �������� �����. ��� ����� �������� � ��������
                int i = 1; //�������
                while (SameName(temp) != null) //���� ����� ��� �������� ������������ ����� ���������� �������� �����
                {
                    temp = extention[0] + $"{i}"; //����� ��� �����
                    i++;
                }
                imageName = temp + '.' + extention[1]; //����� ��� ����� � �����������

                System.IO.File.Copy(result[0], $"Assets/{imageName}", true); //����������� ����� � ����� �������

                tblock_clientPhoto.Text = imageName;
                image_clientPhoto.Source = new Bitmap($"Assets/{imageName}");
                tblock_clientPhoto.IsVisible = image_clientPhoto.IsVisible = btn_deleteImage.IsVisible = true;

                if (_SelectedImage != null && _SelectedImage != _PictureFile) //���� �� ��������� ����� �������� ���� ������� ������, � ��� ���� ��������� �������� �� �������� �� ����, �������� ������������ ����������� ������
                    System.IO.File.Delete($"Assets/{_SelectedImage}"); //�������� ����������� ����������� �� �������
                _SelectedImage = imageName;

                break;
            case "btn_deleteImage":
                tblock_clientPhoto.IsVisible = image_clientPhoto.IsVisible = btn_deleteImage.IsVisible = false;

                if (_SelectedImage != _PictureFile) //�������� ���������� ������ ���� ��������� ����������� �� �������� ��������� �� ����, �������� ������������ ����������� ������
                    System.IO.File.Delete($"Assets/{tblock_clientPhoto.Text}"); //�������� ����� �� �������� �� ������-����������

                _SelectedImage = null;//������� ���� � ��������� ������������

                break;
        }
    }

    private string SameName(string filename) //�������� ������������ ����� �����
    {
        string[] withExtentions = Directory.GetFiles("Assets"); //��������� �������� ���� ����������� �� ������� � ������������
        List<string> withoutExtentions = []; //������������� ������ ������ ��� �������� ������ ��� ����������

        foreach (string file in withExtentions)
            withoutExtentions.Add(System.IO.Path.GetFileNameWithoutExtension(file)); //� ����� ������ ���������� �������� ������ ��� ����������

        foreach (string file1 in withoutExtentions) //������� ������� �������� ����� �� ������ ��������
            if (file1 == filename) //���� �������� ������ �� ������ ��������� �������� ����� ��������� � ������
            {
                return filename; //���������� �������� �����
            }
        return null; //���� ����� ���� �� ��� ������, ���������� null
    }

    private bool IsEmailValid(string emailAddress)
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

    private void ComboBox_PhoneDigitsCount(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        switch (cbox_digitsCount.SelectedIndex)
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

    private bool NamesCheck(string name)
    {
        return new string[] 
        { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
          "`" , "~", "!", "@", "\"", "#", "�", "$", ";", "%",
          "^", ":" , "&", "?", "*", "(", ")", "_", "+", "=",
          "'", "|", "/", "<", ">"
        }.Any(name.Contains);
    }

    private void ShowMainWindow()
    {
        _RedClient = null;
        MainWindow mainWindow = new MainWindow();
        mainWindow.Show();
        this.Close();
    }

}