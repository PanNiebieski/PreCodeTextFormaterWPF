using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace PreCodeTextFormater
{
    public class CodeTitleDataContextModel
    {
        private string _CodeTitle;

        public event PropertyChangedEventHandler PropertyChanged;

        public string CodeTitle
        {
            get { return _CodeTitle; }
            set
            {
                _CodeTitle = value;
                OnPropertyChanged("CodeTitle");
            }
        }

        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
