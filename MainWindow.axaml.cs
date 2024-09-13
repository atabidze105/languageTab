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

namespace languageTab
{
    public partial class MainWindow : Window
    {
        public List<Client> _AllClients = Database.
           Clients.Include(x => x.ClientsTags).
                   Include(x => x.VisitsLogs).
                   Include(x => x.ClientsFiles).ToList(); //Список клиентов из БД (также связанными данными заполнены коллекции в объектах: теги, посещения, файлы)
        private int _SelectedPageIndex; //Выбранный номер страницы

        public MainWindow()
        {
            InitializeComponent();

            cbox_gender.SelectedIndex = _SelectedCBoxItem_gender; //Из соответствующих статичестких полей вып. спискам передаются нужные индексы
            cbox_sorting.SelectedIndex = _SelectedCBoxItem_sorting;
            cbox_display.SelectedIndex = _SelectedCBoxItem_display;
            tbox_search.Text = _InsertedTBoxText; // текстовому полю передается текст, по которому выполнялся поиск
            chbox_birthday.IsChecked = _SelectedChBoxState_birthday; //чекбоксу с ДР передается выбранное ранее значение

            SORTING();
        }

        private void ListBoxInit(List<Client> clients) //применение списка к литсбоксу
        {
            _ClientsDisplayed.Clear();
            _ClientsDisplayed.AddRange(clients);
            lbox_clients.ItemsSource = clients.Select(x => new
            {
                x.IdClient,
                x.Firstname,
                x.Name,
                x.Surname,
                Gender = _Genders[x.IdGender].Name,
                x.DateOfBirth,
                x.Phone,
                x.Email,
                x.RegistrationDate,
                x.LastVisit,

                visitCount = x.VisitsLogs.Count,
                ClientPhoto = System.IO.File.Exists($"Assets/{x.Photo}") == true ? new Bitmap($"Assets/{x.Photo}") : null,
                Tags = x.ClientsTags.Select(y => new
                {
                    _Tags[y.IdTag].Name,
                    Color = $"#{_Tags[y.IdTag].ColorTag}"
                })

            });
            tblock_clientsCount.Text = _ClientsPages.Count > 0 && _ClientsSelection.Count > 0 ? $"{_ClientsPages.SelectMany(list => list).Distinct().Count()} из {_AllClients.Count}" :
                (_ClientsSelection.Count > 0 ? $"{_ClientsSelection.Count} из {_AllClients.Count}" : $"{_AllClients.Count} из {_AllClients.Count}");

            tblock_clientsCount.Text = _ClientsSelection.Count > 0 || tbox_search.Text != "" ? $"{_ClientsSelection.Count} из {_AllClients.Count}" : $"{_AllClients.Count} из {_AllClients.Count}";
            PageTextDisplay();
        }

        private void BirthdaySelection() //выборка по ДР
        {
            if (chbox_birthday.IsChecked == true)
            {
                List<Client> clientsBithday = [];
                clientsBithday.AddRange(_ClientsSelection.Count > 0 ? _ClientsSelection : _AllClients);
                _ClientsSelection.Clear();
                foreach (Client client in clientsBithday)
                {
                    if (client.DateOfBirth.Month == DateTime.Today.Month)
                    {
                        _ClientsSelection.Add(client);
                    }
                }
            }

        }

        private void PageTextDisplay() //отображение номера страницы
        {
            if (_ClientsPages.Count > 0)
            {
                tblock_page.IsVisible = true;
                tblock_pageCount.IsVisible = true;
                tblock_pageCount.Text = $"{_SelectedPageIndex + 1}/{_ClientsPages.Count}";
            }
            else
            {
                tblock_page.IsVisible = tblock_pageCount.IsVisible = false;
            }
        }

        private void Display(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (cbox_display.SelectedIndex != 0)
            {
                var btn = (sender as Button)!;
                switch (btn.Name)
                {
                    case "btn_previousPage":
                        _SelectedPageIndex--;
                        if (_SelectedPageIndex >= 0)
                        {
                            ListBoxInit(_ClientsPages[_SelectedPageIndex]);
                        }
                        else
                        {
                            _SelectedPageIndex++;
                        }
                        break;
                    case "btn_nextPage":
                        _SelectedPageIndex++;
                        if (_SelectedPageIndex < _ClientsPages.Count)
                        {
                            ListBoxInit(_ClientsPages[_SelectedPageIndex]);
                        }
                        else
                        {
                            _SelectedPageIndex--;
                        }
                        break;
                }
            }
        }

        private void ComboBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            SORTING();
        }



        private void SelectionGender(List<Client> clients, int genderSwith)
        {
            switch (genderSwith)
            {
                case 1:
                    _ClientsSelection.AddRange(clients.Where(x => x.IdGender == 0).ToList());
                    break;
                case 2:
                    _ClientsSelection.AddRange(clients.Where(x => x.IdGender == 1).ToList());
                    break;
            }
        }

        private List<Client> SelectionSorting(List<Client> clients, int selectonSwitch)
        {
            switch (selectonSwitch)
            {
                case 1:
                    return clients.OrderBy(x => x.Firstname).ToList();
                case 2:
                    return clients.OrderByDescending(x => x.LastVisit).ToList();
                case 3:
                    return clients.OrderByDescending(x => x.VisitsLogs.Count).ToList();
            }

            return clients;
        }

        private void SORTING()
        {
            _ClientsSelection.Clear();
            SelectionGender(_AllClients, cbox_gender.SelectedIndex);
            BirthdaySelection();
            ClientsDisplayed(TBoxSorting(), cbox_display.SelectedIndex);
        }

        private void ComboboxItemsSave()
        {
            _SelectedCBoxItem_display = cbox_display.SelectedIndex;
            _SelectedCBoxItem_gender = cbox_gender.SelectedIndex;
            _SelectedCBoxItem_sorting = cbox_sorting.SelectedIndex;
        }

        private List<Client> TBoxSorting() //Выборка по строке поиска и сортировка
        {
            if (tbox_search.Text != "")
            {
                List<Client> unsortedClients = [];
                unsortedClients.AddRange(_ClientsSelection.Count > 0 ? _ClientsSelection : _AllClients);
                int prioriryLevel;
                List<Client> clientsPriorityLevel1 = [];
                List<Client> clientsPriorityLevel2 = [];
                List<Client> clientsPriorityLevel3 = [];
                List<Client> clientsPriorityLevel4 = [];
                List<Client> clientsPriorityLevel5 = [];
                foreach (Client client in unsortedClients)
                {
                    prioriryLevel = 0;
                    if (client.Firstname.Trim().ToLower().Contains(tbox_search.Text.Trim().ToLower()))
                    {
                        prioriryLevel++;
                    }
                    if (client.Name.Trim().ToLower().Contains(tbox_search.Text.Trim().ToLower()))
                    {
                        prioriryLevel++;
                    }
                    if (client.Surname.Trim().ToLower().Contains(tbox_search.Text.Trim().ToLower()))
                    {
                        prioriryLevel++;
                    }
                    if (client.Phone.Trim().ToLower().Contains(tbox_search.Text.Trim().ToLower()))
                    {
                        prioriryLevel++;
                    }
                    if (client.Email.Trim().ToLower().Contains(tbox_search.Text.Trim().ToLower()))
                    {
                        prioriryLevel++;
                    }
                    switch (prioriryLevel)
                    {
                        case 1:
                            clientsPriorityLevel1.Add(client);
                            break;
                        case 2:
                            clientsPriorityLevel2.Add(client);
                            break;
                        case 3:
                            clientsPriorityLevel3.Add(client);
                            break;
                        case 4:
                            clientsPriorityLevel4.Add(client);
                            break;
                        case 5:
                            clientsPriorityLevel5.Add(client);
                            break;
                    }
                }
                _ClientsSelection.Clear();
                _ClientsSelection.AddRange(SelectionSorting(clientsPriorityLevel5, cbox_sorting.SelectedIndex));
                _ClientsSelection.AddRange(SelectionSorting(clientsPriorityLevel4, cbox_sorting.SelectedIndex));
                _ClientsSelection.AddRange(SelectionSorting(clientsPriorityLevel3, cbox_sorting.SelectedIndex));
                _ClientsSelection.AddRange(SelectionSorting(clientsPriorityLevel2, cbox_sorting.SelectedIndex));
                _ClientsSelection.AddRange(SelectionSorting(clientsPriorityLevel1, cbox_sorting.SelectedIndex));
                return _ClientsSelection;
            }
            else
            {
                return SelectionSorting(_ClientsSelection.Count > 0 ? _ClientsSelection : _AllClients, cbox_sorting.SelectedIndex);
            }
        }

        private void Selection(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            SORTING();
        }

        private void ClientsDisplayed(List<Client> clients, int forswitch)
        {
            _ClientsPages.Clear();
            int displayedClientsCount = 1;
            switch (forswitch)
            {
                case 1:
                    displayedClientsCount = 10;
                    break;
                case 2:
                    displayedClientsCount = 50;
                    break;
                case 3:
                    displayedClientsCount = 200;
                    break;
                default:
                    ListBoxInit(clients);
                    return;
            }
            displayedClientsCount = displayedClientsCount > clients.Count ? clients.Count : displayedClientsCount;
            int listCount = (int)Math.Ceiling((double)clients.Count / displayedClientsCount);
            int l = 0; //Счетчик для всех клиентов
            for (int j = 0; j < listCount; j++)
            {
                List<Client> displayedClients = [];
                int testint = (displayedClientsCount > clients.Count - displayedClientsCount * j ? clients.Count - displayedClientsCount * j : displayedClientsCount);
                for (int i = 0; i < testint; i++)
                {
                    displayedClients.Add(clients[l]);
                    l++;
                }
                _ClientsPages.Add(displayedClients);
            }
            _SelectedPageIndex = 0;
            ListBoxInit(clients.Count > 0 ? _ClientsPages[_SelectedPageIndex] : clients);
            PageTextDisplay();
        }

        private void TextBox_searchingProcess(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            _InsertedTBoxText = tbox_search.Text;
            SORTING();
        }

        private void CheckBox_birthday(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _SelectedChBoxState_birthday = chbox_birthday.IsChecked;
            SORTING();
        }

        private void AddClient(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ComboboxItemsSave();
            var btn = (sender as Button)!;
            switch (btn.Name)
            {
                case "btn_red":
                    _RedClient = Helper.Database.Clients.Find((int)btn!.Tag!);
                    break;
            }

            AddWindow addWindow = new AddWindow();
            addWindow.Show();
            this.Close();
        }
    }
}