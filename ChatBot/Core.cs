﻿using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ChatBot
{
    public partial class Core
    {
        public Action<bool> OnStateChange = (bool state) => { };
        public Action<string> OnSystemMessage = (string message) => { };

        private const int IO_PLUGIN_INIT_TIMEOUT_SEC = 10;

        private dynamic pluginInstance;
        private string botName;

        public void Connect(string botName, string pluginPath)
        {
            this.pluginInstance = PluginLoader.LoadPlugin(pluginPath);
            this.botName = botName;

            var pluginValidator = new PluginValidator();
            pluginValidator.Validate(pluginInstance);

            initPlugin();
        }

        public void Disconnect()
        {
            pluginReadyOrFail();

            pluginInstance.End();
        }

        public void SendMessage(string message)
        {
            pluginReadyOrFail();

            pluginInstance.SendMessage(message);
            writeSystemMessage($"Bot: {message}");
        }

        private async void initPlugin()
        {
            var startTime = new DateTime();

            try
            {
                pluginInstance.Init(this.botName);
            }
            catch (Exception exception)
            {
                writeSystemMessage($"Błąd inicjalizacji pluginu: {exception.Message}");
                OnStateChange(false);
                return;
            }

            await Task.Run(async () => {
                while(!pluginInstance.IsReady) {
                    if ((new DateTime()).Subtract(startTime).TotalSeconds >= IO_PLUGIN_INIT_TIMEOUT_SEC)
                    {
                        writeSystemMessage($"Nie można załadować pluginu: limit {IO_PLUGIN_INIT_TIMEOUT_SEC} sek. czasu oczekiwania został przekoczony.");
                        OnStateChange(false);
                        return;
                    }

                    await Task.Delay(100);
                }

                writeSystemMessage("Plugin wejścia/wyjścia został zainicjalizowany.");
                OnStateChange(true);
                runMainLoop();
            });
        }

        private async void runMainLoop()
        {
            writeSystemMessage("Rozmowa się rozpoczęła.");

            await Task.Run(async () => {
                while (pluginInstance.IsReady)
                {
                    Tuple<string, string> message = pluginInstance.GetMessage();

                    if (message == null)
                    {
                        await Task.Delay(100);
                        continue;
                    }

                    writeSystemMessage($"{message.Item1}: {message.Item2}");

                    var response = ProcessPhrase(message.Item1, message.Item2);

                    if (response.Length > 0)
                    {
                        pluginInstance.SendMessage(response);
                        writeSystemMessage($"Bot: {response}");
                    }
                }
            });

            writeSystemMessage("Rozmowa się zakończyła.");
            OnStateChange(false);
        }

        private string ProcessPhrase(string userName, string message)
        {
            var dictionary = new Answer();
            return dictionary.GetAnswer(message, userName);
        }

        private void writeSystemMessage(string text)
        {
            var time = DateTime.Now.ToString("hh:mm:ss");
            text = $"<b>[{time}]</b> {text}";

            OnSystemMessage(text);
        }

        private void pluginReadyOrFail()
        {
            if (pluginInstance == null || !pluginInstance.IsReady)
            {
                throw new PluginNotReadyException();
            }
        }

        public class PluginNotReadyException : Exception 
        { 
            public PluginNotReadyException() : base() { }
        }
    }
}
