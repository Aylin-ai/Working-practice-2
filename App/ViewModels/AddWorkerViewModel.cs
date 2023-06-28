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
using System.Windows.Input;

namespace MyApplication.ViewModels
{
    public class AddWorkerViewModel : ViewModel
    {
        private readonly AccountStore _accountStore;

        private int _userId;
        private int _workId;

        public AddWorkerViewModel(AccountStore accountStore, NavigationStore navigationStore)
        {
            _accountStore = accountStore;

            GetWorkId();

            #region Команды

            NavigateToWorkCommand = new NavigateCommand<WorkViewModel>(new NavigationService<WorkViewModel>(
                navigationStore, () => new WorkViewModel(_accountStore, navigationStore)));

            AddWorkerCommand = new LambdaCommand(OnAddWorkerCommandExecuted);

            #endregion
        }

        #region Данные работодателя

        private string _workerName;
        public string WorkerName { get => _workerName; set => Set(ref _workerName, value); }

        private string _workerJob;
        public string WorkerJob { get => _workerJob; set => Set(ref _workerJob, value); }

        private DateTime _workerDateOfHire = DateTime.Now;
        public DateTime WorkerDateOfHire { get => _workerDateOfHire; set => Set(ref _workerDateOfHire, value); }

        private DateTime _workerDateOfReassignment = DateTime.Now;
        public DateTime WorkerDateOfReassignment { get => _workerDateOfReassignment; set => Set(ref _workerDateOfReassignment, value); }

        private DateTime _workerDateOfFire = DateTime.Now;
        public DateTime WorkerDateOfFire { get => _workerDateOfFire; set => Set(ref _workerDateOfFire, value); }

        private string? _workerReasonOfFire;
        public string? WorkerReasonOfFire { get => _workerReasonOfFire; set => Set(ref _workerReasonOfFire, value); }

        #endregion

        #region Команды навигации

        public ICommand NavigateToWorkCommand { get; }

        #endregion

        #region Команды

        #region Команда добавления работодателя

        public ICommand AddWorkerCommand { get; }

        private void OnAddWorkerCommandExecuted(object parameter)
        {
            if (WorkerName == null || WorkerName == "" ||
                WorkerJob == null || WorkerJob == "")
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

                string sql = "select * from работодатели " +
                    "where работодатели.наименование = @name and " +
                    "работодатели.должность = @job and " +
                    "работодатели.id_трудовой_книжки = @workId and " +
                    "(работодатели.дата_приема_на_работу = @dateOfHire or " +
                    "работодатели.дата_перевода = @dateOfReassignment or " +
                    "работодатели.дата_увольнения = @dateOfFire)";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@name", WorkerName);
                cmd.Parameters.AddWithValue("@job", WorkerJob);
                cmd.Parameters.AddWithValue("@workId", _workId);
                cmd.Parameters.AddWithValue("@dateOfHire", WorkerDateOfHire.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@dateOfReassignment", WorkerDateOfReassignment.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("@dateOfFire", WorkerDateOfFire.ToString("yyyy-MM-dd"));

                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    MessageBox.Show("Такой работодатель уже существует");
                    return;
                }
                reader.Close();

                if (WorkerDateOfHire.ToString("yyyy-MM-dd") != DateTime.Now.ToString("yyyy-MM-dd"))
                {
                    sql = "insert into работодатели (наименование, дата_приема_на_работу, должность, id_трудовой_книжки) " +
                        "values (@name, @dateOfHire, @job, @workId);";
                }
                else if (WorkerDateOfReassignment.ToString("yyyy-MM-dd") != DateTime.Now.ToString("yyyy-MM-dd"))
                {
                    sql = "insert into работодатели (наименование, дата_перевода, должность, id_трудовой_книжки) " +
                        "values (@name, @dateOfReassignment, @job, @workId);";
                }
                else if (WorkerDateOfFire.ToString("yyyy-MM-dd") != DateTime.Now.ToString("yyyy-MM-dd"))
                {
                    if (WorkerReasonOfFire == null || WorkerReasonOfFire == "")
                    {
                        MessageBox.Show("Не введена причина увольнения");
                        return;
                    }
                    sql = "insert into работодатели (наименование, дата_увольнения, должность, причина_увольнения, id_трудовой_книжки) " +
                        "values (@name, @dateOfFire, @job, @reasonOfFire, @workId);";
                }

                cmd.CommandText = sql;

                cmd.ExecuteNonQuery();
                reader.Close();

                NavigateToWorkCommand.Execute(null);
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

        private void GetWorkId()
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

                sql = "select * from трудовая_книжка where " +
                    "id_пользователя = @userId";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@userId", _userId);


                reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        _workId = reader.GetInt32(0);
                    }
                }
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
