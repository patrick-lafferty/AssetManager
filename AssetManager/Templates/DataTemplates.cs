using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Glitch2
{
    partial class DataTemplates
    {
        private void NewProperty(object sender, AddingNewItemEventArgs e)
        {
            var grid = sender as DataGrid;
            var viewmodel = (grid.Tag as ActorEditor).viewmodel;

            viewmodel.NewProperty.Execute(e);
        }
    }
}
