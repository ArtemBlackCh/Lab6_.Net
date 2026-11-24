using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace laba5.Tests
{
    [TestClass]
    public class PatientCatalogTests
    {
        private string _testFilePath = string.Empty;
        private PatientCatalog _catalog = null!;
        [TestInitialize]
        public void TestInitialize()
        {
            _testFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".json");
            _catalog = new PatientCatalog(new JsonFileWriter(), new JsonFileReader(), _testFilePath);
        }
        [TestCleanup]
        public void TestCleanup()
        {
            if (File.Exists(_testFilePath))
            {
                File.Delete(_testFilePath);
            }
        }
        
        private void FillCatalog()
        {
            _catalog.AddPatient(new Patient("Іваненко І.І.", "Грип", DateTime.Today.AddDays(-10), 5));
            _catalog.AddPatient(new Patient("Бондаренко Б.Б.", "Ангіна", DateTime.Today.AddDays(-30), 8));
            _catalog.AddPatient(new Patient("Андрієнко А.А.", "Бронхіт", DateTime.Today.AddDays(-7), 3));
            _catalog.AddPatient(new Patient("Коваленко К.К.", "Застуда", DateTime.Today.AddDays(-2), 15));
        }
        [TestMethod]
        public void AddPatient_AddsPatientToVisibleCollection()
        {
            _catalog.AddPatient(new Patient("Іваненко І.І.", "Грип", DateTime.Today, 5));
            Assert.AreEqual(1, _catalog.Patients.Count);
        }
        [TestMethod]
        public void DeletePatient_RemovesPatientFromCollection()
        {
            var patient = new Patient("Іваненко І.І.", "Грип", DateTime.Today, 5);
            _catalog.AddPatient(patient);
            _catalog.DeletePatient(patient);
            Assert.AreEqual(0, _catalog.Patients.Count);
            
            _catalog.ResetFilter();
            Assert.AreEqual(0, _catalog.Patients.Count);
        }
        [TestMethod]
        public void FilterByDays_ReturnsOnlyPatientsTreatedMoreThanGivenDays()
        {
            FillCatalog();
            _catalog.FilterByDays(7);
            Assert.AreEqual(2, _catalog.Patients.Count);
            Assert.IsTrue(_catalog.Patients.All(p => p.DaysInTreatment > 7));
        }
        [TestMethod]
        public void ResetFilter_RestoresAllPatients()
        {
            FillCatalog();
            _catalog.FilterByDays(7);
            _catalog.ResetFilter();
            Assert.AreEqual(4, _catalog.Patients.Count);
        }
        [TestMethod]
        public void SortByName_OrdersPatientsByFullName()
        {
            FillCatalog();
            _catalog.SortByName();
            var names = _catalog.Patients.Select(p => p.FullName).ToList();
            CollectionAssert.AreEqual(
                new List<string> { "Андрієнко А.А.", "Бондаренко Б.Б.", "Іваненко І.І.", "Коваленко К.К." },
                names);
        }
        
        [TestMethod]
        public void FilterThenSort_ReturnsLongTreatedPatientsSortedByName()
        {
            FillCatalog();
            _catalog.FilterByDays(7);
            _catalog.SortByName();
            var names = _catalog.Patients.Select(p => p.FullName).ToList();
            CollectionAssert.AreEqual(new List<string> { "Бондаренко Б.Б.", "Іваненко І.І." }, names);
        }
        [TestMethod]
        public void SearchByNameOrDisease_FindsByPartOfName()
        {
            FillCatalog();
            _catalog.SearchByNameOrDisease("бонд");
            Assert.AreEqual(1, _catalog.Patients.Count);
            Assert.AreEqual("Бондаренко Б.Б.", _catalog.Patients[0].FullName);
        }
        [TestMethod]
        public void SearchByNameOrDisease_FindsByPartOfDisease()
        {
            FillCatalog();
            _catalog.SearchByNameOrDisease("БРОНХ");
            Assert.AreEqual(1, _catalog.Patients.Count);
            Assert.AreEqual("Андрієнко А.А.", _catalog.Patients[0].FullName);
        }
        [TestMethod]
        public void SearchByNameOrDisease_NothingFound_ReturnsEmptyCollection()
        {
            FillCatalog();
            _catalog.SearchByNameOrDisease("xyz");
            Assert.AreEqual(0, _catalog.Patients.Count);
        }
        [TestMethod]
        public void SearchByNameOrDisease_EmptyQuery_RestoresAllPatients()
        {
            FillCatalog();
            _catalog.SearchByNameOrDisease("грип");
            _catalog.SearchByNameOrDisease("");
            Assert.AreEqual(4, _catalog.Patients.Count);
        }
        [TestMethod]
        public void GetUniqueDiseases_ReturnsDistinctSortedDiseases()
        {
            FillCatalog();
            _catalog.AddPatient(new Patient("Петренко П.П.", "Грип", DateTime.Today, 4));
            var diseases = _catalog.GetUniqueDiseases();
            CollectionAssert.AreEqual(
                new List<string> { "Ангіна", "Бронхіт", "Грип", "Застуда" },
                diseases);
        }
        [TestMethod]
        public void GetUniqueDiseases_EmptyCatalog_ReturnsEmptyList()
        {
            Assert.AreEqual(0, _catalog.GetUniqueDiseases().Count);
        }
        [TestMethod]
        public void GetLatestAdmitted_ReturnsThreeNewestPatients()
        {
            FillCatalog();
            var names = _catalog.GetLatestAdmitted(3).Select(p => p.FullName).ToList();
            CollectionAssert.AreEqual(
                new List<string> { "Коваленко К.К.", "Андрієнко А.А.", "Іваненко І.І." },
                names);
        }
        [TestMethod]
        public void GetLatestAdmitted_FewerPatientsThanCount_ReturnsAll()
        {
            _catalog.AddPatient(new Patient("Іваненко І.І.", "Грип", DateTime.Today, 5));
            Assert.AreEqual(1, _catalog.GetLatestAdmitted(3).Count);
        }
        [TestMethod]
        public void GetAverageWorkExperience_ReturnsAverageValue()
        {
            FillCatalog();
            Assert.AreEqual(7.75, _catalog.GetAverageWorkExperience(), 0.0001);
        }
        [TestMethod]
        public void GetAverageWorkExperience_EmptyCatalog_ReturnsZero()
        {
            Assert.AreEqual(0, _catalog.GetAverageWorkExperience());
        }
        [TestMethod]
        public void GroupByAdmissionYear_GroupsFromNewestToOldest()
        {
            _catalog.AddPatient(new Patient("Іваненко І.І.", "Грип", new DateTime(2022, 3, 1), 5));
            _catalog.AddPatient(new Patient("Бондаренко Б.Б.", "Ангіна", new DateTime(2024, 5, 10), 8));
            _catalog.AddPatient(new Patient("Андрієнко А.А.", "Бронхіт", new DateTime(2022, 7, 15), 3));
            _catalog.AddPatient(new Patient("Коваленко К.К.", "Застуда", new DateTime(2023, 1, 20), 15));
            var groups = _catalog.GroupByAdmissionYear();
            CollectionAssert.AreEqual(new List<int> { 2024, 2023, 2022 }, groups.Select(g => g.Key).ToList());
            Assert.AreEqual(2, groups[2].Count());
        }
        [TestMethod]
        public async Task SaveAsync_ThenLoadAsync_RestoresPatients()
        {
            FillCatalog();
            await _catalog.SaveAsync();
            var newCatalog = new PatientCatalog(new JsonFileWriter(), new JsonFileReader(), _testFilePath);
            await newCatalog.LoadAsync();
            Assert.AreEqual(4, newCatalog.Patients.Count);
        }
    }
}
