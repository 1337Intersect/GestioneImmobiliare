// Crea questo file come CalendarHelpers.cs nella cartella Models

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ImmobiGestio.Models
{
    /// <summary>
    /// Rappresenta un evento nel calendario con informazioni di display
    /// </summary>
    public class CalendarEventDisplay : INotifyPropertyChanged
    {
        private EventoCalendario _evento = new();
        private int _giornoDelMese;
        private string _coloreEvento = "#0078D4";
        private string _toolTipText = string.Empty;
        private bool _isSelected = false;
        private double _top = 0;
        private double _height = 20;
        private int _colonna = 0;

        public EventoCalendario Evento
        {
            get => _evento;
            set => SetProperty(ref _evento, value);
        }

        public int GiornoDelMese
        {
            get => _giornoDelMese;
            set => SetProperty(ref _giornoDelMese, value);
        }

        public string ColoreEvento
        {
            get => _coloreEvento;
            set => SetProperty(ref _coloreEvento, value);
        }

        public string ToolTipText
        {
            get => _toolTipText;
            set => SetProperty(ref _toolTipText, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        // Proprietà per il posizionamento nel calendario
        public double Top
        {
            get => _top;
            set => SetProperty(ref _top, value);
        }

        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        public int Colonna
        {
            get => _colonna;
            set => SetProperty(ref _colonna, value);
        }

        // Proprietà calcolate
        public string DisplayText
        {
            get
            {
                var text = Evento?.Titolo ?? "";
                if (text.Length > 25)
                    text = text.Substring(0, 22) + "...";
                return text;
            }
        }

        public string TimeText => Evento?.Inizio.ToString("HH:mm") ?? "";

        public string DurationText
        {
            get
            {
                if (Evento == null) return "";
                var duration = Evento.Fine - Evento.Inizio;
                if (duration.TotalDays >= 1)
                    return "Tutto il giorno";
                else if (duration.TotalHours >= 1)
                    return $"{duration.Hours}h {duration.Minutes}m";
                else
                    return $"{duration.Minutes}m";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    /// <summary>
    /// Rappresenta un giorno nel mini calendario
    /// </summary>
    public class MiniCalendarDay : INotifyPropertyChanged
    {
        private DateTime _date;
        private int _day;
        private bool _hasEvents = false;
        private bool _isToday = false;
        private bool _isSelected = false;
        private bool _isCurrentMonth = true;
        private bool _isWeekend = false;
        private int _eventCount = 0;

        public DateTime Date
        {
            get => _date;
            set
            {
                SetProperty(ref _date, value);
                Day = value.Day;
                IsWeekend = value.DayOfWeek == DayOfWeek.Saturday || value.DayOfWeek == DayOfWeek.Sunday;
            }
        }

        public int Day
        {
            get => _day;
            set => SetProperty(ref _day, value);
        }

        public bool HasEvents
        {
            get => _hasEvents;
            set => SetProperty(ref _hasEvents, value);
        }

        public bool IsToday
        {
            get => _isToday;
            set => SetProperty(ref _isToday, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool IsCurrentMonth
        {
            get => _isCurrentMonth;
            set => SetProperty(ref _isCurrentMonth, value);
        }

        public bool IsWeekend
        {
            get => _isWeekend;
            set => SetProperty(ref _isWeekend, value);
        }

        public int EventCount
        {
            get => _eventCount;
            set => SetProperty(ref _eventCount, value);
        }

        // Proprietà calcolate per lo styling
        public string BackgroundColor
        {
            get
            {
                if (IsToday) return "#0078D4";
                if (IsSelected) return "#E3F2FD";
                if (!IsCurrentMonth) return "Transparent";
                return "Transparent";
            }
        }

        public string ForegroundColor
        {
            get
            {
                if (IsToday) return "White";
                if (!IsCurrentMonth) return "#A19F9D";
                if (IsWeekend) return "#605E5C";
                return "#323130";
            }
        }

        public string FontWeight
        {
            get
            {
                if (IsToday) return "Bold";
                if (IsSelected) return "SemiBold";
                return "Normal";
            }
        }

        public string ToolTip
        {
            get
            {
                var tooltip = Date.ToString("dddd, dd MMMM yyyy");
                if (HasEvents)
                {
                    var eventi = EventCount == 1 ? "evento" : "eventi";
                    tooltip += $"\n{EventCount} {eventi}";
                }
                return tooltip;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    /// <summary>
    /// Rappresenta una settimana nel calendario
    /// </summary>
    public class CalendarWeek
    {
        public List<CalendarDay> Days { get; set; } = new();
        public int WeekNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    /// <summary>
    /// Rappresenta un giorno nel calendario principale
    /// </summary>
    public class CalendarDay : INotifyPropertyChanged
    {
        private DateTime _date;
        private List<CalendarEventDisplay> _events = new();
        private bool _isToday = false;
        private bool _isSelected = false;
        private bool _isCurrentMonth = true;
        private bool _hasOverflow = false;

        public DateTime Date
        {
            get => _date;
            set => SetProperty(ref _date, value);
        }

        public List<CalendarEventDisplay> Events
        {
            get => _events;
            set => SetProperty(ref _events, value);
        }

        public bool IsToday
        {
            get => _isToday;
            set => SetProperty(ref _isToday, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool IsCurrentMonth
        {
            get => _isCurrentMonth;
            set => SetProperty(ref _isCurrentMonth, value);
        }

        public bool HasOverflow
        {
            get => _hasOverflow;
            set => SetProperty(ref _hasOverflow, value);
        }

        // Proprietà calcolate
        public int DayNumber => Date.Day;
        public string DayName => Date.ToString("ddd");
        public int EventCount => Events.Count;
        public int VisibleEventCount => Math.Min(Events.Count, 3); // Massimo 3 eventi visibili
        public int OverflowCount => Math.Max(0, Events.Count - 3);

        public string BackgroundColor
        {
            get
            {
                if (IsToday) return "#E3F2FD";
                if (IsSelected) return "#F0F8FF";
                return "White";
            }
        }

        public string BorderColor
        {
            get
            {
                if (IsToday) return "#0078D4";
                if (IsSelected) return "#0078D4";
                return "#E1DFDD";
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    /// <summary>
    /// Helper statico per calcoli del calendario
    /// </summary>
    public static class CalendarHelper
    {
        public static DateTime GetFirstDayOfMonth(DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        public static DateTime GetLastDayOfMonth(DateTime date)
        {
            return GetFirstDayOfMonth(date).AddMonths(1).AddDays(-1);
        }

        public static DateTime GetFirstDayOfWeek(DateTime date, DayOfWeek firstDayOfWeek = DayOfWeek.Monday)
        {
            var dayOfWeek = (int)date.DayOfWeek;
            var firstDay = (int)firstDayOfWeek;

            var daysBack = (dayOfWeek - firstDay + 7) % 7;
            return date.AddDays(-daysBack);
        }

        public static List<DateTime> GetCalendarDays(DateTime date, DayOfWeek firstDayOfWeek = DayOfWeek.Monday)
        {
            var firstDayOfMonth = GetFirstDayOfMonth(date);
            var lastDayOfMonth = GetLastDayOfMonth(date);

            var startDate = GetFirstDayOfWeek(firstDayOfMonth, firstDayOfWeek);
            var endDate = startDate.AddDays(41); // 6 settimane

            var days = new List<DateTime>();
            for (var current = startDate; current <= endDate; current = current.AddDays(1))
            {
                days.Add(current);
            }

            return days;
        }

        public static int GetWeekNumber(DateTime date)
        {
            var jan1 = new DateTime(date.Year, 1, 1);
            var daysOffset = (int)jan1.DayOfWeek - 1;
            var firstWeekStart = jan1.AddDays(-daysOffset);

            var daysDiff = (date - firstWeekStart).Days;
            return (daysDiff / 7) + 1;
        }

        public static string GetMonthYearString(DateTime date)
        {
            return date.ToString("MMMM yyyy");
        }

        public static string GetWeekRangeString(DateTime date)
        {
            var startOfWeek = GetFirstDayOfWeek(date);
            var endOfWeek = startOfWeek.AddDays(6);

            if (startOfWeek.Month == endOfWeek.Month)
            {
                return $"{startOfWeek:dd} - {endOfWeek:dd} {startOfWeek:MMMM yyyy}";
            }
            else if (startOfWeek.Year == endOfWeek.Year)
            {
                return $"{startOfWeek:dd MMM} - {endOfWeek:dd MMM yyyy}";
            }
            else
            {
                return $"{startOfWeek:dd MMM yyyy} - {endOfWeek:dd MMM yyyy}";
            }
        }
    }
}