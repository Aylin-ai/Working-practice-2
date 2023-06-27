using App.Infrastructure.DB;
using MyApplication.Infrastructure.Commands;
using MyApplication.Infrastructure.Stores;
using MyApplication.Services;
using MyApplication.ViewModels.Base;
using MySql.Data.MySqlClient;
using System;
using System.Windows;
using System.Windows.Input;

namespace MyApplication.ViewModels
{
    public class AddChildViewModel : ViewModel
    {
        private readonly AccountStore _accountStore;

        private int _userId;

        public AddChildViewModel(AccountStore accountStore, NavigationStore navigationStore)
        {
            _accountStore = accountStore;

            GetUserId();

            #region Команды

            NavigateToFamilyCommand = new NavigateCommand<FamilyViewModel>(new NavigationService<FamilyViewModel>(
                navigationStore, () => new FamilyViewModel(_accountStore, navigationStore)));

            AddChildCommand = new LambdaCommand(OnAddChildCommandExecuted);

            #endregion
        }

        #region Данные о ребенке

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

        public ICommand NavigateToFamilyCommand { get; }

        #endregion

        #region Команды

        #region Команда добавления ребенка

        public ICommand AddChildCommand { get; }

        private void OnAddChildCommandExecuted(object parameter)
        {
            if (ChildFio == null || ChildSex == null || 
                ChildDateOfBirth.Date.ToString("yyyy-MM-dd") == "0001-01-01" ||
                ChildPlaceOfLive == null || ChildBirthSeries == null ||
                ChildBirthNumber == null ||
                ChildBirthDateOfGive.Date.ToString("yyyy-MM-dd") == "0001-01-01" ||
                ChildDateOfEntry.Date.ToString("yyyy-MM-dd") == "0001-01-01" ||
                ChildNumberOfEntry == null || ChildPlaceOfRegistration == null)
            {
                MessageBox.Show("Введены не все данные");
                return;
            }
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();

            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;

                string sql = "select * from ребенок " +
                    "where ребенок.серия_свидетельства_о_рождении = @birthSeries and " +
                    "ребенок.номер_свидетельства_о_рождении = @birthNumber";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@birthSeries", ChildBirthSeries);
                cmd.Parameters.AddWithValue("@birthNumber", ChildBirthNumber);

                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    MessageBox.Show("Ребенок с таким свидетельством о рождении уже существует");
                    return;
                }
                reader.Close();

                sql = "insert into ребенок (фио, пол, дата_рождения, " +
                    "страна_регистрации_рождения, серия_свидетельства_о_рождении, " +
                    "номер_свидетельства_о_рождении, дата_выдачи_свидетельства, " +
                    "дата_актовой_записи, номер_актовой_записи, " +
                    "место_государственной_регистрации) values " +
                    "(@fio, @sex, @dateOfBirthday, @placeOfLive, " +
                    "@birthSeries, @birthNumber, @birthDateOfGive, " +
                    "@dateOfEntry, @numberOfEntry, @placeOfRegistration);";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@fio", ChildFio);
                cmd.Parameters.AddWithValue("@sex", ChildSex);
                cmd.Parameters.AddWithValue("@dateOfBirthday", ChildDateOfBirth);
                cmd.Parameters.AddWithValue("@placeOfLive", ChildPlaceOfLive);
                cmd.Parameters.AddWithValue("@birthDateOfGive", ChildBirthDateOfGive);
                cmd.Parameters.AddWithValue("@dateOfEntry", ChildDateOfEntry);
                cmd.Parameters.AddWithValue("@numberOfEntry", ChildNumberOfEntry);
                cmd.Parameters.AddWithValue("@placeOfRegistration", ChildPlaceOfRegistration);

                cmd.ExecuteNonQuery();
                reader.Close();

                sql = "select * from ребенок " +
                    "where ребенок.серия_свидетельства_о_рождении = @birthSeries and " +
                    "ребенок.номер_свидетельства_о_рождении = @birthNumber";
                cmd.CommandText = sql;

                reader = cmd.ExecuteReader();

                int childId = 0;

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        childId = reader.GetInt32(0);
                    }
                }
                reader.Close();

                sql = "insert into дети_пользователя values " +
                    "(@userId, @childId)";
                cmd.CommandText = sql;
                cmd.Parameters.Clear();

                cmd.Parameters.AddWithValue("userId", _userId);
                cmd.Parameters.AddWithValue("childId", childId);

                cmd.ExecuteNonQuery();

                NavigateToFamilyCommand.Execute(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }

        #endregion

        #endregion

        private void GetUserId()
        {
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }
    }
}
