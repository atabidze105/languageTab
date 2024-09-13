using languageTab.Context;
using languageTab.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static languageTab.Context.Helper;


namespace languageTab
{
    internal static class StaticData
    {
        //Универсальные данные:
        public static List<Gender> _Genders = Database.Genders.ToList(); //Список полов из БД (сомнительно но окэй)
        public static List<Models.Tag> _Tags = Database.Tags.ToList(); //Список тегов из БД
        public static List<Models.VisitsLog> _VisitLog = Database.VisitsLogs.ToList(); //Список посещений

        //Для сортировки и выборки:
        public static List<Client> _ClientsDisplayed = []; //Отображаемые элементы
        public static List<List<Client>> _ClientsPages = []; //Список списков элементов (страниц)
        public static List<Client> _ClientsSelection = []; //Выборка элементов
        public static int _SelectedCBoxItem_gender = 0; //Выбранный индекс комбобокса: пол
        public static int _SelectedCBoxItem_sorting = 0; //Выбранный индекс комбобокса: сортировка
        public static int _SelectedCBoxItem_display = 0; //Выбранный индекс комбобокса: отображение
        public static bool? _SelectedChBoxState_birthday = false; //Отображение клиентов с ДР в следующем месяце
        public static string _InsertedTBoxText = ""; //Написанное в строке поика

        //Для создания и редактирования клиентов
        public static Client _RedClient = null;
    }
}
