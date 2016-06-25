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
using System.Windows;
using System.Windows.Controls;
using Assets;

namespace AssetManager
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
