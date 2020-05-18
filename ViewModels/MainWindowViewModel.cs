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

        public float A
        {
            get => _A;
            set => RaisePropertyChangedIfSet(ref _A, value, RelativeProperties);
        }
        float _A = 1;

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
            nameof(RGBAInfo),
            nameof(HSVAInfo)
        };

        public string RGBAInfo => string.Format("R:{0}, G:{1}, B:{2}, A:{3}", R, G, B, A);
        public string HSVAInfo => string.Format("H:{0}, S:{1}, V:{2}, A:{3}", H, S, V, A);
    }
}
