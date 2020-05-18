using Livet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorPicker.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        public float R
        {
            get => _R;
            set => RaisePropertyChangedIfSet(ref _R, value, RelativeProperties);
        }
        float _R = 1;

        public float G
        {
            get => _G;
            set => RaisePropertyChangedIfSet(ref _G, value, RelativeProperties);
        }
        float _G = 1;

        public float B
        {
            get => _B;
            set => RaisePropertyChangedIfSet(ref _B, value, RelativeProperties);
        }
        float _B = 1;

        public float H
        {
            get => _H;
            set => RaisePropertyChangedIfSet(ref _H, value, RelativeProperties);
        }
        float _H = 1;

        public float S
        {
            get => _S;
            set => RaisePropertyChangedIfSet(ref _S, value, RelativeProperties);
        }
        float _S = 0;

        public float V
        {
            get => _V;
            set => RaisePropertyChangedIfSet(ref _V, value, RelativeProperties);
        }
        float _V = 1;

        string[] RelativeProperties = new string[]
        {
            nameof(RGBInfo),
            nameof(HSVInfo)
        };

        public string RGBInfo => string.Format("R:{0}, G:{1}, B:{2}", R, G, B);
        public string HSVInfo => string.Format("H:{0}, S:{1}, V:{2}", H, S, V);
    }
}
