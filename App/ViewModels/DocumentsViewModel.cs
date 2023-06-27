using App.Infrastructure.DB;
using MyApplication.Infrastructure.Commands;
using MyApplication.Infrastructure.Stores;
using MyApplication.Services;
using MyApplication.ViewModels.Base;
using MySql.Data.MySqlClient;
using System;
using System.Windows.Input;

namespace MyApplication.ViewModels
{
    public class DocumentsViewModel : ViewModel
    {
        private readonly AccountStore _accountStore;

        public DocumentsViewModel(AccountStore accountStore, NavigationStore navigationStore)
        {
            _accountStore = accountStore;

            GetPassport();
            GetMilitary();
            GetInn();
            GetSnils();

            #region Команды

            NavigateToProfileCommand = new NavigateCommand<ProfileViewModel>(new NavigationService<ProfileViewModel>(
                navigationStore, () => new ProfileViewModel(_accountStore, navigationStore)));
            NavigateToFamilyCommand = new NavigateCommand<FamilyViewModel>(new NavigationService<FamilyViewModel>(
                navigationStore, () => new FamilyViewModel(_accountStore, navigationStore)));
            NavigateToHealthCommand = new NavigateCommand<HealthViewModel>(new NavigationService<HealthViewModel>(
                navigationStore, () => new HealthViewModel(_accountStore, navigationStore)));
            NavigateToWorkCommand = new NavigateCommand<WorkViewModel>(new NavigationService<WorkViewModel>(
                navigationStore, () => new WorkViewModel(_accountStore, navigationStore)));

            ChangePassportCommand = new LambdaCommand(OnChangePassportCommandExecuted);
            ChangeMilitaryCommand = new LambdaCommand(OnChangeMilitaryCommandExecuted);
            ChangeSnilsCommand = new LambdaCommand(OnChangeSnilsCommandExecuted);
            ChangeInnCommand = new LambdaCommand(OnChangeInnCommandExecuted);

            #endregion
        }

        #region Данные паспорта

        private string _passportSeries;
        public string PassportSeries { get => _passportSeries; set => Set(ref _passportSeries, value); }

        private string _passportNumber;
        public string PassportNumber { get => _passportNumber; set => Set(ref _passportNumber, value); }

        private string _passportGiven;
        public string PassportGiven { get => _passportGiven; set => Set(ref _passportGiven, value); }

        private string _passportUnitCode;
        public string PassportUnitCode { get => _passportUnitCode; set => Set(ref _passportUnitCode, value); }

        private DateTime _passportDateOfGive;
        public DateTime PassportDateOfGive { get => _passportDateOfGive; set => Set(ref _passportDateOfGive, value); }

        private string _passportPlaceOfBirth;
        public string PassportPlaceOfBirth { get => _passportPlaceOfBirth; set => Set(ref _passportPlaceOfBirth, value); }

        private string _passportNationality;
        public string PassportNationality { get => _passportNationality; set => Set(ref _passportNationality, value); }

        #endregion

        #region Данные СНИЛС

        private string _snilsNumber;
        public string SnilsNumber { get => _snilsNumber; set => Set(ref _snilsNumber, value); }

        #endregion

        #region Данные военного билета

        private string _militarySeries;
        public string MilitarySeries { get => _militarySeries; set => Set(ref _militarySeries, value); }

        private string _militaryNumber;
        public string MilitaryNumber { get => _militaryNumber; set => Set(ref _militaryNumber, value); }

        private string _militaryGiven;
        public string MilitaryGiven { get => _militaryGiven; set => Set(ref _militaryGiven, value); }

        private DateTime _militaryDateOfGive;
        public DateTime MilitaryDateOfGive { get => _militaryDateOfGive; set => Set(ref _militaryDateOfGive, value); }

        #endregion

        #region Данные ИНН

        private string _innNumber;
        public string InnNumber { get => _innNumber; set => Set(ref _innNumber, value); }

        #endregion

        #region Команды навигации

        public ICommand NavigateToProfileCommand { get; }

        public ICommand NavigateToFamilyCommand { get; }
        public ICommand NavigateToHealthCommand { get; }
        public ICommand NavigateToWorkCommand { get; }

        #endregion

        #region Команды

        #region Команда изменения данных паспорта

        public ICommand ChangePassportCommand { get; }

        private void OnChangePassportCommandExecuted(object parameter)
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

                int userId = 0;

                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        userId = reader.GetInt32(0);
                    }
                }
                reader.Close();
                cmd.Parameters.Clear();

                sql = "select * from паспорт " +
                    "where паспорт.id_пользователя = @userId";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@userId", userId);

                reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    sql = "update паспорт " +
                        "set паспорт.серия = @series, " +
                        "паспорт.номер = @number, " +
                        "паспорт.гражданство = @nationality, " +
                        "паспорт.место_рождения = @placeOfBirth, " +
                        "паспорт.кем_выдан = @given, " +
                        "паспорт.дата_выдачи = @dateOfGive, " +
                        "паспорт.код_подразделения = @unitCode " +
                        "where паспорт.id_пользователя = @userId";
                }
                else
                {
                    sql = "insert into паспорт " +
                        "values (@series, @number " +
                        "@nationality, @placeOfBirth " +
                        "@given, @dateOfGive, " +
                        "@unitCode, @userId)";
                }
                reader.Close();

                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@series", PassportSeries);
                cmd.Parameters.AddWithValue("@number", PassportNumber);
                cmd.Parameters.AddWithValue("@nationality", PassportNationality);
                cmd.Parameters.AddWithValue("@placeOfBirth", PassportPlaceOfBirth);
                cmd.Parameters.AddWithValue("@given", PassportGiven);
                cmd.Parameters.AddWithValue("@dateOfGive", PassportDateOfGive.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@unitCode", PassportUnitCode);

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

        #region Команда изменения данных военного билета

        public ICommand ChangeMilitaryCommand { get; }

        private void OnChangeMilitaryCommandExecuted(object parameter)
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

                int userId = 0;

                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        userId = reader.GetInt32(0);
                    }
                }
                reader.Close();
                cmd.Parameters.Clear();

                sql = "select * from военный_билет " +
                    "where военный_билет.id_пользователя = @userId";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@userId", userId);

                reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    sql = "update военный_билет " +
                        "set военный_билет.серия = @series, " +
                        "военный_билет.номер = @number, " +
                        "военный_билет.кем_выдан = @given, " +
                        "военный_билет.дата_выдачи = @dateOfGive " +
                        "where военный_билет.id_пользователя = @userId";
                }
                else
                {
                    sql = "insert into военный_билет " +
                        "values (@series, @number " +
                        "@given, @dateOfGive, " +
                        "@userId)";
                }
                reader.Close();

                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@series", MilitarySeries);
                cmd.Parameters.AddWithValue("@number", MilitaryNumber);
                cmd.Parameters.AddWithValue("@given", MilitaryGiven);
                cmd.Parameters.AddWithValue("@dateOfGive", MilitaryDateOfGive.ToString("yyyy-MM-dd"));

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

        #region Команда измнения данных ИНН

        public ICommand ChangeInnCommand { get; }

        private void OnChangeInnCommandExecuted(object parameter)
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

                int userId = 0;

                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        userId = reader.GetInt32(0);
                    }
                }
                reader.Close();
                cmd.Parameters.Clear();

                sql = "select * from инн " +
                    "where инн.id_пользователя = @userId";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@userId", userId);

                reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    sql = "update инн " +
                        "set инн.номер = @number " +
                        "where инн.id_пользователя = @userId";
                }
                else
                {
                    sql = "insert into инн " +
                        "values (@number, " +
                        "@userId)";
                }
                reader.Close();

                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@number", InnNumber);

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

        #region Команда изменения данных СНИЛС

        public ICommand ChangeSnilsCommand { get; }

        private void OnChangeSnilsCommandExecuted(object parameter)
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

                int userId = 0;

                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        userId = reader.GetInt32(0);
                    }
                }
                reader.Close();
                cmd.Parameters.Clear();

                sql = "select * from инн " +
                    "where инн.id_пользователя = @userId";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@userId", userId);

                reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    sql = "update снилс " +
                        "set снилс.номер = @number " +
                        "where снилс.id_пользователя = @userId";
                }
                else
                {
                    sql = "insert into снилс " +
                        "values (@number, " +
                        "@userId)";
                }
                reader.Close();

                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@number", SnilsNumber);

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

        #endregion

        private void GetPassport()
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

                int userId = 0;

                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        userId = reader.GetInt32(0);
                    }
                }
                reader.Close();

                sql = "select * from паспорт " +
                    "where паспорт.id_пользователя = @userId";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@userId", userId);

                reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        PassportSeries = reader.GetString(0);
                        PassportNumber = reader.GetString(1);
                        PassportNationality = reader.GetString(2);
                        PassportPlaceOfBirth = reader.GetString(3);
                        PassportGiven = reader.GetString(4);
                        PassportDateOfGive = reader.GetDateTime(5);
                        PassportUnitCode = reader.GetString(6);
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

        private void GetMilitary()
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

                int userId = 0;

                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        userId = reader.GetInt32(0);
                    }
                }
                reader.Close();

                sql = "select * from военный_билет " +
                    "where военный_билет.id_пользователя = @userId";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@userId", userId);

                reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        MilitarySeries = reader.GetString(0);
                        MilitaryNumber = reader.GetString(1);
                        MilitaryGiven = reader.GetString(2);
                        MilitaryDateOfGive = reader.GetDateTime(3);
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

        private void GetInn()
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

                int userId = 0;

                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        userId = reader.GetInt32(0);
                    }
                }
                reader.Close();

                sql = "select * from инн " +
                    "where инн.id_пользователя = @userId";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@userId", userId);

                reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        InnNumber = reader.GetString(0);
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

        private void GetSnils()
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

                int userId = 0;

                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        userId = reader.GetInt32(0);
                    }
                }
                reader.Close();

                sql = "select * from снилс " +
                    "where снилс.id_пользователя = @userId";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@userId", userId);

                reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        SnilsNumber = reader.GetString(0);
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
