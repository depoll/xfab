using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Xamarin.Forms.Platform.Android;
using Beyond.Droid;

namespace Beyond.Sample.Droid {
  [Activity(Label = "Beyond.Sample", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
  public class MainActivity : AndroidActivity {
    protected override void OnCreate(Bundle bundle) {
      var rendererType = typeof(WebBrowserRenderer);
      base.OnCreate(bundle);

      Xamarin.Forms.Forms.Init(this, bundle);

      SetPage(App.GetMainPage());
    }
  }
}

