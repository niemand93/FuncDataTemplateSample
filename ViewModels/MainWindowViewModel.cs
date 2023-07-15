
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
    switch (e.Action)
    {
        case NotifyCollectionChangedAction.Add: // если добавление
            if(e.NewItems?[0] is String newPerson)
                Console.WriteLine($"Добавлен новый объект:");
            break;
        case NotifyCollectionChangedAction.Remove: // если удаление
            if (e.OldItems?[0] is String oldPerson)
                Console.WriteLine($"Удален объект: ");
            break;
        case NotifyCollectionChangedAction.Replace: // если замена
            if ((e.NewItems?[0] is String replacingPerson)  && 
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
        foreach (var item in AllServicesList) 
        { 
            ServicesList.Add(item); // Добавить все элементы из AllServicesList в ServicesList 
        } 
    } 
    else 
    { 
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
                AllServicesList.CollectionChanged += ServicesList_CollectionChanged;
                this.RaiseAndSetIfChanged(ref _SearchName, value);
                
                  UpdateServicesList();
           

            
            
        
            }

           
            
        }

      
 
 String selectedItem;


    public String SelectedItem
    {
        get => selectedItem;
        
        
        set {
           
            this.RaiseAndSetIfChanged(ref selectedItem, value); 
             
            selectedItem = selectedItem.TrimStart(); 
            
                if (selectedItem.Length > 0 ) 
{ 
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
            
            for(int i = 0; i < AllServicesList.Count; i ++)
            {
                if (AllServicesList[i].Contains(selectedItem) )
            { 
               if(AllServicesList[i].Contains("inactive"))
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

            for(int i1 = 0; i1 < AllServicesList.Count; i1 ++)
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
          Console.WriteLine("Button2 works");
            var procRun = new Process(); 
            string commandRun = "start " + SelectedItem;
            var processStartInfo = new ProcessStartInfo("systemctl", commandRun) 
            { 
                RedirectStandardOutput = true 
            }; 
 
            procRun.StartInfo = processStartInfo; 
            procRun.Start(); 
           
  for(int i = 0; i < AllServicesList.Count; i ++)
            {
                if (AllServicesList[i].Contains(selectedItem)) 
            { 
               AllServicesList[i] = AllServicesList[i].Replace("inactive", "active");
            } 
            }
            UpdateServicesList();

        }

        public void ServicesListGet()
        {
            var serviceManager = new ServiceManager();   
            AllServicesList = serviceManager.GetServicesList(); 
            ServicesList = serviceManager.GetServicesList(); 
        }

        public MainWindowViewModel() 
        {
             ServicesListGet();
              StopService = ReactiveCommand.Create(StopServiceFunc);
               RunService = ReactiveCommand.Create(RunServiceFunc);
        } 




    } 

    

 
    public class ServiceManager 
    { 
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