using App.Infrastructure.DB;
using MyApplication.Infrastructure.Commands;
using MyApplication.Infrastructure.Stores;
using MyApplication.Services;
using MyApplication.ViewModels.Base;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyApplication.ViewModels
{
    public class ApplicationViewModel : ViewModel
    {
        private readonly AccountStore _accountStore;

        private int _userId;

        public ApplicationViewModel(AccountStore accountStore, NavigationStore navigationStore)
        {
            _accountStore = accountStore;

            GetApplicationStatus();

            #region Команды

            NavigateToDocumentsCommand = new NavigateCommand<DocumentsViewModel>(new NavigationService<DocumentsViewModel>(
                navigationStore, () => new DocumentsViewModel(_accountStore, navigationStore)));
            NavigateToProfileCommand = new NavigateCommand<ProfileViewModel>(new NavigationService<ProfileViewModel>(
                navigationStore, () => new ProfileViewModel(_accountStore, navigationStore)));

            AddApplicationCommand = new LambdaCommand(OnAddApplicationCommandExecuted);

            #endregion
        }

        #region Данные заявки

        private string _applicationStatus;
        public string ApplicationStatus { get => _applicationStatus; set => Set(ref _applicationStatus, value); }

        #endregion

        #region Команды навигации

        public ICommand NavigateToDocumentsCommand { get; }
        public ICommand NavigateToProfileCommand { get; }

        #endregion

        #region Команды

        #region Команда подачи заявки на пенсию

        public ICommand AddApplicationCommand { get; }

        private void OnAddApplicationCommandExecuted(object parameter)
        {
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();

            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;

                string sql = "select * from статус_заявки where id_пользователя = @userId;";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@userId", _userId);

                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    MessageBox.Show("Заявка уже подана");
                    return;
                }
                reader.Close();

                sql = "insert into статус_заявки (статус, id_пользователя) " +
                    "values (@status, @userId)";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@status", "Заявка находится на рассмотрении");

                cmd.ExecuteNonQuery();
                GetApplicationStatus();
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

        public void GetApplicationStatus()
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

                sql = "select * from статус_заявки where id_пользователя = @userId";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@userId", _userId);

                reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        ApplicationStatus = reader.GetString(1);
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
