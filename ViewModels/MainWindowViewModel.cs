using System.Collections.Generic;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Linq;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows.Input;


namespace FuncDataTemplateSample.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public ObservableCollection<string> ServicesList { get; set; } = new ObservableCollection<string>();


        public ObservableCollection<string> AllServicesList { get; set; } = new ObservableCollection<string>();


        // Greeting will change based on a Name.


        // Backing field for SearchName
        private string? _SearchName;

        void ServicesList_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // что тут происходит?)
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add: // если добавление
                    if (e.NewItems?[0] is String newPerson)
                        Console.WriteLine($"Добавлен новый объект:");
                    break;
                case NotifyCollectionChangedAction.Remove: // если удаление
                    if (e.OldItems?[0] is String oldPerson)
                        Console.WriteLine($"Удален объект: ");
                    break;
                case NotifyCollectionChangedAction.Replace: // если замена
                    if ((e.NewItems?[0] is String replacingPerson) &&
                        (e.OldItems?[0] is String replacedPerson))
                        Console.WriteLine($"Объект");
                    break;
            }
        }


        private void UpdateServicesList()
        {
            ServicesList.Clear(); // Очистить ServicesList 

            if (String.IsNullOrWhiteSpace(SearchName))
            {
                // почему не стал использовать .AddRange()?
                foreach (var item in AllServicesList)
                {
                    ServicesList.Add(item); // Добавить все элементы из AllServicesList в ServicesList 
                }
            }
            else
            {
                // лучше используй Linq с .Where()
                foreach (var item in AllServicesList)
                {
                    if (item.Contains(SearchName))
                    {
                        ServicesList.Add(item); // Добавить только элементы, содержащие SearchName, в ServicesList 
                    }
                }
            }
        }

        public string? SearchName
        {
            get => _SearchName;
            set
            {
                // подписка в Setter'е - дело не очень хорошее. Даже скорее очень нехорошее.
                // при каждом изменении этого поля у тебя создается подписка, которую ты нигде не очищаешь за собой.
                // эта подписка плодит методы, которые будут вызываться при изменении коллекции.
                // после 1000 написанных символов у тебя будет уже 1000 таких методов, которые будут выполняться 1000 раз 
                // в какой-то момент у тебя просто закончится оперативная память, я уж молчу про процессорное время.
                // да и сам метод непонятно что делает в итоге
                AllServicesList.CollectionChanged += ServicesList_CollectionChanged;
                this.RaiseAndSetIfChanged(ref _SearchName, value);

                UpdateServicesList();
            }
        }


        // по кодстайлу все имена приватных полей в C# должны начинаться с _:
        // _selectedItem
        // также не забывай писать private модификатор всегда, это хороший тон
        String selectedItem;


        public String SelectedItem
        {
            get => selectedItem;


            set
            {
                this.RaiseAndSetIfChanged(ref selectedItem, value);

                selectedItem = selectedItem.TrimStart();

                if (selectedItem.Length > 0)
                {
                    // проще было бы использовать .Split(' ') 
                    int index = SelectedItem.IndexOf(' ');

                    if (index != -1)
                    {
                        // Оставляем только первое слово до пробела 
                        selectedItem = selectedItem.Substring(0, index);
                    }
                }

                Console.WriteLine($"SelectedItem changed: {selectedItem}");
            }
        }


        public ICommand StopService { get; }


        private void StopServiceFunc()
        {
            // используй .FirstOrDefault() из LINQ. Не забывай обрабатывать null значение
            for (int i = 0; i < AllServicesList.Count; i++)
            {
                if (AllServicesList[i].Contains(selectedItem))
                {
                    if (AllServicesList[i].Contains("inactive"))
                    {
                    }
                    else
                    {
                        var procStop = new Process();

                        string commandStop = "stop " + SelectedItem;
                        var processStartInfo = new ProcessStartInfo("systemctl", commandStop)

                        {
                            RedirectStandardOutput = true
                        };

                        procStop.StartInfo = processStartInfo;
                        procStop.Start();

                        // лучше бы конечно спрашивать статус службы и убеждаться, что она действительно выключилась
                        // и завернуть инфу о службах в какую-нибудь ViewModel. Тогда не придётся бежать по списку и
                        // изменять конкретно эту строку. Да и в целом будет логичнее и решит ещё кучу разных проблем
                        for (int i1 = 0; i1 < AllServicesList.Count; i1++)
                        {
                            if (AllServicesList[i1].Contains(selectedItem))
                            {
                                AllServicesList[i1] = AllServicesList[i1].Replace("active", "inactive");
                            }
                        }

                        UpdateServicesList();
                    }
                }
            }
        }

        public ICommand RunService { get; }


        private void RunServiceFunc()
        {
            // тоже самое, что в методе выше
            Console.WriteLine("Button2 works");
            var procRun = new Process();
            string commandRun = "start " + SelectedItem;
            var processStartInfo = new ProcessStartInfo("systemctl", commandRun)
            {
                RedirectStandardOutput = true
            };

            procRun.StartInfo = processStartInfo;
            procRun.Start();

            for (int i = 0; i < AllServicesList.Count; i++)
            {
                if (AllServicesList[i].Contains(selectedItem))
                {
                    AllServicesList[i] = AllServicesList[i].Replace("inactive", "active");
                }
            }
            
            // много кода повторяется, вынес бы выполнение процесса в какой-нибудь статический метод, например:
            // var output = await ProcessExecutor.ExetuceProcess("systemctl", $"start {SelectedItem}")

            UpdateServicesList();
        }

        // корректнее было бы назвать метод GetServicesList()
        public void ServicesListGet()
        {
            var serviceManager = new ServiceManager();
            AllServicesList = serviceManager.GetServicesList();
            ServicesList = serviceManager.GetServicesList();
        }

        public MainWindowViewModel()
        {
            // долгоиграющий метод в конструкторе. Такие вещи лучше выносить в отдельные методы.
            // Конструктор предполагает, что будет выделена память и будет создан экземпляр класса. А в твоем случае
            // метод в теории может вообще зависнуть навечно. 
            // лучше вынеси в метод Initialize(), и будет вообще круто, если он будет асинхронным
            ServicesListGet();
            StopService = ReactiveCommand.Create(StopServiceFunc);
            RunService = ReactiveCommand.Create(RunServiceFunc);
        }
    }


    // более 1 класса в файле - не очень хорошо. принято разделять классы в разные файлы. так ориентироваться легче,
    // а в некоторых языках, например Java, это вообще считается ошибкой компиляции
    public class ServiceManager
    {
        // ты используешь ViewModel-логику внутри Model уровня. почитай подробнее про паттерн MVVM
        // P.S. ObservableCollection - это ViewModel логика. Она должна быть, по хорошему, только во вьюмоделях 
        public ObservableCollection<string> GetServicesList()
        {
            var proc = new Process();
            var processStartInfo = new ProcessStartInfo("systemctl", "list-units --all")
            {
                RedirectStandardOutput = true
            };

            proc.StartInfo = processStartInfo;
            proc.Start();

            var output = string.Empty;
            var servicesList = new ObservableCollection<string>();
            while (!proc.StandardOutput.EndOfStream)
            {
                output = proc.StandardOutput.ReadLine() + "\n";

                servicesList.Add(output);
            }

            proc.WaitForExit();

            return servicesList;
        }
    }
}