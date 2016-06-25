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
using AssetManager;
using System;
using System.IO;

namespace AssetManager
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
            watcher.Path = Path.GetFullPath(Properties.Settings.Default.RawAssetsPath);
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
            watcher.Filter = "*.*";
            watcher.IncludeSubdirectories = true;
            watcher.Changed += watcher_Changed;
            watcher.Renamed += watcher_Renamed;
            watcher.EnableRaisingEvents = true;
        }

        string lastNotifiedName;
        DateTime lastNotifiedTime;

        internal void stopWatching()
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }

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
                catch (IOException)
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
