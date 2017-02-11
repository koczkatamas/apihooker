using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using LiveObjects.ModelDescription;

namespace LiveObjects.Test
{
    [PublicationPolicy(DefaultPublicationMode.OptIn)]
    public class TestObject : IUIObject, INotifyPropertyChanged
    {
        public string ResourceId => "TestObject";

        [Publish]
        public string StringProperty { get; set; }

        [Publish]
        public ObservableCollection<string> List { get; set; }

        [Publish]
        public string Echo(string msg) => $"echo response: {msg}";

        [Publish]
        public async Task<string> SlowEchoAsync(string msg)
        {
            await Task.Delay(100);
            return $"slow echo response: {msg}";
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}