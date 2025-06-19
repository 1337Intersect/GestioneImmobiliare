using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ImmobiGestio.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void NotifyPropertyChanged(string propertyName)
        {
            OnPropertyChanged(propertyName);
        }

        // Validation support
        private readonly Dictionary<string, List<string>> _validationErrors = new();

        public bool HasErrors => _validationErrors.Count > 0;

        public void AddValidationError(string propertyName, string error)
        {
            if (!_validationErrors.ContainsKey(propertyName))
                _validationErrors[propertyName] = new List<string>();

            if (!_validationErrors[propertyName].Contains(error))
                _validationErrors[propertyName].Add(error);
        }

        public void ClearValidationErrors(string propertyName)
        {
            if (_validationErrors.ContainsKey(propertyName))
                _validationErrors.Remove(propertyName);
        }

        public void ClearAllValidationErrors()
        {
            _validationErrors.Clear();
        }

        public List<string> GetValidationErrors(string propertyName)
        {
            return _validationErrors.ContainsKey(propertyName)
                ? _validationErrors[propertyName]
                : new List<string>();
        }
    }
}