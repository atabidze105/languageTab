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
    private string? _SelectedImage = null; //��������� �����������

    private readonly FileDialogFilter fileFilter = new() //������ ��� ����������
    {
        Extensions = new List<string>() { "png", "jpg", "jpeg" }, //��������� ����������, ������������ � ����������
        Name = "����� �����������" //���������
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
                OpenFileDialog dialog = new(); //�������� ����������
                dialog.Filters.Add(fileFilter); //���������� �������
                string[] result = await dialog.ShowAsync(this); //����� �����
                if (result == null || result.Length == 0)
                    return;//���� ������� ��������� �� �������� �� ����� �������

                string imageName = Path.GetFileName(result[0]); //��������� ����� �����
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
                break;
            case "btn_deleteImage":
                tblock_clientPhoto.IsVisible = image_clientPhoto.IsVisible = btn_deleteImage.IsVisible = false;
                _SelectedImage = null;//������� ���� � ��������� ������������

                if (tblock_clientPhoto.Text != _PictureFile) //�������� ���������� ������ ���� ��������� ����������� �� �������� ��������� �� ����, �������� ������������ ����������� ������
                    System.IO.File.Delete($"Assets/{tblock_clientPhoto.Text}"); //�������� ����� �� �������� �� ������-����������

                break;
        }
    }

    private string SameName(string filename) //�������� ������������ ����� �����
    {
        string[] withExtentions = Directory.GetFiles("Assets"); //��������� �������� ���� ����������� �� ������� � ������������
        List<string> withoutExtentions = []; //������������� ������ ������ ��� �������� ������ ��� ����������

        foreach (string file in withExtentions)
            withoutExtentions.Add(Path.GetFileNameWithoutExtension(file)); //� ����� ������ ���������� �������� ������ ��� ����������

        foreach (string file1 in withoutExtentions) //������� ������� �������� ����� �� ������ ��������
            if (file1 == filename) //���� �������� ������ �� ������ ��������� �������� ����� ��������� � ������
            {
                return filename; //���������� �������� �����
            }
        return null; //���� ����� ���� �� ��� ������, ���������� null
    }
}