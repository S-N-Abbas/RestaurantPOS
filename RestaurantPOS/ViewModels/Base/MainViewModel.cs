using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;

namespace RestaurantPOS.ViewModels.Base
{ 
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private string title = "Restaurant POS – Ready";
    }

}
