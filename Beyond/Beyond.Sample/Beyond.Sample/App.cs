using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace Beyond.Sample {
  public class App {
    public static Page GetMainPage() {
      WebBrowser browser;
      Grid grid;
      Button goButton;
      Button runScript;
      Button back;
      Button forward;
      Entry addressEntry;
      var result = new ContentPage {
        Content = grid = new Grid {
          RowDefinitions = {
            new RowDefinition {
              Height = new GridLength(1, GridUnitType.Star)
            },
            new RowDefinition {
              Height = GridLength.Auto
            },
            new RowDefinition {
              Height = GridLength.Auto
            },
            new RowDefinition {
              Height = GridLength.Auto
            },
            new RowDefinition {
              Height = GridLength.Auto
            },
            new RowDefinition {
              Height = GridLength.Auto
            }
          },
          ColumnDefinitions = {
            new ColumnDefinition {
              Width = new GridLength(1, GridUnitType.Star)
            }
          },
          Children = {
            {browser = new WebBrowser(), 0, 0},
            {addressEntry = new Entry(), 0, 1},
            {
              goButton = new Button {
                Text = "Go"
              }, 0, 2
            },
            {
              runScript = new Button {
                Text = "Run"
              }, 0, 3
            },
            {
              back = new Button {
                Text = "Back"
              }, 0, 4
            },
            {
              forward = new Button {
                Text = "Forward"
              }, 0, 5
            }
          }
        },
      };
      addressEntry.Text = "http://www.depoll.com";
      goButton.Clicked += async (_0, _1) => {
        await browser.NavigateAsync(new Uri(addressEntry.Text));
      };
      runScript.Clicked += async (_0, _1) => {
        var scriptResult = await browser.InvokeScriptAsync(
@"
document.title
");
        await browser.NavigateToStringAsync(scriptResult);
      };
      back.Clicked += async (_0, _1) => {
        await browser.GoBackAsync();
      };
      forward.Clicked += async (_0, _1) => {
        await browser.GoForwardAsync();
      };
      result.BindingContext = browser;
      back.SetBinding(Button.IsEnabledProperty, "CanGoBack");
      forward.SetBinding(Button.IsEnabledProperty, "CanGoForward");
      return result;
    }
  }
}
