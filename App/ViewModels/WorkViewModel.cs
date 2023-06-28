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
using System.Xml.Linq;

namespace MyApplication.ViewModels
{
    public class WorkViewModel : ViewModel
    {
        private readonly AccountStore _accountStore;

        private int _userId;
        private int _workId;

        public WorkViewModel(AccountStore accountStore, NavigationStore navigationStore)
        {
            _accountStore = accountStore;

            GetWorkers();

            #region Команды

            NavigateToProfileCommand = new NavigateCommand<ProfileViewModel>(new NavigationService<ProfileViewModel>(
                navigationStore, () => new ProfileViewModel(_accountStore, navigationStore)));
            NavigateToDocumentsCommand = new NavigateCommand<DocumentsViewModel>(new NavigationService<DocumentsViewModel>(
                navigationStore, () => new DocumentsViewModel(_accountStore, navigationStore)));
            NavigateToHealthCommand = new NavigateCommand<HealthViewModel>(new NavigationService<HealthViewModel>(
                navigationStore, () => new HealthViewModel(_accountStore, navigationStore)));
            NavigateToFamilyCommand = new NavigateCommand<FamilyViewModel>(new NavigationService<FamilyViewModel>(
                navigationStore, () => new FamilyViewModel(_accountStore, navigationStore)));
            NavigateToAddWorkerCommand = new NavigateCommand<AddWorkerViewModel>(new NavigationService<AddWorkerViewModel>(
                navigationStore, () => new AddWorkerViewModel(_accountStore, navigationStore)));

            DeleteWorkerCommand = new LambdaCommand(OnDeleteWorkerCommandExecuted);

            #endregion
        }

        #region Данные работодателей

        private List<string> _workers = new List<string>();
        public List<string> Workers { get => _workers; set => Set(ref _workers, value); }

        private string _selectedWorker;
        public string SelectedWorker
        {
            get => _selectedWorker;
            set
            {
                _selectedWorker = value;

                MySqlConnection conn = DBUtils.GetDBConnection();
                conn.Open();

                try
                {
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = conn;

                    string sql = "select работодатели.наименование, работодатели.дата_приема_на_работу, " +
                        "работодатели.дата_перевода, работодатели.дата_увольнения, " +
                        "работодатели.должность, работодатели.причина_увольнения from работодатели " +
                        "where работодатели.id_трудовой_книжки = @workId and " +
                        "работодатели.наименование = @name and работодатели.должность = @job;";
                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@workId", _workId);
                    cmd.Parameters.AddWithValue("name", _selectedWorker.Split('/')[0]);
                    cmd.Parameters.AddWithValue("job", _selectedWorker.Split('/')[1]);

                    var reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            WorkerName = reader.GetString(0);
                            if (!reader.IsDBNull(1))
                                WorkerDateOfHire = reader.GetDateTime(1);
                            if (!reader.IsDBNull(2))
                                WorkerDateOfReassignment = reader.GetDateTime(2);
                            if (!reader.IsDBNull(3))
                                WorkerDateOfFire = reader.GetDateTime(3);
                            WorkerJob = reader.GetString(4);
                            if (!reader.IsDBNull(5))
                                WorkerReasonOfFire = reader.GetString(5);
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

        private string _workerName;
        public string WorkerName { get => _workerName; set => Set(ref _workerName, value); }

        private string _workerJob;
        public string WorkerJob { get => _workerJob; set => Set(ref _workerJob, value); }

        private DateTime? _workerDateOfHire;
        public DateTime? WorkerDateOfHire { get => _workerDateOfHire; set => Set(ref _workerDateOfHire, value); }

        private DateTime? _workerDateOfReassignment;
        public DateTime? WorkerDateOfReassignment { get => _workerDateOfReassignment; set => Set(ref _workerDateOfReassignment, value); }

        private DateTime? _workerDateOfFire;
        public DateTime? WorkerDateOfFire { get => _workerDateOfFire; set => Set(ref _workerDateOfFire, value); }

        private string? _workerReasonOfFire;
        public string? WorkerReasonOfFire { get => _workerReasonOfFire; set => Set(ref _workerReasonOfFire, value); }

        #endregion

        #region Команды навигации

        public ICommand NavigateToProfileCommand { get; }

        public ICommand NavigateToDocumentsCommand { get; }
        public ICommand NavigateToHealthCommand { get; }
        public ICommand NavigateToFamilyCommand { get; }
        public ICommand NavigateToAddWorkerCommand { get; }

        #endregion

        #region Команды

        #region Команда удаления выбранного работодателя

        public ICommand DeleteWorkerCommand { get; }

        private void OnDeleteWorkerCommandExecuted(object parameter)
        {
            if (WorkerName == null || WorkerName == "")
            {
                MessageBox.Show("Работодатель не выбран");
                return;
            }
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();

            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;

                string sql = "delete from работодатели " +
                    "where наименование = @name and " +
                    "должность = @job and " +
                    "id_трудовой_книжки = @workId;";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@name", WorkerName);
                cmd.Parameters.AddWithValue("@job", WorkerJob);
                cmd.Parameters.AddWithValue("@workId", _workId);

                cmd.ExecuteNonQuery();

                GetWorkers();
                WorkerName = "";
                WorkerJob = "";
                WorkerDateOfHire = DateTime.Now;
                WorkerDateOfReassignment = DateTime.Now;
                WorkerDateOfFire = DateTime.Now;
                WorkerReasonOfFire = "";
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

        private void GetWorkers()
        {
            Workers.Clear();
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
                cmd.Parameters.Clear();

                sql = "select трудовая_книжка.id from трудовая_книжка " +
                    "where трудовая_книжка.id_пользователя = @userId";
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
                reader.Close();

                sql = "select работодатели.наименование, работодатели.должность from работодатели " +
                    "where работодатели.id_трудовой_книжки = @workId";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@workId", _workId);

                reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        Workers.Add($"{reader.GetString(0)}/{reader.GetString(1)}");
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
