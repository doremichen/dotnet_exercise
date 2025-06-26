using System.Collections.ObjectModel;
using WpfApp_DataBinding.Models;

namespace WpfApp_DataBinding.ViewModels
{

    public class CollectionViewModel
    {

        public ObservableCollection<Person> People { get; set; }

        public CollectionViewModel()
        {
            People = new ObservableCollection<Person>();
            People.Add(new Person() { Name = "John", Comment = "John comment" });
            People.Add(new Person() { Name = "Jane", Comment = "Jane comment" });
            People.Add(new Person() { Name = "Jack", Comment = "Jack comment" });
            People.Add(new Person() { Name = "Jill", Comment = "Jill comment" });
        }
    }
}
