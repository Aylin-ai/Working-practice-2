using App.Infrastructure.DB;
using MyApplication.Infrastructure.Commands;
using MyApplication.Infrastructure.Stores;
using MyApplication.Services;
using MyApplication.ViewModels.Base;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyApplication.ViewModels
{
    public class HealthViewModel : ViewModel
    {
        private readonly AccountStore _accountStore;

        private int _userId;

        public HealthViewModel(AccountStore accountStore, NavigationStore navigationStore)
        {
            _accountStore = accountStore;

            GetPolis();

            #region Команды

            NavigateToProfileCommand = new NavigateCommand<ProfileViewModel>(new NavigationService<ProfileViewModel>(
    navigationStore, () => new ProfileViewModel(_accountStore, navigationStore)));
            NavigateToDocumentsCommand = new NavigateCommand<DocumentsViewModel>(new NavigationService<DocumentsViewModel>(
                navigationStore, () => new DocumentsViewModel(_accountStore, navigationStore)));
            NavigateToFamilyCommand = new NavigateCommand<FamilyViewModel>(new NavigationService<FamilyViewModel>(
                navigationStore, () => new FamilyViewModel(_accountStore, navigationStore)));
            NavigateToWorkCommand = new NavigateCommand<WorkViewModel>(new NavigationService<WorkViewModel>(
                navigationStore, () => new WorkViewModel(_accountStore, navigationStore)));

            ChangePolisCommand = new LambdaCommand(OnChangePolisCommandExecuted);

            #endregion
        }

        #region Данные Полиса ОМС

        private string _polisNumber;
        public string PolisNumber { get => _polisNumber; set => Set(ref _polisNumber, value); }

        #endregion

        #region Команды навигации

        public ICommand NavigateToProfileCommand { get; }

        public ICommand NavigateToDocumentsCommand { get; }
        public ICommand NavigateToFamilyCommand { get; }
        public ICommand NavigateToWorkCommand { get; }

        #endregion

        #region Команды

        #region Команда изменения данных Полиса ОМС

        public ICommand ChangePolisCommand { get; }

        private void OnChangePolisCommandExecuted(object parameter)
        {
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();

            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;

                string sql = "select * from полис_омс " +
                    "where полис_омс.id_пользователя = @userId";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@userId", _userId);

                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    sql = "update полис_омс " +
                        "set полис_омс.номер = @number " +
                        "where полис_омс.id_пользователя = @userId";
                }
                else
                {
                    sql = "insert into полис_омс " +
                        "values (@number, " +
                        "@userId)";
                }
                reader.Close();

                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@number", PolisNumber);

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

        private void GetPolis()
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

                sql = "select * from полис_омс " +
                    "where полис_омс.id_пользователя = @userId";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@userId", _userId);

                reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        PolisNumber = reader.GetString(0);
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
