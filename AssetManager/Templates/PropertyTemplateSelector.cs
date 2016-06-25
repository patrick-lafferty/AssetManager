using ToolEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Glitch2
{
    class PropertyTemplateSelector : DataTemplateSelector
    {
        public override System.Windows.DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            if (item == null)
                return null;

            var element = container as FrameworkElement;
         
            if (item is Actor)
            {
                return element.FindResource("ActorTemplate") as DataTemplate;
            }
            else if (item is StaticMesh)
            {
                return element.FindResource("StaticMeshTemplate") as DataTemplate;
            }
            else if (item is DynamicMesh)
            {
                return element.FindResource("DynamicMeshTemplate") as DataTemplate;
            }
            else if (item is Light)
            {
                return element.FindResource("LightTemplate") as DataTemplate;
            }
            else if (item is ParticleEmitter)
            {
                return element.FindResource("ParticleEmitterTemplate") as DataTemplate;
            }
            else if (item is Text)
            {
                return element.FindResource("TextTemplate") as DataTemplate;
            }
            else if (item is ToolEvents.Window)
            {
                return element.FindResource("WindowTemplate") as DataTemplate;
            }
            else if (item is ToolEvents.Camera)
            {
                return element.FindResource("CameraTemplate") as DataTemplate;
            }
            else if (item is PrimitiveBody)
            {
                return element.FindResource("PrimitiveBodyTemplate") as DataTemplate;
            }
            else if (item is TriggerVolume)
            {
                return element.FindResource("TriggerVolumeTemplate") as DataTemplate;
            }
            else if (item is SpringConstraint)
            {
                return element.FindResource("SpringConstraintTemplate") as DataTemplate;
            }
            else if (item is Constraint)
            {
                return element.FindResource("ConstraintTemplate") as DataTemplate;
            }
            else if (item is Logic)
            {
                return element.FindResource("LogicTemplate") as DataTemplate;
            }
            else if (item is SoundEffect)
            {
                return element.FindResource("SoundEffectTemplate") as DataTemplate;
            }
            else if (item is Vehicle)
            {
                return element.FindResource("VehicleTemplate") as DataTemplate;
            }

            return null;
        }
    }
}
