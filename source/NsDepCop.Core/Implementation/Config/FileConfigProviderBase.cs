﻿using System;
using System.IO;
using Codartis.NsDepCop.Core.Interface.Config;
using MoreLinq;

namespace Codartis.NsDepCop.Core.Implementation.Config
{
    /// <summary>
    /// Abstract base class for file based config implementations.
    /// </summary>
    internal abstract class FileConfigProviderBase : IConfigProvider
    {
        protected readonly string ConfigFilePath;
        protected Action<string> DiagnosticMessageHandler;

        private readonly object _isInitializedLock = new object();
        private bool _isInitialized;

        private bool _configFileExists;
        private DateTime _configLastLoadUtc;
        private IAnalyzerConfig _config;
        private Exception _configException;

        protected FileConfigProviderBase(string configFilePath, Action<string> diagnosticMessageHandler = null)
        {
            ConfigFilePath = configFilePath;
            DiagnosticMessageHandler = diagnosticMessageHandler;
        }

        private bool IsConfigLoaded => _config != null;
        private bool IsConfigErroneous => _configException != null;

        public IAnalyzerConfig Config
        {
            get
            {
                lock (_isInitializedLock)
                {
                    if (!_isInitialized)
                        Initialize();

                    return _config;
                }
            }
        }

        public AnalyzerState State
        {
            get
            {
                lock (_isInitializedLock)
                {
                    if (!_isInitialized)
                        Initialize();

                    return GetState();
                }
            }
        }

        public Exception ConfigException
        {
            get
            {
                lock (_isInitializedLock)
                {
                    if (!_isInitialized)
                        Initialize();

                    return _configException;
                }
            }
        }

        public void RefreshConfig()
        {
            _configFileExists = File.Exists(ConfigFilePath);

            if (!_configFileExists)
            {
                _configException = null;
                _config = null;
                DiagnosticMessageHandler?.Invoke($"Config file '{ConfigFilePath}' not found.");
                return;
            }

            try
            {
                if (!IsConfigLoaded || ConfigModifiedSinceLastLoad())
                {
                    if (!IsConfigLoaded)
                        DiagnosticMessageHandler?.Invoke($"Loading config file '{ConfigFilePath}' for the first time.");
                    else
                        DiagnosticMessageHandler?.Invoke($"Reloading modified config file '{ConfigFilePath}'.");

                    _configLastLoadUtc = DateTime.UtcNow;
                    _configException = null;
                    _config = LoadConfig();

                    if (DiagnosticMessageHandler != null)
                    {
                        DiagnosticMessageHandler.Invoke($"Config file '{ConfigFilePath}' loaded.");
                        DumpConfigToDiagnosticOutput();
                    }
                }
            }
            catch (Exception e)
            {
                _configException = e;
                DiagnosticMessageHandler?.Invoke($"Config file '{ConfigFilePath}' exception: {e}");
            }
        }

        protected abstract IAnalyzerConfig LoadConfig();

        private void Initialize()
        {
            _isInitialized = true;
            RefreshConfig();
        }

        private AnalyzerState GetState()
        {
            if (!_configFileExists)
                return AnalyzerState.NoConfigFile;

            if (IsConfigErroneous)
                return AnalyzerState.ConfigError;

            if (IsConfigLoaded && !Config.IsEnabled)
                return AnalyzerState.Disabled;

            if (IsConfigLoaded && Config.IsEnabled)
                return AnalyzerState.Enabled;

            throw new Exception("Inconsistent DependencyAnalyzer state.");
        }

        private bool ConfigModifiedSinceLastLoad()
        {
            return _configLastLoadUtc < File.GetLastWriteTimeUtc(ConfigFilePath);
        }

        private void DumpConfigToDiagnosticOutput()
        {
            _config.DumpToStrings().ForEach(i => DiagnosticMessageHandler.Invoke($"  {i}"));
        }
    }
}
