// MIT License.

Sys.INotifyDisposing = function () {
  /// <summary>Implement this interface if the class exposes an event to notify when it's disposing.</summary>
  ##DEBUG throw Error.notImplemented();
};
Sys.INotifyDisposing.prototype = {
  #if DEBUG
  add_disposing: function (handler) {
    throw Error.notImplemented();
  },
  remove_disposing: function (handler) {
    throw Error.notImplemented();
  },
  #endif
};
Sys.INotifyDisposing.registerInterface("Sys.INotifyDisposing");
