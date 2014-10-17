using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Assets;

namespace Glitch2
{
    /// <summary>
    /// Interaction logic for NewParameter.xaml
    /// </summary>
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
                /*"float", "float2", "float3", "float4", "float4 array",
                "int", "int2", "int3", "int4", "int4 array",
                "bool", "bool2", "bool3", "bool4", "bool4 array"*/
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
