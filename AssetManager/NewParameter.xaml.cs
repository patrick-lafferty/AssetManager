/*
MIT License

Copyright (c) 2016 Patrick Lafferty

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System.Collections.Generic;
using System.Windows;
using Assets;

namespace AssetManager
{
    public partial class NewParameter : Window
    {
        public NewParameter()
        {
            Types = new List<Parameter>
            {
                new ScalarParameter<float>(), new ScalarParameter<int>(), new ScalarParameter<bool>(),
                new Vector2Parameter<float>(), new Vector2Parameter<int>(), new Vector2Parameter<bool>(),
                new Vector3Parameter<float>(), new Vector3Parameter<int>(), new Vector3Parameter<bool>(),
                new Vector4Parameter<float>(), new Vector4Parameter<int>(), new Vector4Parameter<bool>(),
                new ArrayParameter<float>(), new ArrayParameter<int>(), new ArrayParameter<bool>(),
            };

            this.DataContext = this;
            InitializeComponent();
            
        }

        public string ParameterName { get; set; }
        public List<Parameter> Types { get; set; }

        public Parameter Parameter { get; set; }

        private void CreateNewParameter(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ParameterName)
                || Parameter == null)
            {
                MessageBox.Show("Name/Type can't be empty!");
                return;
            }

            this.DialogResult = true;
            Parameter.Name = ParameterName;
            this.Close();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;

            this.Close();
        }
    }
}
