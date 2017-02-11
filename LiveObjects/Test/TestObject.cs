using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using LiveObjects.ModelDescription;

namespace LiveObjects.Test
{
    [PublicationPolicy(DefaultPublicationMode.OptIn)]
    public class TestObject : ILiveObject, INotifyPropertyChanged
    {
        public string ResourceId => "TestObject";

        [Publish]
        public string StringProperty { get; set; } = "StringPropertyContent";

        [Publish]
        public ObservableCollection<string> List { get; set; } = new ObservableCollection<string> { "ListItem #1", "ListItem #2", "ListItem #3", "ListItem #4", "ListItem #5" };

        [Publish]
        public string Echo(string msg) => $"echo response: {msg}";

        [Publish]
        public string ComputedProperty => StringProperty + " | " + StringProperty;

        public string SecretProperty { get; set; } = "OhNoIAmASecret!";

        [Publish]
        public async Task<string> SlowEchoAsync(string msg)
        {
            await Task.Delay(100);
            return $"slow echo response: {msg}";
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}