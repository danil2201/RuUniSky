﻿using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FishyFlip;
using FishyFlip.Models;
using UniSky.Extensions;
using UniSky.Helpers;
using UniSky.Models;
using UniSky.Pages;
using UniSky.Services;
using UniSky.ViewModels.Error;

namespace UniSky.ViewModels;

public partial class LoginViewModel : ViewModelBase
{
    private readonly LoginService loginService;
    private readonly SessionService sessionService;
    private readonly INavigationService navigationService;

    [ObservableProperty]
    private bool _advanced;
    [ObservableProperty]
    private string _username;
    [ObservableProperty]
    private string _password;
    [ObservableProperty]
    private string _host;

    public LoginViewModel(LoginService loginService, SessionService sessionService, INavigationServiceLocator navigationServiceLocator)
    {
        this.loginService = loginService;
        this.sessionService = sessionService;
        this.navigationService = navigationServiceLocator.GetNavigationService("Root");

        Advanced = false;
        Username = "";
        Password = "";
        Host = "https://bsky.social";
    }

    [RelayCommand]
    private async Task Login()
    {
        Error = null;

        using var context = GetLoadingContext();
        try
        {
            var normalisedHost = new UriBuilder(Host)
                .Host.ToLowerInvariant();

            var builder = new ATProtocolBuilder()
                .EnableAutoRenewSession(true)
                .WithInstanceUrl(new Uri(Host));

            using var protocol = builder.Build();
            var session = await protocol.AuthenticateWithPasswordAsync(Username, Password, CancellationToken.None)
                .ConfigureAwait(false);

            var loginModel = this.loginService.SaveLogin(normalisedHost, Username, Password);
            var sessionModel = new SessionModel(true, normalisedHost, session);

            sessionService.SaveSession(sessionModel);
            syncContext.Post(() =>
                navigationService.Navigate<HomePage>(session.Did));
        }
        catch (Exception ex)
        {
            Error = new ExceptionViewModel(ex);
        }
    }

    [RelayCommand]
    private void ToggleAdvanced()
    {
        Advanced = !Advanced;
    }
}
