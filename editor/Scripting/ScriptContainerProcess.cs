﻿using StorybrewCommon.Scripting;
using StorybrewEditor.Processes;
using System;
using System.IO;

namespace StorybrewEditor.Scripting
{
    public class ScriptContainerProcess<TScript> : ScriptContainerBase<TScript>
        where TScript : Script
    {
        private RemoteProcessWorkerContainer workerProcess;

        public ScriptContainerProcess(ScriptManager<TScript> manager, string scriptTypeName, string sourcePath, string compiledScriptsPath, params string[] referencedAssemblies)
            : base(manager, scriptTypeName, sourcePath, compiledScriptsPath, referencedAssemblies)
        {
        }

        protected override ScriptProvider<TScript> LoadScript()
        {
            if (disposedValue) throw new ObjectDisposedException(nameof(ScriptContainerAppDomain<TScript>));

            try
            {
                var assemblyPath = Path.Combine(CompiledScriptsPath, $"{Guid.NewGuid().ToString()}.dll");
                ScriptCompiler.Compile(SourcePath, assemblyPath, ReferencedAssemblies);

                workerProcess?.Dispose();
                workerProcess = new RemoteProcessWorkerContainer();

                var scriptProvider = workerProcess.Worker.CreateScriptProvider<TScript>();
                scriptProvider.Initialize(assemblyPath, ScriptTypeName);

                return scriptProvider;
            }
            catch (ScriptCompilationException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new ScriptLoadingException($"{ScriptTypeName} failed to load", e);
            }
        }

        #region IDisposable Support

        private bool disposedValue = false;
        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    workerProcess?.Dispose();
                }
                workerProcess = null;
                disposedValue = true;
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}