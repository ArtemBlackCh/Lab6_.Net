using System.Collections.ObjectModel;

namespace laba5
{
    public class PatientCatalog
    {
        private readonly IFileSaver _fileSaver;
        private readonly IFileLoader _fileLoader;
        private readonly string _filePath;
        public ObservableCollection<Patient> Patients { get; }
        private List<Patient> _allPatients;

        public PatientCatalog(IFileSaver fileSaver, IFileLoader fileLoader, string filePath)
        {
            _fileSaver = fileSaver;
            _fileLoader = fileLoader;
            _filePath = filePath;
            Patients = new ObservableCollection<Patient>();
            _allPatients = new List<Patient>();
        }

        public void AddPatient(Patient patient)
        {
            Patients.Add(patient);
            _allPatients.Add(patient);
        }

        public void DeletePatient(Patient patient)
        {
            Patients.Remove(patient);
            _allPatients.Remove(patient);
        }
        public void SortByName()
        {
            var sortedPatients = Patients.OrderBy(p => p, new PatientNameComparer()).ToList();
            UpdateVisibleCollection(sortedPatients);
        }

        public void FilterByDays(int days)
        {
            var filtered = _allPatients.Where(p => p.DaysInTreatment > days).ToList();
            UpdateVisibleCollection(filtered);
        }

        public void ResetFilter()
        {
            UpdateVisibleCollection(_allPatients);
        }

        public void SearchByNameOrDisease(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                ResetFilter();
                return;
            }
            var text = query.Trim();
            var found = _allPatients
                .Where(p => p.FullName.Contains(text, StringComparison.OrdinalIgnoreCase)
                         || p.Disease.Contains(text, StringComparison.OrdinalIgnoreCase))
                .ToList();
            UpdateVisibleCollection(found);
        }

        public List<string> GetUniqueDiseases()
        {
            return _allPatients
                .Select(p => p.Disease)
                .Distinct()
                .OrderBy(d => d)
                .ToList();
        }

        public List<Patient> GetLatestAdmitted(int count = 3)
        {
            return _allPatients
                .OrderByDescending(p => p.AdmissionDate)
                .Take(count)
                .ToList();
        }

        public double GetAverageWorkExperience()
        {
            if (!_allPatients.Any())
            {
                return 0;
            }
            return _allPatients.Average(p => p.WorkExperience);
        }

        public List<IGrouping<int, Patient>> GroupByAdmissionYear()
        {
            return _allPatients
                .GroupBy(p => p.AdmissionDate.Year)
                .OrderByDescending(g => g.Key)
                .ToList();
        }

        private void UpdateVisibleCollection(List<Patient> newList)
        {
            Patients.Clear();
            foreach (var patient in newList)
            {
                Patients.Add(patient);
            }
        }

        public async Task SaveAsync()
        {
            await _fileSaver.SaveAsync(_filePath, _allPatients);
        }

        public async Task LoadAsync()
        {
            var loadedPatients = await _fileLoader.LoadAsync<Patient>(_filePath);
            if (loadedPatients != null)
            {
                _allPatients = loadedPatients;
                UpdateVisibleCollection(_allPatients);
            }
        }
    }
}
