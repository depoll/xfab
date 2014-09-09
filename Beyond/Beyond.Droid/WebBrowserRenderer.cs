using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Forms.Platform.Android;
using Android.Webkit;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.Graphics;
using Java.Interop;

[assembly: Xamarin.Forms.ExportRenderer(typeof(Beyond.WebBrowser), typeof(Beyond.Droid.WebBrowserRenderer))]
namespace Beyond.Droid {
  public class WebBrowserRenderer : ViewRenderer<WebBrowser, WebView> {
    private class RendererDelegate : WebBrowser.IWebBrowserDelegate {
      private WebBrowserRenderer renderer;
      public RendererDelegate(WebBrowserRenderer renderer) {
        this.renderer = renderer;
      }

      private static Random r = new Random();
      public Task<string> InvokeScriptAsync(string script) {
        var tcs = new TaskCompletionSource<string>();
        var callbackName = "" + r.Next();
        renderer.Callback.RegisterCallback(callbackName,
          s => tcs.TrySetResult(s),
          () => tcs.TrySetException(new Exception("JavaScript execution failed.")),
          script);
        var jsCommand =
@"(function() {
  try {
    var result = eval(_xfabCallbackObject.GetScript(""callbackName""));
    _xfabCallbackObject.Return(""callbackName"", result);
  } catch (e) {
    window.location = ""data:,"" + e;
    /*_xfabCallbackObject.Error(""callbackName"");*/
  }
})()
".Replace("callbackName", callbackName);
        renderer.WebView.LoadUrl("javascript:" + jsCommand.Replace('\n', ' '));
        return tcs.Task;
      }

      private async Task PerformNavigationAsync(Action navigation) {
        var tcs = new TaskCompletionSource<object>();
        Action<WebView, string> loadFinished = (view, url) => {
          tcs.TrySetResult(null);
        };
        Action<WebView, ClientError, string, string> loadError = (view, error, description, url) => {
          tcs.TrySetException(new Exception(string.Format("{0} : {1} <{2}>", error, description, url)));
        };
        renderer.WebViewClient.PageFinished += loadFinished;
        renderer.WebViewClient.ReceivedError += loadError;
        navigation();
        await tcs.Task;
        renderer.WebViewClient.PageFinished -= loadFinished;
        renderer.WebViewClient.ReceivedError -= loadError;
      }

      public Task LoadPageAsync(Uri source, string rawHtml) {
        return PerformNavigationAsync(() => {
          if (rawHtml == null) {
            renderer.WebView.LoadUrl(source.AbsoluteUri);
          } else {
            renderer.WebView.LoadDataWithBaseURL(source.AbsoluteUri, rawHtml, null, null, null);
          }
        });
      }

      public bool CanGoBack {
        get { return renderer.WebView.CanGoBack(); }
      }

      public bool CanGoForward {
        get { return renderer.WebView.CanGoForward(); }
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

    private class EventClient : WebViewClient {
      public override void OnPageFinished(WebView view, string url) {
        base.OnPageFinished(view, url);
        PageFinished.SafeInvoke(view, url);
      }
      public event Action<WebView, string> PageFinished;
      public override void OnReceivedError(WebView view, ClientError errorCode, string description, string failingUrl) {
        base.OnReceivedError(view, errorCode, description, failingUrl);
        ReceivedError.SafeInvoke();
      }
      public event Action<WebView, ClientError, string, string> ReceivedError;
      public override void OnPageStarted(WebView view, string url, Bitmap favicon) {
        base.OnPageStarted(view, url, favicon);
      }
      public event Action<WebView, string, Bitmap> PageStarted;
    }

    public WebBrowserRenderer() {
    }

    private RendererDelegate rendererDelegate;

    private void UpdateProperties() {
      rendererDelegate.OnUpdateProperties();
    }

    private void OnPageStarted(WebView view, string url, Bitmap favicon) {
      UpdateProperties();
    }

    private void OnReceivedError(WebView view, ClientError errorCode, string description, string failingUrl) {
      UpdateProperties();
    }

    private void OnPageFinished(WebView view, string url) {
      UpdateProperties();
    }

    private class CallbackObject : Java.Lang.Object {
      private readonly Dictionary<string, Tuple<Action<string>, Action, string>> callbacks;
      public CallbackObject() {
        callbacks = new Dictionary<string, Tuple<Action<string>, Action, string>>();
      }
      public void RegisterCallback(string name,
          Action<string> callback,
          Action error,
          string script) {
        callbacks[name] = Tuple.Create(callback, error, script);
      }

      public void UnregisterCallback(string name) {
        callbacks.Remove(name);
      }
      [JavascriptInterface]
      [Export]
      public string GetScript(string callbackName) {
        return callbacks[callbackName].Item3;
      }
      [JavascriptInterface]
      [Export]
      public void Return(string callbackName, string result) {
        callbacks[callbackName].Item1(result);
        UnregisterCallback(callbackName);
      }

      [JavascriptInterface]
      [Export]
      public void Error(string callbackName) {
        callbacks[callbackName].Item2();
        UnregisterCallback(callbackName);
      }
    }

    protected override void OnElementChanged(ElementChangedEventArgs<WebBrowser> e) {
      base.OnElementChanged(e);
      if (e.OldElement != null || Element == null) {
        return;
      }
      if (WebView != null) {
        WebViewClient.ReceivedError -= OnReceivedError;
        WebViewClient.PageFinished -= OnPageFinished;
        WebViewClient.PageStarted -= OnPageStarted;
      }
      WebView = new WebView(Context);
      WebView.Settings.JavaScriptEnabled = true;
      WebView.AddJavascriptInterface(Callback = new CallbackObject(), "_xfabCallbackObject");
      WebView.SetWebViewClient(WebViewClient = new EventClient());
      SetNativeControl(WebView);
      rendererDelegate = new RendererDelegate(this);
      WebViewClient.ReceivedError += OnReceivedError;
      WebViewClient.PageFinished += OnPageFinished;
      WebViewClient.PageStarted += OnPageStarted;
      e.NewElement.BrowserDelegate = rendererDelegate;
    }
    protected override void OnElementPropertyChanged(object sender,
        PropertyChangedEventArgs e) {
      base.OnElementPropertyChanged(sender, e);
    }

    private WebView WebView { get; set; }
    private EventClient WebViewClient { get; set; }
    private CallbackObject Callback { get; set; }
  }
}