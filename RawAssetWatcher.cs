using AssetManager;
using Assets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Glitch2
{
    class RawAssetWatcher
    {
        FileSystemWatcher watcher;
        AssetViewmodel viewmodel;
        Action<string> displayError;
        Action<string> displaySuccess;
        Action<object> updateDependents;
        
        internal RawAssetWatcher(AssetViewmodel model, 
            Action<string> displayError, Action<string> displaySuccess, 
            Action<object> updateDependents)
        {
            viewmodel = model;
            this.displayError = displayError;
            this.displaySuccess = displaySuccess;
            this.updateDependents = updateDependents;            

            watcher = new FileSystemWatcher();
            watcher.Path = System.IO.Path.GetFullPath(System.IO.Path.Combine(Environment.CurrentDirectory, @"..\..\..\..\RawAssets"));
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
            watcher.Filter = "*.*";
            watcher.IncludeSubdirectories = true;
            watcher.Changed += watcher_Changed;
            watcher.Renamed += watcher_Renamed;
            watcher.EnableRaisingEvents = true;
        }

        string lastNotifiedName;
        DateTime lastNotifiedTime;

        internal bool TryUpdateAsset(Object asset)
        {
            bool finished = false;

            while (!finished)
            {
                try
                {
                    string error, success;

                    if (Updator.UpdateAsset(asset, out error, out success))
                    {
                        //log success
                        displaySuccess(success);
                        finished = true;

                        updateDependents(asset);

                        return true;
                    }
                    else
                    {
                        if (error.Contains("The process cannot access"))
                            continue;

                        //log failure
                        displayError(error);
                        finished = true;
                    }
                }
                catch (IOException ex)
                {
                    System.Threading.Thread.Sleep(100);
                }
                catch (Exception ex)
                {
                    //log
                    displayError(ex.Message);
                    finished = true;
                }
            }

            return false;
        }

        void watcher_Renamed(object sender, RenamedEventArgs e)
        {
            watcher_Changed(sender, e);
        }

        void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (!e.Name.Contains("."))
                return;            

            if (lastNotifiedName == e.Name)
            {
                if ((DateTime.Now - lastNotifiedTime).TotalMilliseconds < 500)
                {
                    //this is a "ghost" notification
                    //FileSystemWatcher usually gives 2 notifications per actual write
                    lastNotifiedTime = DateTime.Now;
                    return;
                }
                else
                {
                    lastNotifiedTime = DateTime.Now;
                }
            }
            else
            {
                lastNotifiedName = e.Name;
                lastNotifiedTime = DateTime.Now;
            }

            var name = System.IO.Path.GetFullPath(e.FullPath);

            Object asset;

            if (viewmodel.TryGetAsset(name, out asset))
            {
                TryUpdateAsset(asset);                
            }
        }
    }
}
