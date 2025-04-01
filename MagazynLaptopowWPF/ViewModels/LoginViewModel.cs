using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using MagazynLaptopowWPF.ViewModels;
using PCSC; // Główna przestrzeń nazw dla PCSC, zawiera RemovedReaderException, PCSCException, SCardError etc.
// using PCSC.Exceptions; // Ten using prawdopodobnie nie jest potrzebny, jeśli nie używasz innych wyjątków stamtąd
using PCSC.Monitoring; // Dla klas monitorowania
using MahApps.Metro.IconPacks;
using PCSC.Exceptions;

namespace MagazynLaptopowWPF.ViewModels
{
    public class LoginViewModel : BaseViewModel, IDisposable
    {
        private ISCardContext? _context;
        private ISCardMonitor? _monitor;
        private string? _targetReaderName;
        private readonly byte[] _targetAtr = new byte[] {
            0x3B, 0x6B, 0x00, 0x00, 0x00, 0x31, 0xC1, 0x64, 0x08, 0x60, 0x32, 0x1F, 0x0F, 0x90, 0x00
        };
        private const string TargetReaderNamePart = "Identiv uTrust 2700 F";

        private string _statusMessage = "Inicjalizowanie...";
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private string _readerStatusText = "Stan czytnika: Nieznany";
        public string ReaderStatusText
        {
            get => _readerStatusText;
            set => SetProperty(ref _readerStatusText, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private bool _isReaderConnected;
        public bool IsReaderConnected
        {
            get => _isReaderConnected;
            set
            {
                if (SetProperty(ref _isReaderConnected, value))
                {
                    OnPropertyChanged(nameof(StatusIconKind));
                    OnPropertyChanged(nameof(StatusIconColor));
                    UpdateReaderStatusText();
                }
            }
        }

        private bool _isCardInserted;
        public bool IsCardInserted
        {
            get => _isCardInserted;
            set
            {
                if (SetProperty(ref _isCardInserted, value))
                {
                    OnPropertyChanged(nameof(StatusIconKind));
                    OnPropertyChanged(nameof(StatusIconColor));
                }
            }
        }

        // Poprawione nazwy ikon dla MahApps
        public PackIconMaterialKind StatusIconKind
        {
            get
            {
                if (!IsReaderConnected) return PackIconMaterialKind.CreditCardOffOutline;
                if (IsCardInserted) return PackIconMaterialKind.CreditCardChipOutline;
                // return PackIconMaterialKind.CreditCardReaderOutline; // Błędna nazwa
                return PackIconMaterialKind.CreditCardOutline; // Użyj tej lub innej dostępnej
            }
        }

        public Brush StatusIconColor
        {
            get
            {
                if (!IsReaderConnected) return Brushes.Gray;
                return (Brush)Application.Current.Resources["MahApps.Brushes.Accent"];
            }
        }

        public event EventHandler<LoginSuccessEventArgs>? LoginSuccess;

        // W pliku LoginViewModel.cs

        public void InitializeMonitoring()
        {
            try
            {
                _context = ContextFactory.Instance.Establish(SCardScope.System);
                _monitor = new SCardMonitor(ContextFactory.Instance, SCardScope.System);
                AttachMonitorEvents();

                // Najpierw sprawdź status, aby upewnić się, że usługa działa
                // i ewentualnie odnaleźć _targetReaderName
                CheckReaderStatus();

                // Dopiero teraz uruchom monitorowanie - użyjemy _targetReaderName, jeśli został znaleziony,
                // lub null/pustej tablicy, jeśli go nie ma (aby monitorować podłączenie)
                if (_monitor != null) // Dodatkowe sprawdzenie, czy monitor został utworzony
                {
                    // Jeśli znaleźliśmy nasz czytnik, monitoruj tylko jego
                    if (!string.IsNullOrEmpty(_targetReaderName))
                    {
                        _monitor.Start(_targetReaderName);
                        System.Diagnostics.Debug.WriteLine($"Monitor started for specific reader: {_targetReaderName}");
                    }
                    // Jeśli nie znaleźliśmy (lub wystąpił błąd w CheckReaderStatus),
                    // spróbuj monitorować wszystkie systemowe (może zadziała w nowszych PCSC)
                    // lub nie uruchamiaj wcale, jeśli to powoduje błąd.
                    // Testowo: spróbuj monitorować wszystkie
                    else
                    {
                        try
                        {
                            // Spróbuj monitorować wszystkie, może się udać jeśli czytnik zostanie podłączony później
                            _monitor.Start((string[]?)null); // lub new string[0]
                            System.Diagnostics.Debug.WriteLine("Monitor started for all system readers (target not found initially).");
                        }
                        catch (Exception startEx)
                        {
                            // Jeśli startowanie z null/pustą tablicą też daje błąd, złap go
                            StatusMessage = $"Nie można uruchomić monitorowania: {startEx.Message}";
                            System.Diagnostics.Debug.WriteLine($"Error starting monitor with null/empty: {startEx}");
                            // W tym wypadku monitor nie będzie działał, użytkownik będzie musiał
                            // zrestartować aplikację po podłączeniu czytnika.
                        }
                    }
                }
            }
            catch (PCSCException pcscEx)
            {
                StatusMessage = $"Błąd inicjalizacji PC/SC: {pcscEx.Message}\nKod: {pcscEx.SCardError}";
                ReaderStatusText = "Stan czytnika: Błąd inicjalizacji";
                IsReaderConnected = false;
                System.Diagnostics.Debug.WriteLine($"PCSC Init Error: {pcscEx}");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Błąd inicjalizacji: {ex.Message}";
                ReaderStatusText = "Stan czytnika: Błąd inicjalizacji";
                IsReaderConnected = false;
                System.Diagnostics.Debug.WriteLine($"General Init Error: {ex}");
            }
        }

        private void AttachMonitorEvents()
        {
            if (_monitor == null) return;
            // Poprawione subskrypcje z prawidłowymi sygnaturami
            _monitor.StatusChanged += Monitor_StatusChanged;
            _monitor.CardInserted += Monitor_CardInserted;
            _monitor.CardRemoved += Monitor_CardRemoved;
            _monitor.MonitorException += Monitor_MonitorException;
        }

        private void DetachMonitorEvents()
        {
            if (_monitor == null) return;
            // Poprawione odsubskrybowanie
            _monitor.StatusChanged -= Monitor_StatusChanged;
            _monitor.CardInserted -= Monitor_CardInserted;
            _monitor.CardRemoved -= Monitor_CardRemoved;
            _monitor.MonitorException -= Monitor_MonitorException;
        }

        private void CheckReaderStatus()
        {
            if (_context == null) return;
            string[] readerNames; // Zmienna lokalna

            try
            {
                readerNames = _context.GetReaders(); // Pobierz czytniki wewnątrz try
                _targetReaderName = readerNames.FirstOrDefault(r => r.Contains(TargetReaderNamePart));
                IsReaderConnected = !string.IsNullOrEmpty(_targetReaderName);

                if (IsReaderConnected)
                {
                    try
                    {
                        // Użyj zmiennej lokalnej _targetReaderName
                        using var readerStatus = _context.GetReaderStatus(_targetReaderName!);
                        IsCardInserted = readerStatus.Atr != null && readerStatus.Atr.Length > 0;
                        StatusMessage = IsCardInserted ? "Wykryto kartę. Sprawdzanie..." : "Czytnik gotowy. Włóż kartę administratora.";
                        if (IsCardInserted)
                        {
                            VerifyCard(readerStatus.Atr);
                        }
                    }
                    // Zamiast: catch (RemovedReaderException)
                    catch (PCSCException pex) when (pex.SCardError == SCardError.ReaderUnavailable) // Sprawdź kod błędu
                    {
                        IsReaderConnected = false;
                        IsCardInserted = false;
                        StatusMessage = "Czytnik odłączony. Proszę podłączyć czytnik.";
                    }
                    catch (RemovedCardException) // Ten wyjątek zazwyczaj istnieje
                    {
                        IsCardInserted = false;
                        StatusMessage = "Czytnik gotowy. Włóż kartę administratora.";
                    }
                    catch (PCSCException pex) when (pex.SCardError == SCardError.NoSmartcard || pex.SCardError == SCardError.RemovedCard)
                    {
                        IsCardInserted = false;
                        StatusMessage = "Czytnik gotowy. Włóż kartę administratora.";
                    }
                    catch (Exception ex) // Złap inne, bardziej ogólne wyjątki PCSC lub systemowe na końcu
                    {
                        // Możesz tutaj dodać bardziej szczegółowe logowanie, jeśli to konieczne
                        IsReaderConnected = false; // Załóż, że błąd oznacza problem z czytnikiem
                        IsCardInserted = false;
                        StatusMessage = $"Błąd sprawdzania stanu czytnika: {ex.Message}";
                    }
                }
                else
                {
                    IsCardInserted = false;
                    StatusMessage = "Nie wykryto wymaganego czytnika.\nProszę podłączyć czytnik Identiv uTrust 2700 F.";
                }
            }
            catch (PCSCException pcscEx) when (pcscEx.SCardError == SCardError.NoService)
            {
                StatusMessage = "Usługa Karty Inteligentnej (SCardSvr) nie jest uruchomiona.\nProszę ją uruchomić i zrestartować aplikację.";
                ReaderStatusText = "Stan czytnika: Brak usługi systemowej";
                IsReaderConnected = false;
                IsCardInserted = false;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Błąd pobierania listy czytników: {ex.Message}";
                ReaderStatusText = "Stan czytnika: Błąd";
                IsReaderConnected = false;
                IsCardInserted = false;
            }
            finally
            {
                UpdateReaderStatusText();
            }
        }

        private void UpdateReaderStatusText()
        {
            if (!IsReaderConnected)
            {
                ReaderStatusText = "Stan czytnika: Odłączony lub nie znaleziono";
            }
            else
            {
                ReaderStatusText = $"Stan czytnika: Podłączony ({_targetReaderName})";
            }
        }

        // Poprawiona sygnatura dla StatusChanged
        private void Monitor_StatusChanged(object? sender, StatusChangeEventArgs args) // Poprawny typ argumentu
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                CheckReaderStatus();
            });
        }

        private void Monitor_CardInserted(object? sender, CardStatusEventArgs args)
        {
            if (args.ReaderName != _targetReaderName) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                IsCardInserted = true;
                StatusMessage = "Wykryto kartę. Sprawdzanie...";
                IsBusy = true;
                VerifyCard(args.Atr);
                IsBusy = false; // Ustaw IsBusy na false po zakończeniu weryfikacji
            });
        }

        private void Monitor_CardRemoved(object? sender, CardStatusEventArgs args)
        {
            if (args.ReaderName != _targetReaderName) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                IsCardInserted = false;
                StatusMessage = "Karta wyjęta. Włóż kartę administratora.";
                IsBusy = false;
            });
        }

        // Poprawiona sygnatura dla MonitorException
        // W pliku LoginViewModel.cs

        // Poprawiona obsługa wyjątków monitora
        private void Monitor_MonitorException(object? sender, Exception exception)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                string friendlyMessage = $"Wystąpił nieoczekiwany błąd monitorowania: {exception.Message}";
                ReaderStatusText = "Stan czytnika: Błąd monitora";
                IsReaderConnected = false;
                IsCardInserted = false;

                if (exception is PCSCException pcscException)
                {
                    switch (pcscException.SCardError)
                    {
                        case SCardError.Cancelled:
                            friendlyMessage = "Monitorowanie czytnika zostało anulowane.";
                            ReaderStatusText = "Stan czytnika: Monitor zatrzymany";
                            break;

                        // Usunięto CommDataLost, polegamy głównie na ReaderUnavailable
                        case SCardError.ReaderUnavailable:
                            friendlyMessage = "Czytnik został odłączony lub stał się niedostępny.";
                            ReaderStatusText = "Stan czytnika: Odłączony";
                            CheckReaderStatus(); // Spróbuj odświeżyć stan
                            break;

                        case SCardError.InvalidHandle:
                        case SCardError.ServiceStopped:
                            friendlyMessage = "Problem z usługą systemową Karty Inteligentnej. Została zatrzymana lub wystąpił błąd.";
                            ReaderStatusText = "Stan czytnika: Błąd usługi systemowej";
                            break;

                        case SCardError.NoReadersAvailable:
                            friendlyMessage = "Żadne czytniki kart nie są podłączone.";
                            ReaderStatusText = "Stan czytnika: Brak czytników";
                            CheckReaderStatus(); // Zaktualizuj stan
                            break;

                        default:
                            friendlyMessage = $"Wystąpił błąd PC/SC podczas monitorowania: {pcscException.Message} (Kod: {pcscException.SCardError})";
                            break;
                    }
                }
                else
                {
                    friendlyMessage = $"Wystąpił nieznany błąd monitorowania: {exception.Message}";
                }

                StatusMessage = friendlyMessage;
                System.Diagnostics.Debug.WriteLine($"Monitor Exception: {exception}");
            });
        }

        private void VerifyCard(byte[]? atr)
        {
            if (atr == null || atr.Length == 0)
            {
                StatusMessage = "Błąd: Nie można odczytać danych karty (ATR).";
                return;
            }

            if (atr.SequenceEqual(_targetAtr))
            {
                StatusMessage = "Karta administratora poprawna. Logowanie...";
                Task.Delay(1000).ContinueWith(_ =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LoginSuccess?.Invoke(this, new LoginSuccessEventArgs("Admin"));
                    });
                });
            }
            else
            {
                StatusMessage = "Nieprawidłowa karta! Proszę użyć karty administratora.";
                IsCardInserted = false; // Zresetuj stan wizualny
            }
        }

        public void Cleanup()
        {
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_monitor != null)
                {
                    try
                    {
                        DetachMonitorEvents();
                        _monitor.Cancel();
                        _monitor.Dispose();
                    }
                    catch { }
                    _monitor = null;
                }
                _context?.Dispose();
                _context = null;
                // StatusMessage = "Zasoby zwolnione."; // Można usunąć lub zostawić do debugowania
            }
        }

        ~LoginViewModel()
        {
            Dispose(false);
        }
    }

    public class LoginSuccessEventArgs : EventArgs
    {
        public string UserName { get; }
        public LoginSuccessEventArgs(string userName)
        {
            UserName = userName;
        }
    }
}