using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beyond {
  static class UtilityExtensions {
    public static object SafeInvoke(this Delegate d, params object[] parameters) {
      if (d == null) {
        return null;
      }
      return d.DynamicInvoke(parameters);
    }
  }
}
