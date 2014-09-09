using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WinPhone;
[assembly: ExportRenderer(typeof(Beyond.WebBrowser), typeof(Beyond.WP.WebBrowserRenderer))]
namespace Beyond.WP {
  public class WebBrowserRenderer : ViewRenderer<Beyond.WebBrowser, Microsoft.Phone.Controls.WebBrowser> {
    private class RendererDelegate : WebBrowser.IWebBrowserDelegate {
      private WebBrowserRenderer renderer;
      public RendererDelegate(WebBrowserRenderer renderer) {
        this.renderer = renderer;
      }

      public Task<string> InvokeScriptAsync(string script) {
        return Task.FromResult((string)renderer.WebView.InvokeScript("eval", script));
      }

      private async Task PerformNavigationAsync(Action navigation) {
        var tcs = new TaskCompletionSource<object>();
        EventHandler<System.Windows.Navigation.NavigationEventArgs> loadFinished = (sender, args) => {
          tcs.TrySetResult(null);
        };
        NavigationFailedEventHandler loadError = (sender, args) => {
          tcs.TrySetException(args.Exception);
        };
        renderer.WebView.Navigated += loadFinished;
        renderer.WebView.NavigationFailed += loadError;
        navigation();
        await tcs.Task;
        renderer.WebView.Navigated -= loadFinished;
        renderer.WebView.NavigationFailed -= loadError;
      }

      public Task LoadPageAsync(Uri source, string rawHtml) {
        return PerformNavigationAsync(() => {
          if (rawHtml == null) {
            renderer.WebView.Navigate(source);
          } else {
            renderer.WebView.NavigateToString(rawHtml);
          }
        });
      }

      public bool CanGoBack {
        get { return renderer.WebView.CanGoBack; }
      }

      public bool CanGoForward {
        get { return renderer.WebView.CanGoForward; }
      }

      public Task GoBackAsync() {
        return PerformNavigationAsync(renderer.WebView.GoBack);
      }

      public Task GoForwardAsync() {
        return PerformNavigationAsync(renderer.WebView.GoForward);
      }

      public event Action UpdateProperties;

      public void OnUpdateProperties() {
        var up = UpdateProperties;
        if (up != null) {
          up();
        }
      }
    }

    public WebBrowserRenderer() {
    }

    private RendererDelegate rendererDelegate;

    private void UpdateProperties(object sender, object args) {
      rendererDelegate.OnUpdateProperties();
    }

    protected override void OnElementChanged(ElementChangedEventArgs<WebBrowser> e) {
      base.OnElementChanged(e);
      if (e.OldElement != null || Element == null) {
        return;
      }
      if (WebView != null) {
        WebView.Navigated -= UpdateProperties;
        WebView.Navigating -= UpdateProperties;
        WebView.NavigationFailed -= UpdateProperties;
      }
      WebView = new Microsoft.Phone.Controls.WebBrowser();
      SetNativeControl(WebView);
      rendererDelegate = new RendererDelegate(this);
      WebView.NavigationFailed += UpdateProperties;
      WebView.Navigated += UpdateProperties;
      WebView.Navigating += UpdateProperties;
      e.NewElement.BrowserDelegate = rendererDelegate;
    }
    protected override void OnElementPropertyChanged(object sender,
        PropertyChangedEventArgs e) {
      base.OnElementPropertyChanged(sender, e);
    }

    private Microsoft.Phone.Controls.WebBrowser WebView { get; set; }
  }
}
