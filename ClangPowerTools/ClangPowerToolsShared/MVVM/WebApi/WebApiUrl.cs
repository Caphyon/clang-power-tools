﻿namespace ClangPowerTools.MVVM.WebApi
{
  public static class WebApiUrl
  {
    private static readonly string appId = "5d011c6a375f6b5ed9716629";
    private static readonly string url = @"https://account.clangpowertools.com";

    public static readonly string loginUrl = string.Concat(url, "/api/", appId, "/user/", "login");
    public static readonly string licenseUrl = string.Concat(url, "/api/", appId, "/license");
    public static readonly string forgotPasswordUrl = string.Concat(url, "/api/", appId, "/user/", "forgot-password");
    public static readonly string signUpUrl = string.Concat(url, "/api/", appId, "/user/", "register");
    public static readonly string userProfile = string.Concat(url, "/api/", appId, "/user/", "profile");
    public static readonly string settingsConfig = string.Concat(url, "/api/", appId, "/config");
  }
}
