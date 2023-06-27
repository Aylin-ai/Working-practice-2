using App.Infrastructure.DB;
using MyApplication.Infrastructure.Commands;
using MyApplication.Infrastructure.Stores;
using MyApplication.Services;
using MyApplication.ViewModels.Base;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using System.Windows;
using System.Windows.Input;

namespace MyApplication.ViewModels
{
    public class FamilyViewModel : ViewModel
    {
        private readonly AccountStore _accountStore;

        private int _userId;

        public FamilyViewModel(AccountStore accountStore, NavigationStore navigationStore)
        {
            _accountStore = accountStore;

            GetChildren();
            GetMarriage();
            GetDivorce();

            #region Команды

            NavigateToProfileCommand = new NavigateCommand<ProfileViewModel>(new NavigationService<ProfileViewModel>(
                navigationStore, () => new ProfileViewModel(_accountStore, navigationStore)));
            NavigateToDocumentsCommand = new NavigateCommand<DocumentsViewModel>(new NavigationService<DocumentsViewModel>(
                navigationStore, () => new DocumentsViewModel(_accountStore, navigationStore)));
            NavigateToHealthCommand = new NavigateCommand<HealthViewModel>(new NavigationService<HealthViewModel>(
                navigationStore, () => new HealthViewModel(_accountStore, navigationStore)));
            NavigateToWorkCommand = new NavigateCommand<WorkViewModel>(new NavigationService<WorkViewModel>(
                navigationStore, () => new WorkViewModel(_accountStore, navigationStore)));
            NavigateToAddChildCommand = new NavigateCommand<AddChildViewModel>(new NavigationService<AddChildViewModel>(
                navigationStore, () => new AddChildViewModel(_accountStore, navigationStore)));

            ChangeMarriageCommand = new LambdaCommand(OnChangeMarriageCommandExecuted);
            ChangeDivorceCommand = new LambdaCommand(OnChangeDivorceCommandExecuted);
            DeleteChildCommand = new LambdaCommand(OnDeleteChildCommandExecuted);

            #endregion
        }

        #region Данные свидетельства о браке

        private string _marriageSeries;
        public string MarriageSeries { get => _marriageSeries; set => Set(ref _marriageSeries, value); }

        private string _marriageNumber;
        public string MarriageNumber { get => _marriageNumber; set => Set(ref _marriageNumber, value); }

        private DateTime _marriageDateOfGive = DateTime.Now;
        public DateTime MarriageDateOfGive { get => _marriageDateOfGive; set => Set(ref _marriageDateOfGive, value); }

        private DateTime _marriageDateOfEntry = DateTime.Now;
        public DateTime MarriageDateOfEntry { get => _marriageDateOfEntry; set => Set(ref _marriageDateOfEntry, value); }

        private string _marriageNumberOfEntry;
        public string MarriageNumberOfEntry { get => _marriageNumberOfEntry; set => Set(ref _marriageNumberOfEntry, value); }

        private string _marriagePlaceOfRegistration;
        public string MarriagePlaceOfRegistration { get => _marriagePlaceOfRegistration; set => Set(ref _marriagePlaceOfRegistration, value); }

        #endregion

        #region Данные свидетельства о разводе

        private string _divorceSeries;
        public string DivorceSeries { get => _divorceSeries; set => Set(ref _divorceSeries, value); }

        private string _divorceNumber;
        public string DivorceNumber { get => _divorceNumber; set => Set(ref _divorceNumber, value); }

        private DateTime _divorceDateOfGive = DateTime.Now;
        public DateTime DivorceDateOfGive { get => _divorceDateOfGive; set => Set(ref _divorceDateOfGive, value); }

        private DateTime _divorceDateOfEntry = DateTime.Now;
        public DateTime DivorceDateOfEntry { get => _divorceDateOfEntry; set => Set(ref _divorceDateOfEntry, value); }

        private string _divorceNumberOfEntry;
        public string DivorceNumberOfEntry { get => _divorceNumberOfEntry; set => Set(ref _divorceNumberOfEntry, value); }

        private string _divorcePlaceOfRegistration;
        public string DivorcePlaceOfRegistration { get => _divorcePlaceOfRegistration; set => Set(ref _divorcePlaceOfRegistration, value); }

        #endregion

        #region Данные о детях

        private List<string> _children = new List<string>();
        public List<string> Children { get => _children; set => Set(ref _children, value); }

        private string _selectedChild;
        public string SelectedChild
        {
            get => _selectedChild;
            set
            {
                _selectedChild = value;

                MySqlConnection conn = DBUtils.GetDBConnection();
                conn.Open();

                try
                {
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = conn;

                    string sql = "select ребенок.фио, ребенок.пол, " +
                        "ребенок.дата_рождения, ребенок.страна_регистрации_рождения, " +
                        "ребенок.серия_свидетельства_о_рождении, ребенок.номер_свидетельства_о_рождении, " +
                        "ребенок.дата_выдачи_свидетельства, ребенок.дата_актовой_записи, " +
                        "ребенок.номер_актовой_записи, ребенок.место_государственной_регистрации " +
                        "from ребенок inner join дети_пользователя " +
                        "on ребенок.id = дети_пользователя.id_ребенка " +
                        "where дети_пользователя.id_пользователя = @userId and " +
                        "ребенок.фио = @childFio;";
                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@userId", _userId);
                    cmd.Parameters.AddWithValue("childFio", _selectedChild);

                    var reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            ChildFio = reader.GetString(0);
                            ChildSex = reader.GetString(1);
                            ChildDateOfBirth = reader.GetDateTime(2);
                            ChildPlaceOfLive = reader.GetString(3);
                            ChildBirthSeries = reader.GetString(4);
                            ChildBirthNumber = reader.GetString(5);
                            ChildBirthDateOfGive = reader.GetDateTime(6);
                            ChildDateOfEntry = reader.GetDateTime(7);
                            ChildNumberOfEntry = reader.GetString(8);
                            ChildPlaceOfRegistration = reader.GetString(9);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }

        private string _childFio;
        public string ChildFio { get => _childFio; set => Set(ref _childFio, value); }

        private string _childSex;
        public string ChildSex { get => _childSex; set => Set(ref _childSex, value); }

        private DateTime _childDateOfBirth = DateTime.Now;
        public DateTime ChildDateOfBirth { get => _childDateOfBirth; set => Set(ref _childDateOfBirth, value); }

        private string _childPlaceOfLive;
        public string ChildPlaceOfLive { get => _childPlaceOfLive; set => Set(ref _childPlaceOfLive, value); }

        private string _childBirthSeries;
        public string ChildBirthSeries { get => _childBirthSeries; set => Set(ref _childBirthSeries, value); }

        private string _childBirthNumber;
        public string ChildBirthNumber { get => _childBirthNumber; set => Set(ref _childBirthNumber, value); }

        private DateTime _childBirthDateOfGive = DateTime.Now;
        public DateTime ChildBirthDateOfGive { get => _childBirthDateOfGive; set => Set(ref _childBirthDateOfGive, value); }

        private DateTime _childDateOfEntry = DateTime.Now;
        public DateTime ChildDateOfEntry { get => _childDateOfEntry; set => Set(ref _childDateOfEntry, value); }

        private string _childNumberOfEntry;
        public string ChildNumberOfEntry { get => _childNumberOfEntry; set => Set(ref _childNumberOfEntry, value); }

        private string _childPlaceOfRegistration;
        public string ChildPlaceOfRegistration { get => _childPlaceOfRegistration; set => Set(ref _childPlaceOfRegistration, value); }

        #endregion

        #region Команды навигации

        public ICommand NavigateToProfileCommand { get; }

        public ICommand NavigateToDocumentsCommand { get; }
        public ICommand NavigateToHealthCommand { get; }
        public ICommand NavigateToWorkCommand { get; }
        public ICommand NavigateToAddChildCommand { get; }

        #endregion

        #region Команды

        #region Команда изменения данных свидетельства о браке

        public ICommand ChangeMarriageCommand { get; }

        private void OnChangeMarriageCommandExecuted(object parameter)
        {
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();

            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;

                string sql = "select * from свидетельство_о_браке " +
                    "where свидетельство_о_браке.id_пользователя = @userId";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@userId", _userId);

                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    sql = "update свидетельство_о_браке " +
                        "set свидетельство_о_браке.серия = @series, " +
                        "свидетельство_о_браке.номер = @number, " +
                        "свидетельство_о_браке.дата_выдачи = @dateOfGive, " +
                        "свидетельство_о_браке.дата_актовой_записи = @dateOfEntry, " +
                        "свидетельство_о_браке.номер_актовой_записи = @numberOfEntry, " +
                        "свидетельство_о_браке.место_государственной_регистрации = @placeOfRegistration " +
                        "where свидетельство_о_браке.id_пользователя = @userId";
                }
                else
                {
                    sql = "insert into свидетельство_о_браке " +
                        "values (@series, @number, " +
                        "@dateOfGive, @dateOfEntry, " +
                        "@numberOfEntry, @placeOfRegistration, " +
                        "@userId)";
                }
                reader.Close();

                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@series", MarriageSeries);
                cmd.Parameters.AddWithValue("@number", MarriageNumber);
                cmd.Parameters.AddWithValue("@dateOfGive", MarriageDateOfGive);
                cmd.Parameters.AddWithValue("@dateOfEntry", MarriageDateOfEntry);
                cmd.Parameters.AddWithValue("@numberOfEntry", MarriageNumberOfEntry);
                cmd.Parameters.AddWithValue("@placeOfRegistration", MarriagePlaceOfRegistration);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }

        #endregion

        #region Команда изменения данных свидетельства о браке

        public ICommand ChangeDivorceCommand { get; }

        private void OnChangeDivorceCommandExecuted(object parameter)
        {
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();

            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;

                string sql = "select * from свидетельство_о_разводе " +
                    "where свидетельство_о_разводе.id_пользователя = @userId";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@userId", _userId);

                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    sql = "update свидетельство_о_разводе " +
                        "set свидетельство_о_разводе.серия = @series, " +
                        "свидетельство_о_разводе.номер = @number, " +
                        "свидетельство_о_разводе.дата_выдачи = @dateOfGive, " +
                        "свидетельство_о_разводе.дата_актовой_записи = @dateOfEntry, " +
                        "свидетельство_о_разводе.номер_актовой_записи = @numberOfEntry, " +
                        "свидетельство_о_разводе.место_государственной_регистрации = @placeOfRegistration " +
                        "where свидетельство_о_разводе.id_пользователя = @userId";
                }
                else
                {
                    sql = "insert into свидетельство_о_разводе " +
                        "values (@series, @number, " +
                        "@dateOfGive, @dateOfEntry, " +
                        "@numberOfEntry, @placeOfRegistration, " +
                        "@userId)";
                }
                reader.Close();

                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@series", MarriageSeries);
                cmd.Parameters.AddWithValue("@number", MarriageNumber);
                cmd.Parameters.AddWithValue("@dateOfGive", MarriageDateOfGive);
                cmd.Parameters.AddWithValue("@dateOfEntry", MarriageDateOfEntry);
                cmd.Parameters.AddWithValue("@numberOfEntry", MarriageNumberOfEntry);
                cmd.Parameters.AddWithValue("@placeOfRegistration", MarriagePlaceOfRegistration);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }

        #endregion

        #region Команда удаления выбранного ребенка

        public ICommand DeleteChildCommand { get; }

        private void OnDeleteChildCommandExecuted(object parameter)
        {
            if (ChildFio == null || ChildFio == "")
            {
                MessageBox.Show("Ребенок не выбран");
                return;
            }
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();

            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;

                string sql = "delete from ребенок " +
                    "where фио = @fio and " +
                    "серия_свидетельства_о_рождении = @birthSeries and " +
                    "номер_свидетельства_о_рождении = @birthNumber;";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@fio", ChildFio);
                cmd.Parameters.AddWithValue("@birthSeries", ChildBirthSeries);
                cmd.Parameters.AddWithValue("@birthNumber", ChildBirthNumber);

                cmd.ExecuteNonQuery();

                GetChildren();
                ChildFio = "";
                ChildSex = "";
                ChildDateOfBirth = DateTime.Now;
                ChildPlaceOfLive = "";
                ChildBirthSeries = "";
                ChildBirthNumber = "";
                ChildBirthDateOfGive = DateTime.Now;
                ChildDateOfEntry = DateTime.Now;
                ChildNumberOfEntry = "";
                ChildPlaceOfRegistration = "";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }

        #endregion

        #endregion

        private void GetChildren()
        {
            Children.Clear();
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();

            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;

                string sql = "select пользователь.id from пользователь " +
                    "where пользователь.номер_телефона = @number";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@number", _accountStore.CurrentAccount.TelephoneNumber);


                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        _userId = reader.GetInt32(0);
                    }
                }
                reader.Close();

                sql = "select ребенок.фио from ребенок " +
                    "inner join дети_пользователя " +
                    "on ребенок.id = дети_пользователя.id_ребенка " +
                    "where дети_пользователя.id_пользователя = @userId";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@userId", _userId);

                reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        Children.Add(reader.GetString(0));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }

        private void GetMarriage()
        {
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();

            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;

                string sql = "select * from свидетельство_о_браке " +
                    "where id_пользователя = @userId";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@userId", _userId);

                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        MarriageSeries = reader.GetString(0);
                        MarriageNumber = reader.GetString(1);
                        MarriageDateOfGive = reader.GetDateTime(2);
                        MarriageDateOfEntry = reader.GetDateTime(3);
                        MarriageNumberOfEntry = reader.GetString(4);
                        MarriagePlaceOfRegistration = reader.GetString(5);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }

        private void GetDivorce()
        {
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();

            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;

                string sql = "select * from свидетельство_о_разводе " +
                    "where id_пользователя = @userId";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@userId", _userId);

                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        DivorceSeries = reader.GetString(0);
                        DivorceNumber = reader.GetString(1);
                        DivorceDateOfGive = reader.GetDateTime(2);
                        DivorceDateOfEntry = reader.GetDateTime(3);
                        DivorceNumberOfEntry = reader.GetString(4);
                        DivorcePlaceOfRegistration = reader.GetString(5);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }

    }
}
