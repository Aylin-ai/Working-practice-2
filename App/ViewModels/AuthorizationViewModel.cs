using MyApplication.Infrastructure.Commands;
using MyApplication.Infrastructure.Stores;
using MyApplication.Models;
using MySql.Data.Types;
using System;
using System.Windows.Input;
using System.Windows;
using App.Infrastructure.DB;
using MySql.Data.MySqlClient;
using MyApplication.ViewModels.Base;
using MyApplication.Views.Windows;

namespace MyApplication.ViewModels
{
    public class AuthorizationViewModel : ViewModel
    {
        private AccountStore _accountStore;

        private string[] _sex = new string[2] { "М", "Ж" };
        public string[] Sex { get => _sex; }

        #region Данные пользователя

        private string _telephoneNumber;
        public string TelephoneNumber { get => _telephoneNumber; set => Set(ref _telephoneNumber, value); }

        private string _email;
        public string Email { get => _email; set => Set(ref _email, value); }

        private string _password;
        public string Password { get => _password; set => Set(ref _password, value); }

        private string _fio;
        public string Fio { get => _fio; set => Set(ref _fio, value); }

        private char _selectedSex;
        public char SelectedSex { get => _selectedSex; set => Set(ref _selectedSex, value); }

        private MySqlDateTime _dateOfBirthday;
        public MySqlDateTime DateOfBirthday { get => _dateOfBirthday; set => Set(ref _dateOfBirthday, value); }

        #endregion

        #region Команды

        #region Команда авториации

        public ICommand AuthorizationCommand { get; }

        private void OnAuthorizationCommandExecuted(object parameter)
        {
            if (TelephoneNumber == null || Password == null)
            {
                MessageBox.Show("Вы не ввели все данные");
            }
            else
            {
                Account? currentAccount = GetUser(TelephoneNumber, Password);
                if (currentAccount == null)
                {
                    MessageBox.Show("Неправильный номер телефона или пароль");
                    TelephoneNumber = "";
                    Password = "";
                }
                else
                {
                    _accountStore.CurrentAccount = currentAccount;

                    NavigationStore navigationStore = new NavigationStore();
                    navigationStore.CurrentViewModel = new ProfileViewModel(_accountStore, navigationStore);

                    MainWindow mainWindow = new MainWindow()
                    {
                        DataContext = new MainWindowViewModel(_accountStore, navigationStore)
                    };
                    mainWindow.Show();
                }
            }
        }

        #endregion

        #region Команда регистрации

        public ICommand RegistrationCommand { get; }

        private void OnRegistrationCommandExecuted(object parameter)
        {
            if (TelephoneNumber == null || Email == null || Password == null || 
                Fio == null || SelectedSex == null || DateOfBirthday.IsNull)
            {
                MessageBox.Show("Вы не ввели все данные");
            }
            else
            {
                Account? currentAccount = GetUserWithEmail(TelephoneNumber, Email);
                if (currentAccount != null)
                {
                    MessageBox.Show("Пользователь с таким номером телефона или email уже существует");
                    TelephoneNumber = "";
                    Email = "";
                }
                else
                {
                    Account account = new Account()
                    {
                        TelephoneNumber  = TelephoneNumber,
                        Password = Password,
                    };
                    AddUser();
                    _accountStore.CurrentAccount = account;

                    NavigationStore navigationStore = new NavigationStore();
                    navigationStore.CurrentViewModel = new ProfileViewModel(_accountStore, navigationStore);

                    MainWindow mainWindow = new MainWindow()
                    {
                        DataContext = new MainWindowViewModel(_accountStore, navigationStore)
                    };
                    mainWindow.Show();
                }
            }
        }

        #endregion

        #endregion

        public AuthorizationViewModel(AccountStore accountStore)
        {
            _accountStore = accountStore;
            #region Команды

            AuthorizationCommand = new LambdaCommand(OnAuthorizationCommandExecuted);
            RegistrationCommand = new LambdaCommand(OnRegistrationCommandExecuted);

            #endregion
        }

        private Account GetUser(string number, string password)
        {
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();

            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;

                string sql = "select пользователь.номер_телефона, пользователь.пароль from пользователь " +
                    "where пользователь.номер_телефона = @number and пользователь.пароль = @password ";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@number", number);
                cmd.Parameters.AddWithValue("@password", password);

                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    Account account = new Account();
                    while (reader.Read())
                    {
                        account = new Account
                        {
                            TelephoneNumber = reader.GetString(0),
                            Password = reader.GetString(1)
                        };
                    }
                    return account;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }

        private Account GetUserWithEmail(string number, string email)
        {
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();

            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;

                string sql = "select * from пользователь " +
                    "where пользователь.номер_телефона = @number and пользователь.email = @email ";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@number", number);
                cmd.Parameters.AddWithValue("@email", email);

                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    return new Account();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }

        private void AddUser()
        {
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();

            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;

                string sql = "insert into пользователь (номер_телефона, email, пароль, фио, пол, дата_рождения) " +
                    "values (@number, @email, @password, @fio, @sex, @date)";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@number", TelephoneNumber);
                cmd.Parameters.AddWithValue("@email", Email);
                cmd.Parameters.AddWithValue("@password", Password);
                cmd.Parameters.AddWithValue("@fio", Fio);
                cmd.Parameters.AddWithValue("@sex", SelectedSex);
                cmd.Parameters.AddWithValue("@date", DateOfBirthday);

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
    }
}
