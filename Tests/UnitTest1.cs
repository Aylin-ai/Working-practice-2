using MyApplication.ViewModels;
using MyApplication.Infrastructure.Stores;
using MyApplication.Models;

namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void GetUserWithEmailTest()
        {
            AuthorizationViewModel authorizationViewModel = new AuthorizationViewModel(new AccountStore());
            Account expected = new Account();
            Account actual = authorizationViewModel.GetUserWithEmail("+79655974080", "xportbfgh2821@gmail.com");

            Assert.AreEqual(expected.ToString(), actual.ToString());
        }

        [TestMethod]
        public void GetUserTest()
        {
            AuthorizationViewModel authorizationViewModel = new AuthorizationViewModel(new AccountStore());
            Account expected = new Account() { TelephoneNumber = "+79655974080", Password = "12345" };
            Account actual = authorizationViewModel.GetUser("+79655974080", "12345");

            Assert.AreEqual(expected.ToString(), actual.ToString());
        }

        [TestMethod]
        public void GetApplicationStatusTest()
        {
            AccountStore accountStore = new AccountStore();
            accountStore.CurrentAccount = new Account() { TelephoneNumber = "+79655974080", Password = "12345" };
            ApplicationViewModel applicationViewModel = new ApplicationViewModel(accountStore, new NavigationStore());

            string expected = "Заявка находится на рассмотрении";
            string actual;

            applicationViewModel.GetApplicationStatus();
            actual = applicationViewModel.ApplicationStatus;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetPolisTest()
        {
            AccountStore accountStore = new AccountStore();
            accountStore.CurrentAccount = new Account() { TelephoneNumber = "+79655974080", Password = "12345" };
            HealthViewModel healthViewModel = new HealthViewModel(accountStore, new NavigationStore());

            string expected = "1697599729000023";
            string actual;

            healthViewModel.GetPolis();
            actual = healthViewModel.PolisNumber;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void GetUserInformationTest()
        {
            AccountStore accountStore = new AccountStore();
            accountStore.CurrentAccount = new Account() { TelephoneNumber = "+79655974080", Password = "12345" };
            ProfileViewModel profileViewModel = new ProfileViewModel(accountStore, new NavigationStore());

            string expected = "+79655974080";
            string actual;

            profileViewModel.GetUserInformation(accountStore);
            actual = profileViewModel.TelephoneNumber;

            Assert.AreEqual(expected, actual);
        }
    }
}