using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using MagazynLaptopowWPF.ViewModels;
using PCSC;
using PCSC.Monitoring;
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
        private readonly byte[] _jakubAtr = new byte[] {
            0x3B, 0xFB, 0x13, 0x00, 0xFF, 0x10, 0x00, 0x00, 0x31, 0xC1, 0x64, 0x09, 0x97, 0x61, 0x26, 0x0F, 0x90, 0x00
        };
        private const string TargetReaderNamePart = "Identiv uTrust 2700 F";

        // Timer do sprawdzania czytnika
        private DispatcherTimer? _readerCheckTimer;
        private bool _isWaitingForReader = false;

        // Flaga wskazująca, czy czytnik był już wcześniej podłączony i został odłączony
        private bool _wasReaderEverConnected = false;

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
                    // Jeśli czytnik był kiedykolwiek podłączony, a teraz został odłączony,
                    // ustaw flagę wskazującą, że czytnik był podłączony
                    if (!value && _isReaderConnected)
                    {
                        _wasReaderEverConnected = true;
                    }

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

        public PackIconMaterialKind StatusIconKind
        {
            get
            {
                if (!IsReaderConnected) return PackIconMaterialKind.CreditCardOffOutline;
                if (IsCardInserted) return PackIconMaterialKind.CreditCardChipOutline;
                return PackIconMaterialKind.CreditCardOutline;
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

        public void InitializeMonitoring()
        {
            try
            {
                // Inicjalizacja kontekstu i monitora
                InitializePcscContext();

                // Sprawdź status czytnika
                CheckReaderStatus();

                // Jeśli czytnik został znaleziony, rozpocznij monitorowanie
                if (!string.IsNullOrEmpty(_targetReaderName))
                {
                    _monitor?.Start(_targetReaderName);
                    System.Diagnostics.Debug.WriteLine($"Monitor started for specific reader: {_targetReaderName}");
                    _isWaitingForReader = false;
                    _wasReaderEverConnected = true;
                }
                // W przeciwnym razie rozpocznij okresowe sprawdzanie podłączenia czytnika
                else
                {
                    StartReaderCheckTimer();
                    _isWaitingForReader = true;
                }
            }
            catch (PCSCException pcscEx)
            {
                HandlePcscInitializationError(pcscEx);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Błąd inicjalizacji: {ex.Message}";
                ReaderStatusText = "Stan czytnika: Błąd inicjalizacji";
                IsReaderConnected = false;
                System.Diagnostics.Debug.WriteLine($"General Init Error: {ex}");

                // Rozpocznij okresowe sprawdzanie nawet w przypadku błędu
                StartReaderCheckTimer();
                _isWaitingForReader = true;
            }
        }

        // Nowa metoda do inicjalizacji kontekstu PCSC
        private void InitializePcscContext()
        {
            // Zwolnij istniejące zasoby, jeśli istnieją
            if (_monitor != null)
            {
                try
                {
                    DetachMonitorEvents();
                    _monitor.Cancel();
                    _monitor.Dispose();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error disposing monitor: {ex.Message}");
                }
                _monitor = null;
            }

            if (_context != null)
            {
                try
                {
                    _context.Dispose();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error disposing context: {ex.Message}");
                }
                _context = null;
            }

            // Utwórz nowy kontekst
            _context = ContextFactory.Instance.Establish(SCardScope.System);
            _monitor = new SCardMonitor(ContextFactory.Instance, SCardScope.System);
            AttachMonitorEvents();
        }

        private void HandlePcscInitializationError(PCSCException pcscEx)
        {
            if (pcscEx.SCardError == SCardError.NoService)
            {
                StatusMessage = "Usługa Karty Inteligentnej nie jest aktywna.\nSprawdź usługi systemowe lub podłącz czytnik.";
            }
            else
            {
                StatusMessage = $"Błąd inicjalizacji PC/SC: {pcscEx.Message}\nPodłącz czytnik kart.";
            }

            ReaderStatusText = "Stan czytnika: Błąd inicjalizacji";
            IsReaderConnected = false;
            System.Diagnostics.Debug.WriteLine($"PCSC Init Error: {pcscEx}");

            // Rozpocznij sprawdzanie dostępności czytnika
            StartReaderCheckTimer();
            _isWaitingForReader = true;
        }

        private void StartReaderCheckTimer()
        {
            // Zatrzymaj istniejący timer, jeśli jest uruchomiony
            _readerCheckTimer?.Stop();

            // Utwórz nowy timer, który będzie sprawdzać co 3 sekundy
            _readerCheckTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };

            _readerCheckTimer.Tick += ReaderCheckTimer_Tick;
            _readerCheckTimer.Start();

            // Informuj użytkownika - różne komunikaty w zależności od tego czy czytnik był już podłączony
            if (_wasReaderEverConnected)
            {
                StatusMessage = "Czytnik został odłączony.\nPodłącz czytnik ponownie, aby kontynuować.";
                ReaderStatusText = "Stan czytnika: Odłączony";
            }
            else
            {
                StatusMessage = "Czekam na podłączenie czytnika kart...\nPodłącz czytnik, aby kontynuować.";
                ReaderStatusText = "Stan czytnika: Oczekiwanie na podłączenie";
            }
        }

        private void ReaderCheckTimer_Tick(object? sender, EventArgs e)
        {
            if (!_isWaitingForReader) return;

            try
            {
                // Jeśli kontekst jest null lub był w stanie błędu, spróbuj go zainicjalizować ponownie
                if (_context == null)
                {
                    InitializePcscContext();
                }

                // Sprawdź, czy czytnik jest dostępny
                string[] readerNames = _context!.GetReaders();

                if (readerNames.Length > 0)
                {
                    // Znaleziono czytniki - sprawdź, czy jest wśród nich nasz docelowy
                    string? detectedReader = readerNames.FirstOrDefault(r => r.Contains(TargetReaderNamePart));

                    if (!string.IsNullOrEmpty(detectedReader))
                    {
                        // Znaleziono nasz czytnik
                        _targetReaderName = detectedReader;
                        IsReaderConnected = true;
                        _isWaitingForReader = false;
                        _wasReaderEverConnected = true;

                        // Zatrzymaj timer
                        _readerCheckTimer?.Stop();

                        // Uruchom monitorowanie
                        RestartMonitoring();

                        StatusMessage = "Wykryto czytnik. Włóż kartę administratora.";
                        System.Diagnostics.Debug.WriteLine($"Reader detected: {_targetReaderName}");
                    }
                    else
                    {
                        // Znaleziono inne czytniki, ale nie nasz docelowy
                        StatusMessage = "Znaleziono czytnik, ale nie jest to wymagany model.\nPodłącz czytnik Identiv uTrust 2700 F.";
                    }
                }
                else if (_wasReaderEverConnected)
                {
                    // Brak czytników, ale wcześniej był podłączony
                    StatusMessage = "Czytnik został odłączony.\nPodłącz czytnik ponownie, aby kontynuować.";
                }
            }
            catch (PCSCException pcscEx)
            {
                // Obsługa różnych błędów PCSC
                HandleReaderCheckPcscError(pcscEx);
            }
            catch (Exception ex)
            {
                // Ogólny błąd - nie przerywamy czekania, ale resetujemy kontekst
                System.Diagnostics.Debug.WriteLine($"General reader check error: {ex.Message}");

                // W przypadku nieznanych błędów, spróbuj zainicjalizować kontekst od nowa przy następnym sprawdzeniu
                _context = null;
                _monitor = null;

                StatusMessage = "Wystąpił nieoczekiwany błąd podczas sprawdzania czytnika.\nSpróbuj ponownie podłączyć czytnik.";
            }
        }

        // Nowa metoda do obsługi błędów PCSC podczas sprawdzania czytnika
        private void HandleReaderCheckPcscError(PCSCException pcscEx)
        {
            if (pcscEx.SCardError == SCardError.NoReadersAvailable)
            {
                if (_wasReaderEverConnected)
                {
                    StatusMessage = "Czytnik został odłączony.\nPodłącz czytnik ponownie, aby kontynuować.";
                }
                else
                {
                    StatusMessage = "Nie wykryto żadnego czytnika kart.\nPodłącz czytnik, aby kontynuować.";
                }
            }
            else if (pcscEx.SCardError == SCardError.NoService)
            {
                StatusMessage = "Usługa Karty Inteligentnej nie jest uruchomiona.\nSkontaktuj się z administratorem.";

                // Resetuj kontekst, aby przy kolejnym sprawdzeniu spróbować od nowa
                _context = null;
                _monitor = null;
            }
            else if (pcscEx.SCardError == SCardError.ReaderUnavailable ||
                     pcscEx.SCardError == SCardError.UnknownReader)
            {
                if (_wasReaderEverConnected)
                {
                    StatusMessage = "Czytnik został odłączony.\nPodłącz czytnik ponownie, aby kontynuować.";
                }
                else
                {
                    StatusMessage = "Nie można uzyskać dostępu do czytnika.\nSprawdź połączenie czytnika.";
                }

                // Zresetuj kontekst
                _context = null;
                _monitor = null;
            }
            else
            {
                StatusMessage = "Wystąpił błąd podczas sprawdzania czytnika.\nSpróbuj ponownie podłączyć czytnik.";
                System.Diagnostics.Debug.WriteLine($"PCSC error during reader check: {pcscEx.Message}, Code: {pcscEx.SCardError}");

                // Zresetuj kontekst
                _context = null;
                _monitor = null;
            }
        }

        private void RestartMonitoring()
        {
            try
            {
                // Zatrzymaj istniejący monitor, jeśli działa
                if (_monitor != null)
                {
                    try
                    {
                        _monitor.Cancel();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error canceling monitor: {ex.Message}");
                    }

                    try
                    {
                        // Uruchom monitorowanie na wykrytym czytniku
                        _monitor.Start(_targetReaderName);
                        System.Diagnostics.Debug.WriteLine($"Monitoring restarted for reader: {_targetReaderName}");
                    }
                    catch (Exception startEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error starting monitor: {startEx.Message}");

                        // W przypadku błędu startu, zainicjalizuj monitor od nowa
                        InitializePcscContext();
                        _monitor?.Start(_targetReaderName);
                    }

                    // Sprawdź, czy karta jest już włożona
                    CheckReaderStatus();
                }
                else
                {
                    // Monitor jest null, zainicjalizuj go
                    InitializePcscContext();
                    _monitor?.Start(_targetReaderName);
                    CheckReaderStatus();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restarting monitoring: {ex.Message}");

                // Niepowodzenie restartu - kontynuuj sprawdzanie timerami
                _isWaitingForReader = true;
                StatusMessage = "Nie można uruchomić monitorowania czytnika.\nSpróbuj odłączyć i ponownie podłączyć czytnik.";

                // Resetuj kontekst i monitor, aby spróbować ponownie przy następnym sprawdzeniu
                _context = null;
                _monitor = null;
            }
        }

        private void AttachMonitorEvents()
        {
            if (_monitor == null) return;
            _monitor.StatusChanged += Monitor_StatusChanged;
            _monitor.CardInserted += Monitor_CardInserted;
            _monitor.CardRemoved += Monitor_CardRemoved;
            _monitor.MonitorException += Monitor_MonitorException;
        }

        private void DetachMonitorEvents()
        {
            if (_monitor == null) return;
            _monitor.StatusChanged -= Monitor_StatusChanged;
            _monitor.CardInserted -= Monitor_CardInserted;
            _monitor.CardRemoved -= Monitor_CardRemoved;
            _monitor.MonitorException -= Monitor_MonitorException;
        }

        private void CheckReaderStatus()
        {
            if (_context == null) return;

            try
            {
                string[] readerNames = _context.GetReaders(); // Pobierz czytniki
                _targetReaderName = readerNames.FirstOrDefault(r => r.Contains(TargetReaderNamePart));
                IsReaderConnected = !string.IsNullOrEmpty(_targetReaderName);

                if (IsReaderConnected)
                {
                    _wasReaderEverConnected = true;

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
                    catch (PCSCException pex) when (pex.SCardError == SCardError.ReaderUnavailable)
                    {
                        HandleReaderDisconnected();
                    }
                    catch (RemovedCardException)
                    {
                        IsCardInserted = false;
                        StatusMessage = "Czytnik gotowy. Włóż kartę administratora.";
                    }
                    catch (PCSCException pex) when (pex.SCardError == SCardError.NoSmartcard || pex.SCardError == SCardError.RemovedCard)
                    {
                        IsCardInserted = false;
                        StatusMessage = "Czytnik gotowy. Włóż kartę administratora.";
                    }
                    catch (Exception ex)
                    {
                        // W przypadku innych błędów, załóż, że czytnik jest niedostępny
                        HandleReaderDisconnected();
                        System.Diagnostics.Debug.WriteLine($"Error checking reader status: {ex.Message}");
                    }
                }
                else if (_wasReaderEverConnected)
                {
                    // Czytnik był wcześniej podłączony, ale teraz go nie ma
                    HandleReaderDisconnected();
                }
                else
                {
                    IsCardInserted = false;
                    StatusMessage = "Nie wykryto wymaganego czytnika.\nProszę podłączyć czytnik Identiv uTrust 2700 F.";
                }
            }
            catch (PCSCException pcscEx) when (pcscEx.SCardError == SCardError.NoService)
            {
                StatusMessage = "Usługa Karty Inteligentnej (SCardSvr) nie jest uruchomiona.\nSprawdź usługi systemowe.";
                ReaderStatusText = "Stan czytnika: Brak usługi systemowej";
                IsReaderConnected = false;
                IsCardInserted = false;

                // Resetuj kontekst
                _context = null;
                _monitor = null;

                // Rozpocznij timer sprawdzania
                StartReaderCheckTimer();
                _isWaitingForReader = true;
            }
            catch (PCSCException pcscEx) when (pcscEx.SCardError == SCardError.NoReadersAvailable)
            {
                if (_wasReaderEverConnected)
                {
                    HandleReaderDisconnected();
                }
                else
                {
                    StatusMessage = "Nie wykryto żadnego czytnika kart.\nPodłącz czytnik, aby kontynuować.";
                    ReaderStatusText = "Stan czytnika: Brak czytników";
                    IsReaderConnected = false;
                    IsCardInserted = false;

                    // Rozpocznij timer sprawdzania
                    StartReaderCheckTimer();
                    _isWaitingForReader = true;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Błąd pobierania listy czytników: {ex.Message}";
                ReaderStatusText = "Stan czytnika: Błąd";
                IsReaderConnected = false;
                IsCardInserted = false;

                // Resetuj kontekst
                _context = null;
                _monitor = null;

                // Rozpocznij timer sprawdzania
                StartReaderCheckTimer();
                _isWaitingForReader = true;
            }
            finally
            {
                UpdateReaderStatusText();
            }
        }

        // Nowa metoda do obsługi odłączenia czytnika
        private void HandleReaderDisconnected()
        {
            IsReaderConnected = false;
            IsCardInserted = false;
            StatusMessage = "Czytnik został odłączony.\nPodłącz czytnik ponownie, aby kontynuować.";

            // Zresetuj kontekst i monitor
            try
            {
                if (_monitor != null)
                {
                    try
                    {
                        _monitor.Cancel();
                    }
                    catch { }
                }
            }
            catch { }

            // Rozpocznij timer sprawdzania
            StartReaderCheckTimer();
            _isWaitingForReader = true;
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

        private void Monitor_StatusChanged(object? sender, StatusChangeEventArgs args)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                System.Diagnostics.Debug.WriteLine($"Status changed: {args.ReaderName} -> {args.NewState}");

                // Jeśli stan zmienił się na niewłaściwy, sprawdź czy czytnik jest nadal podłączony
                if ((args.NewState & SCRState.Unavailable) == SCRState.Unavailable ||
                    (args.NewState & SCRState.Empty) == SCRState.Empty ||
                    (args.NewState & SCRState.Unknown) == SCRState.Unknown)
                {
                    // Sprawdź, czy czytnik jest nadal dostępny
                    try
                    {
                        CheckReaderStatus();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error in status change handler: {ex.Message}");
                        HandleReaderDisconnected();
                    }
                }
                else
                {
                    // W innych przypadkach po prostu sprawdź stan
                    CheckReaderStatus();
                }
            });
        }

        private void Monitor_CardInserted(object? sender, CardStatusEventArgs args)
        {
            // Sprawdź, czy to nasz czytnik
            if (args.ReaderName != _targetReaderName) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                IsCardInserted = true;
                StatusMessage = "Wykryto kartę. Sprawdzanie...";
                IsBusy = true;
                VerifyCard(args.Atr);
                IsBusy = false;
            });
        }

        private void Monitor_CardRemoved(object? sender, CardStatusEventArgs args)
        {
            // Sprawdź, czy to nasz czytnik
            if (args.ReaderName != _targetReaderName) return;

            Application.Current.Dispatcher.Invoke(() =>
            {
                IsCardInserted = false;
                StatusMessage = "Karta wyjęta. Włóż kartę administratora.";
                IsBusy = false;
            });
        }

        private void Monitor_MonitorException(object? sender, Exception exception)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                System.Diagnostics.Debug.WriteLine($"Monitor exception: {exception.Message}");

                if (exception is PCSCException pcscException)
                {
                    switch (pcscException.SCardError)
                    {
                        case SCardError.Cancelled:
                            // Normalne anulowanie, ignoruj
                            break;

                        case SCardError.ReaderUnavailable:
                        case SCardError.UnknownReader:
                            // Czytnik został odłączony
                            HandleReaderDisconnected();
                            break;

                        case SCardError.NoReadersAvailable:
                            // Brak czytników
                            HandleReaderDisconnected();
                            break;

                        case SCardError.InvalidHandle:
                        case SCardError.ServiceStopped:
                            // Błąd usługi
                            StatusMessage = "Problem z usługą systemową karty inteligentnej.\nSprawdź usługi systemowe.";
                            HandleReaderDisconnected();
                            break;

                        default:
                            // Inne błędy PCSC
                            StatusMessage = "Wystąpił błąd komunikacji z czytnikiem.\nSpróbuj ponownie podłączyć czytnik.";
                            HandleReaderDisconnected();
                            break;
                    }
                }
                else
                {
                    // Nieznany błąd
                    StatusMessage = "Wystąpił nieoczekiwany błąd monitorowania.\nSpróbuj ponownie podłączyć czytnik.";
                    HandleReaderDisconnected();
                }
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
            else if (atr.SequenceEqual(_jakubAtr))
            {
                StatusMessage = "Karta Jakuba Więcka rozpoznana. Logowanie...";
                Task.Delay(1000).ContinueWith(_ =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LoginSuccess?.Invoke(this, new LoginSuccessEventArgs("Jakub Więcek"));
                    });
                });
            }
            else
            {
                StatusMessage = "Nieprawidłowa karta! Proszę użyć zarejestrowanej karty.";
                IsCardInserted = false; // Zresetuj stan wizualny
            }
        }

        public void Cleanup()
        {
            // Zatrzymaj timer, jeśli działa
            if (_readerCheckTimer != null)
            {
                _readerCheckTimer.Stop();
                _readerCheckTimer.Tick -= ReaderCheckTimer_Tick;
                _readerCheckTimer = null;
            }

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
                // Zatrzymaj timer
                if (_readerCheckTimer != null)
                {
                    _readerCheckTimer.Stop();
                    _readerCheckTimer.Tick -= ReaderCheckTimer_Tick;
                    _readerCheckTimer = null;
                }

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

                if (_context != null)
                {
                    try
                    {
                        _context.Dispose();
                    }
                    catch { }
                    _context = null;
                }
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