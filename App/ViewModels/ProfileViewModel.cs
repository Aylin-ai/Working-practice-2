using MyApplication.Infrastructure.Commands;
using App.Infrastructure.DB;
using MyApplication.Infrastructure.Stores;
using MyApplication.Services;
using MyApplication.ViewModels.Base;
using MySql.Data.MySqlClient;
using System;
using System.Windows.Input;
using System.Windows;

namespace MyApplication.ViewModels
{
    public class ProfileViewModel : ViewModel
    {
        private readonly AccountStore _accountStore;

        public ProfileViewModel(AccountStore accountStore, NavigationStore navigationStore)
        {
            _accountStore = accountStore;
            GetUserInformation(_accountStore);

            #region Команды

            NavigateToDocumentsCommand = new NavigateCommand<DocumentsViewModel>(new NavigationService<DocumentsViewModel>(
                navigationStore, () => new DocumentsViewModel(_accountStore, navigationStore)));
            NavigateToApplicationCommand = new NavigateCommand<ApplicationViewModel>(new NavigationService<ApplicationViewModel>(
                navigationStore, () => new ApplicationViewModel(_accountStore, navigationStore)));

            ChangeNumberCommand = new LambdaCommand(OnChangeNumberCommandExecuted);
            ChangeEmailCommand = new LambdaCommand(OnChangeEmailCommandExecuted);
            ChangePasswordCommand = new LambdaCommand(OnChangePasswordCommandExecuted);

            DeleteAccountCommand = new LambdaCommand(OnDeleteAccountCommandExecuted);

            #endregion
        }

        #region Данные пользователя

        private string _telephoneNumber;
        public string TelephoneNumber { get => _telephoneNumber; set => Set(ref _telephoneNumber, value); }

        private string _email;
        public string Email { get => _email; set => Set(ref _email, value); }

        private string _password;
        public string Password { get => _password; set => Set(ref _password, value); }

        #endregion

        #region Команды навигации

        public ICommand NavigateToDocumentsCommand { get; }
        public ICommand NavigateToApplicationCommand { get; }

        #endregion

        #region Команды

        #region Изменить номер телефона

        public ICommand ChangeNumberCommand { get; }

        private void OnChangeNumberCommandExecuted(object parameter)
        {
            if (TelephoneNumber == _accountStore.CurrentAccount.TelephoneNumber)
            {
                MessageBox.Show("Номера телефонов совпадают");
                return;
            }
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();

            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;

                string sql = "update пользователь " +
                    "set пользователь.номер_телефона = @newNumber " +
                    "where пользователь.номер_телефона = @oldNumber";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@oldNumber", _accountStore.CurrentAccount.TelephoneNumber);
                cmd.Parameters.AddWithValue("@newNumber", TelephoneNumber);

                cmd.ExecuteNonQuery();

                _accountStore.CurrentAccount.TelephoneNumber = TelephoneNumber;
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

        #region Изменить email

        public ICommand ChangeEmailCommand { get; }

        private void OnChangeEmailCommandExecuted(object parameter)
        {
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();

            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;

                string sql = "select пользователь.email from пользователь " +
                    "where пользователь.номер_телефона = @number";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@number", _accountStore.CurrentAccount.TelephoneNumber);

                string oldEmail = "";
                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        oldEmail = reader.GetString(0);
                    }
                }
                reader.Close();

                if (Email == oldEmail)
                {
                    MessageBox.Show("Email совпадают");
                    return;
                }

                sql = "update пользователь " +
                    "set пользователь.email = @newEmail " +
                    "where пользователь.номер_телефона = @number";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@newEmail", Email);

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

        #region Изменить пароль

        public ICommand ChangePasswordCommand { get; }

        private void OnChangePasswordCommandExecuted(object parameter)
        {
            if (Password == _accountStore.CurrentAccount.Password)
            {
                MessageBox.Show("Пароли совпадают");
                return;
            }
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();

            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;

                string sql = "update пользователь " +
                    "set пользователь.пароль = @newPassword " +
                    "where пользователь.номер_телефона = @number";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@number", _accountStore.CurrentAccount.TelephoneNumber);
                cmd.Parameters.AddWithValue("@newPassword", Password);

                cmd.ExecuteNonQuery();

                _accountStore.CurrentAccount.Password = Password;
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

        #region Удалить аккаунт

        public ICommand DeleteAccountCommand { get; }

        private void OnDeleteAccountCommandExecuted(object parameter)
        {
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();

            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;

                string sql = "delete from пользователь " +
                    "where пользователь.номер_телефона = @number";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@number", _accountStore.CurrentAccount.TelephoneNumber);

                cmd.ExecuteNonQuery();
                Application.Current.Shutdown();
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

        private void GetUserInformation(AccountStore accountStore)
        {
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();

            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;

                string sql = "select пользователь.номер_телефона, пользователь.пароль, пользователь.email from пользователь " +
                    "where пользователь.номер_телефона = @number and пользователь.пароль = @password ";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@number", accountStore.CurrentAccount.TelephoneNumber);
                cmd.Parameters.AddWithValue("@password", accountStore.CurrentAccount.Password);

                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        TelephoneNumber = reader.GetString(0);
                        Password = reader.GetString(1);
                        Email = reader.GetString(2);
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
