using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Assets;

namespace Glitch2
{
    class ParameterTemplateSelector : DataTemplateSelector
    {
        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            if (item == null)
                return null;


            var element = container as FrameworkElement;
            var parameter = item as Parameter;

            if (parameter == null)
                return null;

            if (parameter.ParameterType == ParameterType.Scalar)
            {
                return element.FindResource("ScalarTemplate") as DataTemplate;
            }
            else if (parameter.ParameterType == ParameterType.Vector2)
            {
                return element.FindResource("Vector2Template") as DataTemplate;
            }
            else if (parameter.ParameterType == ParameterType.Vector3)
            {
                return element.FindResource("Vector3Template") as DataTemplate;
            }
            else if (parameter.ParameterType == ParameterType.Vector4)
            {
                return element.FindResource("Vector4Template") as DataTemplate;
            }
            else if (parameter.ParameterType == ParameterType.Array)
            {
                return element.FindResource("ArrayTemplate") as DataTemplate;
            }

            return null;
        }
    }
}
