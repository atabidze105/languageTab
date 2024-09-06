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

namespace languageTab
{
    public partial class MainWindow : Window
    {
        private List<Client> _ClientsDisplayed = []; //Отображаемые элементы
        private List<List<Client>> _ClientsPages = []; //Список списков элементов (страниц)
        private List<Client> _ClientsSelection = []; //Выборка элементов
        private int _CelectedPageIndex; //Выбранный номер страницы

        public MainWindow()
        {
            InitializeComponent();

            cbox_display.SelectedIndex = 0;            
            cbox_gender.SelectedIndex = 0;
            cbox_sorting.SelectedIndex = 0;
            ListBoxInit(AllClients);
        }

        private void ListBoxInit(List<Client> clients)
        {
            _ClientsDisplayed.Clear();
            _ClientsDisplayed.AddRange(clients);
            lbox_clients.ItemsSource = clients.Select(x => new
            {
                x.IdClient,
                x.Firstname,
                x.Name,
                x.Surname,
                Gender = Genders[x.IdGender].Name,
                x.DateOfBirth,
                x.Phone,
                x.Email,
                x.RegistrationDate,
                x.LastVisit,
                
                visitCount = x.VisitsLogs.Count,
                ClientPhoto = new Bitmap($"Assets/{x.Photo}"),
                Tags = x.ClientsTags.Select(y => new 
                {
                   tags[y.IdTag].Name,
                   Color = $"#{tags[y.IdTag].ColorTag}"
                })

            });


            _ClientsSelection.Clear();
            if (cbox_gender.SelectedIndex != 0 || tbox_search.Text != "") 
            { 
                _ClientsSelection.AddRange(clients); 
            }
            tblock_clientsCount.Text = _ClientsPages.Count > 0 && _ClientsSelection.Count > 0 ? $"{_ClientsPages.SelectMany(list => list).Distinct().Count()} из {AllClients.Count}" :
                (_ClientsSelection.Count > 0 ? $"{_ClientsSelection.Count} из {AllClients.Count}" : $"{AllClients.Count} из {AllClients.Count}");
            PageTextDisplay();
        }


        private void PageTextDisplay()
        {
            if (_ClientsPages.Count > 0)
            {
                tblock_page.IsVisible = true;
                tblock_pageCount.IsVisible = true;
                tblock_pageCount.Text = $"{_CelectedPageIndex + 1}/{_ClientsPages.Count}";
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
                        _CelectedPageIndex--;
                        if (_CelectedPageIndex >= 0)
                        {
                            ListBoxInit(_ClientsPages[_CelectedPageIndex]);
                        }
                        else
                        {
                            _CelectedPageIndex++;
                        }
                        break;
                    case "btn_nextPage":
                        _CelectedPageIndex++;
                        if (_CelectedPageIndex < _ClientsPages.Count)
                        {
                            ListBoxInit(_ClientsPages[_CelectedPageIndex]);
                        }
                        else
                        {
                            _CelectedPageIndex--;
                        }
                        break;
                }
            }
        }

        private void ComboBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            WholeSorting();
        }

        

        private List<Client> SelectionGender(List<Client> clients, int genderSwith)
        {
            switch (genderSwith)
            {
                case 1:
                    return clients.Where(x => x.IdGender == 0).ToList();
                    break;
                case 2:
                    return clients.Where(x => x.IdGender == 1).ToList();
                    break;
            }
            return clients;
        }

        private List<Client> SelectionSorting(List<Client> clients, int selectonSwitch)
        {
            switch (selectonSwitch)
            {
                case 1:
                    return clients.OrderBy(x => x.Firstname).ToList();
                    break;
                case 2:
                    return clients.OrderByDescending(x => x.LastVisit).ToList();
                    break;
                case 3:
                    return clients.OrderByDescending(x => x.VisitsLogs.Count).ToList();
                    break;
            }

            return clients;
        }

        private void Selection(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
        {
            WholeSorting();
            //ListBoxInit(SelectionSorting(SelectionGender(_ClientsSelection.Count > 0 ? _ClientsSelection : AllClients, cbox_gender.SelectedIndex), cbox_sorting.SelectedIndex));
        }

        private void WholeSorting()
        {
            ClientsDisplayed(SelectionSorting(SelectionGender(_ClientsSelection.Count > 0 ? _ClientsSelection : AllClients, cbox_gender.SelectedIndex), cbox_sorting.SelectedIndex), cbox_display.SelectedIndex);

            ListBoxInit(_ClientsPages.Count > 0 ? _ClientsPages[0] : SelectionSorting(SelectionGender(_ClientsSelection.Count > 0 ? _ClientsSelection : AllClients, cbox_gender.SelectedIndex), cbox_sorting.SelectedIndex));
            _CelectedPageIndex = 0;
            PageTextDisplay();
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
                    break;
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
        }
    }
}