﻿using Blu4Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;

namespace BluMiniPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string _playerName;
        private IReadOnlyCollection<string> _mediaTitles;
        private Uri _mediaImageUri;
        private PlayerState _playerState;
        private string _skipAction;
        private string _backAction;
        private int _volume;

        public BluPlayer Player { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;
            Loaded += MainWindow_Loaded;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool SetPropertyValue<T>(ref T storage, T value, [CallerMemberName] string name = null)
        {
            if (!Equals(storage, value))
            {
                storage = value;
                RaisePropertyChanged(name);
                return true;
            }
            return false;
        }

        private void RaisePropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var endpoint = await BluEnvironment.ResolveEndpoints().FirstOrDefaultAsync();
            if (endpoint != null)
            {
                Player = await BluPlayer.Connect(endpoint);
                PlayerName = Player.Name;

                PlayerState = await Player.GetState();
                Player.StateChanges.ObserveOn(SynchronizationContext.Current).Subscribe(value => PlayerState = value);

                var volume = await Player.GetVolume();
                Volume = volume.Percentage;
                Player.VolumeChanges.ObserveOn(SynchronizationContext.Current).Subscribe(value => Volume = value.Percentage);

                UpdateMedia(await Player.GetMedia());
                Player.MediaChanges.ObserveOn(SynchronizationContext.Current).Subscribe(UpdateMedia);
            }
        }

        private void UpdateMedia(PlayerMedia media)
        {
            _mediaTitles = media.Titles;
            RaisePropertyChanged(nameof(MediaTitle1));
            RaisePropertyChanged(nameof(MediaTitle2));
            RaisePropertyChanged(nameof(MediaTitle3));

            MediaImageUri = media.ImageUri;
            _backAction = null;
            _skipAction = null;

            if (media.PlayerState == PlayerState.Streaming)
            {
                var backButtonVisibility = Visibility.Collapsed;
                var skipButtonVisibility = Visibility.Collapsed;
                foreach (var action in media.Actions)
                {
                    if (action.Action == PlayerAction.Back && action.Url != null)
                    {
                        _backAction = action.Url;
                        backButtonVisibility = Visibility.Visible;
                    }
                    if (action.Action == PlayerAction.Skip && action.Url != null)
                    {
                        _skipAction = action.Url;
                        skipButtonVisibility = Visibility.Visible;
                    }
                }
                BackButton.Visibility = backButtonVisibility;
                SkipButton.Visibility = skipButtonVisibility;
            }
            else if (media.PlayerState == PlayerState.Playing)
            {
                BackButton.Visibility = Visibility.Visible;
                SkipButton.Visibility = Visibility.Visible;
            }
        }

        public string PlayerName
        {
            get { return _playerName; }
            set { SetPropertyValue(ref _playerName, value); }
        }

        public string MediaTitle1
        {
            get { return _mediaTitles?.Skip(0).FirstOrDefault(); }
        }

        public string MediaTitle2
        {
            get { return _mediaTitles?.Skip(1).FirstOrDefault(); }
        }

        public string MediaTitle3
        {
            get { return _mediaTitles?.Skip(2).FirstOrDefault(); }
        }

        public Uri MediaImageUri
        {
            get { return _mediaImageUri; }
            set { SetPropertyValue(ref _mediaImageUri, value); }
        }

        public int Volume
        {
            get { return _volume; }
            set { SetPropertyValue(ref _volume, value); }
        }

        public PlayerState PlayerState
        {
            get { return _playerState; }
            set 
            { 
                if (SetPropertyValue(ref _playerState, value))
                {
                    PauseButton.Visibility = PlayerState == PlayerState.Paused ? Visibility.Collapsed : Visibility.Visible;
                    PlayButton.Visibility = PlayerState == PlayerState.Paused ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }


        private async void Preset_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && int.TryParse((string)element.Tag, out var preset))
            {
                await Player.PresetList.LoadPreset(preset);
            }
        }

        private async void Action_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                switch(element.Tag)
                {
                    case "Back":
                        if (_backAction == null)
                        {
                            await Player.Back();
                        }
                        else
                        {
                            await Player.Action(_backAction);
                        }
                        break;
                    case "Play":
                        await Player.Play();
                        break;
                    case "Pause":
                        await Player.Pause();
                        break;
                    case "Skip":
                        if (_skipAction == null)
                        {
                            await Player.Skip();
                        }
                        else 
                        {
                            await Player.Action(_skipAction);
                        }
                        break;

                }
            }
        }

        private async void ChangeVolume_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                var volume = await Player.GetVolume();
                switch (element.Tag)
                {
                    case "Up":
                        await Player.SetVolume(volume.Percentage + 2);
                        break;
                    case "Down":
                        await Player.SetVolume(volume.Percentage - 2);
                        break;
                }
            }
        }

        private void Caption_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
